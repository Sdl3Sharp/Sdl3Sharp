using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sdl3Sharp.Internal;

internal abstract class NativeInstanceIdAndPointerEqualityComparer<TId> :
	IEqualityComparer<TId>,
	IAlternateEqualityComparer<IntPtr, TId>
	where TId : notnull
{
	private readonly IEqualityComparer<TId> mIdComparer;
	private readonly TId mInvalidId;

	protected NativeInstanceIdAndPointerEqualityComparer(IEqualityComparer<TId>? idComparer = default, [DisallowNull] TId? invalidId = default!)
	{
		[DoesNotReturn]
		static void failInvalidIdArgumentNull() => throw new ArgumentNullException(nameof(invalidId), "invalidId cannot be null");

		if (invalidId is null && default(TId) is null)
		{
			failInvalidIdArgumentNull(); // in case TId is a reference type and we got passed a null reference, despite the [DisallowNull] attribute
		}

		mIdComparer = idComparer ?? EqualityComparer<TId>.Default;
		mInvalidId = invalidId!;
	}

	public TId Create(IntPtr alternate)
	{
		if (!TryGetIdFromPointer(alternate, out var id))
		{
			return mInvalidId;
		}

		return id;
	}

	public bool Equals(TId? x, TId? y) => mIdComparer.Equals(x, y);

	public bool Equals(IntPtr alternate, TId other)
	{
		if (!TryGetIdFromPointer(alternate, out var id))
		{
			return mIdComparer.Equals(mInvalidId, other);
		}

		return mIdComparer.Equals(id, other);
	}

	public int GetHashCode([DisallowNull] TId obj) => mIdComparer.GetHashCode(obj);

	public int GetHashCode(IntPtr alternate)
	{
		if (!TryGetIdFromPointer(alternate, out var id))
		{
			return mIdComparer.GetHashCode(mInvalidId);
		}

		return mIdComparer.GetHashCode(id);
	}

	protected unsafe abstract bool TryGetIdFromPointer(IntPtr pointer, [NotNullWhen(true)] out TId? id);
}
