using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Timing;

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public sealed partial class Timer :
	IDisposable, Sdl.IDisposeReceiver, IEquatable<Timer>, IFormattable, ISpanFormattable
{
	private GCHandle mWrapperHandle;
	private uint mId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	public Timer(uint millisecondsInterval, TimerMillisecondsCallback callback)
	{

		if (millisecondsInterval is 0)
		{
			failMillisecondsIntervalArgumentIsZero();
		}

		if (callback is null)
		{
			failCallbackArgumentIsNull();
		}

		unsafe
		{
			mWrapperHandle = GCHandle.Alloc(new MillisecondsCallbackWrapper(this, callback), GCHandleType.Normal);

			try
			{
				mId = SDL_AddTimer(millisecondsInterval, &MillisecondsCallbackWrapper.TimerCallback, unchecked((void*)GCHandle.ToIntPtr(mWrapperHandle)));

				if (mId is 0)
				{
					failCouldNotAddTimer();
				}

				Sdl.TryRegisterDisposable(this);
			}
			catch
			{
				mWrapperHandle.Free();
				mWrapperHandle = default;

				throw;
			}
		}

		[DoesNotReturn]
		static void failMillisecondsIntervalArgumentIsZero() => throw new ArgumentException($"The {nameof(millisecondsInterval)} argument must be greater than zero", nameof(millisecondsInterval));

		[DoesNotReturn]
		static void failCallbackArgumentIsNull() => throw new ArgumentNullException(nameof(callback));

		[DoesNotReturn]
		static void failCouldNotAddTimer() => throw new SdlException("Could not add the timer");
	}

	public Timer(ulong nanosecondsInterval, TimerNanosecondsCallback callback)
	{
		if (nanosecondsInterval is 0)
		{
			failNanosecondsIntervalArgumentIsZero();
		}

		if (callback is null)
		{
			failCallbackArgumentIsNull();
		}

		unsafe
		{
			mWrapperHandle = GCHandle.Alloc(new NanosecondsCallbackWrapper(this, callback), GCHandleType.Normal);

			try
			{
				mId = SDL_AddTimerNS(nanosecondsInterval, &NanosecondsCallbackWrapper.NSTimerCallback, unchecked((void*)GCHandle.ToIntPtr(mWrapperHandle)));

				if (mId is 0)
				{
					failCouldNotAddTimer();
				}

				Sdl.TryRegisterDisposable(this);
			}
			catch
			{
				mWrapperHandle.Free();
				mWrapperHandle = default;

				throw;
			}
		}

		[DoesNotReturn]
		static void failNanosecondsIntervalArgumentIsZero() => throw new ArgumentException($"The {nameof(nanosecondsInterval)} argument must be greater than zero", nameof(nanosecondsInterval));

		[DoesNotReturn]
		static void failCallbackArgumentIsNull() => throw new ArgumentNullException(nameof(callback));

		[DoesNotReturn]
		static void failCouldNotAddTimer() => throw new SdlException("Could not add the timer");
	}

	~Timer() => Dispose(deregisterFromSdl: true);

	public static ulong MillisecondTicks => SDL_GetTicks();

	public static ulong NanosecondTicks => SDL_GetTicksNS();

	public static ulong PerformanceCounter => SDL_GetPerformanceCounter();

	public static ulong PerformanceFrequency => SDL_GetPerformanceFrequency();

	public uint Id { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mId; }

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(deregisterFromSdl: true);
	}

	void Sdl.IDisposeReceiver.DisposeFromSdl(Sdl sdl)
	{
#pragma warning disable IDE0079
#pragma warning disable CA1816
		GC.SuppressFinalize(this);
#pragma warning restore CA1816
#pragma warning restore IDE0079
		Dispose(deregisterFromSdl: false);
	}

	private void Dispose(bool deregisterFromSdl)
	{
		if (mWrapperHandle is { IsAllocated: true, Target: CallbackWrapper wrapper })
		{
			try
			{
				wrapper.Dispose();
			}
			finally
			{
				mWrapperHandle.Free();
			}
		}

		mWrapperHandle = default;

		if (mId is not 0)
		{
			if (deregisterFromSdl)
			{
				Sdl.TryDeregisterDisposable(this);
			}

			SDL_RemoveTimer(mId);

			mId = 0;
		}
	}

	public override bool Equals(object? obj) => Equals(obj as Timer);

	public bool Equals(Timer? other) => other is { mId: var otherId } && mId == otherId;

	public override int GetHashCode() => mId.GetHashCode();

	public override string ToString() => ToString(format: default, formatProvider: default);

	public string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	public string ToString(string? format) => ToString(format, formatProvider: default);

	public string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {nameof(Id)}: {mId.ToString(format, formatProvider)} }}";

	public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite($"{{ {nameof(Id)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mId, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
