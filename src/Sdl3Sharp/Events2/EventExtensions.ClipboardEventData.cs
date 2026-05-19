namespace Sdl3Sharp.Events2;

partial class EventExtensions
{
	extension(EventRef<ClipboardEventData> @event)
	{
		/// <summary>
		/// Gets or sets a value indicating whether SDL "owns" the clipboard, meaning it's an internal update event
		/// </summary>
		/// <value>
		/// A value indicating whether SDL "owns" the clipboard, meaning it's an internal update event
		/// </value>
		/// <inheritdoc cref="EventRef{TEventData}.FailTargetNull"/>
		public bool IsOwned
		{
			get => @event.Target.EventData.IsOwned;
			set => @event.Target.EventData.IsOwned = value;
		}
		/// <summary>
		/// Gets a list of currently available mime types
		/// </summary>
		/// <value>
		/// A list of currently available mime types
		/// </value>
		/// <remarks>
		/// <para>
		/// Reading this property can be very expensive, you should consider caching it's value.
		/// </para>
		/// </remarks>
		/// <inheritdoc cref="EventRef{TEventData}.FailTargetNull"/>
		public string[] MimeTypes => @event.Target.EventData.MimeTypes;
	}

	extension(EventRefReadOnly<ClipboardEventData> @event)
	{
		/// <summary>
		/// Gets a value indicating whether SDL "owns" the clipboard, meaning it's an internal update event
		/// </summary>
		/// <value>
		/// A value indicating whether SDL "owns" the clipboard, meaning it's an internal update event
		/// </value>
		/// <inheritdoc cref="EventRefReadOnly{TEventData}.FailTargetNull"/>
		public bool IsOwned => @event.Target.EventData.IsOwned;

		/// <summary>
		/// Gets a list of currently available mime types
		/// </summary>
		/// <value>
		/// A list of currently available mime types
		/// </value>
		/// <remarks>
		/// <para>
		/// Reading this property can be very expensive, you should consider caching it's value.
		/// </para>
		/// </remarks>
		/// <inheritdoc cref="EventRefReadOnly{TEventData}.FailTargetNull"/>
		public string[] MimeTypes => @event.Target.EventData.MimeTypes;
	}
}
