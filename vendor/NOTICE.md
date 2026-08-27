# Third-party notices for vendored task dependencies

This repository vendors selected binaries from third-party NuGet packages for build-task reliability.

## Included third-party packages

### `System.Memory` 4.6.3

- **Package**: `System.Memory`
- **Source**: <https://www.nuget.org/packages/System.Memory/4.6.3>
- **License**: MIT (<https://licenses.nuget.org/MIT>)
- **Authors**: Microsoft
- **Copyright**: © Microsoft Corporation. All rights reserved.
- **Repository**:
  - **Type**: git
  - **URL**: <https://github.com/dotnet/maintenance-packages>
  - **Commit**: f62ca0009b038cab4725a720f386623a969d73ad
- **Description**:
  > System.Memory
- **Included files *in this repository***:
  - `vendor/system.memory.4.6.3/Icon.png`
  - `vendor/system.memory.4.6.3/PACKAGE.md`
  - `vendor/system.memory.4.6.3/System.Memory.nuspec`
  - `vendor/system.memory.4.6.3/netstandard2.0/System.Memory.dll`

### `System.Text.Json` 10.0.11

- **Package**: `System.Text.Json`
- **Source**: <https://www.nuget.org/packages/System.Text.Json/10.0.11>
- **License**: MIT (<https://licenses.nuget.org/MIT>)
- **Authors**: Microsoft
- **Copyright**: © Microsoft Corporation. All rights reserved.
- **Repository**:
  - **Type**: git
  - **URL**: <https://github.com/dotnet/dotnet>
  - **Commit**: e2f47b0110ed922f21a1522da67279133ce28f32
- **Description**:
  > Provides high-performance and low-allocating types that serialize objects to JavaScript Object Notation (JSON) text and deserialize JSON text to objects, with UTF-8 support built-in. Also provides types to read and write JSON text encoded as UTF-8, and to create an in-memory document object model (DOM), that is read-only, for random access of the JSON elements within a structured view of the data.
  >
  > The System.Text.Json library is built-in as part of the shared framework in .NET Runtime. The package can be installed when you need to use it in other target frameworks.

- **Included files *in this repository***:
  - `vendor/system.text.json.10.0.11/Icon.png`
  - `vendor/system.text.json.10.0.11/PACKAGE.md`
  - `vendor/system.text.json.10.0.11/System.Text.Json.nuspec`
  - `vendor/system.text.json.10.0.11/THIRD-PARTY-NOTICES.TXT`
  - `vendor/system.text.json.10.0.11/netstandard2.0/System.Text.Json.dll`

## Maintainer note

When updating vendored dependencies, also update:

- package versions
- copyright/license metadata
- included file list
- informations in [`vendor/README.md`](README.md)
