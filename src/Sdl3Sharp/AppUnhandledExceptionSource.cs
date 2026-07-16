using Sdl3Sharp.Events;
using System.Runtime.ExceptionServices;

namespace Sdl3Sharp;

/// <summary>
/// Represents the source of an unhandled exception thrown during the execution of an <see cref="App"/> and passed to be handled by <see cref="App.OnUnhandledException(ExceptionDispatchInfo, AppUnhandledExceptionSource)"/>
/// </summary>
public enum AppUnhandledExceptionSource
{
	/// <summary>The exception source is <see cref="App.OnInitialize(string[])"/></summary>
	OnInitialize,

	/// <summary>The exception source is <see cref="App.OnIterate()"/></summary>
	OnIterate,

	/// <summary>The exception source is <see cref="App.OnEvent(ref Event)"/></summary>
	OnEvent,
}
