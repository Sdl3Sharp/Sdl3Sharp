using Sdl3Sharp.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Sdl3Sharp.Input;

partial class Sensor
{
	private sealed class IdEqualityComparer : NativeInstanceIdAndPointerEqualityComparer<uint>
	{
		public static readonly IdEqualityComparer Instance = new();

		private IdEqualityComparer() : base(invalidId: 0) { }

		protected unsafe override bool TryGetIdFromPointer(nint pointer, [NotNullWhen(true)] out uint id)
		{
			id = SDL_GetSensorID((SDL_Sensor*)pointer);
			return id is not 0;
		}
	}
}
