using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using static Sdl3Sharp.SourceGeneration.IdType.Constants;

namespace Sdl3Sharp.SourceGeneration.IdType;

partial class SourceGenerator
{
	private static readonly DiagnosticDescriptor mInvalidTargetTypeKindDescriptor = new(
		id: $"{DiagnosticDescriptorIdPrefix}0011",
		title: "Invalid target type kind",
		messageFormat: "Target type '{0}' must be a top-level, non-generic, non-ref struct and must be declared as partial",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly DiagnosticDescriptor mInvalidPrivateValueFieldNameDescriptor = new(
		id: $"{DiagnosticDescriptorIdPrefix}0021",
		title: "Invalid private value field name",
		messageFormat: "The specified private value field name '{0}' is not a valid C# identifier",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly DiagnosticDescriptor mMissingValueConstructorDescriptor = new(
		id: $"{DiagnosticDescriptorIdPrefix}0111",
		title: "Missing value constructor",
		messageFormat: 
			"The target type '{0}' must have a constructor that takes a single argument of type '{1}'{2}." +
			$" '{GenerateValueConstructor}' was false and the target type does not have a compatible constructor defined on its own.",
		category: DiagnosticDescriptorCategory,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	private static readonly SymbolDisplayFormat mFullyQualifiedUnderlyingTypeDisplayFormat = new(
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
		kindOptions: SymbolDisplayKindOptions.None,
		miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
	);

	private static readonly SymbolDisplayFormat mFullyQualifiedEntityTypeDisplayFormat = new(
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions: SymbolDisplayGenericsOptions.None,
		kindOptions: SymbolDisplayKindOptions.None,
		miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
	);

	private static readonly SymbolDisplayFormat mNamespaceDisplayFormat = new(
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		kindOptions: SymbolDisplayKindOptions.None
	);

	private static void GenerateIdTypeSource(
		SourceProductionContext spc,
		Compilation compilation,
		INamedTypeSymbol targetType,
		ITypeSymbol entityType,
		ITypeSymbol underlyingType,
		object? invalidValue,
		string privateValueFieldName,
		bool generateXmlDocumentationForType,
		bool generatePrivateValueFieldReadOnly,
		bool generateValueConstructor,
		bool generateValueProperty,
		bool generateValueConversionOperators,
		bool generateIsValidProperty,
		bool generateInvalidProperty,
		bool generateIEquatableImplementation,
		bool generateIEqualityOperatorsImplementation,
		bool generateIFormattableImplementation,
		bool generateISpanFormattableImplementation,
		bool generateDebuggerDisplayImplementation,
		Location? location,
		INamedTypeSymbol? iNumberBaseType,
		INamedTypeSymbol? iEquatableType,
		INamedTypeSymbol? iFormatProviderType,
		IMethodSymbol? iFormattableToStringMethod,
		INamedTypeSymbol? spanCharType,
		INamedTypeSymbol? readOnlySpanCharType,
		IMethodSymbol? iSpanFormattableTryFormatMethod,
		INamedTypeSymbol? iEqualityOperatorsType,
		StringBuilder builder
	)
	{
		if (targetType is not { IsValueType: true, IsRefLikeType: false, IsGenericType: false, ContainingType: null } // target type is top-level, non-generic, non-ref struct
			|| !targetType.DeclaringSyntaxReferences.Any(syntaxRef => (syntaxRef.GetSyntax(spc.CancellationToken) as BaseTypeDeclarationSyntax)?.Modifiers.Any(SyntaxKind.PartialKeyword) is true)) // target type is partial
		{
			// The target type must be a top-level, non-generic, non-ref struct and must be declared as partial.
			// If not, fail early and proceed with the next target type.

			spc.ReportDiagnostic(Diagnostic.Create(
				mInvalidTargetTypeKindDescriptor,
				location,
				targetType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
			));

			return;
		}

		if (!SyntaxFacts.IsValidIdentifier(privateValueFieldName))
		{
			// `privateValueFieldName` must be a valid C# field identifier.
			// If not, fail early and proceed with the next target type.

			spc.ReportDiagnostic(Diagnostic.Create(
				mInvalidPrivateValueFieldNameDescriptor,
				location,
				privateValueFieldName
			));

			return;
		}

		var simpleTypeName = $"{targetType.Name}{(targetType.TypeParameters.Length is > 0 ? $"<{string.Join(", ", targetType.TypeParameters.Select(tp => tp.Name))}>" : "")}";
		var typeNameCRef = $"{targetType.Name}{(targetType.TypeParameters.Length is > 0 ? $"{{{string.Join(", ", targetType.TypeParameters.Select(tp => tp.Name))}}}" : "")}";
		var fullyQualifiedUnderlyingTypeName = underlyingType.ToDisplayString(mFullyQualifiedUnderlyingTypeDisplayFormat);
		var fullyQualifiedEntityTypeNameCRef = $"{entityType.ToDisplayString(mFullyQualifiedEntityTypeDisplayFormat)}{(entityType is INamedTypeSymbol { TypeParameters: { Length: > 0 } typeParams } ? $"{{{string.Join(", ", typeParams.Select(tp => tp.Name))}}}" : "")}";

		var underlyingTypeIsPrimitive = underlyingType.SpecialType
			is SpecialType.System_Boolean
			or SpecialType.System_Char
			or SpecialType.System_SByte
			or SpecialType.System_Byte
			or SpecialType.System_Int16
			or SpecialType.System_UInt16
			or SpecialType.System_Int32
			or SpecialType.System_UInt32
			or SpecialType.System_Int64
			or SpecialType.System_UInt64
			or SpecialType.System_Decimal
			or SpecialType.System_Single
			or SpecialType.System_Double
			or SpecialType.System_IntPtr
			or SpecialType.System_UIntPtr;

		builder.Clear();

		builder.Append("""
			#nullable enable			

			""");

		if (targetType.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace)
		{
			builder.Append($$"""

				namespace {{containingNamespace.ToDisplayString(mNamespaceDisplayFormat)}};
				
				""");
		}

		if (generateXmlDocumentationForType)
		{
			builder.Append($$"""

				/// <summary>
				/// Represents an ID used for identifying <see cref="{{fullyQualifiedEntityTypeNameCRef}}"/> instances
				/// </summary>
				""");
		}

		if (generateDebuggerDisplayImplementation)
		{
			builder.Append("""

				[global::System.Diagnostics.DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
				""");
		}

		builder.Append($$"""

			[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential)]
			partial struct {{simpleTypeName}}
			""");

		const ulong iEquatableFlag = 1ul << 0,
					iEqualityOperatorsFlag = 1ul << 1,
					iFormattableFlag = 1ul << 2,
					iSpanFormattableFlag = 1ul << 3;

		var interfaceFlags = (generateIEquatableImplementation ? iEquatableFlag : 0)
			               | (generateIEqualityOperatorsImplementation ? iEqualityOperatorsFlag : 0)
						   | (generateIFormattableImplementation ? iFormattableFlag : 0)
						   | (generateISpanFormattableImplementation ? iSpanFormattableFlag : 0);

		if (interfaceFlags is not 0) // are there any interface implementations requested at all?
		{
			builder.Append(" :");

			if ((interfaceFlags & iEquatableFlag) is not 0)
			{
				interfaceFlags &= ~iEquatableFlag; // remove this flag from the remaining flags

				builder.Append($$"""

						global::System.IEquatable<{{simpleTypeName}}>{{(interfaceFlags is not 0 ? "," : "")}}
					""");
			}

			if ((interfaceFlags & iFormattableFlag) is not 0)
			{
				interfaceFlags &= ~iFormattableFlag; // remove this flag from the remaining flags

				builder.Append($$"""

						global::System.IFormattable{{(interfaceFlags is not 0 ? "," : "")}}
					""");
			}

			if ((interfaceFlags & iSpanFormattableFlag) is not 0)
			{
				interfaceFlags &= ~iSpanFormattableFlag; // remove this flag from the remaining flags

				builder.Append($$"""

						global::System.ISpanFormattable{{(interfaceFlags is not 0 ? "," : "")}}
					""");
			}

			if ((interfaceFlags & iEqualityOperatorsFlag) is not 0)
			{
				// We keep the remaining flags logic here for consistency as well as for future-proofing.
				// In pratice, `interfaceFlags` should be 0 after this point.

				interfaceFlags &= ~iEqualityOperatorsFlag; // remove this flag from the remaining flags

				builder.Append($$"""

						global::System.Numerics.IEqualityOperators<{{simpleTypeName}}, {{simpleTypeName}}, bool>{{(interfaceFlags is not 0 ? "," : "")}}
					""");
			}
		}

		builder.Append("""

			{
			""");

		if (!targetType.MemberNames.Contains(privateValueFieldName)) // We don't generate the value field if the target type already has such a member defined (we assume the user has explicitly defined their own value field).
		{
			builder.Append($$"""

					private {{(generatePrivateValueFieldReadOnly ? "readonly " : "")}}{{fullyQualifiedUnderlyingTypeName}} {{privateValueFieldName}};

				""");
		}

		if (generateValueConstructor)
		{
			if (!targetType.InstanceConstructors.Any(ctor => ctor.Parameters is [{ Type: var argType }] && argType.Equals(underlyingType, SymbolEqualityComparer.Default))) // We don't generate the value constructor if the target type already has a compatible constructor defined (we assume the user has explicitly defined their own value constructor).
			{
				builder.Append($$"""

						/// <summary>
						/// Creates a new <see cref="{{typeNameCRef}}"/> from a specified numerical ID value
						/// </summary>
						/// <param name="value">The numerical ID value to create the <see cref="{{typeNameCRef}}"/> from</param>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
						public {{targetType.Name}}({{fullyQualifiedUnderlyingTypeName}} value) => {{privateValueFieldName}} = value;

					""");
			}
		}

		var underlyingTypeHasNonExplicitToString = iFormattableToStringMethod is not null && underlyingType.FindImplementationForInterfaceMember(iFormattableToStringMethod) is IMethodSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: true };

		if (generateDebuggerDisplayImplementation)
		{
			if (!targetType.MemberNames.Contains("DebuggerDisplay")) // We don't generate the `DebuggerDisplay` property if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				builder.Append("""

						[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]
						[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
						private readonly string DebuggerDisplay
					""");

				if (underlyingTypeHasNonExplicitToString)
				{
					// This means that the target type has a `ToString(IFormatProvider?)` defined (either we generate it further down, or the user has defined it themselves), so we can use that for our `DebuggerDisplay` property.					

					builder.Append($$"""
						 => ToString(global::System.Globalization.CultureInfo.InvariantCulture);

						""");
				}
				else
				{
					// The underlying type implements `IFormattable.ToString(string?, IFormatProvider?)` explicitly, and so does our target type.
					// In that case, we simply use the `ToString()` method on our target type.

					builder.Append($$"""
						 => ToString();

						""");
				}
			}
		}

		if (generateInvalidProperty)
		{
			if (!targetType.MemberNames.Contains("Invalid")) // We don't generate the `Invalid` property if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				if (!generateValueConstructor
					&& !targetType.InstanceConstructors.Any(ctor => ctor is { Parameters: [{ Type: var argType }] } && argType.Equals(underlyingType, SymbolEqualityComparer.Default)))
				{
					// The property's implementation relies on the existence of the value constructor. If there's none, we can't generate the property.
					// We just discard what we've done so far, report a diagnostic, and proceed to the next target type, if any.

					spc.ReportDiagnostic(Diagnostic.Create(
						descriptor: mMissingValueConstructorDescriptor,
						location: location,
						targetType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
						underlyingType.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
						" in order to generate the 'Invalid' property"
					));

					return;
				}

				builder.Append($$"""

						/// <summary>
						/// Gets the invalid <see cref="{{typeNameCRef}}"/>
						/// </summary>
						/// <value>
						/// The invalid <see cref="{{typeNameCRef}}"/>, i.e., the <see cref="{{typeNameCRef}}"/> that is <em>not</em> valid for identifying a <see cref="{{fullyQualifiedEntityTypeNameCRef}}"/>
						/// </value>
					""");

				if (generateIsValidProperty || targetType.MemberNames.Contains("IsValid"))
				{
					builder.Append($$"""

							/// <remarks>
							/// <para>
							/// The value of the <see cref="IsValid"/> property of the <see cref="{{typeNameCRef}}"/> returned by this property will be <c><see langword="false"/></c>.
							/// </para>
							/// </remarks>
						""");
				}

				builder.Append($$"""

						public static {{simpleTypeName}} Invalid
						{
							[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
							get
					""");

				if (invalidValue is null)
				{
					// We don't have to check whether the underlying type implements `INumberBase<T>` for it's own type here, because the attribute's generic type parameter's constraint already ensures that.

					if (underlyingTypeIsPrimitive)
					{
						// If the underlying type is a primitive type, we can just return the `default(<underlying type>)` literal (`0` for numerical type, `false` for `bool`, `'\0'` for `char`, etc.).

						builder.Append($$"""
							 => new(default({{fullyQualifiedUnderlyingTypeName}}));

							""");
					}
					else if (iNumberBaseType is not null
						&& iNumberBaseType
							.Construct(underlyingType)
							.GetMembers("Zero")
							.OfType<IPropertySymbol>()
							.FirstOrDefault(property => property.Type.Equals(underlyingType, SymbolEqualityComparer.Default))
							is { } zeroProperty // We need to extract the `Zero` property dynamically per the underlying type here, because it depends on the actual underlying type (`INumberBase<T>` is obviously generic).
						&& underlyingType.FindImplementationForInterfaceMember(zeroProperty) is IPropertySymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: true })
					{
						// The underlying type implements `INumberBase<T>.Zero` non-explicitly, so we can safely return it directly by name.

						builder.Append($$"""
							 => new({{fullyQualifiedUnderlyingTypeName}}.Zero);

							""");
					}
					else
					{
						// The underlying type implements the `INumberBase<T>.Zero` property explicitly. Thus we can't just access it by name directly.
						// See the comments for the `IsValid` property below for an explanation of what's going on here.

						builder.Append($$"""

									{
										[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
										static T getZero<T>()
											where T : notnull, global::System.Numerics.INumberBase<T>
											=> T.Zero;

										return new(getZero<{{fullyQualifiedUnderlyingTypeName}}>());
									}

							""");
					}
				}
				else
				{
					// We can safely use `SymbolDisplay.FormatPrimitive` here because we not only know that `invalidValue` is a primitive value, but it's also constant (because it was specified in an attribute application).

					builder.Append($$"""

								=> new({{SymbolDisplay.FormatPrimitive(invalidValue, quoteStrings: true, useHexadecimalNumbers: false)}});

						""");
				}

				builder.Append("""
						}

					""");
			}
		}

		if (generateIsValidProperty)
		{
			if (!targetType.MemberNames.Contains("IsValid")) // We don't generate the `IsValid` property if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				builder.Append($$"""

						/// <summary>
						/// Gets a value indicating whether the <see cref="{{typeNameCRef}}"/> is valid
						/// </summary>
						/// <value>
						/// A value indicating whether the <see cref="{{typeNameCRef}}"/> is valid, i.e., whether the ID is valid for identifying a <see cref="{{fullyQualifiedEntityTypeNameCRef}}"/>
						/// </value>
						public readonly bool IsValid
						{
							[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
							get
					""");

				if (invalidValue is null)
				{
					// We don't have to check whether the underlying type implements `INumberBase<T>` for it's own type here, because the attribute's generic type parameter's constraint already ensures that.

					if (underlyingTypeIsPrimitive)
					{
						// If the underlying type is a primitive type, we can just compare against the `default(<underlying type>)` literal (`0` for numerical type, `false` for `bool`, `'\0'` for `char`, etc.).

						builder.Append($$"""
							 => {{privateValueFieldName}} is not default({{fullyQualifiedUnderlyingTypeName}});

							""");
					}
					else if (iNumberBaseType is not null
						&& iNumberBaseType
							.Construct(underlyingType)
							.GetMembers("Zero")
							.OfType<IPropertySymbol>()
							.FirstOrDefault(property => property.Type.Equals(underlyingType, SymbolEqualityComparer.Default))
							is { } zeroProperty // We need to extract the `Zero` property dynamically per the underlying type here, because it depends on the actual underlying type (`INumberBase<T>` is obviously generic).
						&& underlyingType.FindImplementationForInterfaceMember(zeroProperty) is IPropertySymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: true })
					{
						// The underlying type implements `INumberBase<T>.Zero` non-explicitly, so we can safely access it directly by name.

						builder.Append($$"""
							 => {{privateValueFieldName}} != {{fullyQualifiedUnderlyingTypeName}}.Zero;

							""");
					}
					else
					{
						// The underlying type implements the `INumberBase<T>.Zero` property explicitly. Thus we can't just access it by name directly.
						// Instead, we use a local static method to access the `Zero` property through generic interface dispatch.
						// Because the actual underlying type is fully known at JIT time, we force-inline the local static method, and static devirtualization is pretty good by RyuJIT, this should be optimized away to a direct call to the `Zero` property.

						builder.Append($$"""

									{
										[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
										static T getZero<T>()
											where T : notnull, global::System.Numerics.INumberBase<T>
											=> T.Zero;

										return {{privateValueFieldName}} != getZero<{{fullyQualifiedUnderlyingTypeName}}>();
									}

							""");
					}
				}
				else
				{
					// We can safely use `SymbolDisplay.FormatPrimitive` here because we not only know that `invalidValue` is a primitive value, but it's also constant (because it was specified in an attribute application).

					builder.Append($$"""

								=> {{privateValueFieldName}} is not {{SymbolDisplay.FormatPrimitive(invalidValue, quoteStrings: true, useHexadecimalNumbers: false)}};

						""");
				}

				builder.Append("""
						}

					""");
			}
		}

		if (generateValueProperty)
		{
			if (!targetType.MemberNames.Contains("Value")) // We don't generate the value property if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				// This is per-design a `get`-only property.
				// User who wish to have a `set`-able property should omit the `GenerateValueProperty` flag and implement their own property based on the `privateValueFieldName` field.

				builder.Append($$"""

						/// <summary>
						/// Gets the numerical ID value of the <see cref="{{typeNameCRef}}"/>
						/// </summary>
						/// <value>
						/// The numerical ID value of the <see cref="{{typeNameCRef}}"/>
						/// </value>
						public readonly {{fullyQualifiedUnderlyingTypeName}} Value
						{
							[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
							get => {{privateValueFieldName}};
						}

					""");
			}
		}

		if (generateIEquatableImplementation)
		{
			if (!(targetType
					.GetMembers("Equals")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(static method => method is { Parameters: [{ Type.SpecialType: SpecialType.System_Object }] })
					is { IsOverride: true, ContainingType: var equalsOverridingType }
				&& equalsOverridingType.Equals(targetType, SymbolEqualityComparer.Default)))
			{
				// The target type does not have an `Equals(object)` override, so we can generate one.

				builder.Append($$"""
				
						/// <inheritdoc/>
						public readonly override bool Equals([global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is {{targetType.Name}} other && Equals(other);

					""");
			}

			if (targetType
					.GetMembers("Equals")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(method => method is { Parameters: [{ Type: var argType }] } && argType.Equals(targetType, SymbolEqualityComparer.Default))
					is null) // We don't generate the `Equals(T)` method if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				builder.Append($$"""				
				
						/// <inheritdoc/>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
						public readonly bool Equals({{targetType.Name}} other)
					""");


				// We don't need to check whether the underlying type implements `IEquatable<T>` for it's own type here, because the attribute's generic type parameter is constrained to be of `INumberBase<T>`(for its own type), which in turn has `IEquatable<T>` as one of its base interfaces.

				if (iEquatableType is not null
					&& iEquatableType
						.Construct(underlyingType)
						.GetMembers("Equals")
						.OfType<IMethodSymbol>()
						.FirstOrDefault(method => method is { Parameters: [{ Type: var argType }] } && argType.Equals(underlyingType, SymbolEqualityComparer.Default))
						is { } equalsMethod // We need to extract the `Equals` method dynamically per the underlying type here, because it depends on the actual underlying type (`IEquatable<T>` is obviously generic).
					&& underlyingType.FindImplementationForInterfaceMember(equalsMethod) is IMethodSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: true })
				{
					// The underlying type implements `IEquatable<T>.Equals(T)` non-explicitly, so we can safely access it directly by name.

					builder.Append($$"""
						 => {{privateValueFieldName}}.Equals(other.{{privateValueFieldName}});

						""");
				}
				else
				{
					// The underlying type implements the `IEquatable<T>.Equals(T)` method explicitly. Thus we can't just access it by name directly.
					// See the comment above for the `IsValid` property for an explanation about what's going on here.

					builder.Append($$"""

							{
								[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
								static bool equals<T>(T left, T right)
									where T : notnull, global::System.IEquatable<T>
									=> left.Equals(right);

								return equals({{privateValueFieldName}}, other.{{privateValueFieldName}});
							}

						""");
				}
			}

			if (!(targetType
					.GetMembers("GetHashCode")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(static method => method is { Parameters.IsDefaultOrEmpty: true })
					is { IsOverride: true, ContainingType: var getHashCodeOverridingType }
				&& getHashCodeOverridingType.Equals(targetType, SymbolEqualityComparer.Default)))
			{
				// The target type does not have a `GetHashCode()` override, so we can generate one.

				builder.Append($$"""
				
						/// <inheritdoc/>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
						public readonly override int GetHashCode() => {{privateValueFieldName}}.GetHashCode();

					""");
			}
		}

		if (generateIFormattableImplementation)
		{
			// We don't need to check whether the underlying type implements `IFormattable` here, because the attribute's generic type parameter is constrained to be of `INumberBase<T>`(for its own type), which in turn has `IFormattable` as one of its base interfaces.

			if (!(targetType
					.GetMembers("ToString")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(static method => method is { Parameters.IsDefaultOrEmpty: true })
					is { IsOverride: true, ContainingType: var toStringOverridingType }
				&& toStringOverridingType.Equals(targetType, SymbolEqualityComparer.Default)))
			{
				// The target type does not have a `ToString()` override, so we can generate one.

				if (underlyingTypeHasNonExplicitToString)
				{
					// The underlying type implements `IFormattable.ToString(string?, IFormatProvider?)` non-explicitly,
					// thus our target type does have a non-explicit `ToString(string?, IFormatProvider?)` method defined,
					// so we delegate to that method, passing `null` for both arguments.

					builder.Append($$"""
				
						/// <inheritdoc/>
						public readonly override string ToString() => ToString((string?)null, (global::System.IFormatProvider?)null);

					""");
				}
				else
				{
					// The underlying type implements `IFormattable.ToString(string?, IFormatProvider?)` explicitly, and so does our target type.
					// In that case, we delegate to the `ToString()` method on the underlying type (which must exist as it's defined by `object`).

					// If the underlying type covariantly removes the nullability annotation from the base `object.ToString()` method with an override (as it's often done),
					// we want to extract that information and reflect it in our generated `ToString()` method's return type as well.

					if (!(underlyingType
							.GetMembers("ToString")
							.OfType<IMethodSymbol>()
							.FirstOrDefault(method => method is { Parameters.IsDefaultOrEmpty: true })
							is { ReturnNullableAnnotation: var toStringReturnNullability }))
					{
						toStringReturnNullability = NullableAnnotation.None;
					}

					builder.Append($$"""
				
						/// <inheritdoc/>
						public readonly override string{{toStringReturnNullability switch { not NullableAnnotation.NotAnnotated => "?", _ => string.Empty }}} ToString() => {{privateValueFieldName}}.ToString();

					""");
				}
			}

			if (underlyingTypeHasNonExplicitToString
				&& targetType
					.GetMembers("ToString")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(static method => method is { Parameters: [{ Type.SpecialType: SpecialType.System_String }] })
					is null) // We don't generate the `ToString(string?)` method if either `IFormattable.ToString(string?, IFormatProvider?)` is explicitly implemented on the underlying type
							 // or the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				// We just delegate to the `IFormattable.ToString(string?, IFormatProvider?)` method, passing `null` as the `IFormatProvider?` argument.

				builder.Append($$"""

						/// <inheritdoc cref="global::System.IFormattable.ToString(string?, global::System.IFormatProvider?)"/>
						public readonly string ToString(string? format) => {{privateValueFieldName}}.ToString(format, (global::System.IFormatProvider?)null);

					""");
			}

			if (underlyingTypeHasNonExplicitToString
				&& targetType
					.GetMembers("ToString")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(method => method is { Parameters: [{ Type: var argType }] } && argType.Equals(iFormatProviderType, SymbolEqualityComparer.Default))
					is null) // We don't generate the `ToString(IFormatProvider?)` method if either `IFormattable.ToString(string?, IFormatProvider?)` is explicitly implemented on the underlying type
							 // or the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				// We just delegate to the `IFormattable.ToString(string?, IFormatProvider?)` method, passing `null` as the `string?` argument.

				builder.Append($$"""

						/// <inheritdoc cref="global::System.IFormattable.ToString(string?, global::System.IFormatProvider?)"/>
						public readonly string ToString(global::System.IFormatProvider? formatProvider) => {{privateValueFieldName}}.ToString((string?)null, formatProvider);

					""");
			}

			if (targetType
					.GetMembers("ToString")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(method => method is { Parameters: [{ Type.SpecialType: SpecialType.System_String }, { Type: var secondArgType }] } && secondArgType.Equals(iFormatProviderType, SymbolEqualityComparer.Default))
					is null) // We don't generate the `ToString(string?, IFormatProvider?)` method if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				if (underlyingTypeHasNonExplicitToString)
				{
					// The underlying type implements `IFormattable.ToString(string?, IFormatProvider?)` non-explicitly, so we can safely access it directly by name.
					
					builder.Append($$"""

							/// <inheritdoc/>
							public readonly string ToString(string? format, global::System.IFormatProvider? formatProvider) => {{privateValueFieldName}}.ToString(format, formatProvider);

						""");
				}
				else
				{
					// If the underlying type implements `IFormattable.ToString(string?, IFormatProvider?)` explicitly, we too should implement `IFormattable.ToString(string?, IFormatProvider?)` explicitly for the target type.
					// We still going to use a generic interface dispatch approach to call the underlying type's `IFormattable.ToString(string?, IFormatProvider?)` method though.

					builder.Append($$"""

							/// <inheritdoc/>
							readonly string global::System.IFormattable.ToString(string? format, global::System.IFormatProvider? formatProvider)
							{
								[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
								static string toString<T>(T value, string? format, global::System.IFormatProvider? formatProvider)
									where T : notnull, global::System.IFormattable
									=> value.ToString(format, formatProvider);

								return toString({{privateValueFieldName}}, format, formatProvider);
							}

						""");
				}
			}
		}

		if (generateISpanFormattableImplementation)
		{
			// We don't need to check whether the underlying type implements `ISpanFormattable` here, because the attribute's generic type parameter is constrained to be of `INumberBase<T>`(for its own type), which in turn has `ISpanFormattable` as one of its base interfaces.

			if (spanCharType is not null
				&& readOnlySpanCharType is not null
				&& iFormatProviderType is not null
				&& targetType
					.GetMembers("TryFormat")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(method => method is { Parameters: [{ Type: var firstArgType }, { Type.SpecialType: SpecialType.System_Int32, RefKind: RefKind.Out }, { Type: var thirdArgType }, { Type: var fourthArgType }] }
						&& firstArgType.Equals(spanCharType, SymbolEqualityComparer.Default)
						&& thirdArgType.Equals(readOnlySpanCharType, SymbolEqualityComparer.Default)
						&& fourthArgType.Equals(iFormatProviderType, SymbolEqualityComparer.Default))
					is null) // We don't generate the `TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` method if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				if (iSpanFormattableTryFormatMethod is not null
					&& underlyingType.FindImplementationForInterfaceMember(iSpanFormattableTryFormatMethod) is IMethodSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: true })
				{
					// The underlying type implements `ISpanFormattable.TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` non-explicitly, so we can safely access it directly by name.

					builder.Append($$"""
						
							/// <inheritdoc/>
							public readonly bool TryFormat(global::System.Span<char> destination, out int charsWritten, global::System.ReadOnlySpan<char> format = default, global::System.IFormatProvider? formatProvider = default) => {{privateValueFieldName}}.TryFormat(destination, out charsWritten, format, formatProvider);

						""");
				}
				else if (underlyingType.GetMembers("TryFormat")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(method => method is { Parameters: [{ Type: var firstArgType }, { Type.SpecialType: SpecialType.System_Int32, RefKind: RefKind.Out }, { Type: var thirdArgType }] }
						&& firstArgType.Equals(spanCharType, SymbolEqualityComparer.Default)
						&& thirdArgType.Equals(readOnlySpanCharType, SymbolEqualityComparer.Default))
					is { DeclaredAccessibility: Accessibility.Protected or Accessibility.ProtectedOrFriend or Accessibility.ProtectedOrInternal or Accessibility.Public })
				{
					// The underlying type implements `ISpanFormattable.TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` explicitly,
					// but it also has an accessible `TryFormat(Span<char>, out int, ReadOnlySpan<char>)` method defined (sometimes that's done to omit the `IFormatProvider?` argument).

					// We expose that method with the same signature on the target type, and delegate to it.

					builder.Append($$"""

							/// <inheritdoc cref="global::System.ISpanFormattable.TryFormat(global::System.Span{char}, out int, global::System.ReadOnlySpan{char}, global::System.IFormatProvider?)"/>
							public readonly bool TryFormat(global::System.Span<char> destination, out int charsWritten, global::System.ReadOnlySpan<char> format = default) => {{privateValueFieldName}}.TryFormat(destination, out charsWritten, format);

						""");
				}
				else if (underlyingType.GetMembers("TryFormat")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(method => method is { Parameters: [{ Type: var firstArgType }, { Type.SpecialType: SpecialType.System_Int32, RefKind: RefKind.Out }, { Type: var thirdArgType }] }
						&& firstArgType.Equals(spanCharType, SymbolEqualityComparer.Default)
						&& thirdArgType.Equals(iFormatProviderType, SymbolEqualityComparer.Default))
					is { DeclaredAccessibility: Accessibility.Protected or Accessibility.ProtectedOrFriend or Accessibility.ProtectedOrInternal or Accessibility.Public })
				{
					// The underlying type implements `ISpanFormattable.TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` explicitly,
					// but it also has an accessible `TryFormat(Span<char>, out int, IFormatProvider?)` method defined (sometimes that's done to omit the `ReadOnlySpan<char>` argument).

					// We expose that method with the same signature on the target type, and delegate to it.

					builder.Append($$"""

							/// <inheritdoc cref="global::System.ISpanFormattable.TryFormat(global::System.Span{char}, out int, global::System.ReadOnlySpan{char}, global::System.IFormatProvider?)"/>
							public readonly bool TryFormat(global::System.Span<char> destination, out int charsWritten, global::System.IFormatProvider? formatProvider = default) => {{privateValueFieldName}}.TryFormat(destination, out charsWritten, formatProvider);

						""");
				}
				else if (underlyingType.GetMembers("TryFormat")
					.OfType<IMethodSymbol>()
					.FirstOrDefault(method => method is { Parameters: [{ Type: var firstArgType }, { Type.SpecialType: SpecialType.System_Int32, RefKind: RefKind.Out }] } && firstArgType.Equals(spanCharType, SymbolEqualityComparer.Default))
					is { DeclaredAccessibility: Accessibility.Protected or Accessibility.ProtectedOrFriend or Accessibility.ProtectedOrInternal or Accessibility.Public })
				{
					// The underlying type implements `ISpanFormattable.TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` explicitly,
					// but it also has an accessible `TryFormat(Span<char>, out int)` method defined (sometimes that's done to omit the `ReadOnlySpan<char>` and `IFormatProvider?` arguments).

					// We expose that method with the same signature on the target type, and delegate to it.

					builder.Append($$"""

							/// <inheritdoc cref="global::System.ISpanFormattable.TryFormat(global::System.Span{char}, out int, global::System.ReadOnlySpan{char}, global::System.IFormatProvider?)"/>
							public readonly bool TryFormat(global::System.Span<char> destination, out int charsWritten) => {{privateValueFieldName}}.TryFormat(destination, out charsWritten);

						""");
				}
				else
				{
					// If the underlying type implements `ISpanFormattable.TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` explicitly and it does not have any usable `TryFormat` substitutes defined,
					// we too should implement `ISpanFormattable.TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` explicitly for the target type.
					// We still going to use a generic interface dispatch approach to call the underlying type's `ISpanFormattable.TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` method though.

					builder.Append($$"""

							/// <inheritdoc/>
							readonly bool global::System.ISpanFormattable.TryFormat(global::System.Span<char> destination, out int charsWritten, global::System.ReadOnlySpan<char> format, global::System.IFormatProvider? formatProvider)
							{
								[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
								static bool tryFormat<T>(T value, global::System.Span<char> destination, out int charsWritten, global::System.ReadOnlySpan<char> format, global::System.IFormatProvider? formatProvider)
									where T : notnull, global::System.ISpanFormattable
									=> value.TryFormat(destination, out charsWritten, format, formatProvider);

								return tryFormat({{privateValueFieldName}}, destination, out charsWritten, format, formatProvider);
							}

						""");
				}
			}
		}

		if (generateIEqualityOperatorsImplementation)
		{
			// We don't need to check whether the underlying type implements `IEqualityOperators<TSelf, TOther, bool>` here with `TSelf` and `TOther` being the underlying type, because the attribute's generic type parameter is constrained to be of `INumberBase<T>`(for its own type), which in turn has `IEqualityOperators<T, T, bool>` as one of its base interfaces.

			// The underlying might not explicitly implement `IEqualityOperators<TSelf, TOther, bool>` because it has language built-in equality operators defined for it,
			// in which case we can use those instead of the interface's `==` and `!=` operators.
			var constructedIEqualityOperatorsType = !underlyingTypeIsPrimitive
				? iEqualityOperatorsType?.Construct(underlyingType, underlyingType, compilation.GetSpecialType(SpecialType.System_Boolean))
				: null;

			if (targetType
					.GetMembers(WellKnownMemberNames.EqualityOperatorName)
					.OfType<IMethodSymbol>()
					.FirstOrDefault(@operator => @operator is { Parameters: [{ Type: var firstArgType }, { Type: var secondArgType }] }
						&& firstArgType.Equals(targetType, SymbolEqualityComparer.Default)
						&& secondArgType.Equals(targetType, SymbolEqualityComparer.Default))
					is null) // We don't generate the `==` operator if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				builder.Append($$"""

						/// <inheritdoc/>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
						public static bool operator ==({{targetType.Name}} left, {{targetType.Name}} right)
					""");

				if (underlyingTypeIsPrimitive
					|| (constructedIEqualityOperatorsType is not null
						&& constructedIEqualityOperatorsType
							.GetMembers(WellKnownMemberNames.EqualityOperatorName)
							.OfType<IMethodSymbol>()
							.FirstOrDefault(@operator => @operator is { Parameters: [{ Type: var firstArgType }, { Type: var secondArgType }] }
								&& firstArgType.Equals(underlyingType, SymbolEqualityComparer.Default)
								&& secondArgType.Equals(underlyingType, SymbolEqualityComparer.Default))
							is { } equalityOperator // We need to extract the `==` operator method dynamically per the underlying type here, because it depends on the actual underlying type (`IEqualityOperators<TSelf, TOther, TResult>` is obviously generic).
						&& underlyingType.FindImplementationForInterfaceMember(equalityOperator) is IMethodSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: true }))
				{
					// The underlying type has a language built-in `==` operator or implements `IEqualityOperators<T, T, bool>.operator ==(T, T)` non-explicitly, so we can safely access it as an `==` operator call directly.

					builder.Append($$"""
						 => left.{{privateValueFieldName}} == right.{{privateValueFieldName}};

						""");
				}
				else
				{
					// The underlying type implements `IEqualityOperators<T, T, bool>.operator ==(T, T)` explicitly. Thus we can't just access it directly.
					// See the comment above for the `IsValid` property for an explanation about what's going on here.

					builder.Append($$"""

							{
								[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
								static bool equalityOperator<T>(T left, T right)
									where T : notnull, global::System.Numerics.IEqualityOperators<T, T, bool>
									=> left == right;

								return equalityOperator(left.{{privateValueFieldName}}, right.{{privateValueFieldName}});
							}

						""");
				}
			}

			if (targetType
					.GetMembers(WellKnownMemberNames.InequalityOperatorName)
					.OfType<IMethodSymbol>()
					.FirstOrDefault(@operator => @operator is { Parameters: [{ Type: var firstArgType }, { Type: var secondArgType }] }
						&& firstArgType.Equals(targetType, SymbolEqualityComparer.Default)
						&& secondArgType.Equals(targetType, SymbolEqualityComparer.Default))
					is null) // We don't generate the `!=` operator if the target type already has such a member defined (we assume the user has explicitly defined their own implementation).
			{
				builder.Append($$"""

						/// <inheritdoc/>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
						public static bool operator !=({{targetType.Name}} left, {{targetType.Name}} right)
					""");

				if (underlyingTypeIsPrimitive
					|| (constructedIEqualityOperatorsType is not null
						&& constructedIEqualityOperatorsType
							.GetMembers(WellKnownMemberNames.InequalityOperatorName)
							.OfType<IMethodSymbol>()
							.FirstOrDefault(@operator => @operator is { Parameters: [{ Type: var firstArgType }, { Type: var secondArgType }] }
								&& firstArgType.Equals(underlyingType, SymbolEqualityComparer.Default)
								&& secondArgType.Equals(underlyingType, SymbolEqualityComparer.Default))
							is { } inequalityOperator // We need to extract the `!=` operator method dynamically per the underlying type here, because it depends on the actual underlying type (`IEqualityOperators<TSelf, TOther, TResult>` is obviously generic).
						&& underlyingType.FindImplementationForInterfaceMember(inequalityOperator) is IMethodSymbol { ExplicitInterfaceImplementations.IsDefaultOrEmpty: true }))
				{
					// The underlying type has a language built-in `!=` operator or implements `IEqualityOperators<T, T, bool>.operator !=(T, T)` non-explicitly, so we can safely access it as an `!=` operator call directly.

					builder.Append($$"""
						 => left.{{privateValueFieldName}} != right.{{privateValueFieldName}};

						""");
				}
				else
				{
					// The underlying type implements `IEqualityOperators<T, T, bool>.operator !=(T, T)` explicitly. Thus we can't just access it directly.
					// See the comment above for the `IsValid` property for an explanation about what's going on here.

					builder.Append($$"""

							{
								[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
								static bool inequalityOperator<T>(T left, T right)
									where T : notnull, global::System.Numerics.IEqualityOperators<T, T, bool>
									=> left != right;

								return inequalityOperator(left.{{privateValueFieldName}}, right.{{privateValueFieldName}});
							}

						""");
				}
			}
		}

		if (generateValueConversionOperators)
		{
			if (targetType
					.GetMembers(WellKnownMemberNames.ExplicitConversionName)
					.Concat(targetType
						.GetMembers(WellKnownMemberNames.ImplicitConversionName))
					.OfType<IMethodSymbol>()
					.FirstOrDefault(@operator => @operator is { Parameters: [{ Type: var argType }], ReturnType: var returnType }
						&& argType.Equals(underlyingType, SymbolEqualityComparer.Default)
						&& returnType.Equals(targetType, SymbolEqualityComparer.Default))
					is null) // We don't generate the `explicit operator <targetType>(<underlyingType>)` conversion operator if the target type already has a similar conversion operator defined (we assume the user has explicitly defined their own implementation).
			{
				if (!generateValueConstructor
					&& !targetType.InstanceConstructors.Any(ctor => ctor is { Parameters: [{ Type: var argType }] } && argType.Equals(underlyingType, SymbolEqualityComparer.Default)))
				{
					// The operator's implementation relies on the existence of the value constructor. If there's none, we can't generate the operator.
					// We just discard what we've done so far, report a diagnostic, and proceed to the next target type, if any.

					spc.ReportDiagnostic(Diagnostic.Create(
						descriptor: mMissingValueConstructorDescriptor,
						location: location,
						targetType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
						underlyingType.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
						" in order to generate the value conversion operators"
					));

					return;
				}

				builder.Append($$"""

						/// <summary>
						/// Converts the specified numerical ID value to a <see cref="{{typeNameCRef}}"/> instance
						/// </summary>
						/// <param name="value">The numerical ID value to convert</param>
						/// <returns>A <see cref="{{typeNameCRef}}"/> instance with the specified numerical ID value</returns>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
						public static explicit operator {{targetType.Name}}({{fullyQualifiedUnderlyingTypeName}} value) => new(value);

					""");
			}

			if (targetType
					.GetMembers(WellKnownMemberNames.ExplicitConversionName)
					.Concat(targetType
						.GetMembers(WellKnownMemberNames.ImplicitConversionName))
					.OfType<IMethodSymbol>()
					.FirstOrDefault(@operator => @operator is { Parameters: [{ Type: var argType }], ReturnType: var returnType }
						&& argType.Equals(targetType, SymbolEqualityComparer.Default)
						&& returnType.Equals(underlyingType, SymbolEqualityComparer.Default))
					is null) // We don't generate the `explicit operator <underlyingType>(<targetType>)` conversion operator if the target type already has a similar conversion operator defined (we assume the user has explicitly defined their own implementation).
			{
				// Unlike the vice-versa conversion operator, we don't rely on anything special (aside from the value field) to implement this conversion operator, so this will never fail.

				builder.Append($$"""

						/// <summary>
						/// Converts the specified <see cref="{{typeNameCRef}}"/> instance to its numerical ID value
						/// </summary>
						/// <param name="value">The <see cref="{{typeNameCRef}}"/> instance to convert</param>
						/// <returns>The numerical ID value of the specified <see cref="{{typeNameCRef}}"/> instance</returns>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
						public static explicit operator {{fullyQualifiedUnderlyingTypeName}}({{targetType.Name}} value) => value.{{privateValueFieldName}};

					""");
			}
		}

		builder.Append("""
			}

			#nullable restore
			""");

		spc.AddSource($"{targetType.Name}.g.cs", SourceText.From(
			text: builder.ToString(),
			encoding: Encoding.UTF8
		));
	}
}
