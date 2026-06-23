using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	private interface ITransientString<TSelf>
		where TSelf : struct, ITransientString<TSelf>, allows ref struct
	{
		static abstract TSelf Create(ConverterBuffer? buffer, nuint length);
	}

	private static class TransientString
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static TTransientString Create<TTransientString>(ConverterBuffer? buffer, nuint length)
			where TTransientString : struct, ITransientString<TTransientString>, allows ref struct
			=> TTransientString.Create(buffer, length);
	}

	[StructLayout(LayoutKind.Sequential)]
	internal readonly ref struct TransientString<T> : ITransientString<TransientString<T>>, IDisposable
		where T : unmanaged
	{
		private readonly ConverterBuffer? mBuffer;
		private readonly nuint mLength;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private TransientString(ConverterBuffer? buffer, nuint length)
		{
			mBuffer = buffer;
			mLength = length;
		}

		public unsafe readonly T* Buffer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mBuffer switch { null => null, var buffer => unchecked((T*)buffer.Buffer) }; }

		public readonly nuint Capacity { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mBuffer switch { null => default, var buffer => unchecked(buffer.Capacity / (nuint)Unsafe.SizeOf<T>()) }; }

		public readonly nuint Length { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mLength; }

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		static TransientString<T> ITransientString<TransientString<T>>.Create(ConverterBuffer? buffer, nuint length) => new(buffer, length);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void Dispose() => mBuffer?.Dispose();
	}
}
