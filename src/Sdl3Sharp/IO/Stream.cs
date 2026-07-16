using Sdl3Sharp.Internal;
using Sdl3Sharp.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.IO;

/// <summary>
/// A base stream for reading and writing data
/// </summary>
public abstract partial class Stream : IDisposable
{
	private unsafe SDL_IOStream* mContext = null;
	private GCHandle mSelfHandle = default;

	/// <remarks>
	/// <para>
	/// This constructor does neither <see langword="throw"/> nor fail otherwise
	/// </para>
	/// </remarks>
	private protected unsafe Stream(SDL_IOStream* context) => mContext = context;

	/// <summary>
	/// Creates a new <see cref="Stream"/> instance that uses a custom IO implementation provided by the derived class
	/// </summary>
	/// <exception cref="SdlException">The custom IO stream could not be created (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	protected Stream()
	{
		unsafe
		{
			var iface = new SDL_IOStreamInterface(this, out mSelfHandle);
			mContext = SDL_OpenIO(&iface, unchecked((void*)GCHandle.ToIntPtr(mSelfHandle)));

			if (mContext is null)
			{
				mSelfHandle.Free();
				mSelfHandle = default;				

				failCouldNotCreateStream();
			}
		}
			
		[DoesNotReturn]
		static void failCouldNotCreateStream() => throw new SdlException("Could not create the custom IO stream");
	}

	/// <inheritdoc/>
	~Stream() => Dispose(disposing: false, close: true);

	/// <summary>
	/// Gets the total length, in bytes, of the stream
	/// </summary>
	/// <value>
	/// The total length, in bytes, of the stream, or <c>-1</c> on failure
	/// </value>
	/// <seealso cref="Length"/>
	protected abstract long LengthCore { get; }

	/// <summary>
	/// Seeks within the stream
	/// </summary>
	/// <param name="offset">The offset to seek to</param>
	/// <param name="whence">The reference point for the seek operation</param>
	/// <returns>The absolute offset from the start of the stream after seeking, or <c>-1</c> on failure</returns>
	/// <seealso cref="TrySeek(long, StreamWhence, out long)"/>
	protected abstract long SeekCore(long offset, StreamWhence whence);

	/// <summary>
	/// Reads into specified data from the stream
	/// </summary>
	/// <param name="data">The <see cref="Utilities.NativeMemory">memory buffer</see> to read data into</param>
	/// <param name="status">
	/// The <see cref="StreamStatus"/> of the stream.
	/// The value of the referenced <see cref="StreamStatus"/> could change on failure and should not necessarily change on success.
	/// </param>
	/// <returns>The number of bytes read from the stream</returns>
	/// <seealso cref="TryRead(Utilities.NativeMemory, out nuint)"/>
	protected abstract nuint ReadCore(Utilities.NativeMemory data, ref StreamStatus status);

	/// <summary>
	/// Writes all specified data into the stream
	/// </summary>
	/// <param name="data">The <see cref="ReadOnlyNativeMemory">memory buffer</see> containing all the data to be written into the stream</param>
	/// <param name="status">
	/// The <see cref="StreamStatus"/> of the stream.
	/// The value of the referenced <see cref="StreamStatus"/> could change on failure and should not necessarily change on success.
	/// </param>
	/// <returns>The number of bytes read from the stream</returns>
	/// <seealso cref="TryWrite(ReadOnlyNativeMemory, out nuint)"/>
	protected abstract nuint WriteCore(ReadOnlyNativeMemory data, ref StreamStatus status);

	/// <summary>
	/// Flushes any buffered data in the stream
	/// </summary>
	/// <param name="status">
	/// The <see cref="StreamStatus"/> of the stream.
	/// The value of the referenced <see cref="StreamStatus"/> could change on failure and should not necessarily change on success.
	/// </param>
	/// <returns><c><see langword="true"/></c> if the flush was successful; otherwise, <c><see langword="false"/></c></returns>
	/// <seealso cref="TryFlush"/>
	protected abstract bool FlushCore(ref StreamStatus status);

	/// <summary>
	/// Closes the stream and release associated resources
	/// </summary>
	/// <returns><c><see langword="true"/></c> if the stream was closed successfully; otherwise, <c><see langword="false"/></c></returns>
	/// <seealso cref="TryClose"/>
	protected abstract bool CloseCore();

	/// <summary>
	/// Gets a value indicating whether the <see cref="Stream"/> is valid
	/// </summary>
	/// <value>
	/// A value indicating whether the <see cref="Stream"/> is valid
	/// </value>
	/// <remarks>
	/// <para>
	/// A <see cref="Stream"/> becomes invalid after it has been <see cref="TryClose">closed</see> or <see cref="Dispose()">disposed</see>.
	/// </para>
	/// </remarks>
	public bool IsValid { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get { unsafe { return mContext is not null; } } }

	/// <summary>
	/// Gets the length of the stream, in bytes
	/// </summary>
	/// <value>
	/// The length of the stream, in bytes
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is not threadsafe.
	/// </para>
	/// </remarks>
	public long Length
	{
		get
		{
			unsafe
			{
				return SDL_GetIOSize(mContext);
			}
		}
	}	

