using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using static Sdl3Sharp.SourceGeneration.IdType.Constants;

namespace Sdl3Sharp.SourceGeneration.IdType;

partial class Constants
{
	public const string IdTypeAttribute = nameof(IdTypeAttribute);
	public const string InvalidValue = nameof(InvalidValue);
	public const string PrivateValueFieldName = nameof(PrivateValueFieldName);
	public const string PrivateValueFieldNameDefaultValue = "mValue";
	public const string GenerateXmlDocumentationForType = nameof(GenerateXmlDocumentationForType);
	public const string GeneratePrivateValueFieldReadOnly = nameof(GeneratePrivateValueFieldReadOnly);
	public const string GenerateValueConstructor = nameof(GenerateValueConstructor);
	public const string GenerateValueProperty = nameof(GenerateValueProperty);
	public const string GenerateValueConversionOperators = nameof(GenerateValueConversionOperators);
	public const string GenerateIsValidProperty = nameof(GenerateIsValidProperty);
	public const string GenerateIEquatableImplementation = nameof(GenerateIEquatableImplementation);
	public const string GenerateIEqualityOperatorsImplementation = nameof(GenerateIEqualityOperatorsImplementation);
	public const string GenerateIFormattableImplementation = nameof(GenerateIFormattableImplementation);
	public const string GenerateISpanFormattableImplementation = nameof(GenerateISpanFormattableImplementation);
	public const string GenerateDebuggerDisplayImplementation = nameof(GenerateDebuggerDisplayImplementation);
}

partial class SourceGenerator
{
	private static void GenerateIdTypeAttributeSource(IncrementalGeneratorPostInitializationContext context)
	{
		context.AddSource($"Sdl3Sharp.SourceGeneration.IdType.{IdTypeAttribute}.g.cs", SourceText.From(
			text: $$"""
				#nullable enable

				namespace Sdl3Sharp.SourceGeneration.IdType;
			
				[global::Microsoft.CodeAnalysis.EmbeddedAttribute]
				[global::System.AttributeUsage(global::System.AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
				internal sealed class {{IdTypeAttribute}}<TEntity, TUnderlying> : global::System.Attribute
					where TEntity : notnull
					where TUnderlying : notnull, global::System.Numerics.INumberBase<TUnderlying>
				{				
					public TUnderlying {{InvalidValue}} { get; init; } = TUnderlying.Zero;

					public string {{PrivateValueFieldName}} { get; init; } = {{SymbolDisplay.FormatLiteral(PrivateValueFieldNameDefaultValue, quote: true)}};

					public bool {{GenerateXmlDocumentationForType}} { get; init; } = true;

					public bool {{GeneratePrivateValueFieldReadOnly}} { get; init; } = true;

					public bool {{GenerateValueConstructor}} { get; init; } = true;

					public bool {{GenerateValueProperty}} { get; init; } = true;

					public bool {{GenerateValueConversionOperators}} { get; init; } = true;

					public bool {{GenerateIsValidProperty}} { get; init; } = true;

					public bool {{GenerateIEquatableImplementation}} { get; init; } = true;

					public bool {{GenerateIEqualityOperatorsImplementation}} { get; init; } = true;

					public bool {{GenerateIFormattableImplementation}} { get; init; } = true;

					public bool {{GenerateISpanFormattableImplementation}} { get; init; } = true;

					public bool {{GenerateDebuggerDisplayImplementation}} { get; init; } = true;
				}
			
				#nullable restore
				""",
			encoding: Encoding.UTF8
		));
	}
}
