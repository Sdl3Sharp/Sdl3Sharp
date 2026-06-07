# SDL3#

![SDL3# Banner](https://raw.githubusercontent.com/Sdl3Sharp/Sdl3Sharp-assets/main/banner.png)

[![GitHub Release](https://img.shields.io/github/v/release/Sdl3Sharp/Sdl3Sharp?logo=github&label=GitHub%20Release)](https://github.com/Sdl3Sharp/Sdl3Sharp/releases/latest)
[![NuGet Version](https://img.shields.io/nuget/v/Sdl3Sharp?logo=nuget&label=NuGet%20Package)](https://www.nuget.org/packages/Sdl3Sharp/latest)
[![SDL Native Library](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2FSdl3Sharp%2FSdl3Sharp%2Frefs%2Fheads%2Fmain%2Fmake.json&query=%24.runtimesVersion&prefix=v&logo=data%3Aimage%2Fpng%3Bbase64%2CiVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAQAAAC1%2BjfqAAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm%2B48GgAAAB50RVh0VGl0bGUAU2ltcGxlIERpcmVjdE1lZGlhIExheWVy7JOkpQAAANdJREFUKM%2B1kTFLQmEARc8nBr0%2BkMzBGkQQFAuittDf0NAvaHVqEaJd8Cc0tYhNES0F1d5S0OAa4SZBY6FbnNfwKMihzXvXy%2BFyb5D%2FlWPhgTzQoEPCPU3W%2BOSSD7r0CASmGeGWlGuqdIhEnmhzQsKMdZqAeOXUG1uOPRRHnptaEvFYcsAR%2B3xxBkCBDd5%2BC9Qzwrt3vth37KsTB26bOvLZPS8EMXHLslixahSXrFlz16E7PwHmHDzw1E2RIOQpsUxghUiRVQoUeeAxqxH%2BfBGZzQ8VFn%2FWN3OLaWMFnvBEAAAAAElFTkSuQmCC&label=SDL%20Native%20Library)
](https://github.com/Sdl3Sharp/SDL)

## About

**SDL3# provides hand-crafted C# language bindings for [SDL3](https://www.libsdl.org/)**.

In contrast to the [promoted C# bindings (SDL3-CS)](https://github.com/flibitijibibo/SDL3-CS), SDL3# is entirely hand-crafted, with no auto-generation of API code based on the native library.\
Every part of the API is deliberately designed to feel native to C#, translating SDL's functionality into idiomatic, well thought-out counterparts that existing C# users should feel right at home with.\
It makes heavy use of modern C# features to provide an API that is expressive and comfortable to work with from managed code, while still exposing the full breadth of SDL's functionality.

> [!WARNING]
> This project is a work in progress and is not yet complete or usable in production. The public .NET API is subject to change at any time and in any form without prior notice. Use at your own risk.

## Donating

At the moment, I'm the sole maintainer and contributor to this project, and all of it I do in my free time.

If you like this project and like what I'm doing, please consider supporting it by donating. It would help me dedicate more time into developing and improving SDL3#.

Any donation, one-time or recurring, in any amount, is deeply appreciated!\
Thank you so much for your help and support. ❤️

You can donate via PayPal using the button below:

[![Donate with PayPal button](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=UKHU838H8M2H2)

Or you can just scan the QR code below:

![Donate with PayPal](paypal_donations_qr.png)

## Documentation

At the moment, there's a very crude API documentation available under <https://sdl3sharp.github.io/Sdl3Sharp/api/Sdl3Sharp.html>.

For now it's very limited and kind of neglected, and it might stay that way for at least as long as [DocFx](https://dotnet.github.io/docfx/) has some major issues with C# 14 features. SDL3# makes heavy use of those features, which, in turn, renders the generated API documentation partially broken. For now, it might be at least useful as a starting point. For everything else, you'll have to rely on IntelliSense (or a comparable tool in the IDE of you choice) and the source code itself.

We'll improve the documentation eventually!

## Building

For instructions on how to build the project from source, see [BUILDING.md](BUILDING.md).

## Usage

### Requirements

- C# 14 or later
- .NET 10 or later

### Adding SDL3# as a dependency

#### Via your IDE

*(Example: Visual Studio)*

Open the NuGet Package Manager, search for `Sdl3Sharp`, check "Include prerelease", and install the latest version.

#### Via `.csproj`

```xml
<PackageReference Include="Sdl3Sharp" Version="*-*" />
```

#### Via a file-based C# app

Add the following directive at the top of your file:

```csharp
#:package Sdl3Sharp@*-*
```

### Choosing a package variant

The default `Sdl3Sharp` package includes pre-built native SDL3 binaries for all supported platforms and is the easiest way to get started.

If you only need to target specific platforms, you can reference the corresponding RID-specific packages instead, for example `Sdl3Sharp.win-x64` or `Sdl3Sharp.linux-x64`, rather than pulling in binaries for every platform.

If you do not want any native binaries bundled at all, reference `Sdl3Sharp.Core` instead. In that case you are responsible for providing the native SDL3 and libffi binaries yourself, placed alongside the resulting executable of your application.

### Examples

#### Hello World

```csharp
MessageBox.TryShowSimple(MessageBoxFlags.Information, "Hello World", "Hello World from SDL3#!");
```

#### Rendering a triangle

```csharp
using var sdl = new Sdl(static builder => builder
    .SetAppName("Simple SDL3# Triangle example")
    .InitializeSubSystems(SubSystems.Video)
);

return sdl.Run(new App(), args);

class App : AppBase
{
    private Window mWindow = default!;
    private Renderer mRenderer = default!;

    protected override AppResult OnInitialize(Sdl sdl, string[] args)
    {
        if (!Window.TryCreateWithRenderer("Hello World", 800, 600, out mWindow!, out mRenderer!))
        {
            return Failure;
        }

        return Continue;
    }

    protected override AppResult OnIterate(Sdl sdl)
    {
        mRenderer.DrawColorFloat = (0, 0, 0, 1);
        mRenderer.TryClear();

        mRenderer.TryRenderGeometry([
            new Vertex(position: (400, 150), color: (1, 0, 0, 1), texCoord: default),
            new Vertex(position: (200, 450), color: (0, 1, 0, 1), texCoord: default),
            new Vertex(position: (600, 450), color: (0, 0, 1, 1), texCoord: default)
        ]);

        mRenderer.TryRenderPresent();

        return Continue;
    }

    protected override AppResult OnEvent(Sdl sdl, ref Event @event)
    {
        if (@event.Type is EventType.WindowCloseRequested)
        {
            return Success;
        }

        return Continue;
    }

    protected override void OnQuit(Sdl sdl, AppResult result)
    {
        mRenderer?.Dispose();
        mRenderer = default!;

        mWindow?.Dispose();
        mWindow = default!;
    }
}
```

The example above makes use of the `AppBase` lifetime model, where SDL3# manages the main loop for you. You implement `OnInitialize`, `OnIterate`, and `OnEvent` as callbacks, and SDL3# takes care of the rest.

If you prefer to manage the main loop yourself, you can do so: you still initialize SDL via `new Sdl(...)` and pump events on your own, without using `AppBase` at all.

## A note on AI usage

In the spirit of transparency, and in line with the [contributing guidelines](CONTRIBUTING.md), here is an overview of how AI was used in this project:

- **Documentation.** AI was used to help write and improve documentation. The content itself comes from the author, but AI was used to clarify and clean up the writing.
- **API design.** AI was used as a sounding board for API design decisions. Sometimes the options to consider came from the author, sometimes the AI had useful proposals to offer. All decisions were ultimately made by the author.
- **Infrastructural documents.** AI was used to help write project documents such as this [README.md](README.md), the [BUILDING.md](BUILDING.md), the [CONTRIBUTING.md](CONTRIBUTING.md), and the [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
- **Code review.** AI was used to review some of the author's code, and occasionally this turned out to be fruitful, catching bugs that might otherwise have been overlooked.
- **Functional code.** No AI was used to write any functional code. All code in this project was written by the author.

## License

SDL3# is licensed under the [MIT License](./LICENSE.md). See [NOTICE.md](./NOTICE.md) for third-party notices.


