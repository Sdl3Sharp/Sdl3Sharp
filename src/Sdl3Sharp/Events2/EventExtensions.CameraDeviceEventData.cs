namespace Sdl3Sharp.Events2;

partial class EventExtensions
{
	extension(EventRef<CameraDeviceEventData> @event)
	{
		/// <summary>
		/// Gets or sets the camera device ID for the <see cref="Camera"/> being <see cref="EventType.CameraDeviceAdded">added</see>, <see cref="EventType.CameraDeviceRemoved">removed</see>, <see cref="EventType.CameraDeviceApproved">approved</see>, or <see cref="EventType.CameraDeviceDenied">denied</see>
		/// </summary>
		/// <value>
		/// The camera device ID for the <see cref="Camera"/> being <see cref="EventType.CameraDeviceAdded">added</see>, <see cref="EventType.CameraDeviceRemoved">removed</see>, <see cref="EventType.CameraDeviceApproved">approved</see>, or <see cref="EventType.CameraDeviceDenied">denied</see>
		/// </value>
		/// <inheritdoc cref="EventRef{TEventData}.FailTargetNull"/>
		public uint CameraId
		{
			get => @event.Target.EventData.CameraId;
			set => @event.Target.EventData.CameraId = value;
		}
	}

	extension(EventRefReadOnly<CameraDeviceEventData> @event)
	{
		/// <summary>
		/// Gets the camera device ID for the <see cref="Camera"/> being <see cref="EventType.CameraDeviceAdded">added</see>, <see cref="EventType.CameraDeviceRemoved">removed</see>, <see cref="EventType.CameraDeviceApproved">approved</see>, or <see cref="EventType.CameraDeviceDenied">denied</see>
		/// </summary>
		/// <value>
		/// The camera device ID for the <see cref="Camera"/> being <see cref="EventType.CameraDeviceAdded">added</see>, <see cref="EventType.CameraDeviceRemoved">removed</see>, <see cref="EventType.CameraDeviceApproved">approved</see>, or <see cref="EventType.CameraDeviceDenied">denied</see>
		/// </value>
		/// <inheritdoc cref="EventRefReadOnly{TEventData}.FailTargetNull"/>
		public uint CameraId => @event.Target.EventData.CameraId;
	}
}
