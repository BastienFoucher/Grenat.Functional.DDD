namespace Grenat.Functional.DDD.Tests;

internal class EntitySettersTest : TestBase
{
    [TestMethod]
    public void Test010_When_setting_an_entity_in_an_entity_then_the_entity_is_updated()
    {
        var entity = TestEntity.Create(1);

        var sut = MainEntity.Create();
        sut = sut.Set(entity, static (e, vo) => e with { Entity = vo });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.Entity!.Value == 1));
    }

    [TestMethod]
    public void Test011_When_setting_an_entity_in_an_entity_using_an_action_then_the_entity_is_updated()
    {
        var entity = TestEntity.Create(1);

        var sut = MainEntity.Create();
        sut = sut.Set(entity, static (e, entity) => e.Entity = entity);

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.Entity!.Value == 1));
    }

    [TestMethod]
    public void Test020_When_setting_a_null_entity_in_an_entity_then_the_entity_is_not_updated()
    {
        Entity<TestEntity> entity = null!;

        var sut = MainEntity.Create();
        sut = sut.Set(entity, static (e, vo) => e with { Entity = vo });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.Entity == null));
    }

    [TestMethod]
    public void Test030_When_setting_an_invalid_entity_in_an_entity_then_the_entity_is_in_error()
    {
        Entity<TestEntity> entity = new Error("Invalid entity");

        var sut = MainEntity.Create();
        sut = sut.Set(entity, static (e, vo) => e with { Entity = vo });

        Assert.IsTrue(!sut.IsValid);
    }
}
