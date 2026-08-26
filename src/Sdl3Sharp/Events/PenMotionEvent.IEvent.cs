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
		static abstract Event Create(in PenMotionEvent value);

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
		bool TryGetValue(out PenMotionEvent value);
	}
#endif

	[FieldOffset(0)] internal PenMotionEvent PMotion;

#if NET11_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static Event IUnionMembers.Create(in PenMotionEvent value) => From(in value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	readonly bool IUnionMembers.TryGetValue(out PenMotionEvent value) => TryGet(out value);
#endif
}

partial struct PenMotionEvent : IEvent<PenMotionEvent>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static bool AcceptsEventType(EventType type) => type is EventType.PenMotion;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEvent<PenMotionEvent>.TryReadFromEvent(ref readonly Event @event, out PenMotionEvent result)
	{
		if (AcceptsEventType(@event.Type))
		{
			result = @event.PMotion;
			return true;
		}

		Unsafe.SkipInit(out result);
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	readonly void IEvent<PenMotionEvent>.WriteToEvent(ref Event @event) => @event.PMotion = this;
}
