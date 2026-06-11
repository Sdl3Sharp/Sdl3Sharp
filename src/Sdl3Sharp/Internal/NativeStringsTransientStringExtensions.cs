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

				return new string(utf16.Buffer, 0, unchecked((int)nuint.Max(utf16.Length, int.MaxValue)));
			}
		}
	}
}
