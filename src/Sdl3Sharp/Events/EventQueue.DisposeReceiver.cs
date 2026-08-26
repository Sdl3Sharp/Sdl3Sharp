using Sdl3Sharp.Internal;
using System.Threading;

namespace Sdl3Sharp.Events;

partial class EventQueue
{
	// This type only exists because SDL clears the event queue and every adjacent to it (including event filters and watches) when it's shutting down
	private sealed class DisposeReceiver : Sdl.IDisposeReceiver
	{
		private static volatile DisposeReceiver? mInstance = null;
		private static SimpleSpinYieldLock mLock = new();

		public static void Activate()
		{
			if (mInstance is null)
			{
				mLock.Enter(0); // We need to lock here, since there is no reliable way to check and set the instance atomically without a lock
				try
				{
					if (mInstance is null)
					{
						mInstance = new();
						Sdl.TryRegisterDisposable(mInstance);
					}
				}
				finally
				{
					mLock.Exit(0);
				}
			}
		}

		private DisposeReceiver() { }

		public void DisposeFromSdl(Sdl sdl)
		{
			if (Interlocked.Exchange(ref mInstance, null) is not null) // Since we just want to reset the instance to null, we don't need to lock here
			{
				EventQueue.DisposeFromSdl();
			}
		}
	}
}
