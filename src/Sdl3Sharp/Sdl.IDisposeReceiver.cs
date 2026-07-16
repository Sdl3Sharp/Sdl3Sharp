using Sdl3Sharp.Internal;
using System;
using System.Collections.Concurrent;

namespace Sdl3Sharp;

partial class Sdl
{
	internal interface IDisposeReceiver
	{
		void DisposeFromSdl(Sdl sdl);
	}

	private static readonly ConcurrentDictionary<WeakReference<IDisposeReceiver>, byte> mRegisteredDisposeReceivers = new(WeakReferenceEqualityComparer<IDisposeReceiver>.Instance);

	internal static bool TryDeregisterDisposable(IDisposeReceiver disposeReceiver)
		// for performance sake, we don't lock here
		// the worst that can happen are some dangling references, which will get eventually collected by the GC
		=> mRegisteredDisposeReceivers.TryRemove(new(disposeReceiver), out _);

	internal static bool TryRegisterDisposable(IDisposeReceiver disposeReceiver)
		=> new WeakReference<IDisposeReceiver>(disposeReceiver) switch
		{
			var disposeReceiverReference
				=> mRegisteredDisposeReceivers.TryAdd(disposeReceiverReference, default)
				|| mRegisteredDisposeReceivers.ContainsKey(disposeReceiverReference)
		};
}
