using System.Collections.Generic;

namespace Sdl3Sharp.SourceGeneration.IdType;

internal static class Extensions
{
	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
		=> (key, value) = (keyValuePair.Key, keyValuePair.Value);
}
