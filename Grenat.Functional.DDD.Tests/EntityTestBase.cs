namespace Grenat.Functional.DDD.Tests;

public class EntityTestBase
{
    public Action<string> SimpleAction = _ => { };
    public AsyncAction<string> SimpleAsyncAction = async _ => { await Task.FromResult(string.Empty); };

    public Func<int, Entity<int>> IncrementEntityContentFunc = (int x) => Entity<int>.Valid(x + 1);
    public Func<int, int, Entity<int>> ParameterizedIncrementEntityContentFunc = (int x, int y) => Entity<int>.Valid(x + y);
    public AsyncFunc<int, Entity<int>> AwaitableIncrementEntityContentFunc = (int x) => Task.FromResult(Entity<int>.Valid(x + 1));
    public AsyncFunc<int, int, Entity<int>> AwaitableParameterizedIncrementEntityContentFunc = (int x, int y) => Task.FromResult(Entity<int>.Valid(x + y));

    public record TestEntity
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

    public record TestValueObject
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

    public record ContainerEntity
    {
        public int Id { get; set; }
        public List<int>? List { get; set; }
        public HashSet<TestEntity> EntityHashSet { get; set; }
        public TestValueObject? ValueObject { get; set; }
        public TestEntity? Entity { get; set; }
        public ImmutableList<TestEntity> SubEntities { get; set; }
        public ImmutableDictionary<int, TestEntity> SubEntitiesDictionary { get; set; }
        public Option<TestEntity> TestEntityOption { get; set; }

        public ImmutableList<TestValueObject> ValueObjects;
        public Option<TestValueObject> TestValueObjectOption { get; set; }

        public ContainerEntity(ImmutableList<TestEntity> subEntities,
            ImmutableDictionary<int, TestEntity> subEntitiesDictionary,
            Option<TestEntity> testEntityOption,
            ImmutableList<TestValueObject> valueObjects,
            Option<TestValueObject> testValueObjectOption
            )
        {
            SubEntities = subEntities;
            SubEntitiesDictionary = subEntitiesDictionary;
            TestEntityOption = testEntityOption;
            ValueObjects = valueObjects;
            TestValueObjectOption = testValueObjectOption;
        }

        public static Entity<ContainerEntity> Create()
        {
            return Entity<ContainerEntity>.Valid(new ContainerEntity(
                ImmutableList<TestEntity>.Empty,
                ImmutableDictionary<int, TestEntity>.Empty,
                None<TestEntity>(),
                ImmutableList<TestValueObject>.Empty,
                None<TestValueObject>()));
        }
    }
}
