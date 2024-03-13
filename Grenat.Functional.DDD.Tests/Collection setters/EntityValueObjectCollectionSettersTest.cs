namespace Grenat.Functional.DDD.Tests;

[TestClass]
public class EntityValueObjectCollectionSettersTest : TestBase
{
    [TestMethod]
    public void Test010_When_setting_a_collection_of_valueobjects_in_an_entity_then_its_collection_is_updated()
    {
        ImmutableList<ValueObject<TestValueObject>> valueObjects = ImmutableList<ValueObject<TestValueObject>>.Empty;
        valueObjects = valueObjects.Add(TestValueObject.Create(1));
        valueObjects = valueObjects.Add(TestValueObject.Create(2));

        var sut = MainEntity.Create();

        sut = sut.SetCollection(valueObjects, static (e, l) => e with { ImmutableValueObjectList  = l.ToImmutableList() });

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.ImmutableValueObjectList?.Count) == 2);
    }

    [TestMethod]
    public void Test010_When_setting_a_collection_of_valueobjects_in_an_entity_using_an_action_then_its_collection_is_updated()
    {
        var valueObjects = ImmutableList<ValueObject<TestValueObject>>.Empty;
        valueObjects = valueObjects.Add(TestValueObject.Create(1));
        valueObjects = valueObjects.Add(TestValueObject.Create(2));

        var sut = MainEntity.Create();
        sut = sut.SetCollection(valueObjects, static (e, l) => e with { ImmutableValueObjectList = l }) ;

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.ImmutableValueObjectList?.Count) == 2);
    }

    [TestMethod]
    public void Test020_When_setting_a_collection_of_valueobjects_in_an_invalid_entity_then_the_entity_stays_invalid()
    {
        ImmutableList<ValueObject<TestValueObject>> valueObjects = ImmutableList<ValueObject<TestValueObject>>.Empty;
        valueObjects = valueObjects.Add(TestValueObject.Create(1));
        valueObjects = valueObjects.Add(TestValueObject.Create(2));

        var sut = Entity<MainEntity>.Invalid(new Error("Invalid entity"));
        sut = sut.SetCollection(valueObjects, static (e, l) => e with { ImmutableValueObjectList = l });

        Assert.IsFalse(sut.IsValid);

    }

    [TestMethod]
    public void Test030_When_setting_a_collection_with_invalid_valueobjects_in_an_entity_then_its_collection_is_not_updated_and_errors_are_harvested()
    {
        ImmutableList<ValueObject<TestValueObject>> valueObjects = ImmutableList<ValueObject<TestValueObject>>.Empty;
        valueObjects = valueObjects.Add(TestValueObject.Create(1));
        valueObjects = valueObjects.Add(TestValueObject.Create(2));
        valueObjects = valueObjects.Add(new Error("A first invalid entity"));
        valueObjects = valueObjects.Add(new Error("A second invalid entity"));

        var sut = MainEntity.Create();
        sut = sut.SetCollection(valueObjects, static (e, l) => e with { ImmutableValueObjectList = l });

        Assert.IsFalse(sut.IsValid);
        Assert.IsTrue(sut.Errors.Count() == 2);
    }

    [TestMethod]
    public void Test030_When_setting_a_dictionary_with_invalid_valueobjects_in_an_entity_then_its_dictiorary_is_not_updated_and_errors_are_harvested()
    {
        var valueObjects = ImmutableDictionary<int, Entity<TestEntity>>.Empty;
        valueObjects = valueObjects.Add(1, TestEntity.Create(1));
        valueObjects = valueObjects.Add(2, TestEntity.Create(2));
        valueObjects = valueObjects.Add(3, new Error("A first invalid entity"));
        valueObjects = valueObjects.Add(4, new Error("A second invalid entity"));

        var sut = MainEntity.Create();
        sut = sut.SetDictionary(valueObjects, static (e, l) => e with { ImmutableEntityDictionary = l });

        Assert.IsFalse(sut.IsValid);
        Assert.IsTrue(sut.Errors.Count() == 2);
    }
}
