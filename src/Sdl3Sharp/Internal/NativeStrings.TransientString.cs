using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	[StructLayout(LayoutKind.Sequential)]
	internal ref struct TransientString<T> : IDisposable
		where T : unmanaged
	{
		private unsafe T* mBuffer;
		private nuint mCapacity;
		private nuint mLength;
		private readonly bool mZeroMemoryUponDispose;

		[Obsolete($"Do not use directly. Only {nameof(TransientString<>)} created as a result of a call to {nameof(FromUtf16ToUtf8)} or {nameof(FromUtf8ToUtf16)} should be used.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal unsafe TransientString(T* buffer, nuint capacity, nuint length, bool zeroMemoryUponDispose)
		{
			mBuffer = buffer;
			mCapacity = capacity;
			mLength = length;
			mZeroMemoryUponDispose = zeroMemoryUponDispose;
		}

		public unsafe readonly T* Buffer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mBuffer; }

		public readonly nuint Capacity { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mCapacity; }

		public readonly nuint Length { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mLength; }

		public readonly bool IsMemoryZeroedUponDispose { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mZeroMemoryUponDispose; }

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void Dispose()
		{
			unsafe
			{
				var buffer = mBuffer;
				var capacity = mCapacity;

				if (buffer is null)
				{
					return;
				}

				mBuffer = null;
				mCapacity = 0;
				mLength = 0;

				ReturnConverterBuffer(buffer, unchecked(capacity * (nuint)Unsafe.SizeOf<T>()), mZeroMemoryUponDispose);
			}
		}
	}
}
