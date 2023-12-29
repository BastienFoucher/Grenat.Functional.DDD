namespace Grenat.Functional.DDD.Tests;

internal class TestValueObject
{
    public readonly int Value = 0;

    public TestValueObject(int value)
    {
        Value = value;
    }

    public static ValueObject<TestValueObject> Create(int value)
    {
        return ValueObject<TestValueObject>.Valid(new TestValueObject(value));
    }
}
