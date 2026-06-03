using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Internal;

internal static partial class NativeStrings
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal unsafe static bool Equals(byte* str1, byte* str2)
	{
		if (str1 == str2)
		{
			return true;
		}

		if (str1 is not null && str2 is not null)
		{
			for (var ch = *str1; ch == *str2; ch = *++str1, ++str2)
			{
				if (ch is (byte)'\0')
				{
					return true;
				}
			}
		}

		return false;
	}
}
