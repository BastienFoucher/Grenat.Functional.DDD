namespace Grenat.Functional.DDD.Tests;

internal class PositiveValueObject
{
    public readonly int Value = 0;

    public PositiveValueObject(int value)
    {
        Value = value;
    }

    public static ValueObject<PositiveValueObject> Create(int value)
    {
        return value switch
        {
            < 0 => ValueObject<PositiveValueObject>.Invalid(new Error($"{value} cannot be negative")),
            _ => ValueObject<PositiveValueObject>.Valid(new PositiveValueObject(value))
        };
    }
}