	internal unsafe SDL_IOStream* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mContext; }

	/// <summary>
	/// Gets the properties associated with the stream
	/// </summary>
	/// <value>
	/// The properties associated with the stream, or <see langword="null"/> if the properties could not be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is not threadsafe.
	/// </para>
	/// </remarks>
	public Properties? Properties
	{
		get
		{
			unsafe
			{
				return SDL_GetIOProperties(mContext) switch
				{	
					0 => null,
					var id => Properties.GetOrCreate(id, registerWithSdl: false)
				};
			}
		}
	}

	/// <summary>
	/// Gets the current read/write offset in the stream, in bytes
	/// </summary>
	/// <value>
	/// The current read/write offset in the stream, in bytes
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is actually a wrapper around <see cref="TrySeek(long, StreamWhence, out long)"/> with an offset of 0 bytes from <see cref="StreamWhence.Current"/>.
	/// </para>
	/// <para>
	/// This property is not threadsafe.
	/// </para>
	/// </remarks>
	public long Position
	{
		get
		{
			unsafe
			{
				return SDL_TellIO(mContext);
			}
		}
	}

	/// <summary>
	/// Gets a pointer to the underlying SDL <c>SDL_IOStream</c> that the <see cref="Stream"/> represents
	/// </summary>
	/// <value>
	/// A pointer to the underlying SDL <c>SDL_IOStream</c> that the <see cref="Stream"/> represents. This can be cast to a SDL <c>SDL_IOStream*</c>.
	/// </value>
	public IntPtr SdlIOStream { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get { unsafe { return unchecked((IntPtr)mContext); } } }

	/// <summary>
	/// Gets the current status of the stream
	/// </summary>
	/// <value>
	/// The current status of the stream
	/// </value>
	/// <remarks>
	/// <para>
	/// Inspecting the status of the stream can be useful to determine if a short read or write operation was due to an <see cref="StreamStatus.Error">error</see>,
	/// due to <see cref="StreamStatus.Eof">reaching the end of the stream</see>, or due to a <see cref="StreamStatus.NotReady">non-blocking operation that hasn't finished</see>.
	/// </para>
	/// <para>
	/// The value of this property is only expected to change after read or write operations on the stream. Don't expect it to change spontaneously.
	/// </para>
	/// <para>
	/// This property is not threadsafe.
	/// </para>
	/// </remarks>
	public StreamStatus Status
	{
		get
		{
			unsafe
			{
				return SDL_GetIOStatus(mContext);
			}
		}
	}

	/// <inheritdoc/>
#pragma warning disable CA1816 // We do that in Dispose(bool)
	public void Dispose() => Dispose(close: true);
