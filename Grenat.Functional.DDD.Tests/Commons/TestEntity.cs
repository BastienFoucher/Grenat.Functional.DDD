namespace Grenat.Functional.DDD.Tests;

internal record TestEntity
{
    public readonly int Value = 0;

    public TestEntity(int value)
    {
        Value = value;
    }

    public static Entity<TestEntity> Create(int value)
    {
        return Entity<TestEntity>.Valid(new TestEntity(value));
    }
}
