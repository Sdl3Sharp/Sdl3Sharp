# Vendored .NET task dependencies

This directory contains vendored third-party binaries used by the inline MSBuild task(s) in this repository.

## Why these files are vendored

The project uses an inline task (`RoslynCodeTaskFactory`) that requires `System.Memory` and `System.Text.Json` at task compile/runtime.  
Relying on NuGet restore paths for those task-only dependencies proved brittle across SDK/toolset combinations and restore modes.

Vendoring these specific assemblies keeps builds reproducible and independent from NuGet cache layout and restore side effects.

## Scope

Only the minimum required assets are included:

- `System.Memory.dll` from `System.Memory` package (`lib/netstandard2.0`)
- `System.Text.Json.dll` from `System.Text.Json` package (`lib/netstandard2.0`)

Intentionally excluded:

- other target frameworks
- XML documentation files
- analyzers and other non-required package assets

## Provenance

See also [`vendor/NOTICE.md`](NOTICE.md).

### Package: `System.Memory` 4.6.3

- Source (NuGet): <https://www.nuget.org/packages/System.Memory/4.6.3>
- Package file: `system.memory.4.6.3.nupkg`
- Extracted assembly path in this repo: `vendor/system.memory.4.6.3/netstandard2.0/System.Memory.dll`
- Upstream package metadata included:
  - `vendor/system.memory.4.6.3/Icon.png`
  - `vendor/system.memory.4.6.3/PACKAGE.md`
  - `vendor/system.memory.4.6.3/System.Memory.nuspec`

### Package: `System.Text.Json` 10.0.11

- Source (NuGet): <https://www.nuget.org/packages/System.Text.Json/10.0.11>
- Package file: `system.text.json.10.0.11.nupkg`
- Extracted assembly path in this repo: `vendor/system.text.json.10.0.11/netstandard2.0/System.Text.Json.dll`
- Upstream package metadata included:
  - `vendor/system.text.json.10.0.11/Icon.png`
  - `vendor/system.text.json.10.0.11/PACKAGE.md`
  - `vendor/system.text.json.10.0.11/System.Text.Json.nuspec`
  - `vendor/system.text.json.10.0.11/THIRD-PARTY-NOTICES.TXT`

## Integrity

| File | Size (bytes) | Hash (SHA-256) |
| --- | ---: | --- |
| `vendor/system.memory.4.6.3/netstandard2.0/System.Memory.dll` | 145176 | 9052F3B6F64B7B70E54FA417E46027DE1320DE0713881FCCBCB427ECEDDA287A |
| `vendor/system.text.json.10.0.11/netstandard2.0/System.Text.Json.dll` | 767784 | CB053873ACF8905CEB27147351A6C18340EB89E50A1CB931C4D1416133FD903B |

## Update procedure

1. Download the exact package versions from NuGet.
2. Extract only required files listed in **Scope**.
3. Replace files in this directory.
4. Check file sizes and recompute SHA-256 values and update the table above.
5. Verify build and tests.
6. If package versions changed, update MSBuild references in [`GenerateSdlDefines.targets`](../src/GenerateSdlDefines.targets) accordingly.

## Notes

- Do not add unrelated binaries here.
- Keep this directory minimal and auditable.
- See [`vendor/NOTICE.md`](NOTICE.md) for license and attribution details.
  