#pragma warning restore CA1816

	internal void Dispose(bool close)
	{
#pragma warning disable CA1816 // The containing method is essentially our central dipose logic
		Dispose(disposing: true, close);
		GC.SuppressFinalize(this);
#pragma warning restore CA1816
	}

	/// <summary>
	/// Disposes the stream
	/// </summary>
	/// <param name="disposing">A value indicating whether the call came from a call to <see cref="Dispose()"/> or from the finalizer</param>
	/// <param name="close">A value indicating whether the stream should be <see cref="TryClose">closed</see></param>
	/// <remarks>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	/// <seealso cref="TryClose"/>
	protected virtual void Dispose(bool disposing, bool close)
	{
		unsafe
		{
			if (mContext is not null)
			{
				if (close)
				{
					SDL_CloseIO(mContext);
				}

				mContext = null;
			}

			if (mSelfHandle.IsAllocated)
			{
				mSelfHandle.Free();
				mSelfHandle = default;
			}
		}
	}

	/// <summary>
	/// Tries to close the stream and releases associated resources
	/// </summary>
	/// <returns><c><see langword="true"/></c> if the stream was closed without failure; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Note: this method returns <c><see langword="false"/></c> if the stream couldn't be <see cref="TryFlush">flushed</see> successfully before closing. The stream is still <see cref="IsValid">invalid</see> when this method returns.
	/// </para>
	/// <para>
	/// A call to this method flushes any buffered writes to the operating system, but there are no guarantees that those writes have gone to physical media; they might be in the OS's file cache, waiting to go to disk later.
	/// If it's absolutely crucial that writes go to disk immediately, so they are definitely stored even if the power fails before the file cache would have caught up, one should call <see cref="TryFlush"/> before closing.
	/// Note that flushing takes time and makes the system and your app operate less efficiently, so do so sparingly.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryClose()
	{
		unsafe
		{
			bool result = SDL_CloseIO(mContext);

			mContext = null;
			
			if (mSelfHandle.IsAllocated)
			{
				mSelfHandle.Free();
				mSelfHandle = default;
			}

			return result;
		}
	}

	/// <summary>
	/// Tries to flush any buffered data in the stream
	/// </summary>
	/// <returns><c><see langword="true"/></c> if the flush was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// A call to this method makes sure that any buffered data is written to the stream.
	/// Normally this isn't necessary but if the stream is a pipe or socket it guarantees that any pending data is sent.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryFlush()
	{
		unsafe
		{
			return SDL_FlushIO(mContext);
		}
	}

	/// <summary>
	/// Tries to load all the data from the stream
	/// </summary>
	/// <param name="data">A <see cref="NativeMemoryManager"/> managing native memory containing all available data from the stream, when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="closeAfterwards">A value indicating whether the stream should be closed before this method returns (even in case of an error)</param>
	/// <returns><c><see langword="true"/></c>, if all available data from the stream was succesfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting <see cref="NativeMemoryManager"/> should be <see cref="NativeMemoryManagerBase.Dispose()">disposed</see> if the memory it's managing is no longer needed. That also frees the allocated memory.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryLoad([NotNullWhen(true)] out NativeMemoryManager? data, bool closeAfterwards = false)
	{
		unsafe
		{
			nuint dataSize;
			void* dataPtr;
			try
			{
				dataPtr = SDL_LoadFile_IO(mContext, &dataSize, closeAfterwards);
			}
			finally
			{
				if (closeAfterwards)
				{
					Dispose(disposing: true, close: false /* SDL_LoadFile_IO already closed the stream */);
				}
			}

			if (dataPtr is null)
			{
				data = null;
				return false;
			}

			data = new(dataPtr, dataSize, &Utilities.NativeMemory.SDL_free);
			return true;
		}
	}

	/// <summary>
	/// Tries to read data from the stream
	/// </summary>
	/// <param name="data">The <see cref="Utilities.NativeMemory">memory buffer</see> to read data into</param>
	/// <param name="bytesRead">The number of bytes read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, <c>0</c></param>
	/// <returns><c><see langword="true"/></c> if the data was successfully read into the specified <see cref="Utilities.NativeMemory">memory buffer</see>; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method might read less bytes than requested by <paramref name="data"/>'s <see cref="Utilities.NativeMemory.Length"/> property.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c> (<paramref name="bytesRead"/> will be <c>0</c>), if the end of the stream was reached before reading any data (<see cref="Status"/> is <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> (<paramref name="bytesRead"/> is <c>0</c>) and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryRead(Utilities.NativeMemory data, out nuint bytesRead)
	{
		unsafe
		{
			if (!data.IsValid)
			{
				bytesRead = 0;
				return false;
			}

			bytesRead = SDL_ReadIO(mContext, data.RawPointer, data.Length);

			return bytesRead is not 0;
		}
	}

	/// <summary>
	/// Tries to read data from the stream
	/// </summary>
	/// <param name="data">The <see cref="Span{T}"/> to read data into</param>
	/// <param name="bytesRead">The number of bytes read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, <c>0</c></param>
	/// <returns><c><see langword="true"/></c> if the data was successfully read into the specified <see cref="Span{T}"/>; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method might read less bytes than requested by <paramref name="data"/>'s <see cref="Span{T}.Length"/> property.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c> (<paramref name="bytesRead"/> will be <c>0</c>), if the end of the stream was reached before reading any data (<see cref="Status"/> is <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> (<paramref name="bytesRead"/> is <c>0</c>) and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryRead(Span<byte> data, out int bytesRead)
	{
		unsafe
		{
			fixed (byte* ptr = data)
			{
				bytesRead = unchecked((int)SDL_ReadIO(mContext, ptr, unchecked((nuint)data.Length)));
			}

			return bytesRead is not 0;
		}
	}

	/// <summary>
	/// Tries to read data from the stream
	/// </summary>
	/// <param name="data">A pointer to the unmananged memory to read data into</param>
	/// <param name="size">The number of bytes to read</param>
	/// <param name="bytesRead">The number of bytes read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, <c>0</c></param>
	/// <returns><c><see langword="true"/></c> if the data was successfully read into the specified unmanaged memory; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method might read less bytes than requested by <paramref name="size"/>.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c> (<paramref name="bytesRead"/> will be <c>0</c>), if the end of the stream was reached before reading any data (<see cref="Status"/> is <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> (<paramref name="bytesRead"/> is <c>0</c>) and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public unsafe bool TryRead(void* data, nuint size, out nuint bytesRead)
	{
		unsafe
		{
			bytesRead = SDL_ReadIO(mContext, data, size);

			return bytesRead is not 0;
		}
	}

	/// <summary>
	/// Tries to read a value of type <typeparamref name="T"/> from the stream
	/// </summary>
	/// <typeparam name="T">The type of the value to read</typeparam>
	/// <param name="value">The value of type <typeparamref name="T"/> that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, uninitialized</param>
	/// <param name="bytesRead">The number of bytes read from the stream</param>
	/// <returns><c><see langword="true"/></c> if the value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Values will be read bitwise (structure-wise) and in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the value of type <typeparamref name="T"/> and <paramref name="bytesRead"/> be equal to the number of potentially partially read bytes (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryRead<T>(out T value, out nuint bytesRead)
		where T : unmanaged, allows ref struct
	{
		unsafe
		{
			fixed (T* ptr = &value)
			{
				bytesRead = SDL_ReadIO(mContext, &ptr, unchecked((nuint)Unsafe.SizeOf<T>()));

				return bytesRead == unchecked((nuint)Unsafe.SizeOf<T>());
			}
		}
	}

	/// <summary>
	/// Tries to read a big-endian double-precision floating point value from the stream
	/// </summary>
	/// <param name="value">The double-precision floating point value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the double-precision floating point value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the double-precision floating point value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadDoubleBE(out double value)
	{
		unsafe
		{
			ulong localValue;

			var result = SDL_ReadU64BE(mContext, &localValue);

			value = BitConverter.UInt64BitsToDouble(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian double-precision floating point value from the stream
	/// </summary>
	/// <param name="value">The double-precision floating point value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the double-precision floating point value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the double-precision floating point value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadDoubleLE(out double value)
	{
		unsafe
		{
			ulong localValue;

			var result = SDL_ReadU64LE(mContext, &localValue);

			value = BitConverter.UInt64BitsToDouble(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian half-precision floating point value from the stream
	/// </summary>
	/// <param name="value">The half-precision floating point value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the half-precision floating point value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the half-precision floating point value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadHalfBE(out Half value)
	{
		unsafe
		{
			ushort localValue;

			var result = SDL_ReadU16BE(mContext, &localValue);

			value = BitConverter.UInt16BitsToHalf(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian half-precision floating point value from the stream
	/// </summary>
	/// <param name="value">The half-precision floating point value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the half-precision floating point value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the half-precision floating point value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadHalfLE(out Half value)
	{
		unsafe
		{
			ushort localValue;

			var result = SDL_ReadU16LE(mContext, &localValue);

			value = BitConverter.UInt16BitsToHalf(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian 128-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 128-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 128-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 128-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadInt128BE(out Int128 value)
	{
		unsafe
		{
			Int128 localValue;

			var result = SDL_ReadIO(mContext, &localValue, unchecked((nuint)Unsafe.SizeOf<Int128>())) == unchecked((nuint)Unsafe.SizeOf<Int128>());

			value = Integral.FromBigEndianInt128(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian 128-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 128-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 128-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 128-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt128LE(out Int128 value)
	{
		unsafe
		{
			Int128 localValue;

			var result = SDL_ReadIO(mContext, &localValue, unchecked((nuint)Unsafe.SizeOf<Int128>())) == unchecked((nuint)Unsafe.SizeOf<Int128>());

			value = Integral.FromLittleEndianInt128(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian 16-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 16-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 16-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 16-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadInt16BE(out short value)
	{
		unsafe
		{
			short localValue;

			var result = SDL_ReadS16BE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian 16-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 16-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 16-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 16-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadInt16LE(out short value)
	{
		unsafe
		{
			short localValue;

			var result = SDL_ReadS16LE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian 32-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 32-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 32-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 32-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadInt32BE(out int value)
	{
		unsafe
		{
			int localValue;

			var result = SDL_ReadS32BE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian 32-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 32-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 32-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 32-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadInt32LE(out int value)
	{
		unsafe
		{
			int localValue;

			var result = SDL_ReadS32LE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian 64-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 64-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 64-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 64-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadInt64BE(out long value)
	{
		unsafe
		{
			long localValue;

			var result = SDL_ReadS64BE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian 64-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 64-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 64-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 64-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadInt64LE(out long value)
	{
		unsafe
		{
			long localValue;

			var result = SDL_ReadS64LE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read an 8-bit signed integer value from the stream
	/// </summary>
	/// <param name="value">The 8-bit signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 8-bit signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 8-bit signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadInt8(out sbyte value)
	{
		unsafe
		{
			sbyte localValue;

			var result = SDL_ReadS8(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian pointer-sized signed integer value from the stream
	/// </summary>
	/// <param name="value">The pointer-sized signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the pointer-sized signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the pointer-sized signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadIntPtrBE(out nint value)
	{
		unsafe
		{
			nint localValue;

			var result = SDL_ReadIO(mContext, &localValue, unchecked((nuint)Unsafe.SizeOf<nint>())) == unchecked((nuint)Unsafe.SizeOf<nint>());

			value = Integral.FromBigEndianIntPtr(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian pointer-sized signed integer value from the stream
	/// </summary>
	/// <param name="value">The pointer-sized signed integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the pointer-sized signed integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the pointer-sized signed integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadIntPtrLE(out nint value)
	{
		unsafe
		{
			nint localValue;

			var result = SDL_ReadIO(mContext, &localValue, unchecked((nuint)Unsafe.SizeOf<nint>())) == unchecked((nuint)Unsafe.SizeOf<nint>());

			value = Integral.FromLittleEndianIntPtr(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian single-precision floating point value from the stream
	/// </summary>
	/// <param name="value">The single-precision floating point value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the single-precision floating point value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the single-precision floating point value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadSingleBE(out float value)
	{
		unsafe
		{
			uint localValue;

			var result = SDL_ReadU32BE(mContext, &localValue);

			value = BitConverter.UInt32BitsToSingle(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian single-precision floating point value from the stream
	/// </summary>
	/// <param name="value">The single-precision floating point value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the single-precision floating point value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the single-precision floating point value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadSingleLE(out float value)
	{
		unsafe
		{
			uint localValue;

			var result = SDL_ReadU32LE(mContext, &localValue);

			value = BitConverter.UInt32BitsToSingle(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian 128-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 128-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 128-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 128-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt128BE(out UInt128 value)
	{
		unsafe
		{
			UInt128 localValue;

			var result = SDL_ReadIO(mContext, &localValue, unchecked((nuint)Unsafe.SizeOf<UInt128>())) == unchecked((nuint)Unsafe.SizeOf<UInt128>());

			value = Integral.FromBigEndianUInt128(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian 128-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 128-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 128-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 128-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt128LE(out UInt128 value)
	{
		unsafe
		{
			UInt128 localValue;

			var result = SDL_ReadIO(mContext, &localValue, unchecked((nuint)Unsafe.SizeOf<UInt128>())) == unchecked((nuint)Unsafe.SizeOf<UInt128>());

			value = Integral.FromLittleEndianUInt128(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian 16-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 16-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 16-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 16-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt16BE(out ushort value)
	{
		unsafe
		{
			ushort localValue;

			var result = SDL_ReadU16BE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian 16-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 16-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 16-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 16-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt16LE(out ushort value)
	{
		unsafe
		{
			ushort localValue;

			var result = SDL_ReadU16LE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian 32-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 32-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 32-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 32-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt32BE(out uint value)
	{
		unsafe
		{
			uint localValue;

			var result = SDL_ReadU32BE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian 32-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 32-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 32-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 32-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt32LE(out uint value)
	{
		unsafe
		{
			uint localValue;

			var result = SDL_ReadU32LE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian 64-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 64-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 64-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 64-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt64BE(out ulong value)
	{
		unsafe
		{
			ulong localValue;

			var result = SDL_ReadU64BE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian 64-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 64-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 64-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 64-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt64LE(out ulong value)
	{
		unsafe
		{
			ulong localValue;

			var result = SDL_ReadU64LE(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read an 8-bit unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The 8-bit unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the 8-bit unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the 8-bit unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUInt8(out byte value)
	{
		unsafe
		{
			byte localValue;

			var result = SDL_ReadU8(mContext, &localValue);

			value = localValue;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a big-endian pointer-sized unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The pointer-sized unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the pointer-sized unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in big-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the pointer-sized unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUIntPtrBE(out nuint value)
	{
		unsafe
		{
			nuint localValue;

			var result = SDL_ReadIO(mContext, &localValue, unchecked((nuint)Unsafe.SizeOf<nuint>())) == unchecked((nuint)Unsafe.SizeOf<nuint>());

			value = Integral.FromBigEndianUIntPtr(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to read a little-endian pointer-sized unsigned integer value from the stream
	/// </summary>
	/// <param name="value">The pointer-sized unsigned integer value that was read from the stream, when this method returns <c><see langword="true"/></c>; otherwise, undefined</param>
	/// <returns><c><see langword="true"/></c> if the pointer-sized unsigned integer value was successfully read; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value is expected to be stored in the stream in little-endian byte order. The resulting <paramref name="value"/> will be in the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the end of the stream was reached before completely reading the pointer-sized unsigned integer value (<see cref="Status"/> will be <see cref="StreamStatus.Eof"/>).
	/// If this method returns <c><see langword="false"/></c> and the <see cref="Status"/> is <em>not</em> <see cref="StreamStatus.Eof"/>, an error occured and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryReadUIntPtrLE(out nuint value)
	{
		unsafe
		{
			nuint localValue;

			var result = SDL_ReadIO(mContext, &localValue, unchecked((nuint)Unsafe.SizeOf<nuint>())) == unchecked((nuint)Unsafe.SizeOf<nuint>());

			value = Integral.FromLittleEndianUIntPtr(localValue);

			return result;
		}
	}

	/// <summary>
	/// Tries to save specified data into the stream
	/// </summary>
	/// <param name="data">The <see cref="ReadOnlyNativeMemory">memory buffer</see> containing all the data to be saved into the stream</param>
	/// <param name="closeAfterwards">A value indicating whether the stream should be closed before this method returns (even in case of an error)</param>
	/// <returns><c><see langword="true"></see></c> if the data from the specified <see cref="ReadOnlyNativeMemory">memory buffer</see> was successfully saved into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TrySave(ReadOnlyNativeMemory data, bool closeAfterwards = false)
	{
		unsafe
		{
			try
			{
				return data.IsValid && (bool)SDL_SaveFile_IO(mContext, data.RawPointer, data.Length, closeAfterwards);
			}
			finally
			{
				if (closeAfterwards)
				{
					Dispose(disposing: true, close: false /* SDL_SaveFile_IO already closed the stream */);
				}
			}
		}
	}

	/// <summary>
	/// Tries to save specified data into the stream
	/// </summary>
	/// <param name="data">The <see cref="Span{T}"/> containing all the data to be saved into the stream</param>
	/// <param name="closeAfterwards">A value indicating whether the stream should be closed before this method returns (even in case of an error)</param>
	/// <returns><c><see langword="true"></see></c> if the data from the specified <see cref="Span{T}"/> was successfully saved into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TrySave(ReadOnlySpan<byte> data, bool closeAfterwards = false)
	{
		unsafe
		{
			fixed(byte* ptr = data)
			{
				try
				{
					return SDL_SaveFile_IO(mContext, ptr, unchecked((nuint)data.Length), closeAfterwards);
				}
				finally
				{
					if (closeAfterwards)
					{
						Dispose(disposing: true, close: false /* SDL_SaveFile_IO already closed the stream */);
					}
				}
			}
		}
	}

	/// <summary>
	/// Tries to save specified data into the stream
	/// </summary>
	/// <param name="data">A pointer to the unmanaged memory containing all the data to be saved into the stream</param>
	/// <param name="size">The size, in bytes, of the data to be saved</param>
	/// <param name="closeAfterwards">A value indicating whether the stream should be closed before this method returns (even in case of an error)</param>
	/// <returns><c><see langword="true"></see></c> if the data from the specified unmanaged memory was successfully saved into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The unmanaged memory pointed to by <paramref name="data"/> must be safely dereferencable for at least <paramref name="size"/> bytes.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public unsafe bool TrySave(void* data, nuint size, bool closeAfterwards = false)
	{
		unsafe
		{
			try
			{
				return SDL_SaveFile_IO(mContext, data, size, closeAfterwards);
			}
			finally
			{
				if (closeAfterwards)
				{
					Dispose(disposing: true, close: false /* SDL_SaveFile_IO already closed the stream */);
				}
			}
		}
	}

	/// <summary>
	/// Tries to seek within the stream
	/// </summary>
	/// <param name="offset">The offset to seek to</param>
	/// <param name="whence">The reference point for the seek operation</param>
	/// <param name="absoluteOffset">The absolute offset from the start of the stream after seeking, when this method returns <c><see langword="true"/></c>; otherwise, <c>-1</c></param>
	/// <returns><c><see langword="true"/></c> if the seek operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method will return <c><see langword="false"/></c>, if the stream cannot be seeked.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TrySeek(long offset, StreamWhence whence, out long absoluteOffset)
	{
		unsafe
		{
			absoluteOffset = SDL_SeekIO(mContext, offset, whence);

			return absoluteOffset is not -1;
		}
	}

	/// <summary>
	/// Tries to write specified data into the stream
	/// </summary>
	/// <param name="data">The <see cref="ReadOnlyNativeMemory">memory buffer</see> containing all the data to be written into the stream</param>
	/// <param name="bytesWritten">The number of bytes written to the stream</param>
	/// <returns><c><see langword="true"/></c> if the data from the specified <see cref="ReadOnlyNativeMemory">memory buffer</see> was successfully written into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// In case of an error, this method still attempts to write as many bytes as possible before returning.
	/// <paramref name="bytesWritten"/> will be equal to the actual number of bytes written to the stream, even in the case when not all <paramref name="data"/> could be written.
	/// You should check the stream's <see cref="Status"/> property to determine whether a shorted write operation is recoverable or not.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWrite(ReadOnlyNativeMemory data, out nuint bytesWritten)
	{
		unsafe
		{
			if (!data.IsValid)
			{
				bytesWritten = 0;
				return false;
			}

			bytesWritten = SDL_WriteIO(mContext, data.RawPointer, data.Length);

			return bytesWritten == data.Length;
		}
	}

	/// <summary>
	/// Tries to write specified data into the stream
	/// </summary>
	/// <param name="data">The <see cref="Span{T}"/> containing all the data to be written into the stream</param>
	/// <param name="bytesWritten">The number of bytes written to the stream</param>
	/// <returns><c><see langword="true"/></c> if the data from the specified <see cref="Span{T}"/> was successfully written into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// In case of an error, this method still attempts to write as many bytes as possible before returning.
	/// <paramref name="bytesWritten"/> will be equal to the actual number of bytes written to the stream, even in the case when not all <paramref name="data"/> could be written.
	/// You should check the stream's <see cref="Status"/> property to determine whether a shorted write operation is recoverable or not.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWrite(ReadOnlySpan<byte> data, out int bytesWritten)
	{
		unsafe
		{
			fixed(byte* ptr = data)
			{
				bytesWritten = unchecked((int)SDL_WriteIO(mContext, ptr, unchecked((nuint)data.Length)));
			}

			return bytesWritten == data.Length;
		}
	}

	/// <summary>
	/// Tries to write specified data into the stream
	/// </summary>
	/// <param name="data">A pointer to the unmanaged memory containing all the data to be written into the stream</param>
	/// <param name="size">The size, in bytes, of the data to be written</param>
	/// <param name="bytesWritten">The number of bytes written to the stream</param>
	/// <returns><c><see langword="true"/></c> if the data from the specified unmanaged memory was successfully written into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The unmanaged memory pointed to by <paramref name="data"/> must be safely dereferencable for at least <paramref name="size"/> bytes.
	/// </para>
	/// <para>
	/// In case of an error, this method still attempts to write as many bytes as possible before returning.
	/// <paramref name="bytesWritten"/> will be equal to the actual number of bytes written to the stream, even in the case when not all <paramref name="data"/> could be written.
	/// You should check the stream's <see cref="Status"/> property to determine whether a shorted write operation is recoverable or not.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public unsafe bool TryWrite(void* data, nuint size, out nuint bytesWritten)
	{
		unsafe
		{
			bytesWritten = SDL_WriteIO(mContext, data, size);

			return bytesWritten == size;
		}
	}

	/// <summary>
	/// Tries to write a string into the stream
	/// </summary>
	/// <param name="text">The string to be written into the stream</param>
	/// <param name="bytesWritten">The number of bytes written to the stream</param>
	/// <returns><c><see langword="true"/></c> if the string was successfully written into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="text"/> parameter is treated as a C-style <c>printf</c> format string. 
	/// This means that any <c>%</c> characters are interpreted as format specifiers. 
	/// To include a literal <c>%</c> character in the output, you must escape it by writing <c>%%</c>.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWrite(string text, out nuint bytesWritten)
	{
		unsafe
		{
			var textUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(text);
			try
			{
				bytesWritten = SDL_IOprintf(mContext, textUtf8);

				return bytesWritten is not 0 || string.IsNullOrEmpty(text);
			}
			finally
			{
				Utf8StringMarshaller.Free(textUtf8);
			}
		}
	}

	/// <summary>
	/// Tries to write a formatted string into the stream
	/// </summary>
	/// <param name="format">The C-style <c>printf</c> format string to be written into the stream</param>
	/// <param name="bytesWritten">The number of bytes written to the stream</param>
	/// <param name="args">The arguments corresponding to the format specifiers in <paramref name="format"/></param>
	/// <returns><c><see langword="true"/></c> if the formatted string was successfully written into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="format"/> parameter is interpreted as a C-style <c>printf</c> format string, and 
	/// the <paramref name="args"/> parameter supplies the values for its format specifiers. Supported argument 
	/// types include all integral types up to 64-bit (including <c><see langword="bool"/></c> and <c><see langword="char"/></c>), 
	/// floating point types (<c><see langword="float"/></c> and <c><see langword="double"/></c>), pointer types 
	/// (<c><see cref="IntPtr"/></c> and <c><see cref="UIntPtr"/></c>), and <c><see langword="string"/></c>.
	/// </para>
	/// <para>
	/// For a detailed explanation of C-style <c>printf</c> format strings and their specifiers, see 
	/// <see href="https://en.wikipedia.org/wiki/Printf#Format_specifier"/>.
	/// </para>
	/// <para>
	/// Consider using <see cref="TryWrite(string, out nuint)"/> instead when possible, as it may be more efficient. 
	/// In many cases, you can use C# string interpolation to construct the string before writing.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWrite(string format, out nuint bytesWritten, params ReadOnlySpan<object> args)
	{
		unsafe
		{
			bytesWritten = Variadic.Invoke<nuint>(in SDL_IOprintf_var(), 2, [unchecked((IntPtr)mContext), format, ..args]);

			return bytesWritten is not 0 || string.IsNullOrEmpty(format);
		}
	}

	/// <summary>
	/// Tries to write a value of type <typeparamref name="T"/> into the stream
	/// </summary>
	/// <typeparam name="T">The type of the value to write</typeparam>
	/// <param name="value">The value of type <typeparamref name="T"/> to be written into the stream</param>
	/// <param name="bytesWritten">The number of bytes written to the stream</param>
	/// <returns><c><see langword="true"/></c> if the value was successfully written into the stream; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Values will be written bitwise (structure-wise) and in the endianness of the current platform.
	/// </para>
	/// <para>
	/// In case of an error, this method still attempts to write as many bytes as possible before returning.
	/// <paramref name="bytesWritten"/> will be equal to the actual number of bytes written to the stream, even in the case when <paramref name="value"/> was only partially written into stream.
	/// You should check the stream's <see cref="Status"/> property to determine whether a shorted write operation is recoverable or not.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWrite<T>(in T value, out nuint bytesWritten)
		where T : unmanaged, allows ref struct
	{
		unsafe
		{
			fixed (T* ptr = &value)
			{
				bytesWritten = SDL_WriteIO(mContext, ptr, unchecked((nuint)Unsafe.SizeOf<T>()));

				return bytesWritten == unchecked((nuint)Unsafe.SizeOf<T>());
			}
		}
	}

	/// <summary>
	/// Tries to write a big-endian double-precision floating point value to the stream
	/// </summary>
	/// <param name="value">The double-precision floating point value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the double-precision floating point value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteDoubleBE(double value)
	{
		unsafe
		{
			return SDL_WriteU64BE(mContext, BitConverter.DoubleToUInt64Bits(value));
		}
	}

	/// <summary>
	/// Tries to write a little-endian double-precision floating point value to the stream
	/// </summary>
	/// <param name="value">The double-precision floating point value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the double-precision floating point value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteDoubleLE(double value)
	{
		unsafe
		{
			return SDL_WriteU64LE(mContext, BitConverter.DoubleToUInt64Bits(value));
		}
	}

	/// <summary>
	/// Tries to write a big-endian half-precision floating point value to the stream
	/// </summary>
	/// <param name="value">The half-precision floating point value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the half-precision floating point value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteHalfBE(Half value)
	{
		unsafe
		{
			return SDL_WriteU16BE(mContext, BitConverter.HalfToUInt16Bits(value));
		}
	}

	/// <summary>
	/// Tries to write a little-endian half-precision floating point value to the stream
	/// </summary>
	/// <param name="value">The half-precision floating point value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the half-precision floating point value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteHalfLE(Half value)
	{
		unsafe
		{
			return SDL_WriteU16LE(mContext, BitConverter.HalfToUInt16Bits(value));
		}
	}

	/// <summary>
	/// Tries to write a big-endian 128-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 128-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 128-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt128BE(Int128 value)
	{
		unsafe
		{
			var swappedValue = Integral.ToBigEndianInt128(value);

			return SDL_WriteIO(mContext, &swappedValue, unchecked((nuint)Unsafe.SizeOf<Int128>())) == unchecked((nuint)Unsafe.SizeOf<Int128>());
		}
	}

	/// <summary>
	/// Tries to write a little-endian 128-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 128-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 128-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt128LE(Int128 value)
	{
		unsafe
		{
			var swappedValue = Integral.ToLittleEndianInt128(value);

			return SDL_WriteIO(mContext, &swappedValue, unchecked((nuint)Unsafe.SizeOf<Int128>())) == unchecked((nuint)Unsafe.SizeOf<Int128>());
		}
	}

	/// <summary>
	/// Tries to write a big-endian 16-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 16-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 16-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt16BE(short value)
	{
		unsafe
		{
			return SDL_WriteS16BE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a little-endian 16-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 16-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 16-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt16LE(short value)
	{
		unsafe
		{
			return SDL_WriteS16LE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a big-endian 32-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 32-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 32-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt32BE(int value)
	{
		unsafe
		{
			return SDL_WriteS32BE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a little-endian 32-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 32-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 32-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt32LE(int value)
	{
		unsafe
		{
			return SDL_WriteS32LE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a big-endian 64-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 64-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 64-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt64BE(long value)
	{
		unsafe
		{
			return SDL_WriteS64BE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a little-endian 64-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 64-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 64-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt64LE(long value)
	{
		unsafe
		{
			return SDL_WriteS64LE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write an 8-bit signed integer value to the stream
	/// </summary>
	/// <param name="value">The 8-bit signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 8-bit signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteInt8(sbyte value)
	{
		unsafe
		{
			return SDL_WriteS8(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a big-endian pointer-sized signed integer value to the stream
	/// </summary>
	/// <param name="value">The pointer-sized signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the pointer-sized signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteIntPtrBE(nint value)
	{
		unsafe
		{
			var swappedValue = Integral.ToBigEndianIntPtr(value);

			return SDL_WriteIO(mContext, &swappedValue, unchecked((nuint)Unsafe.SizeOf<nint>())) == unchecked((nuint)Unsafe.SizeOf<nint>());
		}
	}

	/// <summary>
	/// Tries to write a little-endian pointer-sized signed integer value to the stream
	/// </summary>
	/// <param name="value">The pointer-sized signed integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the pointer-sized signed integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteIntPtrLE(nint value)
	{
		unsafe
		{
			var swappedValue = Integral.ToLittleEndianIntPtr(value);

			return SDL_WriteIO(mContext, &swappedValue, unchecked((nuint)Unsafe.SizeOf<nint>())) == unchecked((nuint)Unsafe.SizeOf<nint>());
		}
	}

	/// <summary>
	/// Tries to write a big-endian single-precision floating point value to the stream
	/// </summary>
	/// <param name="value">The single-precision floating point value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the single-precision floating point value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteSingleBE(float value)
	{
		unsafe
		{
			return SDL_WriteU32BE(mContext, BitConverter.SingleToUInt32Bits(value));
		}
	}

	/// <summary>
	/// Tries to write a little-endian single-precision floating point value to the stream
	/// </summary>
	/// <param name="value">The single-precision floating point value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the single-precision floating point value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteSingleLE(float value)
	{
		unsafe
		{
			return SDL_WriteU32LE(mContext, BitConverter.SingleToUInt32Bits(value));
		}
	}

	/// <summary>
	/// Tries to write a big-endian 128-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 128-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 128-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt128BE(UInt128 value)
	{
		unsafe
		{
			var swappedValue = Integral.ToBigEndianUInt128(value);

			return SDL_WriteIO(mContext, &swappedValue, unchecked((nuint)Unsafe.SizeOf<UInt128>())) == unchecked((nuint)Unsafe.SizeOf<UInt128>());
		}
	}

	/// <summary>
	/// Tries to write a little-endian 128-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 128-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 128-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt128LE(UInt128 value)
	{
		unsafe
		{
			var swappedValue = Integral.ToLittleEndianUInt128(value);

			return SDL_WriteIO(mContext, &swappedValue, unchecked((nuint)Unsafe.SizeOf<UInt128>())) == unchecked((nuint)Unsafe.SizeOf<UInt128>());
		}
	}

	/// <summary>
	/// Tries to write a big-endian 16-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 16-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 16-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt16BE(ushort value)
	{
		unsafe
		{
			return SDL_WriteU16BE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a little-endian 16-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 16-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 16-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt16LE(ushort value)
	{
		unsafe
		{
			return SDL_WriteU16LE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a big-endian 32-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 32-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 32-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt32BE(uint value)
	{
		unsafe
		{
			return SDL_WriteU32BE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a little-endian 32-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 32-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 32-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt32LE(uint value)
	{
		unsafe
		{
			return SDL_WriteU32LE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a big-endian 64-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 64-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 64-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt64BE(ulong value)
	{
		unsafe
		{
			return SDL_WriteU64BE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a little-endian 64-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 64-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 64-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt64LE(ulong value)
	{
		unsafe
		{
			return SDL_WriteU64LE(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write an 8-bit unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The 8-bit unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the 8-bit unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt8(byte value)
	{
		unsafe
		{
			return SDL_WriteU8(mContext, value);
		}
	}

	/// <summary>
	/// Tries to write a big-endian pointer-sized unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The pointer-sized unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the pointer-sized unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in big-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUIntPtrBE(nuint value)
	{
		unsafe
		{
			var swappedValue = Integral.ToBigEndianUIntPtr(value);

			return SDL_WriteIO(mContext, &swappedValue, unchecked((nuint)Unsafe.SizeOf<nuint>())) == unchecked((nuint)Unsafe.SizeOf<nuint>());
		}
	}

	/// <summary>
	/// Tries to write a little-endian pointer-sized unsigned integer value to the stream
	/// </summary>
	/// <param name="value">The pointer-sized unsigned integer value to write into the stream</param>
	/// <returns><c><see langword="true"></see></c> if the pointer-sized unsigned integer value was successfully written; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The value will be stored in the stream in little-endian byte order regardless of the endianness of the current platform.
	/// </para>
	/// <para>
	/// This method is not threadsafe.
	/// </para>
	/// </remarks>
	public bool TryWriteUInt128LE(nuint value)
	{
		unsafe
		{
			var swappedValue = Integral.ToLittleEndianUIntPtr(value);

			return SDL_WriteIO(mContext, &swappedValue, unchecked((nuint)Unsafe.SizeOf<nuint>())) == unchecked((nuint)Unsafe.SizeOf<nuint>());
		}
	}
}
