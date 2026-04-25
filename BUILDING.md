# Building SDL3#

This project uses [make.cs](https://github.com/Sdl3Sharp/make.cs) as its build tool.\
All build and packaging tasks should be performed through the provided wrapper scripts rather than invoking `dotnet build` directly.

You don't need to get [make.cs](https://github.com/Sdl3Sharp/make.cs) separately, as it is included as a Git submodule in this repository. Just make sure to [initialize the submodules](#getting-the-source) when you clone the repository.

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

If you prefer to initialize submodules separately, or if you already cloned the repository without them, start with or continue from a normal clone:

```shell
git clone https://github.com/Sdl3Sharp/Sdl3Sharp.git
```

Then change into the project directory:

```shell
cd Sdl3Sharp
```

And initialize and fetch the submodules:

```shell
git submodule update --init --recursive
```

## Running the build tool

Change into the project directory first, if you have not done so already:

```shell
cd Sdl3Sharp
```

Then invoke the build tool using one of the following wrapper scripts, depending on your platform:

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

## Native library binaries

By default, the build tool downloads pre-built SDL3 native library binaries from [SDL3#'s native SDL build repository's](https://github.com/Sdl3Sharp/SDL) releases. This is configured via the `runtimesUrl` property in `make.json`, or equivalently via the `--runtimes-url` CLI option.

If you want to use a custom build of SDL3 instead, pass `--runtimes-url` with a URL pointing to your own archive, or update the `runtimesUrl` property in `make.json` accordingly. Local paths are supported using `file://` URLs.

> [!NOTE]
> `cacheDir` and `tempDir` in `make.json` must remain subdirectories of `./src` for `Directory.Build.props` and `Directory.Build.targets` to work correctly. Avoid changing these unless you know what you are doing.

## Commands

For a full list of available options, run `./make.sh --help` or `./make.sh <subcommand> --help`. You can also refer to the [make.cs repository](https://github.com/Sdl3Sharp/make.cs) for further documentation.

### build

Builds the managed project.

```shell
./make.sh build
```

| CLI option  | Config property | Description                                      |
|-------------|-----------------|--------------------------------------------------|
| `--project` | `project`       | Path to a `.csproj` or directory containing one. |
| `--no-logo` | `noLogo`        | Suppress the startup banner.                     |

### pack

Produces NuGet packages: a core package, RID-specific packages containing the native binaries, and a meta package.

```shell
./make.sh pack
```

| CLI option           | Config property   | Description                                                       |
|----------------------|-------------------|-------------------------------------------------------------------|
| `--output-dir`       | `outputDir`       | Output directory (default: `./build`).                            |
| `--runtimes-version` | `runtimesVersion` | Version of the native runtime assets to download.                 |
| `--runtimes-url`     | `runtimesUrl`     | URL or format string for the runtime archive. Supports `file://`. |
| `--targets`          | (no config)       | Which flavors to pack: `core`, `meta`, specific RIDs, or `all`.   |
| `--no-symbols`       | (no config)       | Skip generating a symbols package for the core package.           |
| `--strict`           | (no config)       | Fail if a requested RID has no matching runtime archive.          |
