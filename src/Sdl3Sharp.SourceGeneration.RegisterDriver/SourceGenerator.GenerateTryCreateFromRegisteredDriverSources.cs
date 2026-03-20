using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Sdl3Sharp.SourceGeneration.RegisterDriver;

partial class SourceGenerator
{
	private abstract class BuildTreeNode;

	private sealed class BuildTreeLeaf(INamedTypeSymbol targetType, Location? location) : BuildTreeNode
	{
		public INamedTypeSymbol TargetType => targetType;
		public Location? Location => location;
	}

	private sealed class BuildTree : BuildTreeNode
	{
		private readonly SortedDictionary<byte, BuildTreeNode> mChildren = [];

		public bool IsEmpty => mChildren.Count is not > 0;

		public void Add(SourceProductionContext spc, string driverName, INamedTypeSymbol targetType, Location? location)
		{
			Add(spc, driverName, Encoding.UTF8.GetBytes(driverName), 0, targetType, location);
		}

		private void Add(SourceProductionContext spc, string driverName, byte[] driverNameUtf8, int driverNameIndex, INamedTypeSymbol targetType, Location? location)
		{
			if (driverNameIndex >= driverNameUtf8.Length)
			{
				if (!mChildren.TryGetValue((byte)'\0', out var childNode))
				{
					mChildren.Add((byte)'\0', new BuildTreeLeaf(targetType, location));
				}
				else if (childNode is BuildTreeLeaf leaf)
				{
					if (!SymbolEqualityComparer.Default.Equals(leaf.TargetType, targetType))
					{
						spc.ReportDiagnostic(Diagnostic.Create(
							mDuplicateDriverRegistrationDescriptor,
							location,
							driverName,
							leaf.TargetType.ToDisplayString(mDefaultTypeSymbolDisplayFormat)
						));
					}
				}
			}
			else
			{
				var currentByte = driverNameUtf8[driverNameIndex];

				if (!mChildren.TryGetValue(currentByte, out var childNode))
				{
					mChildren.Add(currentByte, childNode = new BuildTree());
				}

				if (childNode is BuildTree childTree)
				{
					childTree.Add(spc, driverName, driverNameUtf8, driverNameIndex + 1, targetType, location);
				}
				else
				{
					spc.ReportDiagnostic(Diagnostic.Create(
						mDriverNamePrefixConflictDescriptor,
						location,
						driverName
					));
				}
			}
		}

		public void Print(StringBuilder builder, string ntStringPointerArgumentName, Action<StringBuilder, INamedTypeSymbol, string> printLeaf, string indentation = "", string indentationStep = "\t")
		{
			builder.Append($$"""

				{{indentation}}switch (*{{ntStringPointerArgumentName}}++)
				{{indentation}}{
				""");

			var indentationP1 = indentation + indentationStep;
			var indentationP2 = indentationP1 + indentationStep;

			foreach (var (b, child) in mChildren)
			{
				builder.Append($$"""

					{{indentationP1}}case (byte)'{{SymbolDisplay.FormatLiteral(unchecked((char)b), quote: false)}}':
					""");

				switch (child)
				{
					case BuildTreeLeaf leaf:
						printLeaf(builder, leaf.TargetType, indentationP2);
						break;

					case BuildTree tree:
						tree.Print(builder, ntStringPointerArgumentName, printLeaf, indentationP2, indentationStep);

						builder.Append($$"""

							{{indentationP2}}break;

							""");
						break;
				}
			}

			builder.Append($$"""
				{{indentation}}}
				""");
		}
	}

