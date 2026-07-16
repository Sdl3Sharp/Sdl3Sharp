using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Utilities;

partial class Environment
{
	/// <summary>
	/// Enumerates the environment variables in a <see cref="Environment">set of environment variables</see>
	/// </summary>
	public sealed class Enumerator : IEnumerator<KeyValuePair<string, string>>
	{
		private unsafe byte** mArray, mCurrent;

		internal unsafe Enumerator(byte** array) => mArray = mCurrent = array;

		/// <inheritdoc/>
		public KeyValuePair<string, string> Current { get; private set; }

		/// <inheritdoc/>
		object IEnumerator.Current => Current;

		/// <summary>
		/// Tries to create a new <see cref="Enumerator"/> for a <see cref="Environment">set of environment variables</see>
		/// </summary>
		/// <param name="environment">The set of environment variables to enumerate</param>
		/// <param name="enumerator">The newly created enumerator for the <see cref="Environment">set of environment variables</see>, when this mehtod returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
		/// <returns><c><see langword="true"/></c> if a new <see cref="Enumerator"/> was successfully created for the <see cref="Environment">set of environment variables</see>; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <exception cref="ArgumentNullException"><paramref name="environment"/> is <c><see langword="null"/></c></exception>
		/// <exception cref="ObjectDisposedException"><paramref name="environment"/> was already disposed</exception>
		public static bool TryCreate(Environment environment, [MaybeNullWhen(false)] out Enumerator enumerator)
		{
			unsafe
			{
				if (environment is null)
				{
					failEnvironmentArgumentNull();
				}

				if (environment.mEnvironment is null)
				{
					// We intentionally do throw an exception here, as opposed to simply returning false,
					// because this is not an error while creating the enumerator
					failEnvironmentDisposed();
				}

				var array = SDL_GetEnvironmentVariables(environment.mEnvironment);

				if (array is null)
				{
					enumerator = default;
					return false;
				}

				enumerator = new(array);
				return true;
			}

			[DoesNotReturn]
			static void failEnvironmentArgumentNull() => throw new ArgumentNullException(nameof(environment));
						
			[DoesNotReturn]
			static void failEnvironmentDisposed() => throw new ObjectDisposedException(nameof(environment));
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			unsafe
			{
				NativeMemory.SDL_free(mArray);
				mArray = mCurrent = null;
			}
		}

		/// <inheritdoc/>
		public bool MoveNext()
		{
			unsafe
			{
				if (mCurrent is null || *mCurrent is null || Utf8StringMarshaller.ConvertToManaged(*mCurrent++) is not string pair)
				{
					return false;
				}

				var pairSpan = pair.AsSpan();
				var separatorIndex = pairSpan.IndexOf('=');	

				if (separatorIndex is < 0)
				{
					if (pairSpan.IsWhiteSpace()) // 'IsWhiteSpace()' also returns true for empty spans
					{
						return false;
					}

					Current = new(key: pair, value: string.Empty);
				}
				else
				{
					var nameSpan = pairSpan[..separatorIndex].Trim();

					if (nameSpan.IsWhiteSpace()) // 'IsWhiteSpace()' also returns true for empty spans
					{
						return false;
					}

					Current = new(key: new(nameSpan), value: new(pairSpan[(separatorIndex + 1)..]));
				}

				return true;
			}
		}

		/// <inheritdoc/>
		public void Reset()
		{
			unsafe
			{
				mCurrent = mArray;
			}
		}
	}

	/// <summary>
	/// Gets an <see cref="Enumerator"/> for the current <see cref="Environment">set of environment variables</see>
	/// </summary>
	/// <returns>An <see cref="Enumerator"/> for the current <see cref="Environment">set of environment variables</see></returns>
	/// <exception cref="SdlException">Couldn't create an <see cref="Enumerator"/> instance for the current <see cref="Environment">set of environment variables</see></exception>
	/// <remarks>
	/// In contrast to most of the remaining API which uses the <c>Try</c>-method pattern, this method intentionally fails by throwing an exception.
	/// If you want to handle failures wrap the call to this method in a <c><see langword="try"/></c>-block,
	/// and check <see cref="Error.TryGet(out string?)"/> for more information when <c><see langword="catch"/></c>ing a <see cref="SdlException"/>.
	/// </remarks>
	public Enumerator GetEnumerator()
	{
		if (!Enumerator.TryCreate(this, out var enumerator))
		{
			failCouldNotGetEnumerator();
		}

		return enumerator;

		[DoesNotReturn]
		static void failCouldNotGetEnumerator() => throw new SdlException(message: $"Could not create an {nameof(Enumerator)} instance for the given {nameof(Environment)} instance");
	}

	/// <inheritdoc/>
	IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
