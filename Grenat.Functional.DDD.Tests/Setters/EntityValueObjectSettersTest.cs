namespace Grenat.Functional.DDD.Tests;

[TestClass]
public class EntityValueObjectSettersTest : TestBase
{
    [TestMethod]
    public void Test010_When_setting_a_valueobject_in_an_entity_then_the_entity_is_updated()
    {
        var valueObject = TestValueObject.Create(1);

        var sut = MainEntity.Create();
        sut = sut.Set(valueObject, static (e, vo) => e with { ValueObject = vo });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.ValueObject!.Value == 1));
    }

    [TestMethod]
    public void Test020_When_setting_a_null_valueobject_in_an_entity_then_the_entity_is_not_updated()
    {
        ValueObject<TestValueObject> valueObject = null!;

        var sut = MainEntity.Create();
        sut = sut.Set(valueObject, static (e, vo) => e with { ValueObject = vo });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.ValueObject == null));
    }

    [TestMethod]
    public void Test030_When_setting_an_invalid_valueobject_in_an_entity_then_the_entity_is_in_error()
    {
        ValueObject<TestValueObject> valueObject = new Error("Invalid value object");

        var sut = MainEntity.Create();
        sut = sut.Set(valueObject, static (e, vo) => e with { ValueObject = vo });

        Assert.IsTrue(!sut.IsValid);
    }

    [TestMethod]
    public void Test040_When_setting_a_valueobject_in_an_entity_using_an_action_then_the_entity_is_updated()
    {
        var valueObject = TestValueObject.Create(1);

        var sut = MainEntity.Create();
        sut = sut.Set(valueObject, static (e, vo) => e.ValueObject = vo);

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.ValueObject!.Value == 1));
    }


    [TestMethod]
    public void Test050_When_setting_a_null_valueobject_in_an_entity_using_an_action_then_the_entity_is_not_updated()
    {
        ValueObject<TestValueObject> valueObject = null!;

        var sut = MainEntity.Create();
        sut = sut.Set(valueObject, static (e, vo) => e.ValueObject = vo);

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.ValueObject == null));
    }

    [TestMethod]
    public void Test060_When_setting_an_invalid_valueobject_in_an_entity_using_an_action_then_the_entity_is_in_error()
    {
        ValueObject<TestValueObject> valueObject = new Error("Invalid value object");

        var sut = MainEntity.Create();
        sut = sut.Set(valueObject, static (e, vo) => e.ValueObject = vo);

        Assert.IsTrue(!sut.IsValid);
    }
}