	private static readonly DiagnosticDescriptor mDuplicateDriverRegistrationDescriptor = new(
		id: $"{DiagnosticDescriptorIdPrefix}001",
		title: "Duplicate driver registration",
		messageFormat: "The driver '{0}' is already registered with a different type '{1}'",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly DiagnosticDescriptor mDriverNamePrefixConflictDescriptor = new(
		id: $"{DiagnosticDescriptorIdPrefix}002",
		title: "Render name prefix conflict",
		messageFormat: "The driver name '{0}' conflicts with an existing driver name (one is a prefix of the other)",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly DiagnosticDescriptor mMissingRequiredTypeDescriptor = new(
		id: $"{DiagnosticDescriptorIdPrefix}003",
		title: "Missing required type",
		messageFormat: "The required type '{0}' could not be found in the compilation",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly DiagnosticDescriptor mMissingRequiredConstructorDescriptor = new(
		id: $"{DiagnosticDescriptorIdPrefix}004",
		title: "Missing required constructor",
		messageFormat: "The type '{0}' must have a constructor with the signature '({1})'",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly DiagnosticDescriptor mMissingRequiredInterfaceImplementation = new(
		id: $"{DiagnosticDescriptorIdPrefix}005",
		title: "Missing required interface implementation",
		messageFormat: "The driver type '{0}' must implement either the interface '{1}', the interface '{2}', or the interface '{3}'",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly DiagnosticDescriptor mConflictingInterfaceImplementationDescriptor = new(
		id: $"{DiagnosticDescriptorIdPrefix}006",
		title: "Conflicting interface implementation",
		messageFormat: "The driver type '{0}' cannot implement multiple driver interfaces at the same time (only one of the interfaces '{1}', '{2}', and '{3}'')",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly SymbolDisplayFormat mDefaultTypeSymbolDisplayFormat = new(
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
		delegateStyle: SymbolDisplayDelegateStyle.NameOnly,
		miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier | SymbolDisplayMiscellaneousOptions.CollapseTupleTypes
	);

	private static readonly SymbolDisplayFormat mDiagnosticTypeSymbolDisplayFormat = mDefaultTypeSymbolDisplayFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

	private static void GenerateRenderDriverDispatchSources(SourceProductionContext spc, (Compilation compilation, ImmutableArray<(INamedTypeSymbol targetType, string driverName, Location? location)> values) data)
	{
		const string renderingNamespaceName        = "Sdl3Sharp.Video.Rendering",
					 iRenderingDriverNamespaceName = $"{renderingNamespaceName}.Drivers",
					 iRenderingDriverTypeName      = "IRenderingDriver",
					 iRenderingDriverFullTypeName  = $"{iRenderingDriverNamespaceName}.{iRenderingDriverTypeName}",
					 rendererTypeName              = "Renderer",
					 rendererFullTypeName          = $"{renderingNamespaceName}.{rendererTypeName}",
					 sdlRendererTypeName           = "SDL_Renderer",
					 sdlRendererFullTypeName       = $"{rendererFullTypeName}+{sdlRendererTypeName}",
					 rendererTDriverTypeName       = "Renderer`1",
					 rendererTDriverFullTypeName   = $"{renderingNamespaceName}.{rendererTDriverTypeName}",
					 textureTypeName               = "Texture",
					 textureFullTypeName           = $"{renderingNamespaceName}.{textureTypeName}",
					 sdlTextureTypeName            = "SDL_Texture",
					 sdlTextureFullTypeName        = $"{textureFullTypeName}+{sdlTextureTypeName}",
					 textureTDriverTypeName        = "Texture`1",
					 textureTDriverFullTypeName    = $"{renderingNamespaceName}.{textureTDriverTypeName}",
					 sdlUnsupportedDriverDiagId    = "SDL3001",
					 windowingNamespaceName        = "Sdl3Sharp.Video.Windowing",
					 iWindowingDriverNamespaceName = $"{windowingNamespaceName}.Drivers",
					 iWindowingDriverTypeName      = "IWindowingDriver",
					 iWindowingDriverFullTypeName  = $"{iWindowingDriverNamespaceName}.{iWindowingDriverTypeName}",
					 windowTypeName                = "Window",
					 windowFullTypeName            = $"{windowingNamespaceName}.{windowTypeName}",
					 sdlWindowTypeName             = "SDL_Window",
					 sdlWindowFullTypeName         = $"{windowFullTypeName}+{sdlWindowTypeName}",
					 windowTDriverTypeName         = "Window`1",
					 windowTDriverFullTypeName     = $"{windowingNamespaceName}.{windowTDriverTypeName}",
					 displayTypeName               = "Display",
					 displayFullTypeName           = $"{windowingNamespaceName}.{displayTypeName}",
					 displayTDriverTypeName        = "Display`1",
					 displayTDriverFullTypeName    = $"{windowingNamespaceName}.{displayTDriverTypeName}",
					 gpuNamespaceName              = "Sdl3Sharp.Video.Gpu",
					 iGpuDriverNamespaceName       = $"{gpuNamespaceName}.Drivers",
					 iGpuDriverTypeName            = "IGpuDriver",
					 iGpuDriverFullTypeName        = $"{iGpuDriverNamespaceName}.{iGpuDriverTypeName}",
					 gpuDeviceTypeName             = "GpuDevice",
					 gpuDeviceFullTypeName         = $"{gpuNamespaceName}.{gpuDeviceTypeName}",
					 sdlGpuDeviceTypeName          = "SDL_GPUDevice",
					 sdlGpuDeviceFullTypeName      = $"{gpuDeviceFullTypeName}+{sdlGpuDeviceTypeName}",
					 gpuDeviceTDriverTypeName      = "GpuDevice`1",
					 gpuDeviceTDriverFullTypeName  = $"{gpuNamespaceName}.{gpuDeviceTDriverTypeName}";

		var (compilation, values) = data;

		var iRenderingDriverType = compilation.GetTypeByMetadataName(iRenderingDriverFullTypeName);
		if (iRenderingDriverType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					iRenderingDriverFullTypeName
				));
			}
			return;
		}

		var rendererType = compilation.GetTypeByMetadataName(rendererFullTypeName);
		if (rendererType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					rendererFullTypeName
				));
			}
			return;
		}

		var sdlRendererType = compilation.GetTypeByMetadataName(sdlRendererFullTypeName);
		if (sdlRendererType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					sdlRendererFullTypeName
				));
			}
			return;
		}

		var sdlRendererPointerType = compilation.CreatePointerTypeSymbol(sdlRendererType);

		var rendererTDriverType = compilation.GetTypeByMetadataName(rendererTDriverFullTypeName);
		if (rendererTDriverType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					rendererTDriverFullTypeName
				));
			}
			return;
		}

		if (!rendererTDriverType.InstanceConstructors.Any(ctor
			=> ctor is { Parameters: [{ Type: var rendererType }, { Type.SpecialType: SpecialType.System_Boolean }] }
			&& SymbolEqualityComparer.Default.Equals(rendererType, sdlRendererPointerType)
		))
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredConstructorDescriptor,
					location,
					rendererTDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), $"{sdlRendererPointerType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}, {compilation.GetSpecialType(SpecialType.System_Boolean).ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}"
				));
			}
			return;
		}

		var textureType = compilation.GetTypeByMetadataName(textureFullTypeName);
		if (textureType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					textureFullTypeName
				));
			}
			return;
		}

		var sdlTextureType = compilation.GetTypeByMetadataName(sdlTextureFullTypeName);
		if (sdlTextureType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					sdlTextureFullTypeName
				));
			}
			return;
		}

		var sdlTexturePointerType = compilation.CreatePointerTypeSymbol(sdlTextureType);

		var textureTDriverType = compilation.GetTypeByMetadataName(textureTDriverFullTypeName);
		if (textureTDriverType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					textureTDriverFullTypeName
				));
			}
			return;
		}

		if (!textureTDriverType.InstanceConstructors.Any(ctor
			=> ctor is { Parameters: [{ Type: var textureType }, { Type.SpecialType: SpecialType.System_Boolean }] }
			&& SymbolEqualityComparer.Default.Equals(textureType, sdlTexturePointerType)
		))
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredConstructorDescriptor,
					location,
					textureTDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), $"{sdlTexturePointerType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}, {compilation.GetSpecialType(SpecialType.System_Boolean).ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}"
				));
			}
			return;
		}

		var iWindowingDriverType = compilation.GetTypeByMetadataName(iWindowingDriverFullTypeName);
		if (iWindowingDriverType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					iWindowingDriverFullTypeName
				));
			}
			return;
		}

		var windowType = compilation.GetTypeByMetadataName(windowFullTypeName);
		if (windowType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					windowFullTypeName
				));
			}
			return;
		}

		var sdlWindowType = compilation.GetTypeByMetadataName(sdlWindowFullTypeName);
		if (sdlWindowType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					sdlWindowFullTypeName
				));
			}
			return;
		}

		var sdlWindowPointerType = compilation.CreatePointerTypeSymbol(sdlWindowType);

		var windowTDriverType = compilation.GetTypeByMetadataName(windowTDriverFullTypeName);
		if (windowTDriverType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					windowTDriverFullTypeName
				));
			}
			return;
		}

		if (!windowTDriverType.InstanceConstructors.Any(ctor
			=> ctor is { Parameters: [{ Type: var windowType }, { Type.SpecialType: SpecialType.System_Boolean }] }
			&& SymbolEqualityComparer.Default.Equals(windowType, sdlWindowPointerType)
		))
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredConstructorDescriptor,
					location,
					windowTDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), $"{sdlWindowPointerType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}, {compilation.GetSpecialType(SpecialType.System_Boolean).ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}"
				));
			}
			return;
		}

		var displayType = compilation.GetTypeByMetadataName(displayFullTypeName);
		if (displayType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					displayFullTypeName
				));
			}
			return;
		}

		var displayTDriverType = compilation.GetTypeByMetadataName(displayTDriverFullTypeName);
		if (displayTDriverType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					displayTDriverFullTypeName
				));
			}
			return;
		}

		if (!displayTDriverType.InstanceConstructors.Any(ctor
			=> ctor is { Parameters: [{ Type.SpecialType: SpecialType.System_UInt32 }, { Type.SpecialType: SpecialType.System_Boolean }] }
		))
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredConstructorDescriptor,
					location,
					displayTDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), $"{compilation.GetSpecialType(SpecialType.System_UInt32).ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}, {compilation.GetSpecialType(SpecialType.System_Boolean).ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}"
				));
			}
			return;
		}

		
		var iGpuDriverType = compilation.GetTypeByMetadataName(iGpuDriverFullTypeName);
		if (iGpuDriverType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					iGpuDriverFullTypeName
				));
			}
			return;
		}

		var gpuDeviceType = compilation.GetTypeByMetadataName(gpuDeviceFullTypeName);
		if (gpuDeviceType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					gpuDeviceFullTypeName
				));
			}
			return;
		}
		
		var sdlGpuDeviceType = compilation.GetTypeByMetadataName(sdlGpuDeviceFullTypeName);
		if (sdlGpuDeviceType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					sdlGpuDeviceFullTypeName
				));
			}
			return;
		}

		var sdlGpuDevicePointerType = compilation.CreatePointerTypeSymbol(sdlGpuDeviceType);

		var gpuDeviceTDriverType = compilation.GetTypeByMetadataName(gpuDeviceTDriverFullTypeName);
		if (gpuDeviceTDriverType is null)
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredTypeDescriptor,
					location,
					gpuDeviceTDriverFullTypeName
				));
			}
			return;
		}

		if (!gpuDeviceTDriverType.InstanceConstructors.Any(ctor
			=> ctor is { Parameters: [{ Type: var gpuDeviceType }, { Type.SpecialType: SpecialType.System_Boolean }] }
			&& SymbolEqualityComparer.Default.Equals(gpuDeviceType, sdlGpuDevicePointerType)
		))
		{
			foreach (var (_, _, location) in values)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					mMissingRequiredConstructorDescriptor,
					location,
					gpuDeviceTDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), $"{sdlGpuDevicePointerType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}, {compilation.GetSpecialType(SpecialType.System_Boolean).ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)}"
				));
			}
			return;
		}

		var renderingDrivers = new BuildTree();
		var windowingDrivers = new BuildTree();
		var gpuDrivers = new BuildTree();

		foreach (var (targetType, driverName, location) in values)
		{
			var interfaces = targetType.AllInterfaces;

			switch (interfaces.Contains(iRenderingDriverType, SymbolEqualityComparer.Default), interfaces.Contains(iWindowingDriverType, SymbolEqualityComparer.Default), interfaces.Contains(iGpuDriverType, SymbolEqualityComparer.Default))
			{
				case (false, false, false):
					spc.ReportDiagnostic(Diagnostic.Create(
						mMissingRequiredInterfaceImplementation,
						location,
						targetType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat),	iRenderingDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), iWindowingDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), iGpuDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)
					));
					break;

				case (true, false, false):
					renderingDrivers.Add(spc, driverName, targetType, location);
					break;

				case (false, true, false):
					windowingDrivers.Add(spc, driverName, targetType, location);
					break;

				case (false, false, true):
					gpuDrivers.Add(spc, driverName, targetType, location);
					break;

				default:
					spc.ReportDiagnostic(Diagnostic.Create(
						mConflictingInterfaceImplementationDescriptor,
						location,
						targetType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), iRenderingDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), iWindowingDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat), iGpuDriverType.ToDisplayString(mDiagnosticTypeSymbolDisplayFormat)
					));
					break;
			}
		}

		var builder = new StringBuilder();

		builder.Append($$"""
			#nullable enable
			
			namespace {{renderingNamespaceName}};
			
			partial class {{rendererTypeName}}
			{
				[global::System.CodeDom.Compiler.GeneratedCode("{{mTool.Name}}", "{{mTool.Version}}")]
				internal unsafe static bool TryCreateFromRegisteredDriver({{sdlRendererTypeName}}* renderer, bool register, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {{rendererTypeName}}? result)
				{
			""");

		if (!renderingDrivers.IsEmpty)
		{
			builder.Append($$"""

						var name = SDL_GetRendererName(renderer);

						if (name is not null)
						{
				#pragma warning disable {{sdlUnsupportedDriverDiagId}}
				""");

			renderingDrivers.Print(builder, "name", (builder, targetType, indentation) =>
				builder.Append($$"""

					{{indentation}}result = new {{rendererTDriverType.Construct(targetType).ToDisplayString(mDefaultTypeSymbolDisplayFormat)}}(renderer, register);
					{{indentation}}return true;

					"""),
				indentation: "\t\t\t", indentationStep: "\t"
			);

			builder.Append($$"""

				#pragma warning restore {{sdlUnsupportedDriverDiagId}}
						}

				""");
		}

		builder.Append("""

					result = null;
					return false;
				}
			}

			#nullable restore
			""");

		spc.AddSource($"{rendererFullTypeName}_TryCreateFromRegisteredDriver.g.cs", SourceText.From(
			text: builder.ToString(),
			encoding: Encoding.UTF8
		));

		builder.Clear();

		builder.Append($$"""
			#nullable enable
			
			namespace {{renderingNamespaceName}};
			
			partial class {{textureTypeName}}
			{
				[global::System.CodeDom.Compiler.GeneratedCode("{{mTool.Name}}", "{{mTool.Version}}")]
				internal unsafe static bool TryCreateFromRegisteredDriver({{sdlTextureTypeName}}* texture, bool register, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {{textureTypeName}}? result)
				{
			""");

		if (!renderingDrivers.IsEmpty)
		{
			builder.Append($$"""

						var renderer = SDL_GetRendererFromTexture(texture);

						if (renderer is not null)
						{				
							var name = {{rendererTypeName}}.SDL_GetRendererName(renderer);

							if (name is not null)
							{
				#pragma warning disable {{sdlUnsupportedDriverDiagId}}
				""");

			renderingDrivers.Print(builder, "name", (builder, targetType, indentation) =>
				builder.Append($$"""

					{{indentation}}result = new {{textureTDriverType.Construct(targetType).ToDisplayString(mDefaultTypeSymbolDisplayFormat)}}(texture, register);
					{{indentation}}return true;

					"""),
				indentation: "\t\t\t\t", indentationStep: "\t"
			);

			builder.Append($$"""

				#pragma warning restore {{sdlUnsupportedDriverDiagId}}
							}
						}

				""");
		}

		builder.Append($$"""

					result = null;
					return false;
				}
			}

			#nullable restore
			""");

		spc.AddSource($"{textureFullTypeName}_TryCreateFromRegisteredDriver.g.cs", SourceText.From(
				text: builder.ToString(),
				encoding: Encoding.UTF8
			));

		builder.Clear();

		builder.Append($$"""
			#nullable enable
			
			namespace {{windowingNamespaceName}};
			
			partial class {{windowTypeName}}
			{
				[global::System.CodeDom.Compiler.GeneratedCode("{{mTool.Name}}", "{{mTool.Version}}")]
				internal unsafe static bool TryCreateFromRegisteredDriver({{sdlWindowTypeName}}* window, bool register, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {{windowTypeName}}? result)
				{
			""");

		if (!windowingDrivers.IsEmpty)
		{
			builder.Append($$"""

						var name = {{iWindowingDriverType.ToDisplayString(mDefaultTypeSymbolDisplayFormat)}}.SDL_GetCurrentVideoDriver();

						if (name is not null)
						{
				#pragma warning disable {{sdlUnsupportedDriverDiagId}}
				""");

			windowingDrivers.Print(builder, "name", (builder, targetType, indentation) =>
				builder.Append($$"""

					{{indentation}}result = new {{windowTDriverType.Construct(targetType).ToDisplayString(mDefaultTypeSymbolDisplayFormat)}}(window, register);
					{{indentation}}return true;

					"""),
				indentation: "\t\t\t", indentationStep: "\t"
			);

			builder.Append($$"""

				#pragma warning restore {{sdlUnsupportedDriverDiagId}}
						}

				""");
		}

		builder.Append("""

					result = null;
					return false;
				}
			}

			#nullable restore
			""");

		spc.AddSource($"{windowFullTypeName}_TryCreateFromRegisteredDriver.g.cs", SourceText.From(
			text: builder.ToString(),
			encoding: Encoding.UTF8
		));

		builder.Clear();

		builder.Append($$"""
			#nullable enable
			
			namespace {{windowingNamespaceName}};
			
			partial class {{displayTypeName}}
			{
				[global::System.CodeDom.Compiler.GeneratedCode("{{mTool.Name}}", "{{mTool.Version}}")]
				internal unsafe static bool TryCreateFromRegisteredDriver(uint displayId, bool register, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {{displayTypeName}}? result)
				{
			""");

		if (!windowingDrivers.IsEmpty)
		{
			builder.Append($$"""

						var name = {{iWindowingDriverType.ToDisplayString(mDefaultTypeSymbolDisplayFormat)}}.SDL_GetCurrentVideoDriver();

						if (name is not null)
						{
				#pragma warning disable {{sdlUnsupportedDriverDiagId}}
				""");

			windowingDrivers.Print(builder, "name", (builder, targetType, indentation) =>
				builder.Append($$"""

					{{indentation}}result = new {{displayTDriverType.Construct(targetType).ToDisplayString(mDefaultTypeSymbolDisplayFormat)}}(displayId, register);
					{{indentation}}return true;

					"""),
				indentation: "\t\t\t", indentationStep: "\t"
			);

			builder.Append($$"""

				#pragma warning restore {{sdlUnsupportedDriverDiagId}}
						}

				""");
		}

		builder.Append("""

					result = null;
					return false;
				}
			}

			#nullable restore
			""");

		spc.AddSource($"{displayFullTypeName}_TryCreateFromRegisteredDriver.g.cs", SourceText.From(
			text: builder.ToString(),
			encoding: Encoding.UTF8
		));

		builder.Clear();

		builder.Append($$"""
			#nullable enable
			
			namespace {{gpuNamespaceName}};
			
			partial class {{gpuDeviceTypeName}}
			{
				[global::System.CodeDom.Compiler.GeneratedCode("{{mTool.Name}}", "{{mTool.Version}}")]
				internal unsafe static bool TryCreateFromRegisteredDriver({{sdlGpuDeviceTypeName}}* device, bool register, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {{gpuDeviceTypeName}}? result)
				{
			""");

		if (!gpuDrivers.IsEmpty)
		{
			builder.Append($$"""

						var name = SDL_GetGPUDeviceDriver(device);

						if (name is not null)
						{
				""");

			gpuDrivers.Print(builder, "name", (builder, targetType, indentation) =>
				builder.Append($$"""

					{{indentation}}result = new {{gpuDeviceTDriverType.Construct(targetType).ToDisplayString(mDefaultTypeSymbolDisplayFormat)}}(device, register);
					{{indentation}}return true;

					"""),
				indentation: "\t\t\t", indentationStep: "\t"
			);

			builder.Append($$"""

						}

				""");
		}

		builder.Append("""

					result = null;
					return false;
				}
			}

			#nullable restore
			""");

		spc.AddSource($"{gpuDeviceFullTypeName}_TryCreateFromRegisteredDriver.g.cs", SourceText.From(
			text: builder.ToString(),
			encoding: Encoding.UTF8
		));
	}
}
