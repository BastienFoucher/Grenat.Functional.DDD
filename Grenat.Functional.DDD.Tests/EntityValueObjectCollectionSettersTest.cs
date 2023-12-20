namespace Grenat.Functional.DDD.Tests;

[TestClass]
public class EntityValueObjectCollectionSettersTest : EntityTestBase
{
    [TestMethod]
    public void Test010_When_setting_a_collection_of_valueobjects_in_an_entity_then_its_collection_is_updated()
    {
        ImmutableList<ValueObject<TestValueObject>> valueObjects = ImmutableList<ValueObject<TestValueObject>>.Empty;
        valueObjects = valueObjects.Add(TestValueObject.Create(1));
        valueObjects = valueObjects.Add(TestValueObject.Create(2));

        var sut = ContainerEntity.Create();
        sut = sut.SetImmutableList(valueObjects, static (e, l) => e with { ValueObjects = l });

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.ValueObjects?.Count) == 2);
    }

    [TestMethod]
    public void Test010_When_setting_a_collection_of_valueobjects_in_an_entity_using_an_action_then_its_collection_is_updated()
    {
        var valueObjects = ImmutableList<ValueObject<TestValueObject>>.Empty;
        valueObjects = valueObjects.Add(TestValueObject.Create(1));
        valueObjects = valueObjects.Add(TestValueObject.Create(2));

        var sut = ContainerEntity.Create();
        sut = sut.SetImmutableList(valueObjects, static (e, l) => e with { ValueObjects = l }) ;

        Assert.IsTrue(sut.Match(
            Invalid: e => 0,
            Valid: v => v.ValueObjects?.Count) == 2);
    }

    [TestMethod]
    public void Test020_When_setting_a_collection_of_valueobjects_in_an_invalid_entity_then_the_entity_stays_invalid()
    {
        ImmutableList<ValueObject<TestValueObject>> valueObjects = ImmutableList<ValueObject<TestValueObject>>.Empty;
        valueObjects = valueObjects.Add(TestValueObject.Create(1));
        valueObjects = valueObjects.Add(TestValueObject.Create(2));

        var sut = Entity<ContainerEntity>.Invalid(new Error("Invalid entity"));
        sut = sut.SetImmutableList(valueObjects, static (e, l) => e with { ValueObjects = l });

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

        var sut = ContainerEntity.Create();
        sut = sut.SetImmutableList(valueObjects, static (e, l) => e with { ValueObjects = l });

        Assert.IsFalse(sut.IsValid);
        Assert.IsTrue(sut.Errors.Count() == 2);
    }

    [TestMethod]
    public void Test040_When_setting_a_null_collection_an_entity_then_its_collection_is_empty()
    {
        ImmutableList<ValueObject<TestValueObject>> valueObjects = null!;

        var sut = ContainerEntity.Create();
        sut = sut.SetImmutableList(valueObjects, static (e, l) => e with { ValueObjects = l });

        Assert.IsTrue(sut.Match(
            Valid: v => !v.ValueObjects.Any(),
            Invalid: e => false));
    }

    [TestMethod]
    public void Test050_When_setting_a_dictionary_with_null_valueobjects_in_an_entity_then_its_collection_is_not_updated()
    {
        var sut = ContainerEntity.Create();
        ImmutableList<ValueObject<TestValueObject>> valueObjects = null!;
        sut = sut.SetImmutableList(valueObjects, static (e, l) => e with { ValueObjects = l });

        Assert.IsTrue(sut.IsValid);
    }

}
