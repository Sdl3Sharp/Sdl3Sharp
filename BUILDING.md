# Building SDL3\#

This project uses [make.cs](https://github.com/Sdl3Sharp/make.cs) as its build tool. All build, test, documentation, packaging, and publishing tasks should be performed through the provided wrapper scripts rather than by invoking `dotnet build` directly.

You do not need to get [make.cs](https://github.com/Sdl3Sharp/make.cs) separately, as it is included as a Git submodule in this repository. Just make sure to initialize the submodules when getting the source.

## Requirements

- Git 1.7.5 or later (only required to clone the source from GitHub)
- .NET 10 SDK or later
- The `dotnet` CLI must be available in your PATH

## Getting the source

This repository uses Git submodules. Make sure they are initialized before attempting to build.

### Clone with submodules

The recommended way to get the source is to clone the repository with submodules included upfront:

```shell
git clone --recurse-submodules https://github.com/Sdl3Sharp/Sdl3Sharp.git
```

### Initialize submodules separately

If you already cloned the repository without submodules:

```shell
git clone https://github.com/Sdl3Sharp/Sdl3Sharp.git
cd Sdl3Sharp
git submodule update --init --recursive
```

## Running the build tool

Change into the project directory first:

```shell
cd Sdl3Sharp
```

Then invoke the build tool using one of the wrapper scripts provided by the repository:

**Unix-like systems:**

```shell
./make.sh <subcommand> [options]
```

If you get a permission error, run `chmod +x make.sh` first.

**PowerShell:**

```shell
./make.ps1 <subcommand> [options]
```

**Windows CMD:**

```shell
make.cmd <subcommand> [options]
```

All wrapper scripts forward their arguments directly to `make.cs`.

## Building the managed project

To build SDL3#:

```shell
./make.sh build
```

This builds the managed project configured in [`make.json`](./make.json).

## Packing NuGet packages

To create the NuGet packages for SDL3#:

```shell
./make.sh pack
```

This produces the managed core package, RID-specific runtime packages containing the native SDL3 binaries, and the meta package.

## Running tests

To build and run the test projects:

```shell
./make.sh tests
```

The build tool discovers test projects automatically, downloads the configured native runtime assets, selects the most appropriate runtime for the current platform, copies the native binaries into the test output, and then runs the tests.

## Building the documentation

To generate the documentation:

```shell
./make.sh docs
```

Documentation is generated using [DocFX](https://dotnet.github.io/docfx/).

> [!IMPORTANT]
> This requires DocFX to be installed as a local `dotnet` tool.
>
> Example:
>
> ```shell
> dotnet tool install docfx --local
> ```

## Publishing packages

To push generated NuGet packages to a feed:

```shell
./make.sh push --api-key YOUR_API_KEY
```

This pushes the packages from the configured output directory to the configured NuGet source.

If the local package cache is stale, `push` may run `pack` first unless instructed otherwise. The API key must always be passed on the command line and is never read from configuration.

## Comparing managed bindings against native exports

To run the bundled coverage/comparison tool:

```shell
./make.sh ncover
```

This runs the bundled [`ncover.cs`](https://github.com/Sdl3Sharp/ncover.cs) tool against the managed project and the downloaded native runtime package.

This command is mainly useful when working on binding coverage. It downloads the configured runtimes, builds the managed project for Windows, and compares the managed bindings against the native library exports.

## Configuration

### Native library binaries

When packaging, testing, or running `ncover`, the build tool downloads pre-built SDL3 native library binaries from [SDL3#'s native SDL build repository](https://github.com/Sdl3Sharp/SDL) releases.

In this repository, that is configured in [`make.json`](./make.json) through values such as:

- `runtimesVersion`
- `runtimesUrl`
- `runtimesLicenseFileUrl`
- `runtimesLicenseSpdxFileUrl`

By default, SDL3# uses these values to download the runtime archive and associated license metadata.

If you want to use a custom build of SDL3 instead, you can override the configured runtime source on the command line, for example with `--runtimes-url`, or adjust the configuration file accordingly. The underlying build tool also supports absolute URLs and `file://` URLs.

### Further configuration

The project's build configuration lives in [`make.json`](./make.json).

> [!NOTE]
> `cacheDir` and `tempDir` must remain subdirectories of `./src` for `Directory.Build.props` and `Directory.Build.targets` to work correctly. Avoid changing these unless you know exactly why you need to.

For other configuration properties and command-line options, refer to the [make.cs README](https://github.com/Sdl3Sharp/make.cs/blob/main/README.md).
