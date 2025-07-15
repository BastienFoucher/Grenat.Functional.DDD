namespace Grenat.Functional.DDD.Tests;

internal record ChildEntity
{
    public readonly int Value = 0;

    public ChildEntity(int value)
    {
        Value = value;
    }

    public static Entity<ChildEntity> Create(int value)
    {
        return Entity<ChildEntity>.Valid(new ChildEntity(value));
    }
}
