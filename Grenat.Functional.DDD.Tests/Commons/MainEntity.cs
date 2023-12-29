using System.Collections.Generic;

namespace Grenat.Functional.DDD.Tests;

internal record MainEntity
{
    public int Id { get; set; }
    public List<int>? List { get; set; }
    public TestValueObject? ValueObject { get; set; }
    public TestEntity? Entity { get; set; }
    public ImmutableList<TestEntity> ImmutableEntityList { get; set; }
    public HashSet<TestEntity> EntityHashSet { get; set; }
    public ImmutableDictionary<int, TestEntity> ImmutableEntityDictionary { get; set; }
    public Option<TestEntity> EntityOption { get; set; }

    public ImmutableList<TestValueObject> ImmutableValueObjectList;
    public Option<TestValueObject> ValueObjectOption { get; set; }

    public MainEntity(ImmutableList<TestEntity> immutableEntityList,
        ImmutableDictionary<int, TestEntity> immutableEntityDictionary,
        Option<TestEntity> entityOption,
        ImmutableList<TestValueObject> immutableValueObjectList,
        Option<TestValueObject> valueObjectOption,
        HashSet<TestEntity> entityHashSet
        )
    {
        ImmutableEntityList = immutableEntityList;
        ImmutableEntityDictionary = immutableEntityDictionary;
        EntityOption = entityOption;
        ImmutableValueObjectList = immutableValueObjectList;
        ValueObjectOption = valueObjectOption;
        EntityHashSet = entityHashSet;
    }

    public static Entity<MainEntity> Create()
    {
        return Entity<MainEntity>.Valid(new MainEntity(
            ImmutableList<TestEntity>.Empty,
            ImmutableDictionary<int, TestEntity>.Empty,
            None<TestEntity>(),
            ImmutableList<TestValueObject>.Empty,
            None<TestValueObject>(),
            new HashSet<TestEntity>()));
    }
}