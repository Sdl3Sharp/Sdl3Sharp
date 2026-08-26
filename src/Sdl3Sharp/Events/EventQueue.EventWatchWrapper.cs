using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

partial class EventQueue
{
	private sealed class EventWatchWrapper : IDisposable
	{
		private GCHandle mHandle;

		public EventWatchWrapper()
		{
			unsafe
			{
				mHandle = GCHandle.Alloc(this, GCHandleType.Normal);

				if (!SDL_AddEventWatch(&EventWatchImpl, unchecked((void*)GCHandle.ToIntPtr(mHandle))))
				{
					mHandle.Free();
					failCouldNotAddEventWatch();
				}
			}

			[DoesNotReturn]
			static void failCouldNotAddEventWatch() => throw new SdlException("Could not add the event queue watcher");
		}

		~EventWatchWrapper() => DisposeImpl();

		public EventWatch? Watch { get; set; }

		public void Dispose()
		{
			DisposeImpl();
			GC.SuppressFinalize(this);
		}

		private unsafe void DisposeImpl()
		{
			if (mHandle.IsAllocated)
			{
				SDL_RemoveEventWatch(&EventWatchImpl, unchecked((void*)GCHandle.ToIntPtr(mHandle)));
				mHandle.Free();
			}

			Watch = null;
		}
	}
}
