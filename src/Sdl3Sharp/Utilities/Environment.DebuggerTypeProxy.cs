using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Utilities;

[DebuggerTypeProxy(typeof(DebuggerTypeProxy))]
partial class Environment
{
	private sealed class DebuggerTypeProxy(Environment environment)
	{
		[DebuggerDisplay($"{{{nameof(Value)},nq}}", Name = $"{{{nameof(Name)},nq}}", Type = "")]
		public readonly struct Entry(string name, string value)
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)] public readonly string Name = name;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)] public readonly string Value = value;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public Entry[] Entries
		{
			get
			{
				unsafe
				{
					if (environment is { mEnvironment: not null } && SDL_GetEnvironmentVariables(environment.mEnvironment) is var array && array is not null)
					{
						try
						{
							var list = new List<Entry>();

							var current = array;
							while (*current is not null && Utf8StringMarshaller.ConvertToManaged(*current++) is string pair)
							{
								var pairSpan = pair.AsSpan();
								var separatorIndex = pairSpan.IndexOf('=');

								if (separatorIndex is < 0)
								{
									if (!pairSpan.IsWhiteSpace()) // 'IsWhiteSpace()' also returns true for empty spans
									{
										list.Add(new(name: pair, value: string.Empty));
									}
								}
								else
								{
									var nameSpan = pairSpan[..separatorIndex].Trim();

									if (!nameSpan.IsWhiteSpace()) // 'IsWhiteSpace()' also returns true for empty spans
									{
										list.Add(new(name: new(nameSpan), value: new(pairSpan[(separatorIndex + 1)..])));
									}
								}
							}

							list.Sort((x, y) => x.Name.CompareTo(y.Name, StringComparison.InvariantCultureIgnoreCase));

							return [..list];
						}
						finally
						{
							NativeMemory.SDL_free(array);
						}
					}

					return [];
				}
			}
		}
	}
}
