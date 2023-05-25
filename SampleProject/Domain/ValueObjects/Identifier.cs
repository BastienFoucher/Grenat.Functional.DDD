namespace SampleProject.Domain.ValueObjects;

public record IdentifierError(string Message) : Error(Message);

public record Identifier
{
    public const string DEFAULT_VALUE = "";
    public const int MAX_LENGTH = 5;

    public readonly string Value = "";

    public Identifier() { Value = DEFAULT_VALUE; }

    private Identifier(string identifier)
    {
        Value = identifier;
    }

    public static ValueObject<Identifier> Create(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return new IdentifierError("An identifier cannot be null or empty.");

        return new Identifier(identifier);
    }
}
