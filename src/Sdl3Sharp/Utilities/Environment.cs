using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Utilities;

/// <summary>
/// Represents a set of environment variables
/// </summary>
/// <remarks>
/// <para>
/// Operations on instances of this class are thread-safe, except when using the <see cref="TryGetProcessVariableUnsafe(string, out string?)"/>, <see cref="TrySetProcessVariableUnsafe(string, string, bool)"/>, and <see cref="TryUnsetProcessVariableUnsafe(string)"/> methods."/>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public sealed partial class Environment : IDisposable, Sdl.IDisposeReceiver, IEnumerable<KeyValuePair<string, string>>, IEquatable<Environment>
{
	private interface IUnsafeConstructorDispatch;

	private static readonly ConcurrentDictionary<IntPtr, WeakReference<Environment>> mKnownInstance = [];

	/// <exception cref="SdlException">Couldn't create a new <see cref="Environment"/></exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe static SDL_Environment* ValidateEnvironment(SDL_Environment* environment)
	{
		if (environment is null)
		{
			static void failCouldNotCreateEnvironment() => throw new SdlException($"Could not create a new {nameof(Environment)}");

			failCouldNotCreateEnvironment();
		}

		return environment;
	}

	private unsafe SDL_Environment* mEnvironment;

	//TODO: fix this
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private unsafe string DebuggerDisplay => mEnvironment is not null
		? string.Join(" ", this.Select(p => $"{p.Key}={p.Value}"))
		: "<Invalid>";

	private unsafe Environment(SDL_Environment* environment, bool registerWithSdl)
	{
		if (registerWithSdl)
		{
			Sdl.TryDeregisterDisposable(this); // TryRegisterDisposable cannot fail here, because we're registering a new instance
		}

		mEnvironment = environment;
	}

	/// <inheritdoc cref="ValidateEnvironment(SDL_Environment*)"/>
	private unsafe Environment(bool populateFromRuntime, IUnsafeConstructorDispatch? _) :
		this(ValidateEnvironment(SDL_CreateEnvironment(populateFromRuntime)), registerWithSdl: true)
	{
		mKnownInstance.AddOrUpdate(unchecked((IntPtr)mEnvironment), addRef, updateRef, this);

		static WeakReference<Environment> addRef(IntPtr environment, Environment newEnvironment) => new(newEnvironment);

		static WeakReference<Environment> updateRef(IntPtr environment, WeakReference<Environment> existingEnvironmentRef, Environment newEnvironment)
		{
			if (existingEnvironmentRef.TryGetTarget(out var exisitingEnvironment))
			{
#pragma warning disable IDE0079
#pragma warning disable CA1816
				GC.SuppressFinalize(exisitingEnvironment);
#pragma warning restore CA1816
#pragma warning restore IDE0079
				exisitingEnvironment.Dispose(forget: false, deregisterFromSdl: true);
			}

			existingEnvironmentRef.SetTarget(newEnvironment);

			return existingEnvironmentRef;
		}
	}

	/// <summary>
	/// Creates a new <see cref="Environment">set of environment variables</see>
	/// </summary>
	/// <param name="populateFromRuntime">A value indicating whether the newly created <see cref="Environment"/> should be initialized with the environment variables from the C runtime environment</param>
	/// <remarks>
	/// <para>
	/// If <paramref name="populateFromRuntime"/> is set to <c><see langword="false"/></c> (its default value), it is safe to call this constructor from any thread,
	/// otherwise it is only safe to call, if there are no other threads that are calling <see cref="TrySetProcessVariableUnsafe(string, string, bool)"/> or <see cref="TryUnsetProcessVariableUnsafe(string)"/>.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Environment(bool, IUnsafeConstructorDispatch?)"/>
	public Environment(bool populateFromRuntime = false) :
#pragma warning disable IDE0034 // for the sake of explicitness
		this(populateFromRuntime, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034 
	{ }

	/// <summary>
	/// Gets the <see cref="Environment">set of environment variables</see> for the current process
	/// </summary>
	/// <value>
	/// The <see cref="Environment">set of environment variables</see> for the current process, if those could get successfully retrieved; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// This <see cref="Environment">set of environment variables</see> is initialized at application start and is not affected by external calls to modify process environments (e.g. <c>setenv()</c> or <c>unsetenv()</c>) after that point.
	/// </para>
	/// <para>
	/// To modify this <see cref="Environment">set of environment variables</see> use <see cref="TrySetVariable(string, string, bool)"/> or <see cref="TryUnsetVariable(string)"/>.
	/// Changes made in this way will not persist outside of SDL, and especially not after <see cref="Sdl.Dispose()">SDL is shut down</see>.
	/// </para>
	/// <para>
	/// If you want for changes to persist in the C runtime environment after <see cref="Sdl.Dispose()">SDL is shut down</see>, use <see cref="TrySetProcessVariableUnsafe(string, string, bool)"/> or <see cref="TryUnsetProcessVariableUnsafe(string)"/>.
	/// </para>
	/// </remarks>
	public static Environment? ProcessEnvironment
	{
		get
		{
			unsafe
			{
				TryGetOrCreate(SDL_GetEnvironment(), out var result,
					registerWithSdl: false // we shouldn't register the process environment as an Sdl.IDisposeReceiver, while it's true that changes to the process environment will not persist after SDL is shut down, the process environment instance can still outlive SDL
				);

				return result;
			}
		}
	}

	/// <inheritdoc/>
	~Environment() => Dispose(forget: true, deregisterFromSdl: true);

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(forget: true, deregisterFromSdl: true);
	}

	void Sdl.IDisposeReceiver.DisposeFromSdl(Sdl sdl)
	{
#pragma warning disable IDE0079
#pragma warning disable CA1816
		GC.SuppressFinalize(this);
#pragma warning restore CA1816
#pragma warning restore IDE0079
		Dispose(forget: true, deregisterFromSdl: false);
	}

	private unsafe void Dispose(bool forget, bool deregisterFromSdl)
	{
		if (mEnvironment is not null)
		{
			if (deregisterFromSdl)
			{
				Sdl.TryDeregisterDisposable(this);
			}

			SDL_DestroyEnvironment(mEnvironment);

			if (forget)
			{
				mKnownInstance.TryRemove(unchecked((IntPtr)mEnvironment), out _);
			}

			mEnvironment = null;
		}
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)]object? obj) => Equals(obj as Environment);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Environment? other)
	{
		unsafe
		{
			return other is { mEnvironment: var otherPtr } && mEnvironment == otherPtr;
		}
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		unsafe
		{
			return unchecked((IntPtr)mEnvironment).GetHashCode();
		}
	}

	internal unsafe static bool TryGetOrCreate(SDL_Environment* environment, [NotNullWhen(true)] out Environment? result, bool registerWithSdl = true)
	{
		if (environment is null)
		{
			result = null;
			return false;
		}

		var environmentRef = mKnownInstance.GetOrAdd(unchecked((IntPtr)environment), createRef, registerWithSdl);

		if (!environmentRef.TryGetTarget(out result))
		{
			environmentRef.SetTarget(result = create(environment, registerWithSdl));
		}

		return true;

		static WeakReference<Environment> createRef(IntPtr environment, bool registerWithSdl) => new(create(unchecked((SDL_Environment*)environment), registerWithSdl));

		static Environment create(SDL_Environment* environment, bool registerWithSdl) => new(environment, registerWithSdl);
	}

	/// <summary>
	/// Tries to enumerator the process environment variables
	/// </summary>
	/// <param name="enumerator">The resulting <see cref="Enumerator"/> to enumerate the process environment variables, when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c> if an <see cref="Enumerator"/> for the process environment variables were successfully created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Use the resulting <paramref name="enumerator"/> to enumerate the process environment variables.
	/// </para>
	/// </remarks>
	public static bool TryGetProcessEnumerator([NotNullWhen(true)] out Enumerator? enumerator)
	{
		unsafe
		{
			if (SDL_GetEnvironment() is var environmentPtr && environmentPtr is not null
			 && SDL_GetEnvironmentVariables(environmentPtr) is var array && array is not null)
			{
				enumerator = new Enumerator(array);
				return true;
			}

			enumerator = null;
			return false;
		}
	}

	/// <summary>
	/// Tries to get the value of a process environment variable
	/// </summary>
	/// <param name="name">The name of the variable to get</param>
	/// <param name="value">The value of the environment variable, when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="default"/>(<see cref="string"/>?)</c></param>
	/// <returns><c><see langword="true"/></c> if the process environment variable exists and its value could get retrieved successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method uses SDL's cached copy of the process environment and therefore is thread-safe.
	/// </para>
	/// <para>
	/// Alternatively to this method, you could use <see cref="TryGetVariable(string, out string?)"/> on the value of the <see cref="ProcessEnvironment"/> property instead.
	/// </para>
	/// </remarks>
	public static bool TryGetProcessVariable(string name, [NotNullWhen(true)] out string? value)
	{
		unsafe
		{
			var nameUtf8= Utf8StringMarshaller.ConvertToUnmanaged(name);

			try
			{
				if (SDL_getenv(nameUtf8) is var valuePtr && valuePtr is not null)
				{
					value = Utf8StringMarshaller.ConvertToManaged(valuePtr);
					return value is not null;
				}
			}
			finally
			{
				Utf8StringMarshaller.Free(nameUtf8);
			}

			value = default;
			return false;
		}
	}

	/// <summary>
	/// Tries to get the value of a process environment variable
	/// </summary>
	/// <param name="name">The name of the variable to get</param>
	/// <param name="value">The value of the environment variable, when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="default"/>(<see cref="string"/>?)</c></param>
	/// <returns><c><see langword="true"/></c> if the process environment variable exists and its value could get retrieved successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method bypasses SDL's cached copy of the process environment and therefore is <em>not</em> thread-safe.
	/// </para>
	/// </remarks>
	public static bool TryGetProcessVariableUnsafe(string name, [NotNullWhen(true)] out string? value)
	{
		unsafe
		{
			var nameUtf8= Utf8StringMarshaller.ConvertToUnmanaged(name);

			try
			{
				if (SDL_getenv_unsafe(nameUtf8) is var valuePtr && valuePtr is not null)
				{
					value = Utf8StringMarshaller.ConvertToManaged(valuePtr);
					return value is not null;
				}
			}
			finally
			{
				Utf8StringMarshaller.Free(nameUtf8);
			}

			value = default;
			return false;
		}
	}

	/// <summary>
	/// Tries to set a process environment variable
	/// </summary>
	/// <param name="name">The name of the variable to set</param>
	/// <param name="value">The value of the environment variable to set to</param>
	/// <param name="overwrite">
	/// Indicates whether the value an existing environment variable should be overwritten.
	/// If set to <c><see langword="true"/></c>, the value of the environment variable will be set to the given <paramref name="value"/>, even if the variable already exists;
	/// otherwise, if set to <c><see langword="false"/></c>, an existing environment variable will not be changed while this method will still return successfully.
	/// </param>
	/// <returns><c><see langword="true"/></c> if the process environment variable was successfully set to <paramref name="value"/>, or if <paramref name="overwrite"/> was set to <c><see langword="false"/></c> and the environment variable already existed; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is <em>not</em> thread-safe, consider using <see cref="TrySetVariable(string, string, bool)"/> on the value of the <see cref="ProcessEnvironment"/> property instead.
	/// </para>
	/// </remarks>
	public static bool TrySetProcessVariableUnsafe(string name, string value, bool overwrite = true)
	{
		unsafe
		{
			var nameUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(name);
			var valueUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(value);

			try
			{
				return SDL_setenv_unsafe(nameUtf8, valueUtf8, overwrite ? 1 : 0) is 0;
			}
			finally
			{
				Utf8StringMarshaller.Free(valueUtf8);
				Utf8StringMarshaller.Free(nameUtf8);
			}
		}
	}

	/// <summary>
	/// Tries to clear a process environment variable (remove it from the environment)
	/// </summary>
	/// <param name="name">The name of the variable to clear</param>
	/// <returns><c><see langword="true"/></c> if the process environment variable was successfully cleared; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is <em>not</em> thread-safe, consider using <see cref="TryUnsetVariable(string)"/> on the value of the <see cref="ProcessEnvironment"/> property instead.
	/// </para>
	/// </remarks>
	public static bool TryUnsetProcessVariableUnsafe(string name)
	{
		unsafe
		{
			var nameUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(name);

			try
			{
				return SDL_unsetenv_unsafe(nameUtf8) is 0;
			}
			finally
			{
				Utf8StringMarshaller.Free(nameUtf8);
			}
		}
	}

	/// <summary>
	/// Tries to get the value of an environment variable in the current <see cref="Environment"/>
	/// </summary>
	/// <param name="name">The name of the variable to get</param>
	/// <param name="value">The value of the environment variable, when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="default"/>(<see cref="string"/>?)</c></param>
	/// <returns><c><see langword="true"/></c> if the environment variable exists in the current <see cref="Environment"/> and its value could get retrieved successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public bool TryGetVariable(string name, [NotNullWhen(true)] out string? value)
	{
		unsafe
		{
			var nameUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(name);

			try
			{
				if (SDL_GetEnvironmentVariable(mEnvironment, nameUtf8) is var valuePtr && valuePtr is not null)
				{
					value = Utf8StringMarshaller.ConvertToManaged(valuePtr);
					return value is not null;
				}
			}
			finally
			{
				Utf8StringMarshaller.Free(nameUtf8);
			}

			value = default;
			return false;
		}
	}

	/// <summary>
	/// Tries to set an environment variable in the current <see cref="Environment"/>
	/// </summary>
	/// <param name="name">The name of the variable to set</param>
	/// <param name="value">The value of the environment variable to set to</param>
	/// <param name="overwrite">
	/// Indicates whether the value an existing environment variable should be overwritten.
	/// If set to <c><see langword="true"/></c>, the value of the environment variable will be set to the given <paramref name="value"/>, even if the variable already exists;
	/// otherwise, if set to <c><see langword="false"/></c>, an existing environment variable will not be changed while this method will still return successfully.
	/// </param>
	/// <returns><c><see langword="true"/></c> if the environment variable in the current <see cref="Environment"/> was successfully set to <paramref name="value"/>, or if <paramref name="overwrite"/> was set to <c><see langword="false"/></c> and the environment variable already existed; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public bool TrySetVariable(string name, string value, bool overwrite = true)
	{
		unsafe
		{
			var nameUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(name);
			var valueUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(value);

			try
			{
				return SDL_SetEnvironmentVariable(mEnvironment, nameUtf8, valueUtf8, overwrite);
			}
			finally
			{				
				Utf8StringMarshaller.Free(valueUtf8);
				Utf8StringMarshaller.Free(nameUtf8);
			}
		}
	}

	/// <summary>
	/// Tries to clear an environment variable in the current <see cref="Environment"/> (remove it from the environment)
	/// </summary>
	/// <param name="name">The name of the variable to clear</param>
	/// <returns><c><see langword="true"/></c> if the environment variable in the current <see cref="Environment"/> was successfully cleared; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public bool TryUnsetVariable(string name)
	{
		unsafe
		{
			var nameUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(name);

			try
			{
				return SDL_UnsetEnvironmentVariable(mEnvironment, nameUtf8);
			}
			finally
			{
				Utf8StringMarshaller.Free(nameUtf8);
			}
		}
	}
}
