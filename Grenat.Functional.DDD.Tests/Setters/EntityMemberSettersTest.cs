namespace Grenat.Functional.DDD.Tests;

[TestClass]
public class EntityMemberSettersTest : TestBase
{
    [TestMethod]
    public void Test010_When_setting_a_member_in_an_entity_then_the_entity_is_updated()
    {
        var sut = MainEntity.Create();
        sut = sut.Set(99, static (e, me) => e with { Id = me });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.Id == 99));
    }

    [TestMethod]
    public void Test011_When_setting_an_object_member_in_an_entity_then_the_entity_is_updated()
    {
        var sut = MainEntity.Create();
        sut = sut.Set(new List<int>() { 1,2,3,4}, static (e, me) => e with { List = me });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.List!.Count == 4));
    }

    [TestMethod]
    public void Test020_When_setting_a_null_member_in_an_entity_then_the_entity_is_not_updated()
    {
        List<int> list = null!;

        var sut = MainEntity.Create();
        sut = sut.Set(list, static (e, me) => e with { List = me });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.List == null));
    }

    [TestMethod]
    public void Test040_When_setting_a_member_in_an_entity_using_an_action_then_the_entity_is_updated()
    {
        var sut = MainEntity.Create();
        sut = sut.Set(99, static (e, me) => e.Id = me);

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.Id == 99));
    }

    [TestMethod]
    public void Test050_When_setting_a_nullmember_in_an_entity_using_an_action_then_the_entity_is_not_updated()
    {
        List<int> list = null!;

        var sut = MainEntity.Create();
        sut = sut.Set(list, static (e, me) => e.List = me);

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.List == null));
    }
}
