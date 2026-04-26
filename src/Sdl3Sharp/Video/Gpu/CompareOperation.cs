namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents a comparison operator for depth, stencil and sampler operations
/// </summary>
public enum CompareOperation
{
	/// <summary>Represents an invalid comparison operation</summary>
	Invalid,

	/// <summary>The comparison always evaluates to <c>false</c></summary>
	Never,

	/// <summary>The comparison evaluates <c>reference &lt; test</c></summary>
	LessThan,

	/// <summary>The comparison evaluates <c>reference == test</c></summary>
	Equals,

	/// <summary>The comparison evaluates <c>reference &lt;= test</c></summary>
	LessThanOrEquals,

	/// <summary>The comparison evaluates <c>reference &gt; test</c></summary>
	GreaterThan,

	/// <summary>The comparison evaluates <c>reference != test</c></summary>
	NotEquals,

	/// <summary>The comparison evaluates <c>reference &gt;= test</c></summary>
	GreaterThanOrEquals,

	/// <summary>The comparison always evaluates to <c>true</c></summary>
	Always
}
