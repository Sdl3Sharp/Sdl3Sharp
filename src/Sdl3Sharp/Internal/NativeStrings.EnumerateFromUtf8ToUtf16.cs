using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	public unsafe static IEnumerable<string?> EnumerateFromUtf8ToUtf16(byte** strings, int count)
	{
		if (count is < 0)
		{
			return EnumerateNullTerminatedFromUtf8ToUtf16(strings);
		}

		if (strings is null || count is not > 0)
		{
			return [];
		}

		static IEnumerable<string?> enumerate(IntPtr strings, int count)
		{
			for (var i = 0; i < count; i++)
			{
				Unsafe.SkipInit(out IntPtr stringPtr);
				unsafe
				{
					stringPtr = unchecked((IntPtr)((byte**)strings)[i]);
				}

				static string? getUtf16(IntPtr stringPtr)
				{
					unsafe
					{
						using var utf16 = FromUtf8ToUtf16(unchecked((byte*)stringPtr));
						return utf16.ToManaged();
					}
				}

				yield return getUtf16(stringPtr);
			}
		}

		return enumerate(unchecked((IntPtr)strings), count);
	}

	public unsafe static IEnumerable<string> EnumerateNullTerminatedFromUtf8ToUtf16(byte** strings)
	{
		if (strings is null)
		{
			return [];
		}

		static IEnumerable<string> enumerate(IntPtr strings)
		{
			for (var i = 0; ; i++)
			{
				Unsafe.SkipInit(out IntPtr stringPtr);
				unsafe
				{
					stringPtr = unchecked((IntPtr)((byte**)strings)[i]);

					if (unchecked((byte*)stringPtr) is null)
					{
						yield break;
					}
				}

				static string getUtf16(IntPtr stringPtr)
				{
					unsafe
					{
						using var utf16 = FromUtf8ToUtf16(unchecked((byte*)stringPtr));
						return utf16.ToManaged()!; // `ToManaged` only returns null, when the conversion buffer is null, and `FromUtf8ToUtf16` in turn only returns a null buffer if the input pointer was null, which we already checked for above
					}
				}

				yield return getUtf16(stringPtr);
			}
		}

		return enumerate(unchecked((IntPtr)strings));
	}
}