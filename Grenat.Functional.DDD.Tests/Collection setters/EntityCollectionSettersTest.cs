namespace Grenat.Functional.DDD.Tests;

[TestClass]
public class EntityCollectionSettersTest : TestBase
{
    [TestMethod]
    public void Test001_When_setting_a_entities_hashset_in_an_entity_then_the_hashset_is_updated()
    {
        var hashSet = new HashSet<Entity<TestEntity>>
        {
            TestEntity.Create(1),
            TestEntity.Create(2)
        }.Traverse();

        var sut = MainEntity.Create()
            .Set(hashSet, static (e, l) => e with { EntityHashSet = l.ToHashSet() });

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.EntityHashSet?.Count) == 2);
    }

    [TestMethod]
    public void Test002_When_setting_a_entities_hashset_with_an_invalid_entity_then_the_entity_is_invalid()
    {
        var hashSet = new HashSet<Entity<TestEntity>>
        {
            TestEntity.Create(1),
            Entity<TestEntity>.Invalid(new Error("Error"))
        }.Traverse();

        var sut = MainEntity.Create()
            .Set(hashSet, static (e, l) => e with { EntityHashSet = l.ToHashSet() });

        Assert.IsTrue(sut.Match(
            Invalid: e => true,
            Valid: v => false));
    }


    [TestMethod]
    public void Test010_When_setting_a_collection_of_entities_in_an_entity_then_its_collection_is_updated()
    {
        ImmutableList<Entity<TestEntity>> subEntities = ImmutableList<Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(TestEntity.Create(1));
        subEntities = subEntities.Add(TestEntity.Create(2));

        var l = new HashSet<Entity<TestEntity>>
        {
            TestEntity.Create(1),
            TestEntity.Create(2)
        };
        var t = subEntities.Traverse();

        var sut = MainEntity.Create()
                    .SetCollection(subEntities, static (e, l) => e with { ImmutableEntityList = l });
        //            .Set(result, static (e, l) => e with { EntityHashSet = l.ToHashSet() });

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.ImmutableEntityList?.Count) == 2);
    }

    [TestMethod]
    public void Test020_When_setting_a_collection_of_entities_in_an_invalid_entity_then_the_entity_stays_invalid()
    {
        ImmutableList<Entity<TestEntity>> subEntities = ImmutableList<Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(TestEntity.Create(1));
        subEntities = subEntities.Add(TestEntity.Create(2));

        var sut = Entity<MainEntity>.Invalid(new Error("Invalid entity"));
        sut = sut.SetCollection(subEntities, static (e, l) => e with { ImmutableEntityList = l });
        var t = TestEntity.Create(5);
        sut = sut.Set(t, (c, o) => c.EntityHashSet.Add(o));
        sut = sut.Set(5, (c, i) => c.Id = i);

        Assert.IsFalse(sut.IsValid);
    }

    [TestMethod]
    public void Test030_When_setting_a_collection_with_invalid_entities_in_an_entity_then_its_collection_is_not_updated_and_errors_are_harvested()
    {
        ImmutableList<Entity<TestEntity>> subEntities = ImmutableList<Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(TestEntity.Create(1));
        subEntities = subEntities.Add(TestEntity.Create(2));
        subEntities = subEntities.Add(new Error("A first invalid entity"));
        subEntities = subEntities.Add(new Error("A second invalid entity"));

        var sut = MainEntity.Create();
        sut = sut.SetCollection(subEntities, static (e, l) => e with { ImmutableEntityList = l });

        Assert.IsFalse(sut.IsValid);
        Assert.IsTrue(sut.Errors.Count() == 2);
    }

    [TestMethod]
    public void Test040_When_setting_a_list_with_null_entities_in_an_entity_then_its_collection_is_not_updated()
    {
        var sut = MainEntity.Create();
        ImmutableList<Entity<TestEntity>> subEntities = null!;
        sut = sut.SetCollection(subEntities, static (e, l) => e with { ImmutableEntityList = l });

        Assert.IsTrue(sut.IsValid);
    }

    [TestMethod]
    public void Test050_When_setting_a_null_entity_collection_an_entity_then_its_collection_is_empty()
    {
        ImmutableList<Entity<TestEntity>> entities = null!;

        var sut = MainEntity.Create();
        sut = sut.SetCollection(entities, static (e, l) => e with { ImmutableEntityList = l });

        Assert.IsTrue(sut.Match(
            Valid: v => !v.ImmutableEntityList.Any(),
            Invalid: e => false));
    }

    [TestMethod]
    public void Test050_When_setting_a_dictionary_of_entities_in_an_entity_then_its_dictionary_is_updated()
    {
        ImmutableDictionary<int, Entity<TestEntity>> subEntities = ImmutableDictionary<int, Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(1, TestEntity.Create(1));
        subEntities = subEntities.Add(2, TestEntity.Create(2));

        var sut = MainEntity.Create();
        sut = sut.SetDictionary(subEntities, static (e, l) => e with { ImmutableEntityDictionary = l });

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.ImmutableEntityDictionary?.Count) == 2);

    }

    [TestMethod]
    public void Test060_When_setting_a_dictionary_of_entities_in_an_invalid_entity_then_the_entity_stays_invalid()
    {
        ImmutableDictionary<int, Entity<TestEntity>> subEntities = ImmutableDictionary<int, Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(1, TestEntity.Create(1));
        subEntities = subEntities.Add(2, TestEntity.Create(2));

        var sut = Entity<MainEntity>.Invalid(new Error("Invalid entity"));
        sut = sut.SetDictionary(subEntities, static (e, l) => e with { ImmutableEntityDictionary = l });

        Assert.IsFalse(sut.IsValid);

    }

    [TestMethod]
    public void Test070_When_setting_a_dictionary_with_invalid_entities_in_an_entity_then_its_collection_is_not_updated_and_errors_are_harvested()
    {
        ImmutableDictionary<int, Entity<TestEntity>> subEntities = ImmutableDictionary<int, Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(1, TestEntity.Create(1));
        subEntities = subEntities.Add(2, TestEntity.Create(2));
        subEntities = subEntities.Add(3, new Error("A first invalid entity"));
        subEntities = subEntities.Add(4, new Error("A second invalid entity"));

        var sut = MainEntity.Create();
        sut = sut.SetDictionary(subEntities, static (e, l) => e with { ImmutableEntityDictionary = l });

        Assert.IsFalse(sut.IsValid);
        Assert.IsTrue(sut.Errors.Count() == 2);
    }

    [TestMethod]
    public void Test080_When_setting_a_dictionary_with_null_entities_in_an_entity_then_its_collection_is_not_updated()
    {
        var sut = MainEntity.Create();
        ImmutableDictionary<int, Entity<TestEntity>> subEntities = null!;
        sut = sut.SetDictionary(subEntities, static (e, l) => e with { ImmutableEntityDictionary = l });

        Assert.IsTrue(sut.IsValid);
    }

    [TestMethod]
    public void Test090_When_setting_a_null_entity_dictionary_an_entity_then_its_collection_is_empty()
    {
        ImmutableDictionary<int, Entity<TestEntity>> entities = null!;

        var sut = MainEntity.Create();
        sut = sut.SetDictionary(entities, static (e, l) => e with { ImmutableEntityDictionary = l });

        Assert.IsTrue(sut.Match(
            Valid: v => !v.ImmutableEntityDictionary.Any(),
            Invalid: e => false));
    }
}
