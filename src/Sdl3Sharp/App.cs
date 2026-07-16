using Sdl3Sharp.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;

namespace Sdl3Sharp;

/// <summary>
/// A base class for an application's lifetime model
/// </summary>
public abstract partial class App(bool alwaysCallOnQuit = false)
{
	/// <summary>Gets <see cref="AppResult.Continue"/></summary>
	/// <value><see cref="AppResult.Continue"/></value>
	/// <remarks>
	/// <para>
	/// The value of this property is <see cref="AppResult.Continue"/>.
	/// This property is meant to be a convenient shorthand for use by types inheriting from <see cref="App"/>.
	/// </para>
	/// </remarks>
	protected static AppResult Continue => AppResult.Continue;

	/// <summary>Gets <see cref="AppResult.Success"/></summary>
	/// <value><see cref="AppResult.Success"/></value>
	/// <remarks>
	/// <para>
	/// The value of this property is <see cref="AppResult.Success"/>.
	/// This property is meant to be a convenient shorthand for use by types inheriting from <see cref="App"/>.
	/// </para>
	/// </remarks>
	protected static AppResult Success => AppResult.Success;

	/// <summary>Gets <see cref="AppResult.Failure"/></summary>
	/// <value><see cref="AppResult.Failure"/></value>
	/// <remarks>
	/// <para>
	/// The value of this property is <see cref="AppResult.Failure"/>.
	/// This property is meant to be a convenient shorthand for use by types inheriting from <see cref="App"/>.
	/// </para>
	/// </remarks>
	protected static AppResult Failure => AppResult.Failure;

	/// <summary>
	/// The initial entry point for an <see cref="App"/>'s execution
	/// </summary>
	/// <param name="args">A collection of arguments passed to the <see cref="App"/> when it was started, which may have been processed by SDL before being passed to this method</param>
	/// <returns>
	/// How to proceed with the operation of this <see cref="App"/>'s execution:
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="AppResult.Failure"/></term>
	///			<description>Terminate the execution with an error</description>
	///		</item>
	///		<item>
	///			<term><see cref="AppResult.Success"/></term>
	///			<description>Terminate the execution with success</description>
	///		</item>
	///		<item>
	///			<term><see cref="AppResult.Continue"/></term>
	///			<description>Continue with the execution</description>
	///		</item>
	/// </list>
	/// </returns>
	/// <remarks>
	/// <para>
	/// This method is called once, at the start of the <see cref="App"/>'s execution.
	/// It should initialize whatever is necessary, possibly create windows and open audio devices, etc.
	/// </para>
	/// <para>
	/// This method should not go into an infinite loop; it should do any one-time setup it requires and then return.
	/// </para>
	/// <para>
	/// The <paramref name="args"/> parameter contains almost the same collection of arguments as the caller passed to <see cref="App.Run(ReadOnlySpan{string})"/> or <see cref="App.Run(IEnumerable{string})"/>.
	/// Note that SDL may further process those arguments between the calls to those methods and the call to this method (sometimes depending on the platform).
	/// You should treat those arguments as you normally would main entry point arguments.
	/// </para>
	/// <para>
	/// If an exception is <see langword="throw"/>n during the execution of this method, <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> will be called with the exception information.
	/// You can <see langword="override"/> <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> to handle the exception and possibly recover from it.
	/// The return value of such a call to <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> will be used as the return value of this method and execution will proceed as normal.
	/// If you don't <see langword="override"/> <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/>, the default behavior is to re<see langword="throw"/> the exception,
	/// which, depending on whether you specified to always call <see cref="OnQuit(AppResult)"/> or not when creating this <see cref="App"/> instance, will either call that method to perform any necessary cleanup or not,
	/// and then terminate the <see cref="App"/>'s execution while the exception is still being uncaught.
	/// </para>
	/// <para>
	/// If this method returns <see cref="AppResult.Continue"/>, the <see cref="App"/>'s execution will proceed to normal operation, and will begin receiving repeated calls to <see cref="OnIterate()"/> and <see cref="OnEvent(ref Event)"/> for the rest of the <see cref="App"/>'s execution.
	/// If this method returns <see cref="AppResult.Failure"/>, <see cref="OnQuit(AppResult)"/> will be called immediately and terminate the <see cref="App"/>'s execution with a return value that can be used as an exit code for program that reports an error to the platform.
	/// If it returns <see cref="AppResult.Success"/>, <see cref="OnQuit(AppResult)"/> will be called immediately and terminate the <see cref="App"/>'s execution with a return value that can be used as an exit code for the program that reports success to the platform.
	/// </para>
	/// <para>
	/// This method is called by SDL on the main thread.
	/// </para>
	/// </remarks>
	protected virtual AppResult OnInitialize(string[] args) => Continue;

