using Sdl3Sharp.Input;
using Sdl3Sharp.Video.Windowing.Drivers;
using System;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Windowing;

partial class WindowExtensions
{
	extension(Window<Android>.PropertyNames)
	{
		/// <summary>
		/// The name of a <em>read-only</em> <see cref="Window.Properties">property</see> that holds
		/// a pointer to <c><see href="https://developer.android.com/ndk/reference/group/a-native-window">ANativeWindow</see></c> associated with the window
		/// </summary>
		public static string AndroidWindowPointer => "SDL.window.android.window";

		/// <summary>
		/// The name of a <em>read-only</em> <see cref="Window.Properties">property</see> that holds
		/// the <c>EGLSurface</c> associated with the window
		/// </summary>
		public static string AndroidSurfacePointer => "SDL.window.android.surface";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Window{Android}, TextInputType?, Capitalization?, bool?, bool?, string?, string?, string?, int?, int?, Properties?)">property used when starting text input</see>
		/// that hold the <see href="https://developer.android.com/reference/android/text/InputType"><c>android.text.InputType</c></see> value to use for the text input field
		/// </summary>
		/// <remarks>
		/// <para>
		/// On Android, this property allows you to specify the type of input directly, overriding other properties including <see cref="Window.PropertyNames.TextInputTypeNumber"/>.
		/// </para>
		/// <para>
		/// The value of the associated property can be any <c>int</c> constant defined in <see href="https://developer.android.com/reference/android/text/InputType"><c>android.text.InputType</c></see>.
		/// </para>
		/// </remarks>
		public static string AndroidTextInputInputTypeNumber => "SDL.textinput.android.inputtype";
	}

	extension(Window<Android> window)
	{
		/// <summary>
		/// Gets a pointer to the <c><see href="https://developer.android.com/ndk/reference/group/a-native-window">ANativeWindow</see></c> associated with this window
		/// </summary>
		/// <value>
		/// A pointer to the <c><see href="https://developer.android.com/ndk/reference/group/a-native-window">ANativeWindow</see></c> associated with this window
		/// </value>
		/// <remarks>
		/// <para>
		/// The value of this property can be directly cast to an <c><see href="https://developer.android.com/ndk/reference/group/a-native-window">ANativeWindow</see>*</c> pointer.
		/// </para>
		/// <para>
		/// This property should only be accessed from the main thread.
		/// </para>
		/// </remarks>
		public IntPtr AndroidWindow => window?.Properties?.TryGetPointerValue(Window<Android>.PropertyNames.AndroidWindowPointer, out var androidWindowPtr) is true
			? androidWindowPtr
			: default;

		/// <summary>
		/// Gets the <c>EGLSurface</c> associated with this window
		/// </summary>
		/// <value>
		/// The <c>EGLSurface</c> associated with this window
		/// </value>
		/// <remarks>
		/// <para>
		/// The value of this property can be directly cast to an <c>EGLSurface</c> handle.
		/// </para>
		/// <para>
		/// This property should only be accessed from the main thread.
		/// </para>
		/// </remarks>
		public IntPtr AndroidSurface => window?.Properties?.TryGetPointerValue(Window<Android>.PropertyNames.AndroidSurfacePointer, out var androidSurfacePtr) is true
			? androidSurfacePtr
			: default;

#pragma warning disable CS1573 // We already pull those from the `inheritdoc` tag
		/// <inheritdoc cref="Window.TryStartTextInput(TextInputType?, Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)"/>
		/// <param name="androidInputType">
		/// The <see href="https://developer.android.com/reference/android/text/InputType"><c>android.text.InputType</c></see> value to use for the text input field.
		/// This parameter allows you to specify the type of input directly, overriding other properties including <paramref name="type"/>.
		/// The argument value can be any <c>int</c> constant defined in <see href="https://developer.android.com/reference/android/text/InputType"><c>android.text.InputType</c></see>.
		/// </param>
		public bool TryStartTextInput(TextInputType? type = default, Capitalization? capitalization = default, bool? autocorrect = default, bool? multiline = default, string? title = default, string? placeholder = default, string? defaultText = default, int? maxLength = default,
			int? androidInputType = default, Properties? properties = default)
#pragma warning restore CS1573
		{
			Properties propertiesUsed;
			Unsafe.SkipInit(out int? androidInputTypeBackup);

			if (properties is null)
			{
				propertiesUsed = [];

				if (androidInputType is int androidInputTypeValue)
				{
					propertiesUsed.TrySetNumberValue(Window<Android>.PropertyNames.AndroidTextInputInputTypeNumber, androidInputTypeValue);
				}
			}
			else
			{
				propertiesUsed = properties;

				if (androidInputType is int androidInputTypeValue)
				{
					androidInputTypeBackup = propertiesUsed.TryGetNumberValue(Window<Android>.PropertyNames.AndroidTextInputInputTypeNumber, out var androidInputTypeExisting)
						? unchecked((int)androidInputTypeExisting)
						: null;
				}
			}

			try
			{
				return window.TryStartTextInput(type, capitalization, autocorrect, multiline, title, placeholder, defaultText, maxLength, propertiesUsed);
			}
			finally
			{
				if (properties is null)
				{
					// propertiesUsed was just a temporary instance we created for this call, so we need to dispose it now

					propertiesUsed.Dispose();
				}
				else
				{
					// we restored the original properties values from the given properties instance

					if (androidInputTypeBackup is int androidInputTypeBackupValue)
					{
						propertiesUsed.TrySetNumberValue(Window<Android>.PropertyNames.AndroidTextInputInputTypeNumber, androidInputTypeBackupValue);
					}
					else
					{
						propertiesUsed.TryRemove(Window<Android>.PropertyNames.AndroidTextInputInputTypeNumber);
					}
				}
			}
		}
	}
}
