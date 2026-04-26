# Naming Conventions for `Sdl3Sharp.Video.Gpu`

This document defines the naming rules for public types in the `Sdl3Sharp.Video.Gpu` namespace. The SDL C API prefixes all GPU types with `SDL_GPU` because C lacks namespaces. In C#, the `Sdl3Sharp.Video.Gpu` namespace already provides that context, making the `Gpu` prefix redundant in most cases. However, dropping it universally can cause name collisions with types in other namespaces or result in names that are too generic to be immediately recognizable. These rules define when to keep the prefix and when to drop it, ensuring type names are concise yet unambiguous.

## Rules

1. [**R1. Default: Drop the Prefix**](#r1-default-drop-the-prefix): Drop `Gpu` unless another rule requires keeping it.
2. [**R2. Collision**](#r2-collision-keep-when-a-name-clash-exists): Keep `Gpu` if an identically named type exists in another namespace.
3. [**R3. Genericity**](#r3-genericity-keep-when-the-name-is-too-broad): Keep `Gpu` if the unprefixed name is a single, broadly used word.
4. [**R4. Semantic Ambiguity**](#r4-semantic-ambiguity-keep-when-the-term-is-unclear): Keep `Gpu` if the unprefixed name does not clearly convey GPU purpose.
5. [**R5. Associated Type Propagation**](#r5-associated-type-propagation): Propagate `Gpu` to types that solely serve a `Gpu`-prefixed type.
6. [**R5a. Redundancy Avoidance**](#r5a-redundancy-avoidance): Don't double up `Gpu` when an outer prefix already provides context.
7. [**R5b. Family Consistency**](#r5b-family-consistency): Keep `Gpu` across a tightly coupled user-facing family when dropping it from one member would create an unexpected naming asymmetry.
8. [**R6. Internal Interop Types**](#r6-internal-interop-types): Internal native mirror types keep their original SDL names.
9. [**R7. Future-Proofing**](#r7-future-proofing): Apply all rules at the time a type is added; retroactive changes are possible but breaking.

### R1. Default: Drop the Prefix

Drop the `Gpu` prefix. The `Sdl3Sharp.Video.Gpu` namespace provides sufficient context.

```
SDL_GPUShader        → Shader
SDL_GPURenderPass    → RenderPass
SDL_GPUComputePass   → ComputePass
```

### R2. Collision: Keep When a Name Clash Exists

Keep `Gpu` if an identically named public type already exists in another `Sdl3Sharp` namespace.

| Type | Collision with | Result |
| --- | --- | --- |
| `SDL_GPUTexture` | `Sdl3Sharp.Video.Rendering.Texture` | `GpuTexture` |
| `SDL_GPUBlendFactor` | `Sdl3Sharp.Video.Blending.BlendFactor` | `GpuBlendFactor` |

### R3. Genericity: Keep When the Name Is Too Broad

Keep `Gpu` if the resulting name is a single, broadly-used word that does not inherently evoke GPU or graphics concepts.

The test: *"If a user sees this name alone in an IntelliSense completion list, without namespace context, do they immediately think GPU?"* If no, keep `Gpu`.

| Type | Why it's too generic | Result |
| --- | --- | --- |
| `SDL_GPUBuffer` | "Buffer" is ubiquitous (`System.Buffer`, stream buffers, …) | `GpuBuffer` |
| `SDL_GPUDevice` | "Device" could be audio, input, etc. | `GpuDevice` |
| `SDL_GPUFilter` | "Filter" could be LINQ, image processing, etc. | `GpuFilter` |

### R4. Semantic Ambiguity: Keep When the Term Is Unclear

Keep `Gpu` if the resulting name, even within the `Sdl3Sharp.Video.Gpu` namespace, is semantically ambiguous or does not clearly convey its GPU-specific purpose to users, particularly when surfaced in IntelliSense alongside other `using` directives.

| Type | Why it's ambiguous | Result |
| --- | --- | --- |
| `SDL_GPUFence` | "Fence" could refer to memory fences, synchronization primitives in other domains; doesn't clearly signal *GPU synchronization fence* without context | `GpuFence` |

### R5. Associated Type Propagation

When a type keeps `Gpu` (per R2, R3, or R4), types whose name clearly references that type and whose sole purpose is to describe, configure, or operate on that type, also keep the `Gpu`-qualified form of the referenced type in their own name.

For example, because `GpuBuffer` keeps its prefix:

```
SDL_GPUBufferCreateInfo   → GpuBufferCreateInfo
SDL_GPUBufferUsageFlags   → GpuBufferUsageFlags
SDL_GPUBufferBinding      → GpuBufferBinding
SDL_GPUBufferLocation     → GpuBufferLocation
SDL_GPUBufferRegion       → GpuBufferRegion
```

And because `GpuTexture` keeps its prefix:

```
SDL_GPUTextureUsageFlags     → GpuTextureUsageFlags
SDL_GPUTextureCreateInfo     → GpuTextureCreateInfo
SDL_GPUTextureFormat         → GpuTextureFormat
SDL_GPUTextureType           → GpuTextureType
SDL_GPUTextureLocation       → GpuTextureLocation
SDL_GPUTextureRegion         → GpuTextureRegion
SDL_GPUTextureSamplerBinding → GpuTextureSamplerBinding
SDL_GPUTextureTransferInfo   → GpuTextureTransferInfo
```

Note that a type must **solely serve** the referenced type for propagation to apply. Types that merely *mention* a concept without being dedicated to it do not propagate. For instance, `VertexBufferDescription` describes vertex layout for the vertex input state. It is not a configuration type for `GpuBuffer` itself, so it does not propagate.

### R5a. Redundancy Avoidance

When a type already carries the `Gpu` prefix for an independent reason (e.g., to disambiguate a different part of its name), references to `Gpu`-prefixed types embedded within that name do not need their own `Gpu` prefix, because the outer `Gpu` already establishes GPU context.

This applies to:

```
SDL_GPUStorageBufferReadWriteBinding  → GpuStorageBufferReadWriteBinding
SDL_GPUStorageTextureReadWriteBinding → GpuStorageTextureReadWriteBinding
```

Here, the `Gpu` prefix is warranted because "Storage" on its own triggers confusion with `Sdl3Sharp.IO.Storage` (R3). Once `Gpu` is at the front, the inner "Buffer" and "Texture" references are unambiguously GPU-related, so writing `GpuStorageGpuBuffer...` would be redundant.

### R5b. Family Consistency

When a small set of public GPU types forms a tightly coupled, user-facing family that developers are expected to discover and use together, and one member keeps `Gpu` under R2, R3, or R4, sibling members in that family may also keep `Gpu` if dropping it from only some members would create an unexpected naming asymmetry.

This rule is intended for explicit peer relationships, not for broad thematic similarity. Sharing a general subject area is not enough; the types should read as a matched set in API usage and documentation.

For example:

```text
SDL_GPUBlendFactor    → GpuBlendFactor
SDL_GPUBlendOperation → GpuBlendOperation
```

Here, if `BlendFactor` keeps `Gpu` because of a collision, retaining `Gpu` on `BlendOperation` preserves the expected pairing of the blend-related types that users commonly encounter together.

### R6. Internal Interop Types

Internal types that mirror native SDL types solely for interop purposes (opaque struct definitions for pointer usage, or struct definitions mirroring native layout) keep their original SDL names. These are typically nested inside the corresponding managed wrapper class in a `*.Interop.cs` partial file.

```csharp
// GpuBuffer.Interop.cs
partial class GpuBuffer
{
    [StructLayout(LayoutKind.Sequential, Size = 0)]
    internal readonly struct SDL_GPUBuffer;
}
```

### R7. Future-Proofing

When adding a new type to this namespace, apply R1 through R5b at that moment. If a future collision, ambiguity, or family-consistency concern arises for an existing type, the prefix can be added retroactively (noting that this is a breaking change for consumers).

## How to Apply

When translating a new SDL GPU API type to this namespace, follow these steps in order:

1. **Start with R1**: assume the `Gpu` prefix will be dropped.
2. **Check R2**: does the unprefixed name collide with a type in another `Sdl3Sharp` namespace? If yes → keep `Gpu`.
3. **Check R3**: is the unprefixed name a single, generic word that doesn't inherently evoke GPU concepts? If yes → keep `Gpu`.
4. **Check R4**: even if not generic, is the unprefixed name semantically ambiguous in a way that could confuse users browsing IntelliSense? If yes → keep `Gpu`.
5. **Check R5**: does the type solely serve/describe/configure another type that keeps `Gpu`? If yes → keep the `Gpu`-qualified reference in the name.
6. **Check R5b**: is the type part of a small, tightly coupled public family where another member already keeps `Gpu`, and would dropping it here create an unexpected asymmetry? If yes → keep `Gpu`.
7. **Check R5a**: if you're about to add `Gpu` for an independent reason (e.g., disambiguating "Storage"), check whether the name also references another `Gpu`-prefixed type. If so, the outer `Gpu` is sufficient; don't double up.
8. **For interop types**: apply R6; keep the original SDL name.

If none of R2 through R5b apply, the type drops its `Gpu` prefix per R1.

## Examples

### Example 1: `SDL_GPUShader` → [`Shader`](Shader.cs)

1. R1 says drop → candidate name: `Shader`.
2. R2: no type named `Shader` exists in another `Sdl3Sharp` namespace → no collision.
3. R3: "Shader" is not a generic single word; it clearly evokes GPU/graphics.
4. R4: not semantically ambiguous.
5. R5: N/A (not an associated type).

**Result:** `Shader` ✓

### Example 2: `SDL_GPUTextureFormat` → [`GpuTextureFormat`](GpuTextureFormat.cs)

1. R1 says drop → candidate name: `TextureFormat`.
2. R2: no exact collision.
3. R3: "TextureFormat" is compound, not overly generic.
4. R4: not ambiguous.
5. R5: `TextureFormat` solely describes the format of a [`GpuTexture`](GpuTexture.cs), which keeps its `Gpu` prefix (via R2). Propagation applies.

**Result:** `GpuTextureFormat` ✓

### Example 3: `SDL_GPUStorageTextureReadWriteBinding` → [`GpuStorageTextureReadWriteBinding`](GpuStorageTextureReadWriteBinding.cs)

1. R1 says drop → candidate name: `StorageTextureReadWriteBinding`.
2. R2: no exact collision.
3. R3: the name starts with "Storage", which triggers confusion with `Sdl3Sharp.IO.Storage` → keep `Gpu`.
4. R5: the "Texture" part references `GpuTexture`, but R5a applies: the outer `Gpu` prefix (warranted by R3 for "Storage") already establishes GPU context, so the inner "Texture" does not need its own `Gpu`.

**Result:** `GpuStorageTextureReadWriteBinding` ✓

---

> [!NOTE]
> The naming rules and guidelines in this file are the result of an elaborate discussion between a human contributor and an AI/LLM (GitHub Copilot). The contents were initially written in full by the AI/LLM and may have been altered by humans to some degree since.
