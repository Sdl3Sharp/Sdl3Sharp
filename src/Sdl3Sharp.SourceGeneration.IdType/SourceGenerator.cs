using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Sdl3Sharp.SourceGeneration.IdType.Constants;

namespace Sdl3Sharp.SourceGeneration.IdType;

[Generator(LanguageNames.CSharp)]
internal sealed partial class SourceGenerator : IIncrementalGenerator
{
	private const string DiagnosticDescriptorIdPrefix = "SDL3IDT";
	private const string DiagnosticDescriptorCategory = $"{nameof(Sdl3Sharp)}.{nameof(SourceGeneration)}.{nameof(IdType)}";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(static pic => pic.AddEmbeddedAttributeDefinition());

		context.RegisterPostInitializationOutput(GenerateIdTypeAttributeSource);

		context.RegisterImplementationSourceOutput(
			source: context.CompilationProvider.Combine(
				context.SyntaxProvider.ForAttributeWithMetadataName($"Sdl3Sharp.SourceGeneration.IdType.{IdTypeAttribute}`2",
					predicate: static (node, _) => node is StructDeclarationSyntax,
					transform: static (gasc, cancellationToken) => gasc switch
					{
						{ TargetSymbol: INamedTypeSymbol targetType, Attributes: [{ AttributeClass.TypeArguments: [var entityType, var underlyingType], NamedArguments: var namedArguments, ApplicationSyntaxReference: var syntaxRef }, ..] } =>
						(
							targetType,
							entityType,
							underlyingType,
							invalidValue: namedArguments.FirstOrDefault(static p => p.Key is InvalidValue) switch
							{
								{ Key: InvalidValue, Value: { Kind: TypedConstantKind.Primitive, Value: var invalidValue } } => invalidValue,
								_ => null // if this is null, we later on have to generate code the defaults to INumberBase<TUnderlying>.Zero (because that's what the attribute does)
							},
							privateValueFieldName: namedArguments.FirstOrDefault(static p => p.Key is PrivateValueFieldName) switch
							{
								{ Key: PrivateValueFieldName, Value: { Kind: TypedConstantKind.Primitive, Value: string privateValueFieldName } } => privateValueFieldName,
								_ => PrivateValueFieldNameDefaultValue // defaults to "mValue"
							},
							generateXmlDocumentationForType: namedArguments.FirstOrDefault(static p => p.Key is GenerateXmlDocumentationForType) switch
							{
								{ Key: GenerateXmlDocumentationForType, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateXmlDocumentationForType } } => generateXmlDocumentationForType,
								_ => true // defaults to true (see attribute definition)
							},
							generatePrivateValueFieldReadOnly: namedArguments.FirstOrDefault(static p => p.Key is GeneratePrivateValueFieldReadOnly) switch
							{
								{ Key: GeneratePrivateValueFieldReadOnly, Value: { Kind: TypedConstantKind.Primitive, Value: bool generatePrivateValueFieldReadOnly } } => generatePrivateValueFieldReadOnly,
								_ => true // defaults to true (see attribute definition)
							},
							generateValueConstructor: namedArguments.FirstOrDefault(static p => p.Key is GenerateValueConstructor) switch
							{
								{ Key: GenerateValueConstructor, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateValueConstructor } } => generateValueConstructor,
								_ => true // defaults to true (see attribute definition)
							},
							generateValueProperty: namedArguments.FirstOrDefault(static p => p.Key is GenerateValueProperty) switch
							{
								{ Key: GenerateValueProperty, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateValueProperty } } => generateValueProperty,
								_ => true // defaults to true (see attribute definition)
							},
							generateValueConversionOperators: namedArguments.FirstOrDefault(static p => p.Key is GenerateValueConversionOperators) switch
							{
								{ Key: GenerateValueConversionOperators, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateValueConversionOperators } } => generateValueConversionOperators,
								_ => true // defaults to true (see attribute definition)
							},
							generateIsValidProperty: namedArguments.FirstOrDefault(static p => p.Key is GenerateIsValidProperty) switch
							{
								{ Key: GenerateIsValidProperty, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateIsValidProperty } } => generateIsValidProperty,
								_ => true // defaults to true (see attribute definition)
							},
							generateInvalidProperty: namedArguments.FirstOrDefault(static p => p.Key is GenerateInvalidProperty) switch
							{
								{ Key: GenerateInvalidProperty, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateInvalidProperty } } => generateInvalidProperty,
								_ => true // defaults to true (see attribute definition)
							},
							generateIEquatableImplementation: namedArguments.FirstOrDefault(static p => p.Key is GenerateIEquatableImplementation) switch
							{
								{ Key: GenerateIEquatableImplementation, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateIEquatableImplementation } } => generateIEquatableImplementation,
								_ => true // defaults to true (see attribute definition)
							},
							generateIEqualityOperatorsImplementation: namedArguments.FirstOrDefault(static p => p.Key is GenerateIEqualityOperatorsImplementation) switch
							{
								{ Key: GenerateIEqualityOperatorsImplementation, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateIEqualityOperatorsImplementation } } => generateIEqualityOperatorsImplementation,
								_ => true // defaults to true (see attribute definition)
							},
							generateIFormattableImplementation: namedArguments.FirstOrDefault(static p => p.Key is GenerateIFormattableImplementation) switch
							{
								{ Key: GenerateIFormattableImplementation, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateIFormattableImplementation } } => generateIFormattableImplementation,
								_ => true // defaults to true (see attribute definition)
							},
							generateISpanFormattableImplementation: namedArguments.FirstOrDefault(static p => p.Key is GenerateISpanFormattableImplementation) switch
							{
								{ Key: GenerateISpanFormattableImplementation, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateISpanFormattableImplementation } } => generateISpanFormattableImplementation,
								_ => true // defaults to true (see attribute definition)
							},
							generateDebuggerDisplayImplementation: namedArguments.FirstOrDefault(static p => p.Key is GenerateDebuggerDisplayImplementation) switch
							{
								{ Key: GenerateDebuggerDisplayImplementation, Value: { Kind: TypedConstantKind.Primitive, Value: bool generateDebuggerDisplayImplementation } } => generateDebuggerDisplayImplementation,
								_ => true // defaults to true (see attribute definition)
							},
							location: syntaxRef?.GetSyntax(cancellationToken).GetLocation()
						),
						_ => default // despite the annotated nullability of `targetType` being non-nullable, using `default` here will set the field to null, which allows us to filter out invalid attribute applications later
					}
				)
					.Where(static t => t.targetType is not null) // filter out invalid attribute applications
					.Collect()
				),
			action: GenerateIdTypesSources
		);
	}
}
