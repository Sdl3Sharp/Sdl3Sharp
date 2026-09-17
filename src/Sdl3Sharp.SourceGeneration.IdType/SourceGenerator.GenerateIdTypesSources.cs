using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Sdl3Sharp.SourceGeneration.IdType;

partial class SourceGenerator
{
	private static void GenerateIdTypesSources(
		SourceProductionContext spc,
		(
			Compilation compilation,
			ImmutableArray<
			(
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
				bool generateIEquatableImplementation,
				bool generateIEqualityOperatorsImplementation,
				bool generateIFormattableImplementation,
				bool generateISpanFormattableImplementation,
				bool generateDebuggerDisplayImplementation,
				Location? location
			)> attributeApplications
		) values
	)
	{
		var builder = new StringBuilder();

		var (compilation, attributeApplications) = values;

		if (attributeApplications.IsDefaultOrEmpty)
		{
			// there's nothing to do for us
			return;
		}

		var iNumberBaseType = compilation.GetTypeByMetadataName("System.Numerics.INumberBase`1")?.OriginalDefinition;
		var iEquatableType = compilation.GetTypeByMetadataName("System.IEquatable`1")?.OriginalDefinition;

		var iFormatProviderType = compilation.GetTypeByMetadataName("System.IFormatProvider")?.OriginalDefinition;

		var iFormattableType = compilation.GetTypeByMetadataName("System.IFormattable")?.OriginalDefinition;
		var iFormattableToStringMethod = iFormattableType?
			.GetMembers("ToString")
			.OfType<IMethodSymbol>()
			.FirstOrDefault(method => method is { Parameters: [{ Type.SpecialType: SpecialType.System_String }, { Type: ITypeSymbol secondArgType }] }
				&& secondArgType.Equals(iFormatProviderType, SymbolEqualityComparer.Default))?
			.OriginalDefinition; // `IFormattable.ToString(string, IFormatProvider)`

		var spanType = compilation.GetTypeByMetadataName("System.Span`1")?.OriginalDefinition;
		var spanCharType = spanType?.Construct(compilation.GetSpecialType(SpecialType.System_Char));
		var readOnlySpanType = compilation.GetTypeByMetadataName("System.ReadOnlySpan`1")?.OriginalDefinition;
		var readOnlySpanCharType = readOnlySpanType?.Construct(compilation.GetSpecialType(SpecialType.System_Char));

		var iSpanFormattableType = compilation.GetTypeByMetadataName("System.ISpanFormattable")?.OriginalDefinition;
		var iSpanFormattableTryFormatMethod = iSpanFormattableType? 
			.GetMembers("TryFormat")
			.OfType<IMethodSymbol>()
			.FirstOrDefault(method => method is { Parameters: [{ Type: INamedTypeSymbol firstArgType }, { Type.SpecialType: SpecialType.System_Int32, RefKind: RefKind.Out }, { Type: INamedTypeSymbol thirdArgType }, { Type: ITypeSymbol fourthArgType }] }
				&& firstArgType.Equals(spanCharType, SymbolEqualityComparer.Default)
				&& thirdArgType.Equals(readOnlySpanCharType, SymbolEqualityComparer.Default)
				&& fourthArgType.Equals(iFormatProviderType, SymbolEqualityComparer.Default))?
			.OriginalDefinition; // `ISpanFormattable.TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider)`

		var iEqualityOperatorsType = compilation.GetTypeByMetadataName("System.Numerics.IEqualityOperators`3")?.OriginalDefinition;

		foreach (var
			(
				targetType,
				entityType,
				underlyingType,
				invalidValue,
				privateValueFieldName,
				generateTypeXmlDocumentation,
				generatePrivateFieldReadOnly,
				generateValueConstructor,
				generateValueProperty,
				generateValueConversionOperators,
				generateIsValidProperty,
				generateIEquatableImplementation,
				generateIEqualityOperatorsImplementation,
				generateIFormattableImplementation,
				generateISpanFormattableImplementation,
				generateDebuggerDisplayImplementation,
				location
			) in attributeApplications)
		{
			GenerateIdTypeSource(
				spc,
				compilation,
				targetType,
				entityType,
				underlyingType,
				invalidValue,
				privateValueFieldName,
				generateTypeXmlDocumentation,
				generatePrivateFieldReadOnly,
				generateValueConstructor,
				generateValueProperty,
				generateValueConversionOperators,
				generateIsValidProperty,
				generateIEquatableImplementation,
				generateIEqualityOperatorsImplementation,
				generateIFormattableImplementation,
				generateISpanFormattableImplementation,
				generateDebuggerDisplayImplementation,
				location,
				iNumberBaseType,
				iEquatableType,
				iFormatProviderType,
				iFormattableToStringMethod,
				spanCharType,
				readOnlySpanCharType,
				iSpanFormattableTryFormatMethod,
				iEqualityOperatorsType,
				builder
			);
		}
	}
}
