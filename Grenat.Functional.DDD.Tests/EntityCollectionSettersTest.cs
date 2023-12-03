namespace Grenat.Functional.DDD.Tests;

[TestClass]
internal class EntityCollectionSettersTest : EntityTestBase
{
    [TestMethod]
    public void Test010_When_setting_a_collection_of_entities_in_an_entity_then_its_collection_is_updated()
    {
        ImmutableList<Entity<TestEntity>> subEntities = ImmutableList<Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(TestEntity.Create(1));
        subEntities = subEntities.Add(TestEntity.Create(2));

        var sut = ContainerEntity.Create();
        sut = sut.SetEntityList(subEntities, static (e, l) => e with { SubEntities = l });

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.SubEntities?.Count) == 2);
    }

    [TestMethod]
    public void Test020_When_setting_a_collection_of_entities_in_an_invalid_entity_then_the_entity_stays_invalid()
    {
        ImmutableList<Entity<TestEntity>> subEntities = ImmutableList<Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(TestEntity.Create(1));
        subEntities = subEntities.Add(TestEntity.Create(2));

        var sut = Entity<ContainerEntity>.Invalid(new Error("Invalid entity"));
        sut = sut.SetEntityList(subEntities, static (e, l) => e with { SubEntities = l });

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

        var sut = ContainerEntity.Create();
        sut = sut.SetEntityList(subEntities, static (e, l) => e with { SubEntities = l });

        Assert.IsFalse(sut.IsValid);
        Assert.IsTrue(sut.Errors.Count() == 2);
    }

    [TestMethod]
    public void Test040_When_setting_a_list_with_null_entities_in_an_entity_then_its_collection_is_not_updated()
    {
        var sut = ContainerEntity.Create();
        ImmutableList<Entity<TestEntity>> subEntities = null!;
        sut = sut.SetEntityList(subEntities, static (e, l) => e with { SubEntities = l });

        Assert.IsTrue(sut.IsValid);
    }

    [TestMethod]
    public void Test050_When_setting_a_null_entity_collection_an_entity_then_its_collection_is_empty()
    {
        ImmutableList<Entity<TestEntity>> entities = null!;

        var sut = ContainerEntity.Create();
        sut = sut.SetEntityList(entities, static (e, l) => e with { SubEntities = l });

        Assert.IsTrue(sut.Match(
            Valid: v => !v.SubEntities.Any(),
            Invalid: e => false));
    }

    [TestMethod]
    public void Test050_When_setting_a_dictionary_of_entities_in_an_entity_then_its_dictionary_is_updated()
    {
        ImmutableDictionary<int, Entity<TestEntity>> subEntities = ImmutableDictionary<int, Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(1, TestEntity.Create(1));
        subEntities = subEntities.Add(2, TestEntity.Create(2));

        var sut = ContainerEntity.Create();
        sut = sut.SetEntityDictionary(subEntities, static (e, l) => e with { SubEntitiesDictionary = l });

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.SubEntitiesDictionary?.Count) == 2);

    }

    [TestMethod]
    public void Test060_When_setting_a_dictionary_of_entities_in_an_invalid_entity_then_the_entity_stays_invalid()
    {
        ImmutableDictionary<int, Entity<TestEntity>> subEntities = ImmutableDictionary<int, Entity<TestEntity>>.Empty;
        subEntities = subEntities.Add(1, TestEntity.Create(1));
        subEntities = subEntities.Add(2, TestEntity.Create(2));

        var sut = Entity<ContainerEntity>.Invalid(new Error("Invalid entity"));
        sut = sut.SetEntityDictionary(subEntities, static (e, l) => e with { SubEntitiesDictionary = l });

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

        var sut = ContainerEntity.Create();
        sut = sut.SetEntityDictionary(subEntities, static (e, l) => e with { SubEntitiesDictionary = l });

        Assert.IsFalse(sut.IsValid);
        Assert.IsTrue(sut.Errors.Count() == 2);
    }

    [TestMethod]
    public void Test080_When_setting_a_dictionary_with_null_entities_in_an_entity_then_its_collection_is_not_updated()
    {
        var sut = ContainerEntity.Create();
        ImmutableDictionary<int, Entity<TestEntity>> subEntities = null!;
        sut = sut.SetEntityDictionary(subEntities, static (e, l) => e with { SubEntitiesDictionary = l });

        Assert.IsTrue(sut.IsValid);
    }

    [TestMethod]
    public void Test090_When_setting_a_null_entity_dictionary_an_entity_then_its_collection_is_empty()
    {
        ImmutableDictionary<int, Entity<TestEntity>> entities = null!;

        var sut = ContainerEntity.Create();
        sut = sut.SetEntityDictionary(entities, static (e, l) => e with { SubEntitiesDictionary = l });

        Assert.IsTrue(sut.Match(
            Valid: v => !v.SubEntitiesDictionary.Any(),
            Invalid: e => false));
    }
}