	/// <summary>
	/// The iteration entry point for an <see cref="App"/>'s execution
	/// </summary>
	/// <returns>
	/// How to proceed with the operation of this <see cref="App"/>'s execution:
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="AppResult.Failure"/></term>
	///			<description>Terminate the execution with an error</description>
	///		</item>
	///		<item>
	///			<term><see cref="AppResult.Success"/></term>
	///			<description>Terminate the execution with success</description>
	///		</item>
	///		<item>
	///			<term><see cref="AppResult.Continue"/></term>
	///			<description>Continue with the execution</description>
	///		</item>
	/// </list>
	/// </returns>
	/// <remarks>
	/// <para>
	/// This method is called repeatedly after <see cref="OnInitialize(string[])"/> returned <see cref="AppResult.Continue"/>.
	/// The method should operate as a single iteration the <see cref="App"/>'s primary loop; it should update whatever state it needs and draw a new frame of video, usually.
	/// </para>
	/// <para>
	/// On some platforms, this method will be called at the refresh rate of the display (which might change during the lifetime of your app!).
	/// There are no promises made about what frequency this method might run at.
	/// But you can set an upper limit on the frequency of this method being called by setting the <see cref="Hint"/> <see cref="Hint.MainCallbackRate"/>.
	/// SDL will try its best to call this method at that given frequency, but it is not guaranteed to be exact.
	/// You should use the <see cref="Timing.Timer"/> functionality (e.g. <see cref="Timing.Timer.MillisecondTicks"/>) if you need to see how much time has passed since the last iteration.
	/// </para>
	/// <para>
	/// There is no need to process events during this method; events will be send to <see cref="OnEvent(ref Event)"/> as they arrive, and in most cases the event queue will be empty when this method runs anyhow.
	/// </para>
	/// <para>
	/// This method should not go into an infinite loop; it should do one iteration of whatever it needs to do and return.
	/// </para>
	/// <para>
	/// If an exception is <see langword="throw"/>n during the execution of this method, <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> will be called with the exception information.
	/// You can <see langword="override"/> <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> to handle the exception and possibly recover from it.
	/// The return value of such a call to <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> will be used as the return value of this method and execution will proceed as normal.
	/// If you don't <see langword="override"/> <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/>, the default behavior is to re<see langword="throw"/> the exception,
	/// which, depending on whether you specified to always call <see cref="OnQuit(AppResult)"/> or not when creating this <see cref="App"/> instance, will either call that method to perform any necessary cleanup or not,
	/// and then terminate the <see cref="App"/>'s execution while the exception is still being uncaught.
	/// </para>
	/// <para>
	/// If this method returns <see cref="AppResult.Continue"/>, the <see cref="App"/>'s execution will continue normal operation, receiving repeated calls to <see cref="OnIterate()"/> and <see cref="OnEvent(ref Event)"/> for the rest of the <see cref="App"/>'s execution.
	/// If this method returns <see cref="AppResult.Failure"/>, <see cref="OnQuit(AppResult)"/> will be called immediately and terminate the <see cref="App"/>'s execution with a return value that can be used as an exit code for program that reports an error to the platform.
	/// If it returns <see cref="AppResult.Success"/>, <see cref="OnQuit(AppResult)"/> will be called immediately and terminate the <see cref="App"/>'s execution with a return value that can be used as an exit code for the program that reports success to the platform.
	/// </para>
	/// <para>
	/// This method is called by SDL on the main thread.
	/// </para>
	/// </remarks>
	protected virtual AppResult OnIterate() => Success;

