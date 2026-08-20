using Sdl3Sharp.Input;
using Sdl3Sharp.Video.Rendering.Drivers;
using Sdl3Sharp.Video.Windowing;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp;

partial struct Hint
{
	/// <summary>
	/// Gets a hint specifying the behavior of Alt+Tab while the keyboard is grabbed
	/// </summary>
	/// <value>
	/// A hint specifing the behavior of Alt+Tab while the keyboard is grabbed
	/// </value>
	/// <remarks>
	/// <para>
	/// By default, SDL emulates Alt+Tab functionality while the keyboard is grabbed and your window is full-screen. This prevents the user from getting stuck in your application if you've enabled keyboard grab.
	/// </para>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>SDL will not handle Alt+Tab. Your application is responsible for handling Alt+Tab while the keyboard is grabbed.</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>SDL will minimize your window when Alt+Tab is pressed (default)</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_ALLOW_ALT_TAB_WHILE_GRABBED">SDL_HINT_ALLOW_ALT_TAB_WHILE_GRABBED</seealso>
	public static Hint AllowAltTabWhileGrabbed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_ALLOW_ALT_TAB_WHILE_GRABBED"); }

	/// <summary>SDL_HINT_ANDROID_*</summary>
	public static class Android
	{
		/// <summary>
		/// Gets a hint to control whether the SDL activity is allowed to be re-created
		/// </summary>
		/// <value>
		/// A hint to control whether the SDL activity is allowed to be re-created
		/// </value>
		/// <remarks>
		/// <para>
		/// If this hint is true, the activity can be recreated on demand by the OS, and Java static data and C++ static data remain with their current values. If this hint is false, then SDL will call <c>exit()</c> when you return from your main function and the application will be terminated and then started fresh each time.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>The application starts fresh at each launch (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>The application activity can be recreated by the OS</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para> 
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_ANDROID_ALLOW_RECREATE_ACTIVITY">SDL_HINT_ANDROID_ALLOW_RECREATE_ACTIVITY</seealso>
		public static Hint AllowRecreateActivity { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_ANDROID_ALLOW_RECREATE_ACTIVITY"); }

		/// <summary>
		/// Gets a hint to control whether the event loop will block itself when the app is paused
		/// </summary>
		/// <value>
		/// A hint to control whether the event loop will block itself when the app is paused
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Non blocking</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Blocking (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para> 
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_ANDROID_BLOCK_ON_PAUSE"></seealso>
		public static Hint BlockOnPause { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_ANDROID_BLOCK_ON_PAUSE"); }

		/// <summary>
		/// Gets a hint to control whether low latency audio should be enabled
		/// </summary>
		/// <value>
		/// A hint to control whether low latency audio should be enabled
		/// </value>
		/// <remarks>
		/// <para>
		/// Some devices have poor quality output when this is enabled, but this is usually an improvement in audio latency.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Low latency audio is not enabled</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Low latency audio is enabled (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="SubSystems.Audio">SDL audio</see> is initialized.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_ANDROID_LOW_LATENCY_AUDIO">SDL_HINT_ANDROID_LOW_LATENCY_AUDIO</seealso>
		public static Hint LowLatencyAudio { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_ANDROID_LOW_LATENCY_AUDIO"); }

		/// <summary>
		/// Gets a hint to control whether we trap the Android back button to handle it manually
		/// </summary>
		/// <value>
		/// A hint to control whether we trap the Android back button to handle it manually
		/// </value>
		/// <remarks>
		/// <para>
		/// This is necessary for the right mouse button to work on some Android devices, or to be able to trap the back button for use in your code reliably. If this hint is true, the back button will show up as an <see cref="EventType.Keyboard.KeyDown"/> / <see cref="EventType.Keyboard.KeyUp"/> pair with a keycode of <see cref="Scancode.ApplicationControl.Back"/>.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Back button will be handled as usual for system (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Back button will be trapped, allowing you to handle the key press manually (This will also let right mouse click work on systems where the right mouse button functions as back)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_ANDROID_TRAP_BACK_BUTTON">SDL_HINT_ANDROID_TRAP_BACK_BUTTON</seealso>
		public static Hint TrapBackButton { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_ANDROID_TRAP_BACK_BUTTON"); }
	}

	/// <summary>
	/// Gets a hint setting the app ID string
	/// </summary>
	/// <value>
	/// A hint setting the app ID string
	/// </value>
	/// <remarks>
	/// <para>
	/// This string is used by desktop compositors to identify and group windows together, as well as match applications with associated desktop settings and icons.
	/// </para>
	/// <para>
	/// This will override <see cref="Sdl.Metadata.Identifier"/>, if set by the application.
	/// </para>
	/// <para>
	/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_APP_ID">SDL_HINT_APP_ID</seealso>
	public static Hint AppId { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_APP_ID"); }

	/// <summary>
	/// Gets a hint setting the application name
	/// </summary>
	/// <value>
	/// A hint setting the application name
	/// </value>
	/// <remarks>
	/// <para>
	/// This hint lets you specify the application name sent to the OS when required. For example, this will often appear in volume control applets for audio streams, and in lists of applications which are inhibiting the screensaver. You should use a string that describes your program ("My Game 2: The Revenge").
	/// </para>
	/// <para>
	/// This will override <see cref="Sdl.Metadata.Name"/>, if set by the application.
	/// </para>
	/// <para>
	/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_APP_NAME">SDL_HINT_APP_NAME</seealso>
	public static Hint AppName { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_APP_NAME"); }

	/// <summary>SDL_HINT_APPLE_TV_*</summary>
	public static class AppleTV
	{
		/// <summary>
		/// Gets a hint controlling whether controllers used with the Apple TV generate UI events
		/// </summary>
		/// <value>
		/// A hint controlling whether controllers used with the Apple TV generate UI events
		/// </value>
		/// <remarks>
		/// <para>
		/// When UI events are generated by controller input, the app will be backgrounded when the Apple TV remote's menu button is pressed, and when the pause or B buttons on gamepads are pressed.
		/// </para>
		/// <para>
		/// More information about properly making use of controllers for the Apple TV can be found here: <see href="https://developer.apple.com/tvos/human-interface-guidelines/remote-and-controllers/"/>.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Controller input does not generate UI events (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Controller input generates UI events</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_APPLE_TV_CONTROLLER_UI_EVENTS">SDL_HINT_APPLE_TV_CONTROLLER_UI_EVENTS</seealso>
		public static Hint ControllerUIEvents { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_APPLE_TV_CONTROLLER_UI_EVENTS"); }

		/// <summary>
		/// Gets a hint controlling whether the Apple TV remote's joystick axes will automatically match the rotation of the remote
		/// </summary>
		/// <value>
		/// A hint controlling whether the Apple TV remote's joystick axes will automatically match the rotation of the remote
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Remote orientation does not affect joystick axes (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Joystick axes are based on the orientation of the remote</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_APPLE_TV_REMOTE_ALLOW_ROTATION">SDL_HINT_APPLE_TV_REMOTE_ALLOW_ROTATION</seealso>
		public static Hint RemoteAllowRotation { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_APPLE_TV_REMOTE_ALLOW_ROTATION"); }
	}

	/// <summary>
	/// Gets a hint controlling response to <see cref="SDL_assert"/> failures
	/// </summary>
	/// <value>
	/// A hint controlling response to <see cref="SDL_assert"/> failures
	/// </value>
	/// <remarks>
	/// <para>
	/// The hint can be set to the following case-sensitive values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"abort"</c></term>
	///			<description>Program terminates immediately</description>
	///		</item>
	///		<item>
	///			<term><c>"break"</c></term>
	///			<description>Program triggers a debugger breakpoint</description>
	///		</item>
	///		<item>
	///			<term><c>"retry"</c></term>
	///			<description>Program reruns the <see cref="SDL_assert"/>'s test again</description>
	///		</item>
	///		<item>
	///			<term><c>"ignore"</c></term>
	///			<description>Program continues on, ignoring this assertion failure this time</description>
	///		</item>
	///		<item>
	///			<term><c>"always_ignore"</c></term>
	///			<description>Program continues on, ignoring this assertion failure for the rest of the run</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// Note that <see cref="SDL_SetAssertionHandler"/> offers a programmatic means to deal with assertion failures through a callback, and this hint is largely intended to be used via environment variables by end users and automated tools.
	/// </para>
	/// <para>
	/// This hint should be set before an assertion failure is triggered and can be changed at any time.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_ASSERT">SDL_HINT_ASSERT</seealso>
	public static Hint Assert { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_ASSERT"); }

	/// <summary>SDL_HINT_AUDIO_*</summary>
	public static class Audio
	{
		/// <summary>SDL_HINT_AUDIO_ALSA_*</summary>
		public static class Alsa
		{
			/// <summary>
			/// Gets a hint specifying the default ALSA audio device name
			/// </summary>
			/// <value>
			/// A hint specifying the default ALSA audio device name
			/// </value>
			/// <remarks>
			/// <para>
			/// This hint is a specific audio device to open when the "default" audio device is used.
			/// </para>
			/// <para>
			/// This hint will be ignored when opening the default playback device if <see cref="DefaultPlaybackDevice">Hint.Audio.Alsa.DefaultPlaybackDevice</see> is set, or when opening the default recording device if <see cref="DefaultPlaybackDevice">Hint.Audio.Alsa.DefaultPlaybackDevice</see> is set.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_ALSA_DEFAULT_DEVICE">SDL_HINT_AUDIO_ALSA_DEFAULT_DEVICE</seealso>
			public static Hint DefaultDevice { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_ALSA_DEFAULT_DEVICE"); }

			/// <summary>
			/// Gets a hint specifying the default ALSA audio playback device name
			/// </summary>
			/// <value>
			/// A hint specifying the default ALSA audio playback device name
			/// </value>
			/// <remarks>
			/// <para>
			/// This hint is a specific audio device to open for playback, when the "default" audio device is used.
			/// </para>
			/// <para>
			/// If this hint isn't set, SDL will check <see cref="DefaultDevice">Hint.Audio.Alsa.DefaultDevice</see> before choosing a reasonable default.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_ALSA_DEFAULT_PLAYBACK_DEVICE">SDL_HINT_AUDIO_ALSA_DEFAULT_PLAYBACK_DEVICE</seealso>
			public static Hint DefaultPlaybackDevice { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_ALSA_DEFAULT_PLAYBACK_DEVICE"); }

			/// <summary>
			/// Gets a hint specifying the default ALSA audio recording device name
			/// </summary>
			/// <value>
			/// A hint specifying the default ALSA audio recording device name
			/// </value>
			/// <remarks>
			/// <para>
			/// This hint is a specific audio device to open for recording, when the "default" audio device is used.
			/// </para>
			/// <para>
			/// If this hint isn't set, SDL will check <see cref="DefaultDevice">Hint.Audio.Alsa.DefaultDevice</see> before choosing a reasonable default.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_ALSA_DEFAULT_RECORDING_DEVICE">SDL_HINT_AUDIO_ALSA_DEFAULT_RECORDING_DEVICE</seealso>
			public static Hint DefaultRecordingDevice { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_ALSA_DEFAULT_RECORDING_DEVICE"); }
		}

		/// <summary>
		/// Gets a hint controlling the audio category on iOS and macOS
		/// </summary>
		/// <value>
		/// A hint controlling the audio category on iOS and macOS
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"ambient"</c></term>
		///			<description>Use the <c>AVAudioSessionCategoryAmbient</c> audio category, will be muted by the phone mute switch (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"playback"</c></term>
		///			<description>Use the <c>AVAudioSessionCategoryPlayback</c> category</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// For more information, see Apple's documentation: <see href="https://developer.apple.com/library/content/documentation/Audio/Conceptual/AudioSessionProgrammingGuide/AudioSessionCategoriesandModes/AudioSessionCategoriesandModes.html"/>.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">an audio device is opened</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_CATEGORY">SDL_HINT_AUDIO_CATEGORY</seealso>
		public static Hint Category { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_CATEGORY"); }

		/// <summary>
		/// Gets a hint controlling the default audio channel count
		/// </summary>
		/// <value>
		/// A hint controlling the default audio channel count
		/// </value>
		/// <remarks>
		/// <para>
		/// If the application doesn't specify the audio channel count when opening the device, this hint can be used to specify a default channel count that will be used. This defaults to <c>"1"</c> for recording and <c>"2"</c> for playback devices.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">an audio device is opened</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_CHANNELS">SDL_HINT_AUDIO_CHANNELS</seealso>
		public static Hint Channels { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_CHANNELS"); }

		/// <summary>SDL_HINT_AUDIO_DEVICE_*</summary>
		public static class Device
		{
			/// <summary>
			/// Gets a hint specifying an application icon name for an audio device
			/// </summary>
			/// <value>
			/// A hint specifying an application icon name for an audio device
			/// </value>
			/// <remarks>
			/// <para>
			/// Some audio backends (such as Pulseaudio and Pipewire) allow you to set an XDG icon name for your application. Among other things, this icon might show up in a system control panel that lets the user adjust the volume on specific audio streams instead of using one giant master volume slider. Note that this is unrelated to the icon used by the windowing system, which may be set with <see cref="SDL_SetWindowIcon"/> (or via desktop file on Wayland).
			/// </para>
			/// <para>
			/// Setting this to <c>""</c> or leaving it unset will have SDL use a reasonable default, <c>"applications-games"</c>, which is likely to be installed. See <see href="https://specifications.freedesktop.org/icon-theme-spec/icon-theme-spec-latest.html"/> and <see href="https://specifications.freedesktop.org/icon-naming-spec/icon-naming-spec-latest.html"/> for the relevant XDG icon specs.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DEVICE_APP_ICON_NAME">SDL_HINT_AUDIO_DEVICE_APP_ICON_NAME</seealso>
			public static Hint AppIconName { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DEVICE_APP_ICON_NAME"); }

			/// <summary>
			/// Gets a hint controlling device buffer size
			/// </summary>
			/// <value>
			/// A hint controlling device buffer size
			/// </value>
			/// <remarks>
			/// <para>
			/// This hint is an integer <c>&gt; 0</c>, that represents the size of the device's buffer in sample frames (stereo audio data in 16-bit format is 4 bytes per sample frame, for example).
			/// </para>
			/// <para>
			/// SDL3 generally decides this value on behalf of the app, but if for some reason the app needs to dictate this (because they want either lower latency or higher throughput <em>AND ARE WILLING TO DEAL WITH</em> what that might require of the app), they can specify it.
			/// </para>
			/// <para>
			/// SDL will try to accommodate this value, but there is no promise you'll get the buffer size requested. Many platforms won't honor this request at all, or might adjust it.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DEVICE_SAMPLE_FRAMES">SDL_HINT_AUDIO_DEVICE_SAMPLE_FRAMES</seealso>
			public static Hint SampleFrames { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DEVICE_SAMPLE_FRAMES"); }

			/// <summary>
			/// Gets a hint specifying an audio stream name for an audio device
			/// </summary>
			/// <value>
			/// A hint specifying an audio stream name for an audio device
			/// </value>
			/// <remarks>
			/// <para>
			/// Some audio backends (such as PulseAudio) allow you to describe your audio stream. Among other things, this description might show up in a system control panel that lets the user adjust the volume on specific audio streams instead of using one giant master volume slider.
			/// </para>
			/// <para>
			/// This hints lets you transmit that information to the OS. The contents of this hint are used while opening an audio device. You should use a string that describes your what your program is playing (<c>"audio stream"</c> is probably sufficient in many cases, but this could be useful for something like <c>"team chat"</c> if you have a headset playing VoIP audio separately).
			/// </para>
			/// <para>
			/// Setting this to <c>""</c> or leaving it unset will have SDL use a reasonable default: <c>"audio stream"</c> or something similar.
			/// </para>
			/// <para>
			/// Note that while this talks about audio streams, this is an OS-level concept, so it applies to a physical audio device in this case, and not an <see cref="SDL_AudioStream"/>, nor an SDL logical audio device.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DEVICE_STREAM_NAME">SDL_HINT_AUDIO_DEVICE_STREAM_NAME</seealso>
			public static Hint StreamName { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DEVICE_STREAM_NAME"); }

			/// <summary>
			/// Gets a hint specifying an application role for an audio device
			/// </summary>
			/// <value>
			/// A hint specifying an application role for an audio device
			/// </value>
			/// <remarks>
			/// <para>
			/// Some audio backends (such as Pipewire) allow you to describe the role of your audio stream. Among other things, this description might show up in a system control panel or software for displaying and manipulating media playback/recording graphs.
			/// </para>
			/// <para>
			/// This hints lets you transmit that information to the OS. The contents of this hint are used while opening an audio device. You should use a string that describes your what your program is playing (Game, Music, Movie, etc...).
			/// </para>
			/// <para>
			/// Setting this to <c>""</c> or leaving it unset will have SDL use a reasonable default: <c>"Game"</c> or something similar.
			/// </para>
			/// <para>
			/// Note that while this talks about audio streams, this is an OS-level concept, so it applies to a physical audio device in this case, and not an <see cref="SDL_AudioStream"/>, nor an SDL logical audio device.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DEVICE_STREAM_ROLE">SDL_HINT_AUDIO_DEVICE_STREAM_ROLE</seealso>
			public static Hint StreamRole { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DEVICE_STREAM_ROLE"); }
		}

		/// <summary>SDL_HINT_AUDIO_DISK_*</summary>
		public static class Disk
		{
			/// <summary>
			/// Gets a hint specifying the input file when recording audio using the disk audio driver
			/// </summary>
			/// <value>
			/// A hint specifying the input file when recording audio using the disk audio driver
			/// </value>
			/// <remarks>
			/// <para>
			/// This defaults to <c>"sdlaudio-in.raw"</c>.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DISK_INPUT_FILE">SDL_HINT_AUDIO_DISK_INPUT_FILE</seealso>
			public static Hint InputFile { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DISK_INPUT_FILE"); }

			/// <summary>
			/// Gets a hint specifying the output file when playing audio using the disk audio driver
			/// </summary>
			/// <value>
			/// A hint specifying the output file when playing audio using the disk audio driver
			/// </value>
			/// <remarks>
			/// <para>
			/// This defaults to <c>"sdlaudio.raw"</c>.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DISK_OUTPUT_FILE">SDL_HINT_AUDIO_DISK_OUTPUT_FILE</seealso>
			public static Hint OutputFile { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DISK_OUTPUT_FILE"); }

			/// <summary>
			/// Gets a hint controlling the audio rate when using the disk audio driver
			/// </summary>
			/// <value>
			/// A hint controlling the audio rate when using the disk audio driver
			/// </value>
			/// <remarks>
			/// <para>
			/// The disk audio driver normally simulates real-time for the audio rate that was specified, but you can use this hint to adjust this rate higher or lower down to 0. The default value is <c>"1.0"</c>.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DISK_TIMESCALE">SDL_HINT_AUDIO_DISK_TIMESCALE</seealso>
			public static Hint Timescale { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DISK_TIMESCALE"); }
		}

		/// <summary>
		/// Gets a hint that specifies an audio backend to use
		/// </summary>
		/// <value>
		/// A hint that specifies an audio backend to use
		/// </value>
		/// <remarks>
		/// <para>
		/// By default, SDL will try all available audio backends in a reasonable order until it finds one that can work, but this hint allows the app or user to force a specific driver, such as <c>"pipewire"</c> if, say, you are on PulseAudio but want to try talking to the lower level instead.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DRIVER">SDL_HINT_AUDIO_DRIVER</seealso>
		public static Hint Driver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DRIVER"); }

		/// <summary>SDL_HINT_AUDIO_DUMMY_*</summary>
		public static class Dummy
		{
			/// <summary>
			/// Gets a hint controlling the audio rate when using the dummy audio driver
			/// </summary>
			/// <value>
			/// A hint controlling the audio rate when using the dummy audio driver
			/// </value>
			/// <remarks>
			/// <para>
			/// The dummy audio driver normally simulates real-time for the audio rate that was specified, but you can use this hint to adjust this rate higher or lower down to 0. The default value is <c>"1.0"</c>.
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">an audio device is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_DUMMY_TIMESCALE">SDL_HINT_AUDIO_DUMMY_TIMESCALE</seealso>
			public static Hint Timescale { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_DUMMY_TIMESCALE"); }
		}

		/// <summary>
		/// Gets a hint controlling the default audio format
		/// </summary>
		/// <value>
		/// A hint controlling the default audio format
		/// </value>
		/// <remarks>
		/// <para>
		/// If the application doesn't specify the audio format when opening the device, this hint can be used to specify a default format that will be used.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"U8"</c></term>
		///			<description>Unsigned 8-bit audio</description>
		///		</item>
		///		<item>
		///			<term><c>"S8"</c></term>
		///			<description>Signed 8-bit audio</description>
		///		</item>
		///		<item>
		///			<term><c>"S16LE"</c></term>
		///			<description>Signed 16-bit little-endian audio</description>
		///		</item>
		///		<item>
		///			<term><c>"S16BE"</c></term>
		///			<description>Signed 16-bit big-endian audio</description>
		///		</item>
		///		<item>
		///			<term><c>"S16"</c></term>
		///			<description>Signed 16-bit native-endian audio (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"S32LE"</c></term>
		///			<description>Signed 32-bit little-endian audio</description>
		///		</item>
		///		<item>
		///			<term><c>"S32BE"</c></term>
		///			<description>Signed 32-bit big-endian audio</description>
		///		</item>
		///		<item>
		///			<term><c>"S32"</c></term>
		///			<description>Signed 32-bit native-endian audio</description>
		///		</item>
		///		<item>
		///			<term><c>"F32LE"</c></term>
		///			<description>Floating point little-endian audio</description>
		///		</item>
		///		<item>
		///			<term><c>"F32BE"</c></term>
		///			<description>Floating point big-endian audio</description>
		///		</item>
		///		<item>
		///			<term><c>"F32"</c></term>
		///			<description>Floating point native-endian audio</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">an audio device is opened</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_FORMAT">SDL_HINT_AUDIO_FORMAT</seealso>
		public static Hint Format { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_FORMAT"); }

		/// <summary>
		/// Gets a hint controlling the default audio frequency
		/// </summary>
		/// <value>
		/// A hint controlling the default audio frequency
		/// </value>
		/// <remarks>
		/// <para>
		/// If the application doesn't specify the audio frequency when opening the device, this hint can be used to specify a default frequency that will be used. This defaults to <c>"44100"</c>.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">an audio device is opened</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_FREQUENCY">SDL_HINT_AUDIO_FREQUENCY</seealso>
		public static Hint Frequency { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_FREQUENCY"); }

		/// <summary>
		/// Gets a hint that causes SDL to not ignore audio "monitors"
		/// </summary>
		/// <value>
		/// A hint that causes SDL to not ignore audio "monitors"
		/// </value>
		/// <remarks>
		/// <para>
		/// This is currently only used by the PulseAudio driver.
		/// </para>
		/// <para>
		/// By default, SDL ignores audio devices that aren't associated with physical hardware. Changing this hint to <c>"1"</c> will expose anything SDL sees that appears to be an audio source or sink. This will add "devices" to the list that the user probably doesn't want or need, but it can be useful in scenarios where you want to hook up SDL to some sort of virtual device, etc.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Audio monitor devices will be ignored (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Audio monitor devices will show up in the device list</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUDIO_INCLUDE_MONITORS">SDL_HINT_AUDIO_INCLUDE_MONITORS</seealso>
		public static Hint IncludeMonitors { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUDIO_INCLUDE_MONITORS"); }
	}

	/// <summary>
	/// Gets a hint controlling whether SDL updates joystick state when getting input events
	/// </summary>
	/// <value>
	/// A hint controlling whether SDL updates joystick state when getting input events
	/// </value>
	/// <remarks>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>You'll call <see cref="SDL_UpdateJoysticks"/>() manually</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>SDL will automatically call <see cref="SDL_UpdateJoysticks"/>() (default)</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUTO_UPDATE_JOYSTICKS">SDL_HINT_AUTO_UPDATE_JOYSTICKS</seealso>
	public static Hint AutoUpdateJoysticks { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUTO_UPDATE_JOYSTICKS"); }

	/// <summary>
	/// Gets a hint controlling whether SDL updates sensor state when getting input events
	/// </summary>
	/// <value>
	/// A hint controlling whether SDL updates sensor state when getting input events
	/// </value>
	/// <remarks>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>You'll call <see cref="SDL_UpdateSensors"/>() manually</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>SDL will automatically call <see cref="SDL_UpdateSensors"/>() (default)</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_AUTO_UPDATE_SENSORS">SDL_HINT_AUTO_UPDATE_SENSORS</seealso>
	public static Hint AutoUpdateSensors { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_AUTO_UPDATE_SENSORS"); }

	/// <summary>
	/// Gets a hint to control whether or not to prevent SDL from using version 4 of the bitmap header when saving BMPs
	/// </summary>
	/// <value>
	/// A hint to control whether or not to prevent SDL from using version 4 of the bitmap header when saving BMPs
	/// </value>
	/// <remarks>
	/// <para>
	/// The bitmap header version 4 is required for proper alpha channel support and SDL will use it when required. Should this not be desired, this hint can force the use of the 40 byte header version which is supported everywhere.
	/// </para>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>Surfaces with a colorkey or an alpha channel are saved to a 32-bit BMP file with an alpha mask. SDL will use the bitmap header version 4 and set the alpha mask accordingly. (default)</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>Surfaces with a colorkey or an alpha channel are saved to a 32-bit BMP file without an alpha mask. The alpha channel data will be in the file, but applications are going to ignore it.</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_BMP_SAVE_LEGACY_FORMAT">SDL_HINT_BMP_SAVE_LEGACY_FORMAT</seealso>
	public static Hint BmpSaveLegacyFormat { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_BMP_SAVE_LEGACY_FORMAT"); }

	/// <summary>SDL_HINT_CAMERA_*</summary>
	public static class Camera
	{
		/// <summary>
		/// Get a hint that decides what camera backend to use
		/// </summary>
		/// <value>
		/// A hint that decides what camera backend to use
		/// </value>
		/// <remarks>
		/// <para>
		/// By default, SDL will try all available camera backends in a reasonable order until it finds one that can work, but this hint allows the app or user to force a specific target, such as <c>"directshow"</c> if, say, you are on Windows Media Foundations but want to try DirectShow instead.
		/// </para>
		/// <para>
		/// The default value is unset, in which case SDL will try to figure out the best camera backend on your behalf. This hint needs to be set before <see cref="Sdl(Sdl.BuildAction?)">the <see cref="Sdl"/> instance is created</see> to be useful.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_CAMERA_DRIVER">SDL_HINT_CAMERA_DRIVER</seealso>
		public static Hint Driver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_CAMERA_DRIVER"); }
	}

	/// <summary>
	/// Gets a hint that limits what CPU features are available
	/// </summary>
	/// <value>
	/// A hint that limits what CPU features are available
	/// </value>
	/// <remarks>
	/// <para>
	/// By default, SDL marks all features the current CPU supports as available. This hint allows to limit these to a subset.
	/// </para>
	/// <para>
	/// When the hint is unset, or empty, SDL will enable all detected CPU features.
	/// </para>
	/// <para>
	/// The hint can be set to a comma separated list containing the following items:
	/// <list type="bullet">
	///		<item><description><c>"all"</c></description></item>
	///		<item><description><c>"altivec"</c></description></item>
	///		<item><description><c>"sse"</c></description></item>
	///		<item><description><c>"sse2"</c></description></item>
	///		<item><description><c>"sse3"</c></description></item>
	///		<item><description><c>"sse41"</c></description></item>
	///		<item><description><c>"sse42"</c></description></item>
	///		<item><description><c>"avx"</c></description></item>
	///		<item><description><c>"avx2"</c></description></item>
	///		<item><description><c>"avx512f"</c></description></item>
	///		<item><description><c>"arm-simd"</c></description></item>
	///		<item><description><c>"neon"</c></description></item>
	///		<item><description><c>"lsx"</c></description></item>
	///		<item><description><c>"lasx"</c></description></item>
	/// </list>
	/// </para>
	/// <para>
	/// The items can be prefixed by <c>'+'</c>/<c>'-'</c> to add/remove features.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_CPU_FEATURE_MASK">SDL_HINT_CPU_FEATURE_MASK</seealso>
	public static Hint CpuFeatureMask { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_CPU_FEATURE_MASK"); }

	/// <summary>SDL_HINT_DISPLAY_*</summary>
	public static class Display
	{
		/// <summary>
		/// Gets a hint serving as an override for <see cref="SDL_GetDisplayUsableBounds"/>()
		/// </summary>
		/// <value>
		/// A hint serving as an override for <see cref="SDL_GetDisplayUsableBounds"/>()
		/// </value>
		/// <remarks>
		/// <para>
		/// If set, this hint will override the expected results for <see cref="SDL_GetDisplayUsableBounds"/>() for display index 0. Generally you don't want to do this, but this allows an embedded system to request that some of the screen be reserved for other uses when paired with a well-behaved application.
		/// </para>
		/// <para>
		/// The contents of this hint must be 4 comma-separated integers, the first is the bounds x, then y, width and height, in that order.
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_DISPLAY_USABLE_BOUNDS">SDL_HINT_DISPLAY_USABLE_BOUNDS</seealso>
		public static Hint UsableBounds { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_DISPLAY_USABLE_BOUNDS"); }
	}

	/// <summary>SDL_HINT_EGL_*</summary>
	public static class EGL
	{
		/// <summary>
		/// Gets a hint specifying the EGL library to load
		/// </summary>
		/// <value>
		/// A hint specifying the EGL library to load
		/// </value>
		/// <remarks>
		/// This hint should be set before <see cref="">creating an OpenGL window</see> or <see cref="">creating an OpenGL context</see>. This hint is only considered if SDL is using EGL to manage OpenGL contexts. If this hint isn't set, SDL will choose a reasonable default.
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_EGL_LIBRARY">SDL_HINT_EGL_LIBRARY</seealso>
		public static Hint Library { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_EGL_LIBRARY"); }
	}

	/// <summary>SDL_HINT_EMSCRIPTEN_*</summary>
	public static class Emscripten
	{
		/// <summary>
		/// Gets a hint to disable giving back control to the browser automatically when running with asyncify
		/// </summary>
		/// <value>
		/// A hint to disable giving back control to the browser automatically when running with asyncify
		/// </value>
		/// <remarks>
		/// <para>
		/// With <c>-s ASYNCIFY</c>, SDL calls <c>emscripten_sleep</c> during operations such as refreshing the screen or polling events.
		/// </para>
		/// <para>
		/// This hint only applies to the emscripten platform.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Disable <c>emscripten_sleep</c> calls (if you give back browser control manually or use asyncify for other purposes)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Enable <c>emscripten_sleep</c> calls (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_EMSCRIPTEN_ASYNCIFY">SDL_HINT_EMSCRIPTEN_ASYNCIFY</seealso>
		public static Hint Asyncify { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_EMSCRIPTEN_ASYNCIFY"); }

		/// <summary>
		/// Gets a hint specifying the CSS selector used for the "default" window/canvas
		/// </summary>
		/// <value>
		/// A hint specifying the CSS selector used for the "default" window/canvas
		/// </value>
		/// <remarks>
		/// <para>
		/// This hint only applies to the emscripten platform.
		/// </para>
		/// <para>
		/// The default value is <c>"#canvas"</c>.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">creating a window</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_EMSCRIPTEN_CANVAS_SELECTOR">SDL_HINT_EMSCRIPTEN_CANVAS_SELECTOR</seealso>
		public static Hint CanvasSelector { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_EMSCRIPTEN_CANVAS_SELECTOR"); }

		/// <summary>
		/// Gets a hint serving as an override for the binding element for keyboard inputs for Emscripten builds
		/// </summary>
		/// <value>
		/// A hint serving as an override for the binding element for keyboard inputs for Emscripten builds
		/// </value>
		/// <remarks>
		/// <para>
		/// This hint only applies to the emscripten platform.
		/// </para>
		/// <para>
		/// The hint can be one of:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"#window"</c></term>
		///			<description>The javascript window object (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"#document"</c></term>
		///			<description>The javascript document object</description>
		///		</item>
		///		<item>
		///			<term><c>#screen"</c></term>
		///			<description>The javascript window.screen object</description>
		///		</item>
		///		<item>
		///			<term><c>"#canvas"</c></term>
		///			<description>The WebGL canvas element</description>
		///		</item>
		///		<item>
		///			<term><c>"#none"</c></term>
		///			<description>Don't bind anything at all</description>
		///		</item>
		///		<item>
		///			<term>Any other string <em>without</em> a leading <c>#</c> sign</term>
		///			<description>The element on the page with that ID</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">creating a window</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_EMSCRIPTEN_KEYBOARD_ELEMENT">SDL_HINT_EMSCRIPTEN_KEYBOARD_ELEMENT</seealso>
		public static Hint KeyboardElement { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_EMSCRIPTEN_KEYBOARD_ELEMENT"); }
	}

	/// <summary>
	/// Gets a hint that controls whether the on-screen keyboard should be shown when text input is active
	/// </summary>
	/// <value>
	/// A hint that controls whether the on-screen keyboard should be shown when text input is active
	/// </value>
	/// <remarks>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"auto"</c></term>
	///			<description>The on-screen keyboard will be shown if there is no physical keyboard attached (default)</description>
	///		</item>
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>Do not show the on-screen keyboard</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>Show the on-screen keyboard, if available</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This hint must be set before <see cref="SDL_StartTextInput"/>() is called.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_ENABLE_SCREEN_KEYBOARD">SDL_HINT_ENABLE_SCREEN_KEYBOARD</seealso>
	public static Hint EnableScreenKeyboard { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_ENABLE_SCREEN_KEYBOARD"); }

	/// <summary>
	/// Gets a hint containing a list of evdev devices to use if udev is not available
	/// </summary>
	/// <value>
	/// A hint containing a list of evdev devices to use if udev is not available
	/// </value>
	/// <remarks>
	/// <para>
	/// The list of devices is in the form:
	/// <c>deviceclass:path[,deviceclass:path[,...]]</c>,
	/// where <c>deviceclass</c> is an integer representing the <see cref="SDL_UDEV_deviceclass"/> and <c>path</c> is the full path to the event device.
	/// </para>
	/// <para>
	/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_EVDEV_DEVICES">SDL_HINT_EVDEV_DEVICES</seealso>
	public static Hint EvDevDevices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_EVDEV_DEVICES"); }

	/// <summary>
	/// Gets a hint controlling verbosity of the logging of SDL events pushed onto the internal queue
	/// </summary>
	/// <value>
	/// A hint controlling verbosity of the logging of SDL events pushed onto the internal queue
	/// </value>
	/// <remarks>
	/// <para>
	/// The hint can be set to the following values, from least to most verbose:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>Don't log any events. (default)</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>Log most events (other than the really spammy ones)</description>
	///		</item>
	///		<item>
	///			<term><c>"2"</c></term>
	///			<description>Include mouse and finger motion events</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This is generally meant to be used to debug SDL itself, but can be useful for application developers that need better visibility into what is going on in the event queue.
	/// Logged events are sent through <see cref="Log.Info(string)"/>, which means by default they appear on stdout on most platforms or maybe <see href="https://learn.microsoft.com/de-de/windows/win32/api/debugapi/nf-debugapi-outputdebugstringa">OutputDebugString</see>() on Windows
	/// (when <see cref="Log.UseDefaultOutputForUnhandledMessages"/> is <c><see langword="true"/></c>), and can be funneled by the app with the <see cref="Log.Output"/> event, etc.
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_EVENT_LOGGING">SDL_HINT_EVENT_LOGGING</seealso>
	public static Hint EventLogging { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_EVENT_LOGGING"); }

	/// <summary>
	/// Gets a hint that specifies a dialog backend to use
	/// </summary>
	/// <value>
	/// A hint that specifies a dialog backend to use
	/// </value>
	/// <remarks>
	/// <para>
	/// By default, SDL will try all available dialog backends in a reasonable order until it finds one that can work, but this hint allows the app or user to force a specific target.
	/// </para>
	/// <para>
	/// If the specified target does not exist or is not available, the dialog-related calls will fail.
	/// </para>
	/// <para>
	/// This hint currently only applies to platforms using the generic "Unix" dialog implementation, but may be extended to more platforms in the future.
	/// Note that some Unix and Unix-like platforms have their own implementation, such as macOS and Haiku.
	/// </para>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c><see langword="null"/></c></term>
	///			<description>Select automatically (default, all platforms)</description>
	///		</item>
	///		<item>
	///			<term><c>"portal"</c></term>
	///			<description>Use XDG Portals through DBus (Unix only)</description>
	///		</item>
	///		<item>
	///			<term><c>"zenity"</c></term>
	///			<description>Use the Zenity program (Unix only)</description>
	///		</item>
	/// </list>
	/// More options may be added in the future.
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_FILE_DIALOG_DRIVER">SDL_HINT_FILE_DIALOG_DRIVER</seealso>
	public static Hint FileDialogDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_FILE_DIALOG_DRIVER"); }

	/// <summary>
	/// Gets a hint controlling whether raising the window should be done more forcefully
	/// </summary>
	/// <value>
	/// A hint controlling whether raising the window should be done more forcefully
	/// </value>
	/// <remarks>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>Honor the OS policy for raising windows (default)</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>Force the window to be raised, overriding any OS policy</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// At present, this is only an issue under MS Windows, which makes it nearly impossible to programmatically move a window to the foreground, for "security" reasons.
	/// See <see href="http://stackoverflow.com/a/34414846"/> for a discussion.
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_FORCE_RAISEWINDOW">SDL_HINT_FORCE_RAISEWINDOW</seealso>
	public static Hint ForceRaiseWindow { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_FORCE_RAISEWINDOW"); }

	/// <summary>
	/// Gets a hint controlling how 3D acceleration is used to accelerate the SDL screen surface
	/// </summary>
	/// <value>
	/// A hint controlling how 3D acceleration is used to accelerate the SDL screen surface
	/// </value>
	/// <remarks>
	/// <para>
	/// SDL can try to accelerate the SDL screen surface by using streaming textures with a 3D rendering engine. This hint controls whether and how this is done.
	/// </para>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>Disable 3D acceleration</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>Enable 3D acceleration, using the default renderer (default)</description>
	///		</item>
	///		<item>
	///			<term>Any other string</term>
	///			<description>
	///				Enable 3D acceleration, using the specified <em>valid</em> <see cref="IRenderingDriver">rendering drivers</see> (e.g. <c>"direct3d"</c>, <c>"opengl"</c>, etc.).
	///				You can use the <see cref="IRenderingDriver.Name"/> value of any of the predefined rendering drivers to set this hint (e.g. <see cref="Direct3D11.Name"/>).
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This hint should be set before accessing a <see cref="Window.Surface"/>.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_FRAMEBUFFER_ACCELERATION">SDL_HINT_FRAMEBUFFER_ACCELERATION</seealso>
	public static Hint FrameBufferAcceleration { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_FRAMEBUFFER_ACCELERATION"); }

	/// <summary>SDL_HINT_GAMECONTROLLER*</summary>
	public static class GameController
	{
		/// <summary>
		/// Gets a hint that lets you manually hint extra gamecontroller db entries
		/// </summary>
		/// <value>
		/// A hint that lets you manually hint extra gamecontroller db entries
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint should be newline delimited rows of gamecontroller config data, see <see cref="">SDL_gamepad</see>.h
		/// </para>
		/// <para>
		/// You can update mappings after SDL is initialized with <see cref="SDL_GetGamepadMappingForGUID"/>() and <see cref="SDL_AddGamepadMapping"/>()
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GAMECONTROLLERCONFIG">SDL_HINT_GAMECONTROLLERCONFIG</seealso>
		public static Hint Config { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GAMECONTROLLERCONFIG"); }

		/// <summary>
		/// Gets a hint that lets you provide a file with extra gamecontroller db entries
		/// </summary>
		/// <value>
		/// A hint that lets you provide a file with extra gamecontroller db entries
		/// </value>
		/// <remarks>
		/// <para>
		/// The file should contain lines of gamecontroller config data, see <see cref="">SDL_gamepad</see>.h
		/// </para>
		/// <para>
		/// You can update mappings after SDL is initialized with <see cref="SDL_GetGamepadMappingForGUID"/>() and <see cref="SDL_AddGamepadMapping"/>()
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GAMECONTROLLERCONFIG_FILE">SDL_HINT_GAMECONTROLLERCONFIG_FILE</seealso>
		public static Hint ConfigFile { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GAMECONTROLLERCONFIG_FILE"); }

		/// <summary>
		/// Gets a hint containing a list of devices to skip when scanning for game controllers
		/// </summary>
		/// <value>
		/// A hint containing a list of devices to skip when scanning for game controllers
		/// </value>
		/// <remarks>
		/// <para>
		/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
		/// </para>
		/// <para>
		/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GAMECONTROLLER_IGNORE_DEVICES">SDL_HINT_GAMECONTROLLER_IGNORE_DEVICES</seealso>
		public static Hint IgnoreDevices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GAMECONTROLLER_IGNORE_DEVICES"); }

		/// <summary>
		/// Gets a hint that if set, controls to skip all devices when scanning for game controllers except for the ones listed in this hint
		/// </summary>
		/// <value>
		/// A hint that if set, controls to skip all devices when scanning for game controllers except for the ones listed in this hint
		/// </value>
		/// <remarks>
		/// <para>
		/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
		/// </para>
		/// <para>
		/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GAMECONTROLLER_IGNORE_DEVICES_EXCEPT">SDL_HINT_GAMECONTROLLER_IGNORE_DEVICES_EXCEPT</seealso>
		public static Hint IgnoreDevicesExcept { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GAMECONTROLLER_IGNORE_DEVICES_EXCEPT"); }

		/// <summary>
		/// Gets a hint that controls whether the device's built-in accelerometer and gyro should be used as sensors for gamepads
		/// </summary>
		/// <value>
		/// A hint that controls whether the device's built-in accelerometer and gyro should be used as sensors for gamepads
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Sensor fusion is disabled</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Sensor fusion is enabled for all controllers that lack sensors</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// Or the hint can be a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
		/// </para>
		/// <para>
		/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">a gamepad is opened</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GAMECONTROLLER_SENSOR_FUSION">SDL_HINT_GAMECONTROLLER_SENSOR_FUSION</seealso>
		public static Hint SensorFusion { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GAMECONTROLLER_SENSOR_FUSION"); }

		/// <summary>
		/// Gets a hint that overrides the automatic controller type detection
		/// </summary>
		/// <value>
		/// A hint that overrides the automatic controller type detection
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint should be comma separated entries, in the form: <c>"VID/PID=type"</c>, where the <c>VID</c> and <c>PID</c> should be hexadecimal with exactly 4 digits, e.g. <c>0x00fd</c>.
		/// </para>
		/// <para>
		/// This hint affects what low level protocol is used with the HIDAPI driver.
		/// </para>
		/// <para>
		/// The <c>type</c> can be set to the following values:
		/// <list type="bullet">
		///		<item><description><c>Xbox360</c></description></item>
		///		<item><description><c>XboxOne</c></description></item>
		///		<item><description><c>PS3</c></description></item>
		///		<item><description><c>PS4</c></description></item>
		///		<item><description><c>PS5</c></description></item>
		///		<item><description><c>SwitchPro</c></description></item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GAMECONTROLLERTYPE">SDL_HINT_GAMECONTROLLERTYPE</seealso>
		public static Hint Type { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GAMECONTROLLERTYPE"); }
	}

	/// <summary>SDL_HINT_GDK_*</summary>
	public static class Gdk
	{
		/// <summary>SDL_HINT_GDK_TEXTINPUT_*</summary>
		public static class TextInput
		{
			/// <summary>
			/// Gets a hint that sets the default text of the TextInput window on GDK platforms
			/// </summary>
			/// <value>
			/// A hint that sets the default text of the TextInput window on GDK platforms
			/// </value>
			/// <remarks>
			/// <para>
			/// This hint is available only if <see cref="SDL_GDK_TEXTINPUT"/> defined.
			/// </para>
			/// <para>
			/// This hint should be set before calling <see cref="SDL_StartTextInput"/>().
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GDK_TEXTINPUT_DEFAULT_TEXT">SDL_HINT_GDK_TEXTINPUT_DEFAULT_TEXT</seealso>
			public static Hint DefaultText { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GDK_TEXTINPUT_DEFAULT_TEXT"); }

			/// <summary>
			/// Gets a hint that sets the description of the TextInput window on GDK platforms
			/// </summary>
			/// <value>
			/// A hint that sets the description of the TextInput window on GDK platforms
			/// </value>
			/// <remarks>
			/// <para>
			/// This hint is available only if <see cref="SDL_GDK_TEXTINPUT"/> defined.
			/// </para>
			/// <para>
			/// This hint should be set before calling <see cref="SDL_StartTextInput"/>().
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GDK_TEXTINPUT_DESCRIPTION">SDL_HINT_GDK_TEXTINPUT_DESCRIPTION</seealso>
			public static Hint Description { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GDK_TEXTINPUT_DESCRIPTION"); }

			/// <summary>
			/// Gets a hint that sets the maximum input length of the TextInput window on GDK platforms
			/// </summary>
			/// <value>
			/// A hint that sets the maximum input length of the TextInput window on GDK platforms
			/// </value>
			/// <remarks>
			/// <para>
			/// The value must be a stringified integer, for example <c>"10"</c> to allow for up to 10 characters of text input.
			/// </para>
			/// <para>
			/// This hint is available only if <see cref="SDL_GDK_TEXTINPUT"/> defined.
			/// </para>
			/// <para>
			/// This hint should be set before calling <see cref="SDL_StartTextInput"/>().
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GDK_TEXTINPUT_MAX_LENGTH">SDL_HINT_GDK_TEXTINPUT_MAX_LENGTH</seealso>
			public static Hint MaxLength { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GDK_TEXTINPUT_MAX_LENGTH"); }

			/// <summary>
			/// Gets a hint that sets the input scope of the TextInput window on GDK platforms
			/// </summary>
			/// <value>
			/// A hint that sets the input scope of the TextInput window on GDK platforms
			/// </value>
			/// <remarks>
			/// <para>
			/// </para>
			/// Set this hint to change the <c>XGameUiTextEntryInputScope</c> value that will be passed to the window creation function.
			/// The value must be a stringified integer, for example <c>"0"</c> for <c>XGameUiTextEntryInputScope::Default</c>.
			/// <para>
			/// This hint is available only if <see cref="SDL_GDK_TEXTINPUT"/> defined.
			/// </para>
			/// <para>
			/// This hint should be set before calling <see cref="SDL_StartTextInput"/>().
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GDK_TEXTINPUT_SCOPE">SDL_HINT_GDK_TEXTINPUT_SCOPE</seealso>
			public static Hint Scope { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GDK_TEXTINPUT_SCOPE"); }

			/// <summary>
			/// Gets a hint that sets the title of the TextInput window on GDK platforms
			/// </summary>
			/// <value>
			/// A hint that sets the title of the TextInput window on GDK platforms
			/// </value>
			/// <remarks>
			/// <para>
			/// This hint is available only if <see cref="SDL_GDK_TEXTINPUT"/> defined.
			/// </para>
			/// <para>
			/// This hint should be set before calling <see cref="SDL_StartTextInput"/>().
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GDK_TEXTINPUT_TITLE">SDL_HINT_GDK_TEXTINPUT_TITLE</seealso>
			public static Hint Title { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GDK_TEXTINPUT_TITLE"); }
		}
	}

	/// <summary>SDL_HINT_GPU_*</summary>
	public static class Gpu
	{
		/// <summary>
		/// Gets a hint that specifies a GPU backend to use
		/// </summary>
		/// <value>
		/// A hint that specifies a GPU backend to use
		/// </value>
		/// <remarks>
		/// <para>
		/// By default, SDL will try all available GPU backends in a reasonable order until it finds one that can work,
		/// but this hint allows the app or user to force a specific target, such as <c>"direct3d11"</c> if, say, your hardware supports D3D12 but want to try using D3D11 instead.
		/// </para>
		/// <para>
		/// This hint should be set before any GPU functions are called.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_GPU_DRIVER">SDL_HINT_GPU_DRIVER</seealso>
		public static Hint Driver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_GPU_DRIVER"); }
	}

	/// <summary>SDL_HINT_HIDAPI_*</summary>
	public static class HidApi
	{
		/// <summary>
		/// Gets a hint to control whether <see cref="SDL_hid_enumerate"/>() enumerates all HID devices or only controllers
		/// </summary>
		/// <value>
		/// A hint to control whether <see cref="SDL_hid_enumerate"/>() enumerates all HID devices or only controllers
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description><see cref="SDL_hid_enumerate"/>() will enumerate all HID devices</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description><see cref="SDL_hid_enumerate"/>() will only enumerate controllers (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// By default SDL will only enumerate controllers, to reduce risk of hanging or crashing on devices with bad drivers and avoiding macOS keyboard capture permission prompts.
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_HIDAPI_ENUMERATE_ONLY_CONTROLLERS">SDL_HINT_HIDAPI_ENUMERATE_ONLY_CONTROLLERS</seealso>
		public static Hint EnumerateOnlyControllers { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_HIDAPI_ENUMERATE_ONLY_CONTROLLERS"); }

		/// <summary>
		/// Gets a hint containing a list of devices to ignore in <see cref="SDL_hid_enumerate"/>()
		/// </summary>
		/// <value>
		/// A hint containing a list of devices to ignore in <see cref="SDL_hid_enumerate"/>()
		/// </value>
		/// <remarks>
		/// <para>
		/// The format of the hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
		/// </para>
		/// <para>
		/// For example, to ignore the Shanwan DS3 controller and any Valve controller, you might use the string <c>"0x2563/0x0523,0x28de/0x0000"</c>.
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_HIDAPI_IGNORE_DEVICES">SDL_HINT_HIDAPI_IGNORE_DEVICES</seealso>
		public static Hint IgnoreDevices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_HIDAPI_IGNORE_DEVICES"); }

		/// <summary>
		/// Gets a hint to control whether HIDAPI uses libusb for device access
		/// </summary>
		/// <value>
		/// A hint to control whether HIDAPI uses libusb for device access
		/// </value>
		/// <remarks>
		/// <para>
		/// By default libusb will only be used for a few devices that require direct USB access, and this can be controlled with <see cref="LibUsbWhitelist"/>.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>HIDAPI will not use libusb for device access</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>HIDAPI will use libusb for device access if available (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_HIDAPI_LIBUSB">SDL_HINT_HIDAPI_LIBUSB</seealso>
		public static Hint LibUsb { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_HIDAPI_LIBUSB"); }

		/// <summary>
		/// Gets a hint to control whether HIDAPI uses libusb only for whitelisted devices
		/// </summary>
		/// <value>
		/// A hint to control whether HIDAPI uses libusb only for whitelisted devices
		/// </value>
		/// <remarks>
		/// <para>
		/// By default libusb will only be used for a few devices that require direct USB access.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>HIDAPI will use libusb for all device access</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>HIDAPI will use libusb only for whitelisted devices (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_HIDAPI_LIBUSB_WHITELIST">SDL_HINT_HIDAPI_LIBUSB_WHITELIST</seealso>
		public static Hint LibUsbWhitelist { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_HIDAPI_LIBUSB_WHITELIST"); }

		/// <summary>
		/// Gets a hint to control whether HIDAPI uses udev for device detection
		/// </summary>
		/// <value>
		/// A hint to control whether HIDAPI uses udev for device detection
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>HIDAPI will poll for device changes</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>HIDAPI will use udev for device detection (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_HIDAPI_UDEV">SDL_HINT_HIDAPI_UDEV</seealso>
		public static Hint UDev { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_HIDAPI_UDEV"); }
	}

	/// <summary>
	/// Gets a hint describing what IME UI elements the application can display
	/// </summary>
	/// <value>
	/// A hint describing what IME UI elements the application can display
	/// </value>
	/// <remarks>
	/// <para>
	/// By default IME UI is handled using native components by the OS where possible, however this can interfere with or not be visible when exclusive fullscreen mode is used.
	/// </para>
	/// <para>
	/// The hint can be set to a comma separated list containing the following items:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"none"</c> or <c>"0"</c></term>
	///			<description>The application can't render any IME elements, and native UI should be used (default)</description>
	///		</item>
	///		<item>
	///			<term><c>"composition"</c></term>
	///			<description>The application handles <see cref="EventType.Keyboard.TextEditing"/> events and can render the composition text</description>
	///		</item>
	///		<item>
	///			<term><c>"candidates"</c></term>
	///			<description>The application handles <see cref="EventType.Keyboard.TextEditingCandidates"/> and can render the candidate list</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_IME_IMPLEMENTED_UI">SDL_HINT_IME_IMPLEMENTED_UI</seealso>
	public static Hint ImeImplementedUI { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_IME_IMPLEMENTED_UI"); }

	/// <summary>SDL_HINT_IOS_*</summary>
	public static class IOS
	{
		/// <summary>
		/// Gets a hint controlling whether the home indicator bar on iPhone X should be hidden
		/// </summary>
		/// <value>
		/// A hint controlling whether the home indicator bar on iPhone X should be hidden
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>The indicator bar is not hidden (default for windowed applications)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>The indicator bar is hidden and is shown when the screen is touched (useful for movie playback applications)</description>
		///		</item>
		///		<item>
		///			<term><c>"2"</c></term>
		///			<description>The indicator bar is dim and the first swipe makes it visible and the second swipe performs the "home" action (default for fullscreen applications)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_IOS_HIDE_HOME_INDICATOR">SDL_HINT_IOS_HIDE_HOME_INDICATOR</seealso>
		public static Hint HideHomeIndicator { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_IOS_HIDE_HOME_INDICATOR"); }
	}

	/// <summary>SDL_HINT_JOYSTICK_*</summary>
	public static class Joystick
	{
		/// <summary>
		/// Gets a hint that lets you enable joystick (and gamecontroller) events even when your app is in the background
		/// </summary>
		/// <value>
		/// A hint that lets you enable joystick (and gamecontroller) events even when your app is in the background
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Disable joystick &amp; gamecontroller input events when the application is in the background (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Enable joystick &amp; gamecontroller input events when the application is in the background</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS">SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS</seealso>
		public static Hint AllowBackgroundEvents { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS"); }

		/// <summary>SDL_HINT_JOYSTICK_ARCADESTICK_*</summary>
		public static class ArcadeStick
		{
			/// <summary>
			/// Gets a hint containing a list of arcade stick style controllers
			/// </summary>
			/// <value>
			/// A hint containing a list of arcade stick style controllers
			/// </value>
			/// <remarks>
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_ARCADESTICK_DEVICES">SDL_HINT_JOYSTICK_ARCADESTICK_DEVICES</seealso>
			public static Hint Devices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_ARCADESTICK_DEVICES"); }

			/// <summary>
			/// Gets a hint containing a list of devices that are not arcade stick style controllers
			/// </summary>
			/// <value>
			/// A hint containing a list of devices that are not arcade stick style controllers
			/// </value>
			/// <remarks>
			/// <para>
			/// </para>
			/// This hint will override <see cref="Devices"/> and the built in device list.
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_ARCADESTICK_DEVICES_EXCLUDED">SDL_HINT_JOYSTICK_ARCADESTICK_DEVICES_EXCLUDED</seealso>
			public static Hint DevicesExcluded { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_ARCADESTICK_DEVICES_EXCLUDED"); }
		}

		/// <summary>SDL_HINT_JOYSTICK_BLACKLIST_*</summary>
		public static class Blacklist
		{
			/// <summary>
			/// Gets a hint containing a list of devices that should not be considered joysticks
			/// </summary>
			/// <value>
			/// A hint containing a list of devices that should not be considered joysticks
			/// </value>
			/// <remarks>
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_BLACKLIST_DEVICES">SDL_HINT_JOYSTICK_BLACKLIST_DEVICES</seealso>
			public static Hint Devices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_BLACKLIST_DEVICES"); }

			/// <summary>
			/// Gets a hint containing a list of devices that should be considered joysticks
			/// </summary>
			/// <value>
			/// A hint containing a list of devices that should be considered joysticks
			/// </value>
			/// <remarks>
			/// <para>
			/// </para>
			/// This hint will override <see cref="Devices"/> and the built in device list.
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_BLACKLIST_DEVICES_EXCLUDED">SDL_HINT_JOYSTICK_BLACKLIST_DEVICES_EXCLUDED</seealso>
			public static Hint DevicesExcluded { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_BLACKLIST_DEVICES_EXCLUDED"); }
		}

		/// <summary>
		/// Gets a hint containing a comma separated list of devices to open as joysticks
		/// </summary>
		/// <value>
		/// A hint containing a comma separated list of devices to open as joysticks
		/// </value>
		/// <remarks>
		/// <para>
		/// This hint is currently only used by the Linux joystick driver
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_DEVICE">SDL_HINT_JOYSTICK_DEVICE</seealso>
		public static Hint Device { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_HINT_JOYSTICK_DEVICE"); }

		/// <summary>
		/// Gets a hint controlling whether DirectInput should be used for controllers
		/// </summary>
		/// <value>
		/// A hint controlling whether DirectInput should be used for controllers
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Disable DirectInput detection</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Enable DirectInput detection (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_DIRECTINPUT">SDL_HINT_JOYSTICK_DIRECTINPUT</seealso>
		public static Hint DirectInput { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_DIRECTINPUT"); }

		/// <summary>
		/// Gets a hint controlling whether enhanced reports should be used for controllers when using the HIDAPI driver
		/// </summary>
		/// <value>
		/// A hint controlling whether enhanced reports should be used for controllers when using the HIDAPI driver
		/// </value>
		/// <remarks>
		/// <para>
		/// Enhanced reports allow rumble and effects on Bluetooth PlayStation controllers and gyro on Nintendo Switch controllers, 
		/// but break Windows DirectInput for other applications that don't use SDL.
		/// </para>
		///<para>
		/// Once enhanced reports are enabled, they can't be disabled on PlayStation controllers without power cycling the controller.
		/// </para>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Enhanced reports are not enabled</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Enhanced reports are enabled (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"auto"</c></term>
		///			<description>Enhanced features are advertised to the application, but SDL doesn't change the controller report mode unless the application uses them</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_ENHANCED_REPORTS">SDL_HINT_JOYSTICK_ENHANCED_REPORTS</seealso>
		public static Hint EnhancedReports { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_ENHANCED_REPORTS"); }

		/// <summary>SDL_HINT_JOYSTICK_FLIGHTSTICK_*</summary>
		public static class FlightStick
		{
			/// <summary>
			/// Gets a hint containing a list of flightstick style controllers
			/// </summary>
			/// <value>
			/// A hint containing a list of flightstick style controllers
			/// </value>
			/// <remarks>
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_FLIGHTSTICK_DEVICES">SDL_HINT_JOYSTICK_FLIGHTSTICK_DEVICES</seealso>
			public static Hint Devices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_FLIGHTSTICK_DEVICES"); }

			/// <summary>
			/// Gets a hint containing a list of devices that are not flightstick style controllers
			/// </summary>
			/// <value>
			/// A hint containing a list of devices that are not flightstick style controllers
			/// </value>
			/// <remarks>
			/// <para>
			/// </para>
			/// This hint will override <see cref="Devices"/> and the built in device list.
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_FLIGHTSTICK_DEVICES_EXCLUDED">SDL_HINT_JOYSTICK_FLIGHTSTICK_DEVICES_EXCLUDED</seealso>
			public static Hint DevicesExcluded { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_FLIGHTSTICK_DEVICES_EXCLUDED"); }
		}

		/// <summary>SDL_HINT_JOYSTICK_GAMECUBE_*</summary>
		public static class GameCube
		{
			/// <summary>
			/// Gets a hint containing a list of devices known to have a GameCube form factor
			/// </summary>
			/// <value>
			/// A hint containing a list of devices known to have a GameCube form factor
			/// </value>
			/// <remarks>
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_GAMECUBE_DEVICES">SDL_HINT_JOYSTICK_GAMECUBE_DEVICES</seealso>
			public static Hint Devices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_GAMECUBE_DEVICES"); }

			/// <summary>
			/// Gets a hint containing a list of devices known not to have a GameCube form factor
			/// </summary>
			/// <value>
			/// A hint containing a list of devices known not to have a GameCube form factor
			/// </value>
			/// <remarks>
			/// <para>
			/// </para>
			/// This hint will override <see cref="Devices"/> and the built in device list.
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_GAMECUBE_DEVICES_EXCLUDED">SDL_HINT_JOYSTICK_GAMECUBE_DEVICES_EXCLUDED</seealso>
			public static Hint DevicesExcluded { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_GAMECUBE_DEVICES_EXCLUDED"); }
		}

		/// <summary>
		/// Gets a hint controlling whether GameInput should be used for controller handling on Windows
		/// </summary>
		/// <value>
		/// A hint controlling whether GameInput should be used for controller handling on Windows
		/// </value> 
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>GameInput is not used</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>GameInput is used</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// The default is <c>"1"</c> on GDK platforms, and <c>"0"</c> otherwise.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_GAMEINPUT">SDL_HINT_JOYSTICK_GAMEINPUT</seealso>
		public static Hint GameInput { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_GAMEINPUT"); }

		/// <summary>
		/// Gets a hint containing a list of devices and their desired number of haptic (force feedback) enabled axis
		/// </summary>
		/// <value>
		/// A hint containing a list of devices and their desired number of haptic (force feedback) enabled axis
		/// </value>
		/// <remarks>
		/// <para>
		/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form plus the number of desired axes,
		/// e.g. <c>"0xAAAA/0xBBBB/1,0xCCCC/0xDDDD/3"</c>.
		/// </para>
		/// <para>
		/// This hint supports a "wildcard" device that will set the number of haptic axes on all initialized haptic devices which were not defined explicitly in this hint:
		/// <c>"0xFFFF/0xFFFF/1"</c>.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">a controller is opened</see>. The number of haptic axes won't exceed the number of real axes found on the device.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HAPTIC_AXES">SDL_HINT_JOYSTICK_HAPTIC_AXES</seealso>
		public static Hint HapticAxes { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HAPTIC_AXES"); }

		/// <summary>SDL_HINT_JOYSTICK_HIDAPI_*</summary>
		/// <remarks>
		/// For the hint SDL_HINT_JOYSTICK_HIDAPI (<c>"SDL_JOYSTICK_HIDAPI"</c>) see <see cref="UseDrivers"/>.
		/// </remarks>
		public static class HidApi
		{
			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_8BITDO_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_8BITDO (<c>"SDL_JOYSTICK_HIDAPI_8BITDO"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class _8BitDo
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for 8BitDo controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for 8BitDo controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_8BITDO">SDL_HINT_JOYSTICK_HIDAPI_8BITDO</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_8BITDO"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_FLYDIGI_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_FLYDIGI (<c>"SDL_JOYSTICK_HIDAPI_FLYDIGI"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Flydigi
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Flydigi controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Flydigi controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_FLYDIGI">SDL_HINT_JOYSTICK_HIDAPI_FLYDIGI</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_FLYDIGI"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE (<c>"SDL_JOYSTICK_HIDAPI_GAMECUBE"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class GameCube
			{
				/// <summary>
				/// Gets a hint controlling whether rumble is used to implement the GameCube controller's 3 rumble modes (Stop(0), Rumble(1), and StopHard(2))
				/// </summary>
				/// <value>
				/// A hint controlling whether rumble is used to implement the GameCube controller's 3 rumble modes (Stop(0), Rumble(1), and StopHard(2))
				/// </value>
				/// <remarks>
				/// <para>
				/// This is useful for applications that need full compatibility for things like ADSR envelopes.
				/// <list type="bullet">
				///		<item>
				///			<description>Stop is implemented by setting <see cref="low_frequency_rumble"/> to <c>0</c> and <see cref="high_frequency_rumble"/> <c>&gt; 0</c></description>
				///		</item>
				///		<item>
				///			<description>Rumble is both at any arbitrary value</description>
				///		</item>
				///		<item>
				///			<description>StopHard is implemented by setting both <see cref="low_frequency_rumble"/> and <see cref="high_frequency_rumble"/> to <c>0</c></description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Normal rumble behavior is behavior is used (default)</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Proper GameCube controller rumble behavior is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// This hint can be set anytime.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE_RUMBLE_BRAKE">SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE_RUMBLE_BRAKE</seealso>
				public static Hint RumbleBrake { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_GAMECUBE_RUMBLE_BRAKE"); }

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Nintendo GameCube controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Nintendo GameCube controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE">SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_GAMECUBE"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_GIP_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_GIP (<c>"SDL_JOYSTICK_HIDAPI_GIP"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Gip
			{
				/// <summary>
				/// Gets a hint controlling whether the new HIDAPI driver for wired Xbox One (GIP) controllers should reset the controller if it can't get the metadata from the controller
				/// </summary>
				/// <value>
				/// A hint controlling whether the new HIDAPI driver for wired Xbox One (GIP) controllers should reset the controller if it can't get the metadata from the controller
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Assume this is a generic controller</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Reset the controller to get metadata</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// By default the controller is not reset.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_GIP_RESET_FOR_METADATA">SDL_HINT_JOYSTICK_HIDAPI_GIP_RESET_FOR_METADATA</seealso>
				/// </remarks>
				public static Hint ResetForMetadata { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_GIP_RESET_FOR_METADATA"); }

				/// <summary>
				/// Gets a hint controlling whether the new HIDAPI driver for wired Xbox One (GIP) controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the new HIDAPI driver for wired Xbox One (GIP) controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="XBoxOne.UseDriver"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_GIP">SDL_HINT_JOYSTICK_HIDAPI_GIP</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_GIP"); }
			}

			/// <summary>
			/// SDL_HINT_JOYSTICK_HIDAPI_JOYCON_*, SDL_HINT_JOYSTICK_HIDAPI_JOY_CONS_* and SDL_HINT_JOYSTICK_HIDAPI_*_JOY_CONS
			/// </summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_JOY_CONS (<c>"SDL_JOYSTICK_HIDAPI_JOY_CONS"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class JoyCons
			{
				/// <summary>
				/// Gets a hint controlling whether Nintendo Switch Joy-Con controllers will be combined into a single Pro-like controller when using the HIDAPI driver
				/// </summary>
				/// <value>
				/// A hint controlling whether Nintendo Switch Joy-Con controllers will be combined into a single Pro-like controller when using the HIDAPI driver
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Left and right Joy-Con controllers will not be combined and each will be a mini-gamepad</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Left and right Joy-Con controllers will be combined into a single controller (default)</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_COMBINE_JOY_CONS">SDL_HINT_JOYSTICK_HIDAPI_COMBINE_JOY_CONS</seealso>
				public static Hint Combine { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_COMBINE_JOY_CONS"); }

				/// <summary>
				/// Gets a hint controlling whether the Home button LED should be turned on when a Nintendo Switch Joy-Con controller is opened
				/// </summary>
				/// <value>
				/// A hint controlling whether the Home button LED should be turned on when a Nintendo Switch Joy-Con controller is opened
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Home button LED is turned off</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Home button LED is turned on</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// By default the Home button LED state is not changed.
				/// This hint can also be set to a floating point value between <c>0.0</c> and <c>1.0</c> which controls the brightness of the Home button LED.
				/// </para>
				/// <para>
				/// This hint can be set anytime.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_JOYCON_HOME_LED">SDL_HINT_JOYSTICK_HIDAPI_JOYCON_HOME_LED</seealso>
				public static Hint HomeLed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_JOYCON_HOME_LED"); }

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Nintendo Switch Joy-Cons should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Nintendo Switch Joy-Cons should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_JOY_CONS">SDL_HINT_JOYSTICK_HIDAPI_JOY_CONS</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_JOY_CONS"); }

				/// <summary>
				/// Gets a hint controlling whether Nintendo Switch Joy-Con controllers will be in vertical mode when using the HIDAPI driver
				/// </summary>
				/// <value>
				/// A hint controlling whether Nintendo Switch Joy-Con controllers will be in vertical mode when using the HIDAPI driver
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Left and right Joy-Con controllers will not be in vertical mode (default)</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Left and right Joy-Con controllers will be in vertical mode</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// This hint should be set before <see cref="">opening a Joy-Con controller</see>.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_VERTICAL_JOY_CONS">SDL_HINT_JOYSTICK_HIDAPI_VERTICAL_JOY_CONS</seealso>
				public static Hint Vertical { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_VERTICAL_JOY_CONS"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_LG4FF_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_LG4FF (<c>"SDL_JOYSTICK_HIDAPI_LG4FF"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Lg4Ff
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for some Logitech wheels should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for some Logitech wheels should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_LG4FF">SDL_HINT_JOYSTICK_HIDAPI_LG4FF</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_LG4FF"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_LUNA_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_LUNA (<c>"SDL_JOYSTICK_HIDAPI_LUNA"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Luna
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Amazon Luna controllers connected via Bluetooth should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Amazon Luna controllers connected via Bluetooth should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_LUNA">SDL_HINT_JOYSTICK_HIDAPI_LUNA</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_LUNA"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_NINTENDO_CLASSIC_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_NINTENDO_CLASSIC (<c>"SDL_JOYSTICK_HIDAPI_NINTENDO_CLASSIC"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class NintendoClassic
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Nintendo Online classic controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Nintendo Online classic controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_NINTENDO_CLASSIC">SDL_HINT_JOYSTICK_HIDAPI_NINTENDO_CLASSIC</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_NINTENDO_CLASSIC"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_PS3_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_PS3 (<c>"SDL_JOYSTICK_HIDAPI_PS3"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class PS3
			{
				/// <summary>
				/// Gets a hint controlling whether the Sony driver (sixaxis.sys) for PS3 controllers (Sixaxis/DualShock 3) should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the Sony driver (sixaxis.sys) for PS3 controllers (Sixaxis/DualShock 3) should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Sony driver (sixaxis.sys) is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Sony driver (sixaxis.sys) is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is <c>0</c>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_PS3_SIXAXIS_DRIVER">SDL_HINT_JOYSTICK_HIDAPI_PS3_SIXAXIS_DRIVER</seealso>
				public static Hint SixaxisDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_PS3_SIXAXIS_DRIVER"); }

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for PS3 controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for PS3 controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/> on macOS, and <c>"0"</c> on other platforms.
				/// </para>
				/// <para>
				/// For official Sony driver (sixaxis.sys) use <see cref="SixaxisDriver"/>. See <see href="https://github.com/ViGEm/DsHidMini"/> for an alternative driver on Windows.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_PS3">SDL_HINT_JOYSTICK_HIDAPI_PS3</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_PS3"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_PS4_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_PS4 (<c>"SDL_JOYSTICK_HIDAPI_PS4"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class PS4
			{
				/// <summary>
				/// Gets a hint controlling the update rate of the PS4 controller over Bluetooth when using the HIDAPI driver
				/// </summary>
				/// <value>
				/// A hint controlling the update rate of the PS4 controller over Bluetooth when using the HIDAPI driver
				/// </value>
				/// <remarks>
				/// <para>
				/// This defaults to 4 ms, to match the behavior over USB, and to be more friendly to other Bluetooth devices and older Bluetooth hardware on the computer.
				/// It can be set to <c>"1"</c> (1000 Hz), <c>"2"</c> (500 Hz), and <c>"4"</c> (250 Hz).
				/// </para>
				/// <para>
				/// This hint can be set anytime, but only takes effect when extended input reports are enabled.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_PS4_REPORT_INTERVAL">SDL_HINT_JOYSTICK_HIDAPI_PS4_REPORT_INTERVAL</seealso>
				public static Hint ReportInterval { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_PS4_REPORT_INTERVAL"); }

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for PS4 controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for PS4 controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_PS4">SDL_HINT_JOYSTICK_HIDAPI_PS4</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_PS4"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_PS5_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_PS5 (<c>"SDL_JOYSTICK_HIDAPI_PS5"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class PS5
			{
				/// <summary>
				/// Gets a hint controlling whether the player LEDs should be lit to indicate which player is associated with a PS5 controller
				/// </summary>
				/// <value>
				/// A hint controlling whether the player LEDs should be lit to indicate which player is associated with a PS5 controller
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Player LEDs are not enabled</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Player LEDs are enabled (default)</description>
				///		</item>
				/// </list>
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_PS5_PLAYER_LED">SDL_HINT_JOYSTICK_HIDAPI_PS5_PLAYER_LED</seealso>
				public static Hint PlayerLed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_PS5_PLAYER_LED"); }

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for PS5 controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for PS5 controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_PS5">SDL_HINT_JOYSTICK_HIDAPI_PS5</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_PS5"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_SINPUT_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_SINPUT (<c>"SDL_JOYSTICK_HIDAPI_SINPUT"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class SInput
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for SInput controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for SInput controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_SINPUT">SDL_HINT_JOYSTICK_HIDAPI_SINPUT</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_SINPUT"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_SHIELD_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_SHIELD (<c>"SDL_JOYSTICK_HIDAPI_SHIELD"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Shield
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for NVIDIA SHIELD controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for NVIDIA SHIELD controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_SHIELD">SDL_HINT_JOYSTICK_HIDAPI_SHIELD</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_SHIELD"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_STADIA_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_STADIA (<c>"SDL_JOYSTICK_HIDAPI_STADIA"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Stadia
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Google Stadia controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Google Stadia controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_STADIA">SDL_HINT_JOYSTICK_HIDAPI_STADIA</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_STADIA"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_STEAM_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_STEAM (<c>"SDL_JOYSTICK_HIDAPI_STEAM"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Steam
			{
				/// <summary>
				/// Gets a hint controlling whether the Steam button LED should be turned on when a Steam controller is opened
				/// </summary>
				/// <value>
				/// A hint controlling whether the Steam button LED should be turned on when a Steam controller is opened
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Steam button LED is turned off</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Steam button LED is turned on</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// By default the Steam button LED state is not changed.
				/// This hint can also be set to a floating point value between <c>0.0</c> and <c>1.0</c> which controls the brightness of the Steam button LED.
				/// </para>	
				/// <para>
				/// This hint can be set anytime.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_STEAM_HOME_LED">SDL_HINT_JOYSTICK_HIDAPI_STEAM_HOME_LED</seealso>
				public static Hint HomeLed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_STEAM_HOME_LED"); }

				/// <summary>SDL_HINT_JOYSTICK_HIDAPI_STEAM_HORI_*</summary>
				/// <remarks>
				/// For the hint SDL_HINT_JOYSTICK_HIDAPI_STEAM_HORI (<c>"SDL_JOYSTICK_HIDAPI_STEAM_HORI"</c>) see <see cref="UseDriver"/>.
				/// </remarks>
				public static class Hori
				{
					/// <summary>
					/// Gets a hint controlling whether the HIDAPI driver for HORI licensed Steam controllers should be used
					/// </summary>
					/// <value>
					/// A hint controlling whether the HIDAPI driver for HORI licensed Steam controllers should be used
					/// </value>
					/// <remarks>
					/// <para>
					/// The hint can be set to the following values:
					/// <list type="bullet">
					///		<item>
					///			<term><c>"0"</c></term>
					///			<description>HIDAPI driver is not used</description>
					///		</item>
					///		<item>
					///			<term><c>"1"</c></term>
					///			<description>HIDAPI driver is used</description>
					///		</item>
					/// </list>
					/// </para>
					/// <para>
					/// The default value for this hint is the value of <see cref="UseDrivers"/>.
					/// </para>
					/// </remarks>
					/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_STEAM_HORI">SDL_HINT_JOYSTICK_HIDAPI_STEAM_HORI</seealso>
					public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_STEAM_HORI"); }
				}

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Bluetooth Steam Controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Bluetooth Steam Controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used (default)</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used for Steam Controllers, which requires Bluetooth access and may prompt the user for permission on iOS and Android</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_STEAM">SDL_HINT_JOYSTICK_HIDAPI_STEAM</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_STEAM"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_STEAMDECK_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_STEAMDECK (<c>"SDL_JOYSTICK_HIDAPI_STEAMDECK"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class SteamDeck
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for the Steam Deck builtin controller should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for the Steam Deck builtin controller should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_STEAMDECK">SDL_HINT_JOYSTICK_HIDAPI_STEAMDECK</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_STEAMDECK"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_SWITCH_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_SWITCH (<c>"SDL_JOYSTICK_HIDAPI_SWITCH"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Switch
			{
				/// <summary>
				/// Gets a hint controlling whether the Home button LED should be turned on when a Nintendo Switch Pro controller is opened
				/// </summary>
				/// <value>
				/// A hint controlling whether the Home button LED should be turned on when a Nintendo Switch Pro controller is opened
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Home button LED is turned off</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Home button LED is turned on</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// By default the Home button LED state is not changed.
				/// This hint can also be set to a floating point value between <c>0.0</c> and <c>1.0</c> which controls the brightness of the Home button LED.
				/// </para>
				/// <para>
				/// This hint can be set anytime.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_SWITCH_HOME_LED">SDL_HINT_JOYSTICK_HIDAPI_SWITCH_HOME_LED</seealso>
				public static Hint HomeLed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_SWITCH_HOME_LED"); }

				/// <summary>
				/// Gets a hint controlling whether the player LEDs should be lit to indicate which player is associated with a Nintendo Switch controller
				/// </summary>
				/// <value>
				/// A hint controlling whether the player LEDs should be lit to indicate which player is associated with a Nintendo Switch controller
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Player LEDs are not enabled</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Player LEDs are enabled (default)</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// This hint can be set anytime.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_SWITCH_PLAYER_LED">SDL_HINT_JOYSTICK_HIDAPI_SWITCH_PLAYER_LED</seealso>
				public static Hint PlayerLed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_SWITCH_PLAYER_LED"); } 

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Nintendo Switch controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Nintendo Switch controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_SWITCH">SDL_HINT_JOYSTICK_HIDAPI_SWITCH</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_SWITCH"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_WII_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_WII (<c>"SDL_JOYSTICK_HIDAPI_WII"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class Wii
			{
				/// <summary>
				/// Gets a hint controlling whether the player LEDs should be lit to indicate which player is associated with a Wii controller
				/// </summary>
				/// <value>
				/// A hint controlling whether the player LEDs should be lit to indicate which player is associated with a Wii controller
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Player LEDs are not enabled</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Player LEDs are enabled (default)</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// This hint can be set anytime.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_WII_PLAYER_LED">SDL_HINT_JOYSTICK_HIDAPI_WII_PLAYER_LED</seealso>
				public static Hint PlayerLed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_WII_PLAYER_LED"); } 

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for Nintendo Wii and Wii U controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for Nintendo Wii and Wii U controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// This driver doesn't work with the dolphinbar, so the default is <c>"0"</c> for now.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_WII">SDL_HINT_JOYSTICK_HIDAPI_WII</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_WII"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_XBOX_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_XBOX (<c>"SDL_JOYSTICK_HIDAPI_XBOX"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class XBox
			{
				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for XBox controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for XBox controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default is <c>"0"</c> on Windows, otherwise the value of <see cref="UseDrivers"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_XBOX">SDL_HINT_JOYSTICK_HIDAPI_XBOX</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_XBOX"); }
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_XBOX_360 (<c>"SDL_JOYSTICK_HIDAPI_XBOX_360"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class XBox360
			{
				/// <summary>
				/// Gets a hint controlling whether the player LEDs should be lit to indicate which player is associated with an Xbox 360 controller
				/// </summary>
				/// <value>
				/// A hint controlling whether the player LEDs should be lit to indicate which player is associated with an Xbox 360 controller
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Player LEDs are not enabled</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Player LEDs are enabled (default)</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// This hint can be set anytime.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_PLAYER_LED">SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_PLAYER_LED</seealso>
				public static Hint PlayerLed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_XBOX_360_PLAYER_LED"); }

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for XBox 360 controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for XBox 360 controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="XBox.UseDriver"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_XBOX_360">SDL_HINT_JOYSTICK_HIDAPI_XBOX_360</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_XBOX_360"); }

				/// <summary>SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_WIRELESS_*</summary>
				/// <remarks>
				/// For the hint SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_WIRELESS (<c>"SDL_JOYSTICK_HIDAPI_XBOX_360_WIRELESS"</c>) see <see cref="UseDriver"/>.
				/// </remarks>
				public static class Wireless
				{
					/// <summary>
					/// Gets a hint controlling whether the HIDAPI driver for XBox 360 wireless controllers should be used
					/// </summary>
					/// <value>
					/// A hint controlling whether the HIDAPI driver for XBox 360 wireless controllers should be used
					/// </value>
					/// <remarks>
					/// <para>
					/// The hint can be set to the following values:
					/// <list type="bullet">
					///		<item>
					///			<term><c>"0"</c></term>
					///			<description>HIDAPI driver is not used</description>
					///		</item>
					///		<item>
					///			<term><c>"1"</c></term>
					///			<description>HIDAPI driver is used</description>
					///		</item>
					/// </list>
					/// </para>
					/// <para>
					/// The default value for this hint is the value of <see cref="XBox360.UseDriver"/>.
					/// </para>
					/// <para>
					/// This hint should be set before initializing joysticks and gamepads
					/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
					/// </para>
					/// </remarks>
					/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_WIRELESS">SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_WIRELESS</seealso>
					public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_XBOX_360_WIRELESS"); }
				}
			}

			/// <summary>SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE_*</summary>
			/// <remarks>
			/// For the hint SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE (<c>"SDL_JOYSTICK_HIDAPI_XBOX_ONE"</c>) see <see cref="UseDriver"/>.
			/// </remarks>
			public static class XBoxOne
			{
				/// <summary>
				/// Gets a hint controlling whether the Home button LED should be turned on when an Xbox One controller is opened
				/// </summary>
				/// <value>
				/// A hint controlling whether the Home button LED should be turned on when an Xbox One controller is opened
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>Home button LED is turned off</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>Home button LED is turned on</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// By default the Home button LED state is not changed.
				/// This hint can also be set to a floating point value between <c>0.0</c> and <c>1.0</c> which controls the brightness of the Home button LED.
				/// The default brightness is <c>0.4</c>.
				/// </para>
				/// <para>
				/// This hint can be set anytime.
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE_HOME_LED">SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE_HOME_LED</seealso>
				public static Hint HomeLed { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_XBOX_ONE_HOME_LED"); }

				/// <summary>
				/// Gets a hint controlling whether the HIDAPI driver for XBox One controllers should be used
				/// </summary>
				/// <value>
				/// A hint controlling whether the HIDAPI driver for XBox One controllers should be used
				/// </value>
				/// <remarks>
				/// <para>
				/// The hint can be set to the following values:
				/// <list type="bullet">
				///		<item>
				///			<term><c>"0"</c></term>
				///			<description>HIDAPI driver is not used</description>
				///		</item>
				///		<item>
				///			<term><c>"1"</c></term>
				///			<description>HIDAPI driver is used</description>
				///		</item>
				/// </list>
				/// </para>
				/// <para>
				/// The default value for this hint is the value of <see cref="XBox.UseDriver"/>.
				/// </para>
				/// <para>
				/// This hint should be set before initializing joysticks and gamepads
				/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
				/// </para>
				/// </remarks>
				/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE">SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE</seealso>
				public static Hint UseDriver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI_XBOX_ONE"); }
			}

			/// <summary>
			/// Gets a hint controlling whether the HIDAPI joystick drivers should be used
			/// </summary>
			/// <value>
			/// A hint controlling whether the HIDAPI joystick drivers should be used
			/// </value>
			/// <remarks>
			/// <para>
			/// The hint can be set to the following values:
			/// <list type="bullet">
			///		<item>
			///			<term><c>"0"</c></term>
			///			<description>HIDAPI drivers are not used</description>
			///		</item>
			///		<item>
			///			<term><c>"1"</c></term>
			///			<description>HIDAPI drivers are used (default)</description>
			///		</item>
			/// </list>
			/// </para>
			/// <para>
			/// This variable is the default for all drivers, but can be overridden by the hints for specific drivers.
			/// </para>
			/// <para>
			/// This hint should be set before initializing joysticks and gamepads
			/// (either by <see cref="Sdl(Sdl.BuildAction?)">initializing SDL</see> or by <see cref="Sdl.InitializeSubSystems(SubSystems)">initializing them on their own</see>).
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_HIDAPI">SDL_HINT_JOYSTICK_HIDAPI</seealso>
			public static Hint UseDrivers { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_HIDAPI"); }
		}

		/// <summary>
		/// Gets a hint controlling whether IOKit should be used for controller handling
		/// </summary>
		/// <value>
		/// A hint controlling whether IOKit should be used for controller handling
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>IOKit is not used</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>IOKit is used (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_IOKIT">SDL_HINT_JOYSTICK_IOKIT</seealso>
		public static Hint IOKit { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_IOKIT"); }

		/// <summary>SDL_HINT_JOYSTICK_LINUX_*</summary>
		public static class Linux
		{
			/// <summary>
			/// Gets a hint controlling whether to use the classic /dev/input/js* joystick interface or the newer /dev/input/event* joystick interface on Linux
			/// </summary>
			/// <value>
			/// A hint controlling whether to use the classic /dev/input/js* joystick interface or the newer /dev/input/event* joystick interface on Linux
			/// </value>
			/// <remarks>
			/// <para>
			/// The hint can be set to the following values:
			/// <list type="bullet">
			///		<item>
			///			<term><c>"0"</c></term>
			///			<description>Use /dev/input/event* (default)</description>
			///		</item>
			///		<item>
			///			<term><c>"1"</c></term>
			///			<description>Use /dev/input/js*</description>
			///		</item>
			/// </list>
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_LINUX_CLASSIC">SDL_HINT_JOYSTICK_LINUX_CLASSIC</seealso>
			public static Hint Classic { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_LINUX_CLASSIC"); }

			/// <summary>
			/// Gets a hint controlling whether joysticks on Linux adhere to their HID-defined deadzones or return unfiltered values
			/// </summary>
			/// <value>
			/// A hint controlling whether joysticks on Linux adhere to their HID-defined deadzones or return unfiltered values
			/// </value>
			/// <remarks>
			/// <para>
			/// The hint can be set to the following values:
			/// <list type="bullet">
			///		<item>
			///			<term><c>"0"</c></term>
			///			<description>Return unfiltered joystick axis values (default)</description>
			///		</item>
			///		<item>
			///			<term><c>"1"</c></term>
			///			<description>Return axis values with deadzones taken into account</description>
			///		</item>
			/// </list>
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">a controller is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_LINUX_DEADZONES">SDL_HINT_JOYSTICK_LINUX_DEADZONES</seealso>
			public static Hint Deadzones { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_LINUX_DEADZONES"); }

			/// <summary>
			/// Gets a hint controlling whether joysticks on Linux will always treat 'hat' axis inputs (ABS_HAT0X - ABS_HAT3Y) as 8-way digital hats without checking whether they may be analog
			/// </summary>
			/// <value>
			/// A hint controlling whether joysticks on Linux will always treat 'hat' axis inputs (ABS_HAT0X - ABS_HAT3Y) as 8-way digital hats without checking whether they may be analog
			/// </value>
			/// <remarks>
			/// <para>
			/// The hint can be set to the following values:
			/// <list type="bullet">
			///		<item>
			///			<term><c>"0"</c></term>
			///			<description>Only map hat axis inputs to digital hat outputs if the input axes appear to actually be digital (default)</description>
			///		</item>
			///		<item>
			///			<term><c>"1"</c></term>
			///			<description>Always handle the input axes numbered ABS_HAT0X to ABS_HAT3Y as digital hats</description>
			///		</item>
			/// </list>
			/// </para>	
			/// <para>
			/// This hint should be set before <see cref="">a controller is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_LINUX_DIGITAL_HATS">SDL_HINT_JOYSTICK_LINUX_DIGITAL_HATS</seealso>
			public static Hint DigitalHats { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_LINUX_DIGITAL_HATS"); }

			/// <summary>
			/// Gets a hint controlling whether digital hats on Linux will apply deadzones to their underlying input axes or use unfiltered values
			/// </summary>
			/// <value>
			/// A hint controlling whether digital hats on Linux will apply deadzones to their underlying input axes or use unfiltered values
			/// </value>
			/// <remarks>
			/// <para>
			/// The hint can be set to the following values:
			/// <list type="bullet">
			///		<item>
			///			<term><c>"0"</c></term>
			///			<description>Return digital hat values based on unfiltered input axis values</description>
			///		</item>
			///		<item>
			///			<term><c>"1"</c></term>
			///			<description>Return digital hat values with deadzones on the input axes taken into account (default)</description>
			///		</item>
			/// </list>
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">a controller is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_LINUX_HAT_DEADZONES">SDL_HINT_JOYSTICK_LINUX_HAT_DEADZONES</seealso>
			public static Hint HatDeadzones { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_LINUX_HAT_DEADZONES"); }
		}

		/// <summary>
		/// Gets a hint controlling whether GCController should be used for controller handling
		/// </summary>
		/// <value>
		/// A hint controlling whether GCController should be used for controller handling
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>GCController is not used</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>GCController is used (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_MFI">SDL_HINT_JOYSTICK_MFI</seealso>
		public static Hint Mfi { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_MFI"); }

		/// <summary>SDL_HINT_JOYSTICK_RAWINPUT_*</summary>
		/// <remarks>
		/// For the hint SDL_HINT_JOYSTICK_RAWINPUT (<c>"SDL_HINT_JOYSTICK_RAWINPUT"</c>) see <see cref="UseDrivers"/>.
		/// </remarks>
		public static class Rawinput
		{
			/// <summary>
			/// Gets a hint controlling whether the RAWINPUT driver should pull correlated data from XInput
			/// </summary>
			/// <value>
			/// A hint controlling whether the RAWINPUT driver should pull correlated data from XInput
			/// </value>
			/// <remarks>
			/// <para>
			/// The hint can be set to the following values:
			/// <list type="bullet">
			///		<item>
			///			<term><c>"0"</c></term>
			///			<description>RAWINPUT driver will only use data from raw input APIs</description>
			///		</item>
			///		<item>
			///			<term><c>"1"</c></term>
			///			<description>RAWINPUT driver will also pull data from XInput and Windows.Gaming.Input, providing better trigger axes, guide button presses, and rumble support for Xbox controllers (default)</description>
			///		</item>
			/// </list>
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="">a controller is opened</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_RAWINPUT_CORRELATE_XINPUT">SDL_HINT_JOYSTICK_RAWINPUT_CORRELATE_XINPUT</seealso>
			public static Hint CorrelateXInput { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_RAWINPUT_CORRELATE_XINPUT"); }

			/// <summary>
			/// Gets a hint controlling whether the RAWINPUT joystick drivers should be used for better handling XInput-capable devices
			/// </summary>
			/// <value>
			/// A hint controlling whether the RAWINPUT joystick drivers should be used for better handling XInput-capable devices
			/// </value>
			/// <remarks>
			/// <para>
			/// The hint can be set to the following values:
			/// <list type="bullet">
			///		<item>
			///			<term><c>"0"</c></term>
			///			<description>RAWINPUT drivers are not used (default)</description>
			///		</item>
			///		<item>
			///			<term><c>"1"</c></term>
			///			<description>RAWINPUT drivers are used</description>
			///		</item>
			/// </list>
			/// </para>
			/// <para>
			/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_RAWINPUT">SDL_HINT_JOYSTICK_RAWINPUT</seealso>
			public static Hint UseDrivers { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_HINT_JOYSTICK_RAWINPUT"); }
		}

		/// <summary>
		/// Gets a hint controlling whether the ROG Chakram mice should show up as joysticks
		/// </summary>
		/// <value>
		/// A hint controlling whether the ROG Chakram mice should show up as joysticks
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>ROG Chakram mice do not show up as joysticks (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>ROG Chakram mice show up as joysticks</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_ROG_CHAKRAM">SDL_HINT_JOYSTICK_ROG_CHAKRAM</seealso>
		public static Hint RogChakram { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_ROG_CHAKRAM"); }

		/// <summary>
		/// Gets a hint controlling whether a separate thread should be used for handling joystick detection and raw input messages on Windows
		/// </summary>
		/// <value>
		/// A hint controlling whether a separate thread should be used for handling joystick detection and raw input messages on Windows
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>A separate thread is not used</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>A separate thread is used for handling raw input messages (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_THREAD">SDL_HINT_JOYSTICK_THREAD</seealso>
		public static Hint Thread { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_THREAD"); }

		/// <summary>SDL_HINT_JOYSTICK_THROTTLE_*</summary>
		public static class Throttle
		{
			/// <summary>
			/// Gets a hint containing a list of throttle style controllers
			/// </summary>
			/// <value>
			/// A hint containing a list of throttle style controllers
			/// </value>
			/// <remarks>
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_THROTTLE_DEVICES">SDL_HINT_JOYSTICK_THROTTLE_DEVICES</seealso>
			public static Hint Devices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_THROTTLE_DEVICES"); }

			/// <summary>
			/// Gets a hint containing a list of devices that are not throttle style controllers
			/// </summary>
			/// <value>
			/// A hint containing a list of devices that are not throttle style controllers
			/// </value>
			/// <remarks>
			/// <para>
			/// </para>
			/// This hint will override <see cref="Devices"/> and the built in device list.
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_THROTTLE_DEVICES_EXCLUDED">SDL_HINT_JOYSTICK_THROTTLE_DEVICES_EXCLUDED</seealso>
			public static Hint DevicesExcluded { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_THROTTLE_DEVICES_EXCLUDED"); }
		}

		/// <summary>
		/// Gets a hint controlling whether Windows.Gaming.Input should be used for controller handling
		/// </summary>
		/// <value>
		/// A hint controlling whether Windows.Gaming.Input should be used for controller handling
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>WGI is not used</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>WGI is used (default)</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_WGI">SDL_HINT_JOYSTICK_WGI</seealso>
		public static Hint Wgi { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_WGI"); }

		/// <summary>SDL_HINT_JOYSTICK_WHEEL_*</summary>
		public static class Wheel
		{
			/// <summary>
			/// Gets a hint containing a list of wheel style controllers
			/// </summary>
			/// <value>
			/// A hint containing a list of wheel style controllers
			/// </value>
			/// <remarks>
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_WHEEL_DEVICES">SDL_HINT_JOYSTICK_WHEEL_DEVICES</seealso>
			public static Hint Devices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_WHEEL_DEVICES"); }

			/// <summary>
			/// Gets a hint containing a list of devices that are not wheel style controllers
			/// </summary>
			/// <value>
			/// A hint containing a list of devices that are not wheel style controllers
			/// </value>
			/// <remarks>
			/// <para>
			/// </para>
			/// This hint will override <see cref="Devices"/> and the built in device list.
			/// <para>
			/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
			/// </para>
			/// <para>
			/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
			/// </para>
			/// <para>
			/// This hint can be set anytime.
			/// </para>
			/// </remarks>
			/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_WHEEL_DEVICES_EXCLUDED">SDL_HINT_JOYSTICK_WHEEL_DEVICES_EXCLUDED</seealso>
			public static Hint DevicesExcluded { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_WHEEL_DEVICES_EXCLUDED"); }
		}

		/// <summary>
		/// Gets a hint containing a list of devices known to have all axes centered at zero
		/// </summary>
		/// <value>
		/// A hint containing a list of devices known to have all axes centered at zero
		/// </value>
		/// <remarks>
		/// <para>
		/// The format of this hint is a comma separated list of USB VID/PID pairs in hexadecimal form, e.g. <c>"0xAAAA/0xBBBB,0xCCCC/0xDDDD"</c>.
		/// </para>
		/// <para>
		/// The hint can also take the form of <c>"@file"</c>, in which case the named <c>file</c> will be loaded and interpreted as the value of the hint.
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="">a controller is opened</see>.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_JOYSTICK_ZERO_CENTERED_DEVICES">SDL_HINT_JOYSTICK_ZERO_CENTERED_DEVICES</seealso>
		public static Hint ZeroCenteredDevices { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_JOYSTICK_ZERO_CENTERED_DEVICES"); }
	}

	/// <summary>SDL_HINT_MOUSE_*</summary>
	public static class Mouse
	{
		/// <summary>
		/// Gets a hint controlling whether the mouse is captured while mouse buttons are pressed
		/// </summary>
		/// <value>
		/// A hint controlling whether the mouse is captured while mouse buttons are pressed
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>The mouse is not captured while mouse buttons are pressed</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>The mouse is captured while mouse buttons are pressed</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// By default the mouse is captured while mouse buttons are pressed.
		/// So if the mouse is dragged outside the window, the application continues to receive mouse events until the button is released.
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_MOUSE_AUTO_CAPTURE">SDL_HINT_MOUSE_AUTO_CAPTURE</seealso>
		public static Hint AutoCapture { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_MOUSE_AUTO_CAPTURE"); }

#if SDL3_4_0_OR_GREATER

		/// <summary>
		/// Gets a hint controlling whether we should scale cursors by the current display scale
		/// </summary>
		/// <value>
		/// A hint controlling whether we should scale cursors by the current display scale
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Cursors will not change size based on the display content scale (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>
		///				Cursors will automatically match the display content scale (e.g. a 2x sized cursor will be used when the window is on a monitor with 200% scale).
		///				This is currently implemented on Windows.
		///			</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint needs to be set before <see cref="">creating cursors</see>.
		/// </para>
		/// </remarks>
		public static Hint DpiScaleCursors { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_MOUSE_DPI_SCALE_CURSORS"); }

#endif

		/// <summary>
		/// Get a hint controlling whether a motion event should be generated for mouse warping in relative mode
		/// </summary>
		/// <value>
		/// A hint controlling whether a motion event should be generated for mouse warping in relative mode
		/// </value>
		/// <remarks>
		///  <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>Warping the mouse will not generate a motion event in relative mode</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>Warping the mouse will generate a motion event in relative mode</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// By default warping the mouse will not generate motion events in relative mode.
		/// This avoids the application having to filter out large relative motion due to warping.
		/// </para>
		/// <para>
		/// This hint can be set anytime.
		/// </para>
		/// </remarks>
		/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_MOUSE_RELATIVE_WARP_MOTION">SDL_HINT_MOUSE_RELATIVE_WARP_MOTION</seealso>
		public static Hint RelativeWarpMotion { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_MOUSE_RELATIVE_WARP_MOTION"); }
	}

	/// <summary>SDL_HINT_VIDEO_*</summary>
	public static class Video
	{
		/// <summary>
		/// Gets a hint controlling whether the screen saver is enabled
		/// </summary>
		/// <value>
		/// A hint controlling whether the screen saver is enabled
		/// </value>
		/// <remarks>
		/// <para>
		/// The hint can be set to the following values:
		/// <list type="bullet">
		///		<item>
		///			<term><c>"0"</c></term>
		///			<description>The screen saver is disabled (default)</description>
		///		</item>
		///		<item>
		///			<term><c>"1"</c></term>
		///			<description>The screen saver is enabled</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// This hint should be set before <see cref="Sdl(Sdl.BuildAction?)">SDL is initialized</see>.
		/// </para>
		/// </remarks>
		/// <seealso hcref="https://wiki.libsdl.org/SDL3/SDL_HINT_VIDEO_ALLOW_SCREENSAVER">SDL_HINT_VIDEO_ALLOW_SCREENSAVER</seealso>
		public static Hint AllowScreenSaver { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_VIDEO_ALLOW_SCREENSAVER"); }
	}

	/// <summary>
	/// Gets a hint that controls keycode representation in keyboard events
	/// </summary>
	/// <value>
	/// A hint that controls keycode representation in keyboard events
	/// </value>
	/// <remarks>
	/// <para>
	/// The hint is a comma separated set of options for translating keycodes in events:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"none"</c></term>
	///			<description>Keycode options are cleared, this overrides other options</description>
	///		</item>
	///		<item>
	///			<term><c>"hide_numpad"</c></term>
	///			<description>The numpad keysyms will be translated into their non-numpad versions based on the current NumLock state. For example, <see cref="Keycode.Keypad4"/> would become <see cref="Keycode._4"/> if <see cref="Keymod.NumLock"/> is set in the event modifiers, and <see cref="Keycode.Left"/> if it is unset.</description>
	///		</item>
	///		<item>
	///			<term><c>"french_numbers"</c></term>
	///			<description>The number row on French keyboards is inverted, so pressing the 1 key would yield the keycode <see cref="Keycode._1"/>, or <c>'1'</c>, instead of <see cref="Keycode.Ampersand"/>, or <c>'&amp;'</c></description>
	///		</item>
	///		<item>
	///			<term><c>"latin_letters"</c></term>
	///			<description>For keyboards using non-Latin letters, such as Russian or Thai, the letter keys generate keycodes as though it had an en_US layout. E.g. pressing the key associated with <see cref="Scancode.A"/> on a Russian keyboard would yield <c>'a'</c> instead of a Cyrillic letter.</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// The default value for this hint is <c>"french_numbers,latin_letters"</c>
	/// </para>
	/// <para>
	/// Some platforms like Emscripten only provide modified keycodes and the options are not used.
	/// </para>
	/// <para>
	/// These options do not affect the return value of <see cref="KeycodeExtensions.TryGetFromScancode(Scancode, Keymod, out Keycode)"/>() or <see cref="ScancodeExtensions.TryGetFromKeycode(Keycode, out Keymod, out Scancode)"/>(), they just apply to the keycode included in key events.
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_KEYCODE_OPTIONS">SDL_HINT_KEYCODE_OPTIONS</seealso>
	public static Hint KeycodeOptions { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_KEYCODE_OPTIONS"); }

	// TODO

	/// <summary>
	/// Gets a hint that request <see cref="App.OnIterate(Sdl)"/> to be called at a specific rate
	/// </summary>
	/// <value>
	/// A hint that request <see cref="App.OnIterate(Sdl)"/> to be called at a specific rate
	/// </value>
	/// <remarks>
	/// <para>
	/// If this hint is set to a number, it represents Hz, so <c>"60"</c> means try to iterate 60 times per second.
	/// <c>"0"</c> means to iterate as fast as possible.
	/// Negative values are illegal, but reserved, in case they are useful in a future revision of SDL.
	/// </para>
	/// <para>
	/// There are other string values that have special meaning.
	/// If set to <c>"waitevent"</c>, <see cref="App.OnIterate(Sdl)"/> will not be called until new event(s) have arrived (and been processed by <see cref="App.OnEvent(Sdl, ref Events.Event)"/>).
	/// This can be useful for apps that are completely idle except in response to input.
	/// </para>
	/// <para>
	/// This hint defaults to 0, and specifying <c><see langword="null"/></c> for the hint's value will restore the default.
	/// </para>
	/// <para>
	/// This hint can be set anytime and is allowed to be changed at anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_MAIN_CALLBACK_RATE">SDL_HINT_MAIN_CALLBACK_RATE</seealso>
	public static Hint MainCallbackRate { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_MAIN_CALLBACK_RATE"); }

	/// <summary>
	/// Gets a hint to control whether the return key on the soft keyboard should hide the soft keyboard on Android and iOS
	/// </summary>
	/// <value>
	/// A hint to control whether the return key on the soft keyboard should hide the soft keyboard on Android and iOS
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this hint controls the default value of the <c>multiline</c> argument in a call to <see cref="Window.TryStartTextInput(TextInputType?, Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)"/>.
	/// </para>
	/// <para>
	/// The hint can be set to the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"0"</c></term>
	///			<description>The return key will be handled as a key event (default)</description>
	///		</item>
	///		<item>
	///			<term><c>"1"</c></term>
	///			<description>The return key will hide the keyboard</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This hint can be set anytime.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HINT_RETURN_KEY_HIDES_IME">SDL_HINT_RETURN_KEY_HIDES_IME</seealso>
	public static Hint ReturnKeyHidesIme { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new("SDL_RETURN_KEY_HIDES_IME"); }

	// TODO
}
