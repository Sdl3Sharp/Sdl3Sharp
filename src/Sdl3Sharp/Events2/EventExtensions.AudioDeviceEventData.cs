namespace Sdl3Sharp.Events2;

partial class EventExtensions
{
	extension(EventRef<AudioDeviceEventData> @event)
	{
		/// <summary>
		/// Gets or sets the audio device ID for the <see cref="AudioDevice"/> being <see cref="EventType.AudioDeviceAdded">added</see>, <see cref="EventType.AudioDeviceRemoved">removed</see>, or <see cref="EventType.AudioDeviceFormatChanged">changed</see>
		/// </summary>
		/// <value>
		/// The audio device IDfor the <see cref="AudioDevice"/> being <see cref="EventType.AudioDeviceAdded">added</see>, <see cref="EventType.AudioDeviceRemoved">removed</see>, or <see cref="EventType.AudioDeviceFormatChanged">changed</see>
		/// </value>
		/// <inheritdoc cref="EventRef{TEventData}.FailTargetNull"/>
		public uint AudioDeviceId
		{
			get => @event.Target.EventData.AudioDeviceId;
			set => @event.Target.EventData.AudioDeviceId = value;
		}

		/// <summary>
		/// Gets or sets a value indicating if the <see cref="get_AudioDeviceId(EventRef{AudioDeviceEventData})">specific audio device</see> is a recording device or a playback device
		/// </summary>
		/// <value>
		/// A value indicating if the <see cref="get_AudioDeviceId(EventRef{AudioDeviceEventData})">specific audio device</see> is a recording device (when <c><see langword="true"/></c>) or a playback device (when <c><see langword="false"/></c>)
		/// </value>
		/// <inheritdoc cref="EventRef{TEventData}.FailTargetNull"/>
		public bool IsRecordingDevice
		{
			get => @event.Target.EventData.IsRecordingDevice;
			set => @event.Target.EventData.IsRecordingDevice = value;
		}
	}

	extension(EventRefReadOnly<AudioDeviceEventData> @event)
	{
		/// <summary>
		/// Gets the audio device ID for the <see cref="AudioDevice"/> being <see cref="EventType.AudioDeviceAdded">added</see>, <see cref="EventType.AudioDeviceRemoved">removed</see>, or <see cref="EventType.AudioDeviceFormatChanged">changed</see>
		/// </summary>
		/// <value>
		/// The audio device IDfor the <see cref="AudioDevice"/> being <see cref="EventType.AudioDeviceAdded">added</see>, <see cref="EventType.AudioDeviceRemoved">removed</see>, or <see cref="EventType.AudioDeviceFormatChanged">changed</see>
		/// </value>
		/// <inheritdoc cref="EventRefReadOnly{TEventData}.FailTargetNull"/>
		public uint AudioDeviceId => @event.Target.EventData.AudioDeviceId;

		/// <summary>
		/// Gets a value indicating if the <see cref="get_AudioDeviceId(EventRefReadOnly{AudioDeviceEventData})">specific audio device</see> is a recording device or a playback device
		/// </summary>
		/// <value>
		/// A value indicating if the <see cref="get_AudioDeviceId(EventRefReadOnly{AudioDeviceEventData})">specific audio device</see> is a recording device (when <c><see langword="true"/></c>) or a playback device (when <c><see langword="false"/></c>)
		/// </value>
		/// <inheritdoc cref="EventRefReadOnly{TEventData}.FailTargetNull"/>
		public bool IsRecordingDevice => @event.Target.EventData.IsRecordingDevice;
	}
}