	/// <summary>
	/// The event handling entry point for an <see cref="App"/>'s execution
	/// </summary>
	/// <param name="event">A reference to the newly arrived event to examine</param>
	/// <returns>
	/// How to proceed with the operation of this <see cref="App"/>'s execution:
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="AppResult.Failure"/></term>
	///			<description>Terminate the execution with an error</description>
	///		</item>
	///		<item>
	///			<term><see cref="AppResult.Success"/></term>
	///			<description>Terminate the execution with success</description>
	///		</item>
	///		<item>
	///			<term><see cref="AppResult.Continue"/></term>
	///			<description>Continue with the execution</description>
	///		</item>
	/// </list>
	/// </returns>
	/// <remarks>
	/// <para>
	/// This method is called alongside <see cref="OnInitialize(string[])"/> returning <see cref="AppResult.Continue"/>.
	/// It is called once for each new event.
	/// </para>
	/// <para>
	/// There is (currently) no guarantee about what thread this will be called from; whatever thread pushes an event onto SDL's event queue will trigger a call to this method.
	/// SDL is responsible for pumping the event queue between each call to <see cref="OnIterate()"/>, so in normal operation one should only get events in a serial fashion,
	/// but be careful if you have a thread that explicitly calls <see cref="Sdl.TryPushEvent(in Event)"/>.
	/// SDL itself will push events to the queue on the main thread.
	/// </para>
	/// <para>
	/// <see cref="Event"/>s sent to this method by reference are not owned (by you), nor are those references required to be alive after the call to this method;
	/// if you need to save the data, you should copy it!
	/// </para>
	/// <para>
	/// This method should not go into an infinite loop; it should handle the provided event appropriately and return.
	/// </para>
	/// <para>
	/// If an exception is <see langword="throw"/>n during the execution of this method, <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> will be called with the exception information.
	/// You can <see langword="override"/> <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> to handle the exception and possibly recover from it.
	/// The return value of such a call to <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> will be used as the return value of this method and execution will proceed as normal.
	/// If you don't <see langword="override"/> <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/>, the default behavior is to re<see langword="throw"/> the exception,
	/// which, depending on whether you specified to always call <see cref="OnQuit(AppResult)"/> or not when creating this <see cref="App"/> instance, will either call that method to perform any necessary cleanup or not,
	/// and then terminate the <see cref="App"/>'s execution while the exception is still being uncaught.
	/// </para>
	/// <para>
	/// If this method returns <see cref="AppResult.Continue"/>, the <see cref="App"/>'s execution will continue normal operation, receiving repeated calls to <see cref="OnIterate()"/> and <see cref="OnEvent(ref Event)"/> for the execution time of the <see cref="App"/>.
	/// If this method returns <see cref="AppResult.Failure"/>, <see cref="OnQuit(AppResult)"/> will be called and terminate the <see cref="App"/>'s execution with a return value that can be used as an exit code for program that reports an error to the platform.
	/// If it returns <see cref="AppResult.Success"/>, <see cref="OnQuit(AppResult)"/> will be called and terminate the <see cref="App"/>'s execution with a return value that can be used as an exit code for the program that reports success to the platform.
	/// </para>
	/// <para>
	/// This method may get called concurrently with <see cref="OnIterate()"/> or <see cref="OnQuit(AppResult)"/> for events not pushed from the main thread.
	/// </para>
	/// </remarks>
	protected virtual AppResult OnEvent(ref Event @event) => Continue;

	/// <summary>
	/// The unhandled exception entry point for an <see cref="App"/>'s execution
	/// </summary>
	/// <param name="info">The <see cref="ExceptionDispatchInfo"/> containing the unhandled exception information</param>
	/// <param name="source">
	/// The source method where the unhandled exception was thrown.
	/// <see cref="AppUnhandledExceptionSource.OnInitialize"/> if the exception was thrown during <see cref="OnInitialize(string[])"/>,
	/// <see cref="AppUnhandledExceptionSource.OnIterate"/> if the exception was thrown during <see cref="OnIterate()"/>,
	/// or <see cref="AppUnhandledExceptionSource.OnEvent"/> if the exception was thrown during <see cref="OnEvent(ref Event)"/>.
	/// </param>
	/// <returns>
	/// The <see cref="AppResult"/> indicating how the application should proceed.
	/// Depending on whether a call to this method is a result of an exception <see langword="throw"/>n during <see cref="OnInitialize(string[])"/>, <see cref="OnIterate()"/>, or <see cref="OnEvent(ref Event)"/>,
	/// the return value of this method will be used as the return value of that method and execution will proceed as normal.
	/// Note that the return value could have different semantics depending on the source method that threw the exception.
	/// </returns>
	/// <remarks>
	/// <para>
	/// This method is called when an exception is <see langword="throw"/>n during the execution of <see cref="OnInitialize(string[])"/>, <see cref="OnIterate()"/>, or <see cref="OnEvent(ref Event)"/>.
	/// Note that this method won't be called for exceptions <see langword="throw"/>n during the execution of <see cref="OnQuit(AppResult)"/>.
	/// </para>
	/// <para>
	/// You can use this method to handle the exception and possibly recover from it.
	/// If you can recover from the given exception, you should return a return value that you would have normally returned from the method that threw the exception.
	/// The return value will then be used as the return value of that method and execution will proceed as normal.
	/// </para>
	/// <para>
	/// You should only return from this method if you can recover from the exception; otherwise, you should re<see langword="throw"/> the given exception or <see langword="throw"/> a new exception,
	/// or return <see cref="AppResult.Failure"/> to indicate that the application should terminate with an error.
	/// </para>
	/// <para>
	/// This method is called on the same thread as the method that threw the exception.
	/// </para>
	/// </remarks>
	protected virtual AppResult OnUnhandledException(ExceptionDispatchInfo info, AppUnhandledExceptionSource source) => throw info.SourceException;

