namespace Grenat.Functional.DDD.Tests;

[TestClass]
internal class EntityOptionSettersTest : EntityTestBase
{
    [TestMethod]
    public void Test010_When_setting_an_entity_option_then_the_entity_is_updated_with_a_value()
    {
        var entity = TestEntity.Create(1);

        var sut = ContainerEntity.Create();
        sut = sut.SetOption(entity, v => v.Value >= 1, static (entity, optionedEntity) => entity with { TestEntityOption = optionedEntity });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestEntityOption.Match(
                None: () => false,
                Some: v => v.Value == 1)));
    }

    [TestMethod]
    public void Test020_When_setting_an_entity_option_with_an_unverified_predicate_then_the_entity_is_updated_with_a_none_value()
    {
        var entity = TestEntity.Create(0);

        var sut = ContainerEntity.Create();
        sut = sut.SetOption(entity, v => v.Value >= 1, static (e, v) => e with { TestEntityOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestEntityOption.Match(
                None: () => true,
                Some: v => false)));
    }

    [TestMethod]
    public void Test030_When_setting_an_invalid_entity_in_an_entity_option_then_the_entity_invalid()
    {
        Entity<TestEntity> entity = new Error("Invalid subentity");

        var sut = ContainerEntity.Create();
        sut = sut.SetOption(entity, v => v.Value == 1, static (e, v) => e with { TestEntityOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => true));
    }

    [TestMethod]
    public void Test040_When_setting_an_invalid_entity_in_an_entity_option_then_the_errors_are_harvested()
    {
        Entity<TestEntity> entity = new Error("Invalid subentity");

        Entity<ContainerEntity> sut = new Error("Invalid entity");
        sut = sut.SetOption(entity, v => v.Value == 1, static (e, v) => e with { TestEntityOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => e.Count() == 2,
            Valid: v => false));
    }

    [TestMethod]
    public void Test050_When_setting_a_null_entity_in_an_valid_entity_then_the_resulting_option_is_none_2()
    {
        Entity<ContainerEntity> sut = ContainerEntity.Create();
        sut = sut.SetOption(
            (Entity<TestEntity>)null!,
            (entity) => true,
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => true,
            Valid: v => v.TestValueObjectOption.IsSome));
    }

    [TestMethod]
    public void Test060_When_setting_an_entity_option_without_predicate_then_the_entity_is_updated_with_some()
    {
        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            Some(TestEntity.Create(1)),
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestEntityOption.Match(
                None: () => false,
                Some: v => true)));
    }

    [TestMethod]
    public void Test070_When_setting_a_valueobject_option_without_predicate_then_the_entity_is_updated_with_none()
    {
        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            None<Entity<TestEntity>>(),
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestEntityOption.Match(
                None: () => true,
                Some: v => false)));
    }

    [TestMethod]
    public void Test080_When_setting_an_invalid_valueobject_option_without_predicate_then_the_entity_is_updated_with_none()
    {
        var sut = ContainerEntity.Create();
        Entity<TestEntity> entity = new Error("Invalid value object");

        sut = sut.SetOption(
            Some(entity),
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => true,
            Valid: v => v.TestEntityOption.Match(
                None: () => false,
                Some: v => false)));
    }

    [TestMethod]
    public void Test090_When_setting_a_valid_valueobject_without_predicate_in_an_invalid_entity_then_the_resulting_entity_is_invalid()
    {
        Entity<ContainerEntity> sut = new Error("Invalid entity");
        sut = sut.SetOption(
            Some(TestEntity.Create(1)),
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => true));
    }

    [TestMethod]
    public void Test100_When_setting_a_null_entity_option_in_a_valid_entity_then_the_resulting_option_is_none_2()
    {
        Entity<ContainerEntity> sut = ContainerEntity.Create();
        sut = sut.SetOption(
            (Option<Entity<TestEntity>>)null!,
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestEntityOption.IsSome));
    }

    [TestMethod]
    public void Test110_When_setting_a_entity_option_with_a_null_inner_value_in_a_valid_entity_then_the_resulting_option_is_none()
    {
        Entity<ContainerEntity> sut = ContainerEntity.Create();
        sut = sut.SetOption(
            Some((Entity<TestEntity>)null!),
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestEntityOption.IsSome));
    }

    [TestMethod]
    public void Test120_When_setting_a_valueobject_option_then_the_entity_is_updated_with_some()
    {
        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            () => TestValueObject.Create(1),
            () => true,
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestValueObjectOption.Match(
                None: () => false,
                Some: v => true)));
    }


    [TestMethod]
    public void Test130_When_setting_a_valueobject_option_then_the_entity_is_updated_with_none()
    {
        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            () => TestValueObject.Create(1),
            () => false,
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestValueObjectOption.Match(
                None: () => true,
                Some: v => false)));
    }

    [TestMethod]
    public void Test140_When_setting_an_invalid_valueobject_in_a_valueobject_option_then_the_resulting_entity_invalid()
    {
        ValueObject<TestValueObject> valueObject = new Error("Invalid value object");

        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            () => valueObject,
            () => true,
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => true));

        Assert.IsTrue(sut.Errors.Count() == 1);
    }

    [TestMethod]
    public void Test150_When_setting_a_valueobject_option_without_predicate_then_the_entity_is_updated_with_some()
    {
        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            Some(TestValueObject.Create(1)),
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestValueObjectOption.Match(
                None: () => false,
                Some: v => true)));
    }

    [TestMethod]
    public void Test160_When_setting_an_entity_option_without_predicate_then_the_entity_is_updated_with_none()
    {
        var sut = ContainerEntity.Create();
        sut = sut.SetOption<ContainerEntity, TestValueObject>(
                None<ValueObject<TestValueObject>>(),
                static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestValueObjectOption.Match(
                None: () => true,
                Some: v => false)));
    }

    [TestMethod]
    public void Test170_When_setting_an_invalid_entity_option_without_predicate_then_the_entity_is_updated_with_none()
    {
        var sut = ContainerEntity.Create();
        ValueObject<TestValueObject> valueObject = new Error("Invalid value object");

        sut = sut.SetOption(
            Some(valueObject),
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => true,
            Valid: v => v.TestValueObjectOption.Match(
                None: () => false,
                Some: v => false)));
    }

    [TestMethod]
    public void Test180_When_setting_a_valid_entity_without_predicate_in_an_invalid_entity_then_the_resulting_entity_is_invalid()
    {
        Entity<ContainerEntity> sut = new Error("Invalid entity");
        var test = sut.SetOption<ContainerEntity, TestValueObject>(
            Some(TestValueObject.Create(1)),
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => true));
    }

    [TestMethod]
    public void Test190_When_setting_a_null_valueobject_option_in_a_valid_entity_then_the_resulting_option_is_none_2()
    {
        Entity<ContainerEntity> sut = ContainerEntity.Create();
        sut = sut.SetOption(
            (Option<ValueObject<TestValueObject>>)null!,
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestValueObjectOption.IsSome));
    }

    [TestMethod]
    public void Test200_When_setting_a_valueobject_option_with_a_null_inner_value_in_a_valid_entity_then_the_resulting_option_is_none()
    {
        Entity<ContainerEntity> sut = ContainerEntity.Create();
        sut = sut.SetOption(
            Some((ValueObject<TestValueObject>)null!),
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestValueObjectOption.IsSome));
    }

    [TestMethod]
    public void Test210_When_setting_a_valid_valueobject_option_in_an_invalid_entity_then_the_resulting_entity_is_invalid()
    {
        Entity<ContainerEntity> sut = new Error("Invalid entity");
        sut = sut.SetOption(
            () => TestValueObject.Create(1),
            () => true,
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => true));
    }

    [TestMethod]
    public void Test220_When_setting_a_null_valueobject_option_in_a_valid_entity_then_the_resulting_option_is_some()
    {
        Entity<ContainerEntity> sut = ContainerEntity.Create();
        sut = sut.SetOption(
            () => (ValueObject<TestValueObject>)null!,
            () => true,
            static (e, v) => e with { TestValueObjectOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestValueObjectOption.IsSome));
    }

    [TestMethod]
    public void Test230_When_setting_an_entity_option_then_the_entity_is_updated_with_some()
    {
        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            () => TestEntity.Create(1),
            () => true,
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestEntityOption.Match(
                None: () => false,
                Some: v => true)));
    }

    [TestMethod]
    public void Test240_When_setting_an_entity_option_then_the_entity_is_updated_with_none()
    {
        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            () => TestEntity.Create(1),
            () => false,
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsTrue(sut.Match(
            Invalid: e => false,
            Valid: v => v.TestEntityOption.Match(
                None: () => true,
                Some: v => false)));
    }

    [TestMethod]
    public void Test250_When_setting_an_invalid_entity_in_an_entity_option_then_the_resulting_entity_invalid()
    {
        Entity<TestEntity> entity = new Error("Invalid entity");

        var sut = ContainerEntity.Create();
        sut = sut.SetOption(
            () => entity,
            () => true,
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => true));

        Assert.IsTrue(sut.Errors.Count() == 1);
    }

    [TestMethod]
    public void Test260_When_setting_a_valid_entity_in_an_invalid_entity_then_the_resulting_entity_is_invalid()
    {
        Entity<ContainerEntity> sut = new Error("Invalid entity");
        sut = sut.SetOption(
            () => TestEntity.Create(1),
            () => true,
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => false,
            Valid: v => true));

    }

    [TestMethod]
    public void Test270_When_setting_a_null_entity_in_an_valid_entity_then_the_resulting_option_is_none()
    {
        Entity<ContainerEntity> sut = ContainerEntity.Create();
        sut = sut.SetOption(
            () => (Entity<TestEntity>)null!,
            () => true,
            static (e, v) => e with { TestEntityOption = v });

        Assert.IsFalse(sut.Match(
            Invalid: e => true,
            Valid: v => v.TestValueObjectOption.IsSome));
    }
}
