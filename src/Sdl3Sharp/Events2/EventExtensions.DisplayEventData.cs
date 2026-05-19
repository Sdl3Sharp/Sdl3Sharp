using Sdl3Sharp.Video.Windowing;

namespace Sdl3Sharp.Events2;

partial class EventExtensions
{
	extension(EventRef<DisplayEventData> @event)
	{
		/// <summary>
		/// Gets or sets the display ID of the <see cref="Display"/> which changes it's state
		/// </summary>
		/// <value>
		/// The display ID of the <see cref="Display"/> which changes it's state
		/// </value>
		/// <inheritdoc cref="EventRef{TEventData}.FailTargetNull"/>
		public uint DisplayId
		{
			get => @event.Target.EventData.DisplayId;
			set => @event.Target.EventData.DisplayId = value;
		}

		/// <summary>
		/// Gets or sets the value of first event dependent data slot
		/// </summary>
		/// <value>
		/// The value of the first event dependent data slot
		/// </value>
		/// <remarks>
		/// <para>
		/// The value of this property may reflect different data semantics dependent on the actual <see cref="Event{TEventData}.Type"/>.
		/// </para>
		/// </remarks>
		/// <inheritdoc cref="EventRef{TEventData}.FailTargetNull"/>
		public int Data1
		{
			get => @event.Target.EventData.Data1;
			set => @event.Target.EventData.Data1 = value;
		}

		/// <summary>
		/// Gets or sets the value of second event dependent data slot
		/// </summary>
		/// <value>
		/// The value of the second event dependent data slot
		/// </value>
		/// <remarks>
		/// <para>
		/// The value of this property may reflect different data semantics dependent on the actual <see cref="Event{TEventData}.Type"/>.
		/// </para>
		/// </remarks>
		/// <inheritdoc cref="EventRef{TEventData}.FailTargetNull"/>
		public int Data2
		{
			get => @event.Target.EventData.Data2;
			set => @event.Target.EventData.Data2 = value;
		}
	}

	extension(EventRefReadOnly<DisplayEventData> @event)
	{
		/// <summary>
		/// Gets the display ID of the <see cref="Display"/> which changes it's state
		/// </summary>
		/// <value>
		/// The display ID of the <see cref="Display"/> which changes it's state
		/// </value>
		/// <inheritdoc cref="EventRefReadOnly{TEventData}.FailTargetNull"/>
		public uint DisplayId => @event.Target.EventData.DisplayId;

		/// <summary>
		/// Gets the value of first event dependent data slot
		/// </summary>
		/// <value>
		/// The value of the first event dependent data slot
		/// </value>
		/// <remarks>
		/// <para>
		/// The value of this property may reflect different data semantics dependent on the actual <see cref="Event{TEventData}.Type"/>.
		/// </para>
		/// </remarks>
		/// <inheritdoc cref="EventRefReadOnly{TEventData}.FailTargetNull"/>
		public int Data1 => @event.Target.EventData.Data1;

		/// <summary>
		/// Gets the value of second event dependent data slot
		/// </summary>
		/// <value>
		/// The value of the second event dependent data slot
		/// </value>
		/// <remarks>
		/// <para>
		/// The value of this property may reflect different data semantics dependent on the actual <see cref="Event{TEventData}.Type"/>.
		/// </para>
		/// </remarks>
		/// <inheritdoc cref="EventRefReadOnly{TEventData}.FailTargetNull"/>
		public int Data2 => @event.Target.EventData.Data2;
	}
}