	/// <summary>
	/// The deinitialising entry point for an <see cref="App"/>'s execution
	/// </summary>
	/// <param name="result">The <see cref="AppResult"/> that terminated the execution of this <see cref="App"/> (<see cref="AppResult.Success"/> or <see cref="AppResult.Failure"/>)</param>
	/// <remarks>
	/// <para>
	/// This method is called once before terminating the execution of the <see cref="App"/>.
	/// </para>
	/// <para>
	/// This method will be called if there were no unhandled exceptions thrown during the execution of <see cref="OnInitialize(string[])"/>, <see cref="OnIterate()"/>, or <see cref="OnEvent(ref Event)"/>,
	/// or if all exceptions were recovered from by <see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/> and the execution of the <see cref="App"/> is terminating normally.
	/// If you specified to always call <see cref="OnQuit(AppResult)"/> when creating this <see cref="App"/> instance,
	/// this method will also be called if there was an unhandled exception thrown during the execution of <see cref="OnInitialize(string[])"/>, <see cref="OnIterate()"/>, or <see cref="OnEvent(ref Event)"/>,
	/// so you can ensure that any necessary cleanup is performed before the <see cref="App"/> terminates.
	/// It will even be called if <see cref="OnInitialize(string[])"/> requests termination. 
	/// </para>
	/// <para>
	/// This method should not go into an infinite loop; it should deinitialize any resources necessary, perform whatever shutdown activities, and return.
	/// </para>
	/// <para>
	/// This method is called by SDL on the main thread as a result of the <see cref="App"/>'s execution terminating normally,
	/// though <see cref="OnEvent(ref Event)"/> may get called concurrently with this method if other threads that push events are still active.
	/// If you specified to always call <see cref="OnQuit(AppResult)"/> when creating this <see cref="App"/> instance,
	/// this method will be called on the same thread as SDL's main application runs on, which is usually the main thread.
	/// </para>
	/// </remarks>
	protected virtual void OnQuit(AppResult result) { }

	/// <param name="args">A collection of arguments to pass to the <see cref="App"/> when it is started</param>
	/// <inheritdoc cref="Run(int, byte**)"/>
	[OverloadResolutionPriority(1)]
	public int Run(params ReadOnlySpan<string> args)
	{
		unsafe
		{
			// convert the args into a byte** array
			int argc = 0;
			byte** argv;

			if (args.Length is > 0)
			{
				argv = unchecked((byte**)Utilities.NativeMemory.Malloc((nuint)args.Length * (nuint)sizeof(byte*)));

				if (argv is not null)
				{
					foreach (var arg in args)
					{
						argv[argc++] = Utf8StringMarshaller.ConvertToUnmanaged(arg);
					}
				}
			}
			else
			{
				argv = null;
			}

			try
			{
				return Run(argc, argv);
			}
			finally
			{
				if (argv is not null)
				{
					while (argc is > 0)
					{
						Utf8StringMarshaller.Free(argv[--argc]);
					}

					Utilities.NativeMemory.Free(argv);
				}
			}
		}
	}

	/// <param name="args">A collection of arguments to pass to the <see cref="App"/> when it is started</param>
	/// <inheritdoc cref="Run(int, byte**)"/>
	public int Run(IEnumerable<string> args)
	{
		const int chunkSize = 16;

		unsafe
		{
			int argc = 0;
			byte** argv;

			if (!args.TryGetNonEnumeratedCount(out var size))
			{
				size = chunkSize; // we start with a single chunk and will realloc as needed
			}

			if (size is not 0)
			{
				argv = unchecked((byte**)Utilities.NativeMemory.Malloc((nuint)size * (nuint)sizeof(byte*)));
			}
			else
			{
				argv = null;
			}

			if (argv is not null)
			{
				foreach (var arg in args)
				{
					if (argc >= size)
					{
						// realloc chunk-wise
						size += chunkSize;
						var newArgv = unchecked((byte**)Utilities.NativeMemory.Realloc(argv, (nuint)size * (nuint)sizeof(byte*)));

						if (newArgv is null)
						{
							// realloc failed: proceed with using argv and argc as they currently are, and breaking out of the loop							
							break;
						}

						argv = newArgv;
					}

					argv[argc++] = Utf8StringMarshaller.ConvertToUnmanaged(arg);
				}
			}

			try
			{
				return Run(argc, argv);
			}
			finally
			{
				if (argv is not null)
				{
					while (argc is > 0)
					{
						Utf8StringMarshaller.Free(argv[--argc]);
					}

					Utilities.NativeMemory.Free(argv);
				}
			}
		}
	}

