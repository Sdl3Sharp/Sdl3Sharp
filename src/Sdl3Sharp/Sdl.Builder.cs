using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp;

partial class Sdl
{
	/// <summary>
	/// A builder that lets you perfom some preliminaries right before an <see cref="Sdl"/> instance is created
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly ref struct Builder
	{
		private readonly ref SubSystems mSubSystems;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		internal Builder(ref SubSystems subSystems) => mSubSystems = ref subSystems;

		/// <summary>
		/// Sets certain <see cref="SubSystems">sub systems</see> to be <em>not</em> initialized with the <see cref="Sdl"/> instance
		/// </summary>
		/// <param name="subSystems"><see cref="SubSystems">Sub systems</see> to be <em>not</em> initialized with the <see cref="Sdl"/> instance</param>
		/// <returns>The current <see cref="Builder"/> so that additional calls can be chained</returns>
		/// <remarks>
		/// <para>
		/// Note that this does not prevent depend sub system from being initialized (e.g. <see cref="SubSystems.Events"/> when <see cref="SubSystems.Audio"/> should be initialized).
		/// It also does not deinitialize any sub systems that have already been initialized (e.g. if you create a new instance of <see cref="Sdl"/>).
		/// </para>
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly Builder DontInitializeSubSystems(SubSystems subSystems)
		{
			mSubSystems &= ~subSystems;

			return this;
		}

		/// <summary>
		/// Sets certain <see cref="SubSystems">sub systems</see> to be initialized with the <see cref="Sdl"/> instance
		/// </summary>
		/// <param name="subSystems"><see cref="SubSystems">Sub systems</see> to be initialized with the <see cref="Sdl"/> instance</param>
		/// <returns>The current <see cref="Builder"/> so that additional calls can be chained</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly Builder InitializeSubSystems(SubSystems subSystems)
		{
			mSubSystems |= subSystems;

			return this;
		}

		/// <summary>
		/// Tries to set metadata about your app
		/// </summary>
		/// <param name="name">The name of the metadata</param>
		/// <param name="value">The value of the metadata, or <c><see langword="null"/></c> to remove that metadata</param>
		/// <returns><c><see langword="true"/></c> if the value of the metadata was successfully set; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// You can optionally provide metadata about your app to SDL. This is not required, but strongly encouraged.
		/// </para>
		/// <para>
		/// There are several locations where SDL can make use of metadata (an "About" box in the macOS menu bar, the name of the app can be shown on some audio mixers, etc).
		/// Any piece of metadata can be left out, if a specific detail doesn't make sense for the app.
		/// </para>
		/// <para>
		/// See <see cref="Metadata"/> for a overview over the available metadata properties and their meanings.
		/// </para>
		/// <para>
		/// Multiple calls to this method with the same <paramref name="name"/> value are allowed, but various state might not change once it has been already set up.
		/// You should only try to set metadata once, and only when creating the very first instance of <see cref="Sdl"/>. If you try to set metadata after that, it will likely fail.
		/// </para>
		/// </remarks>
#pragma warning disable CA1822 // This is intentionally an instance method, because it is part of the builder pattern and should be called on a Builder instance
		public readonly bool TrySetMetadata(string name, string? value)
#pragma warning restore CA1822 
		{
			unsafe
			{
				var nameUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(name);
				var valueUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(value);

				try
				{
					return SDL_SetAppMetadataProperty(nameUtf8, valueUtf8);
				}
				finally
				{
					Utf8StringMarshaller.Free(valueUtf8);
					Utf8StringMarshaller.Free(nameUtf8);
				}
			}
		}

		/// <summary>
		/// Tries to set basic metadata about your app
		/// </summary>
		/// <param name="appName">The name of the application (<c>"My Game 2: Bad Guy's Revenge!"</c>)</param>
		/// <param name="appVersion">The version of the application (<c>"1.0.0beta5"</c> or a git hash, or whatever makes sense)</param>
		/// <param name="appIdentifier">A unique string in reverse-domain format that identifies this app (<c>"com.example.mygame2"</c>)</param>
		/// <returns><c><see langword="true"/></c> if the value of the metadata values are successfully set; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// You can optionally provide metadata about your app to SDL. This is not required, but strongly encouraged.
		/// </para>
		/// <para>
		/// There are several locations where SDL can make use of metadata (an "About" box in the macOS menu bar, the name of the app can be shown on some audio mixers, etc).
		/// Any piece of metadata can be left out as a <c><see langword="null"/></c> value, if a specific detail doesn't make sense for the app.
		/// </para>
		/// <para>
		/// Passing a <c><see langword="null"/></c> value removes any previous metadata.
		/// </para>
		/// <para>
		/// Multiple calls to this method are allowed, but various state might not change once it has been already set up.
		/// You should only try to set metadata once, and only when creating the very first instance of <see cref="Sdl"/>. If you try to set metadata after that, it will likely fail.
		/// </para>
		/// <para>
		/// This is a simplified interface for the most important information. You can supply significantly more detailed metadata with <see cref="TrySetMetadata(string, string?)"/>.
		/// </para>
		/// </remarks>
#pragma warning disable CA1822 // This is intentionally an instance method, because it is part of the builder pattern and should be called on a Builder instance
		public readonly bool TrySetMetadata(string? appName, string? appVersion, string? appIdentifier)
#pragma warning restore CA1822
		{
			unsafe
			{
				var appNameUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(appName);
				var appVersionUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(appVersion);
				var appIdentifierUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(appIdentifier);

				try
				{
					return SDL_SetAppMetadata(appNameUtf8, appVersionUtf8, appIdentifierUtf8);
				}
				finally
				{
					Utf8StringMarshaller.Free(appIdentifierUtf8);
					Utf8StringMarshaller.Free(appVersionUtf8);
					Utf8StringMarshaller.Free(appNameUtf8);
				}
			}
		}
	}
}
