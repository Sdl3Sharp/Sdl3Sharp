using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	[StructLayout(LayoutKind.Sequential)]
	internal ref struct TransientUtf8String : IDisposable
	{
		private unsafe byte* mBuffer;
		private nuint mCapacity;
		private readonly bool mZeroMemoryUponDispose;

		[Obsolete($"Do not use directly. Only {nameof(TransientUtf8String)} created as a result of a call to {nameof(FromUtf16ToUtf8)} should be used.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal unsafe TransientUtf8String(byte* buffer, nuint capacity, bool zeroMemoryUponDispose)
		{
			mBuffer = buffer;
			mCapacity = capacity;
			mZeroMemoryUponDispose = zeroMemoryUponDispose;
		}

		public unsafe readonly byte* Buffer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mBuffer; }

		public readonly nuint Capacity { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mCapacity; }

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

				ReturnConverterBuffer(buffer, capacity, mZeroMemoryUponDispose);
			}
		}
	}
}