	private static volatile App? mRunningApp = null;

	private bool mOnQuitCalled;

	/// <summary>
	/// Executes the <see cref="App"/>
	/// </summary>
	/// <returns>A standard Unix main return value (<c>0</c> if the execution terminated successfully, or a non-zero value if an error occurred)</returns>
	/// <remarks>
	/// <para>
	/// This method is the main entry point for running an <see cref="App"/> instance.
	/// Calling this method will start the execution of the <see cref="App"/> and will not return until the execution has terminated.
	/// </para>
	/// <para>
	/// Note that it is only possible to run a single <see cref="App"/> instance at a time!
	/// If you try to run another <see cref="App"/> instance while one is already running, an <see cref="InvalidOperationException"/> will be thrown.
	/// </para>
	/// <para>
	/// An <see cref="App"/>'s execution lifetime model is as follows:
	/// <list type="bullet">
	/// <item>
	///		<term><see cref="OnInitialize(string[])"/></term>
	///		<description>is called once at startup to perform one-time setup and choose whether execution should continue or terminate immediately.</description>
	/// </item>
	/// <item>
	///		<term><see cref="OnIterate()"/></term>
	///		<description>is called repeatedly while execution continues, representing one iteration of ongoing work; in typical operation it can be treated like event processing already happened and the queue is currently empty.</description>
	/// </item>
	/// <item>
	///		<term><see cref="OnEvent(ref Event)"/></term>
	///		<description>is called once per arriving event while execution continues, processing each event individually; events can originate from non-main threads, so this callback may run outside the main thread context.</description>
	/// </item>
	/// <item>
	///		<term><see cref="OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/></term>
	///		<description>is called when <see cref="OnInitialize(string[])"/>, <see cref="OnIterate()"/>, or <see cref="OnEvent(ref Event)"/> throws, and its returned <see cref="AppResult"/> is used as if it were returned by the throwing callback.</description>
	/// </item>
	/// <item>
	///		<term><see cref="OnQuit(AppResult)"/></term>
	///		<description>is called once when execution is terminating (with <see cref="AppResult.Success"/> or <see cref="AppResult.Failure"/>) so resources can be released before this method returns.</description>
	/// </item>
	/// </list>
	/// </para>
	/// > [!IMPORTANT]
	/// > This method will <see cref="Sdl.Dispose()">dispose</see> of <em>all</em> <see cref="Sdl"/> instances that are alive at the end of the <see cref="App"/>'s execution, regardless of whether they were created by this <see cref="App"/> or not.
	/// > This is a technical limitation and <em>inevitable</em>!
	/// > Do not use any <see cref="Sdl"/> instances that were created before or during a call to this method after it returns, instead you may create a new <see cref="Sdl"/> instance after this method returns, if needed.
	/// <para>
	/// However, you can reuse the same <see cref="App"/> instance after this method returns and even run it again by calling this method again, if you want to.
	/// </para>
	/// </remarks>
	/// <exception cref="InvalidOperationException">An <see cref="App"/> instance is already running. Only one instance can run at a time.</exception>
	private unsafe int Run(int argc, byte** argv)
	{
		if (Interlocked.CompareExchange(ref mRunningApp, this, null) is not null)
		{
			// if there's already an instance running, we throw

			[DoesNotReturn]
			static void failAlreadyRunning() => throw new InvalidOperationException($"An {nameof(App)} instance is already running. Only one instance can run at a time.");

			failAlreadyRunning();
		}
		try
		{
			mOnQuitCalled = false; // reset mOnQuitCalled for this run

			try
			{
				// enter the SDL main app loop with Main as the entry point, which in turn will use the App* methods as callbacks for SDL_EnterAppMainCallbacks
				return SDL_RunApp(argc, argv, &Main, reserved: null);
			}
			finally
			{
				var onQuitCalled = mOnQuitCalled;
				mOnQuitCalled = false; // just to make sure

				if (alwaysCallOnQuit && !onQuitCalled)
				{
					// we can only be at this point if there was an unhandled exception that was not recovered from,
					// so we call OnQuit with AppResult.Failure to indicate that there was an unhandled exception thrown
					OnQuit(AppResult.Failure);
				}
			}
		}
		finally
		{
			mRunningApp = null; // reset mRunningApp to null (no need for Interlocked.Exchange here, since mRunningApp is already declared as volatile)
		}
	}
}
