using TeamTasks.Domain.Common.Core.Errors;
using TeamTasks.Domain.Common.Core.Primitives;
using TeamTasks.Domain.Common.Core.Primitives.Result;
using TeamTasks.Domain.Core.Primitives.Result;

namespace TeamTasks.Domain.Common.ValueObjects;

/// <summary>
/// Represents the name value object.
/// </summary>
public class Name : ValueObject
{
    /// <summary>
    /// The name maximum length.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="Name"/> class.
    /// </summary>
    /// <param name="value">The name value.</param>
    public Name(string value) => Value = value;

    /// <summary>
    /// Gets the name value.
    /// </summary>
    public string Value { get; set; }

    public static implicit operator string(Name name) =>
        name.Value;

    public static implicit operator Name(string name) =>
        new(name);
    
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Creates a new <see cref="Name"/> instance based on the specified value.
    /// </summary>
    /// <param name="name">The name value.</param>
    /// <returns>The result of the name creation process containing the name or an error.</returns>
    public static Result<Name> Create(string name) =>
        Result.Create(name, DomainErrors.Name.NullOrEmpty)
            .Ensure(n => !string.IsNullOrWhiteSpace(n), DomainErrors.Name.NullOrEmpty)
            .Ensure(n => n.Length <= MaxLength, DomainErrors.Name.LongerThanAllowed)
            .Map(f => new Name(f));

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}