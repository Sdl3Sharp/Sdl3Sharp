using System;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Internal;

internal static class NativeStringsTransientStringExtensions
{
	extension(NativeStrings.TransientString<char> utf16)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal string? ToManaged()
		{
			unsafe
			{
				if (utf16.Buffer is null)
				{
					return null;
				}

				if (utf16.Length is 0)
				{
					return string.Empty;
				}

				return new string(utf16.Buffer, 0, unchecked((int)nuint.Min(utf16.Length, int.MaxValue)));
			}
		}
	}

	extension<T>(NativeStrings.TransientString<T> str)
		where T : unmanaged
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal ReadOnlySpan<T> AsSpan()
		{
			unsafe
			{
				if (str.Buffer is null || str.Length is 0)
				{
					return [];
				}

				return new(str.Buffer, unchecked((int)nuint.Min(str.Length, int.MaxValue)));
			}
		}
	}
}
