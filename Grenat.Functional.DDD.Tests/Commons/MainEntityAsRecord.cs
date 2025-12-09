namespace Grenat.Functional.DDD.Tests;

internal record MainEntityAsRecord
{
    public int Id { get; set; }
    public List<int>? List { get; set; }
    public PositiveValueObject? ValueObject { get; set; }
    public ChildEntity? Entity { get; set; }
    public ICollection<ChildEntity> ImmutableEntityList { get; set; }
    public HashSet<ChildEntity> EntityHashSet { get; set; }
    public IDictionary<int, ChildEntity> ImmutableEntityDictionary { get; set; }
    public Option<ChildEntity> ChildOptionEntity { get; set; }

    public ICollection<PositiveValueObject> ImmutableValueObjectList;
    public Option<PositiveValueObject> ValueObjectOption { get; set; }

    public MainEntityAsRecord(
        ICollection<ChildEntity> immutableEntityList,
        IDictionary<int, ChildEntity> immutableEntityDictionary,
        Option<ChildEntity> entityOption,
        ImmutableList<PositiveValueObject> immutableValueObjectList,
        Option<PositiveValueObject> valueObjectOption,
        HashSet<ChildEntity> entityHashSet
        )
    {
        ImmutableEntityList = immutableEntityList;
        ImmutableEntityDictionary = immutableEntityDictionary;
        ChildOptionEntity = entityOption;
        ImmutableValueObjectList = immutableValueObjectList;
        ValueObjectOption = valueObjectOption;
        EntityHashSet = entityHashSet;
    }

    public static Entity<MainEntityAsRecord> Create()
    {
        return Entity<MainEntityAsRecord>.Valid(new MainEntityAsRecord(
            ImmutableHashSet<ChildEntity>.Empty,
            ImmutableDictionary<int, ChildEntity>.Empty,
            None<ChildEntity>(),
            ImmutableList<PositiveValueObject>.Empty,
            None<PositiveValueObject>(),
            new HashSet<ChildEntity>()));
    }

    public class MainEntityAsClass
    {
        public PositiveValueObject? ValueObject1 { get; set; }
        public PositiveValueObject? ValueObject2 { get; set; }
    }
}