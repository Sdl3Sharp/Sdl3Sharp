using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

partial struct Event
{
#if NET11_0_OR_GREATER
	partial interface IUnionMembers
	{
		/// <summary>
		/// Creates a new instance of the union type from the given value
		/// </summary>
		/// <param name="value">The value to create the union type from</param>
		/// <returns>The new instance of the union type created from the given value</returns>
		static abstract Event Create(in DisplayEvent value);

		/// <summary>
		/// Tries to get the value of the union type as the given type
		/// </summary>
		/// <param name="value">The value of the union type as the given type, if the union type is of that type</param>
		/// <returns><c><see langword="true"/></c>, if the union type is of the given type and the value was successfully retrieved; otherwise, <c><see langword="false"/></c></returns>
		/// <remarks>
		/// <para>
		/// In the case of the <see cref="Event"/> union type, this method will leave <paramref name="value"/> uninitialized if it returns <c><see langword="false"/></c>.
		/// It is the caller's responsibility to ensure that <paramref name="value"/> is not used in that case.
		/// </para>
		/// </remarks>
		bool TryGetValue(out DisplayEvent value);
	}
#endif

	[FieldOffset(0)] internal DisplayEvent Display;

#if NET11_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static Event IUnionMembers.Create(in DisplayEvent value) => From(in value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	readonly bool IUnionMembers.TryGetValue(out DisplayEvent value) => TryGet(out value);
#endif
}

partial struct DisplayEvent : IEvent<DisplayEvent>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#pragma warning disable CS0618 // We can use them here, that's what they're there for
	private static bool AcceptsEventType(EventType type) => type is >= EventType.DisplayFirst and <= EventType.DisplayLast;
#pragma warning restore CS0618

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEvent<DisplayEvent>.TryReadFromEvent(ref readonly Event @event, out DisplayEvent result)
	{
		if (AcceptsEventType(@event.Type))
		{
			result = @event.Display;
			return true;
		}

		Unsafe.SkipInit(out result);
		return false;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	readonly void IEvent<DisplayEvent>.WriteToEvent(ref Event @event) => @event.Display = this;
}
