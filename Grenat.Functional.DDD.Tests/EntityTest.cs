namespace Grenat.Functional.DDD.Tests;

[TestClass]
public class EntityTests : EntityTestBase
{
    [TestMethod]
    public async Task When_mapping_parallel_operations_on_entity_then_the_result_is_correct()
    {
        var sut = Entity<int>.Valid(5);
        var funcs = new List<AsyncFunc<int, int>>()
        {
            async (p) => await Task.FromResult(p + 1),
            async (p) => await Task.FromResult(p + 2),
            async (p) => await Task.FromResult(p + 3)
        };

        var result = (await sut.MapParallel(funcs, (values) => values.Sum()))
            .Match(
                Valid: v => v,
                Invalid: e => 0);

        Assert.IsTrue(result == 21);
    }


    [TestMethod]
    public async Task When_mapping_parallel_operations_an_invalid_entity_then_the_result_is_an_error()
    {
        var sut = Entity<int>.Invalid(new Error("Error"));
        var funcs = new List<AsyncFunc<int, int>>()
        {
            async (p) => await Task.FromResult(p + 1),
            async (p) => await Task.FromResult(p + 2),
            async (p) => await Task.FromResult(p + 3)
        };

        var result = await sut.MapParallel(funcs, (values) => values.Sum());

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task When_binding_parallel_operations_on_entity_then_the_result_is_correct()
    {
        var sut = Entity<int>.Valid(5);
        var funcs = new List<AsyncFunc<int, Entity<int>>>()
        {
            async (p) => await Task.FromResult(Entity<int>.Valid(p + 1)),
            async (p) => await Task.FromResult(Entity<int>.Valid(p + 2)),
            async (p) => await Task.FromResult(Entity<int>.Valid(p + 3)),
        };

        var result = (await sut.BindParallel(funcs, (values) => values.Sum()))
            .Match(
                Valid: v => v,
                Invalid: e => 0);

        Assert.IsTrue(result == 21);
    }

    [TestMethod]
    public async Task When_binding_parallel_operations_on_of_which_is_an_error_on_an_entity_then_the_errors_are_harvested()
    {
        var sut = Entity<int>.Valid(5);
        var funcs = new List<AsyncFunc<int, Entity<int>>>()
        {
            async (p) => await Task.FromResult(Entity<int>.Valid(p + 1)),
            async (p) => await Task.FromResult(Entity<int>.Invalid(new Error("Erreur 1"))),
            async (p) => await Task.FromResult(Entity<int>.Invalid(new Error("Erreur 2")))
        };

        var result = await sut.BindParallel(funcs, (values) => values.Sum());

        Assert.IsTrue(result.Errors.Count() == 2);
    }

    [TestMethod]
    public async Task When_binding_parallel_operations_an_invalid_entity_then_the_result_is_an_error()
    {
        var sut = Entity<int>.Invalid(new Error("Error"));
        var funcs = new List<AsyncFunc<int, Entity<int>>>()
        {
            async (p) => await Task.FromResult(Entity<int>.Valid(p + 1)),
            async (p) => await Task.FromResult(Entity<int>.Valid(p + 2)),
            async (p) => await Task.FromResult(Entity<int>.Valid(p + 3))
        };

        var result = await sut.BindParallel(funcs, (values) => values.Sum());

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void When_creating_a_valid_entity_then_its_state_is_valid()
    {
        var sut = Entity<int>.Valid(5);

        Assert.IsTrue(sut.IsValid);
    }

    [TestMethod]
    public void When_creating_an_invalid_entity_then_its_state_is_invalid()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        Assert.IsFalse(sut.IsValid);
    }

    [TestMethod]
    public void When_creating_a_valid_entity_then_the_match_valid_function_is_fired()
    {
        var sut = Entity<int>.Valid(0);
        var countValid = 0;

        countValid = sut.Match(
                        Valid: v => v + 1,
                        Invalid: e => 0);

        Assert.AreEqual(1, countValid);
    }

    [TestMethod]
    public void When_creating_an_invalid_entity_then_the_match_invalid_function_is_fired()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));
        var countInvalid = 0;

        countInvalid = sut.Match(
                        Valid: v => 0,
                        Invalid: e => countInvalid + 1);

        Assert.AreEqual(1, countInvalid);
    }

    [TestMethod]
    public void When_creating_a_valid_entity_then_map_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = sut.Map(IncrementEntityContentFunc)
                        .Match(
                            Valid: v => v,
                            Invalid: e => 0)
                        .Match(
                            Valid: v => v,
                            Invalid: e => 0);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void When_creating_an_invalid_entity_then_map_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        // 2 match function because Map, contrary to Bind, leads to Entity<Entity<int>>
        var result = sut.Map(IncrementEntityContentFunc)
                        .Match(
                            Valid: v => v,
                            Invalid: e => 0)
                        .Match(
                            Valid: v => v,
                            Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_entity_then_the_async_map_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = (await sut.MapAsync(AwaitableIncrementEntityContentFunc))
                                .Match(
                                   Valid: v => "valid function",
                                   Invalid: e => "invalid function");

        Assert.AreEqual("valid function", result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_entity_then_the_async_map_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = (await sut.MapAsync(AwaitableIncrementEntityContentFunc))
                                .Match(
                                   Valid: v => "valid function",
                                   Invalid: e => "invalid function");

        Assert.AreEqual("invalid function", result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_awaitable_entity_then_the_async_map_fires_the_valid_function()
    {
        var sut = Task.FromResult(Entity<int>.Valid(0));

        var result = (await sut.MapAsync(AwaitableIncrementEntityContentFunc))
                                .Match(
                                   Valid: v => "valid function",
                                   Invalid: e => "invalid function");

        Assert.AreEqual("valid function", result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_awaitable_entity_then_the_async_map_fires_the_invalid_function()
    {
        var sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));

        var result = (await sut.MapAsync(AwaitableIncrementEntityContentFunc))
                                .Match(
                                   Valid: v => "valid function",
                                   Invalid: e => "invalid function");

        Assert.AreEqual("invalid function", result);
    }

    [TestMethod]
    public void When_creating_a_valid_entity_then_map_with_an_action_returns_the_original_entity_value()
    {
        var sut = Entity<string>.Valid("valid entity");

        var result = sut.Map(SimpleAction)
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => "invalid function");

        Assert.AreEqual("valid entity", result);
    }

    [TestMethod]
    public void When_creating_an_invalid_entity_then_map_with_an_action_fires_the_invalid_action()
    {
        var sut = Entity<string>.Invalid(new Error("invalid entity"));

        var result = sut.Map(SimpleAction)
                                .Match(
                                    Valid: v => "valid function",
                                    Invalid: e => "invalid function");

        Assert.AreEqual("invalid function", result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_entity_then_map_with_an_asyncaction_returns_the_original_entity_value()
    {
        var sut = Entity<string>.Valid("valid entity");

        var result = (await sut.MapAsync(SimpleAsyncAction))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => "invalid function");

        Assert.AreEqual("valid entity", result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_entity_then_map_with_an_asyncaction_fires_the_invalid_function()
    {
        var sut = Entity<string>.Invalid(new Error("invalid entity"));

        var result = (await sut.MapAsync(SimpleAsyncAction))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => "invalid function");

        Assert.AreEqual("invalid function", result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_async_entity_then_map_with_an_asyncaction_returns_the_original_entity_value()
    {
        var sut = Task.FromResult(Entity<string>.Valid("valid entity"));

        var result = (await sut.MapAsync(SimpleAsyncAction))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => "invalid function");

        Assert.AreEqual("valid entity", result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_asyncentity_then_map_with_an_asyncaction_fires_the_invalid_function()
    {
        var sut = Task.FromResult(Entity<string>.Invalid(new Error("invalid entity")));

        var result = (await sut.MapAsync(SimpleAsyncAction))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => "invalid function");

        Assert.AreEqual("invalid function", result);
    }

    [TestMethod]
    public void When_creating_a_valid_entity_then_bind_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = sut.Bind(IncrementEntityContentFunc)
                        .Match(
                           Valid: v => v,
                           Invalid: e => 0);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void When_creating_an_invalid_entity_then_bind_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = sut.Bind(IncrementEntityContentFunc)
                        .Match(
                            Valid: v => v,
                            Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void When_creating_a_valid_entity_then_the_parameterized_bind_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = sut.Bind(ParameterizedIncrementEntityContentFunc, 5)
                        .Match(
                           Valid: v => v,
                           Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public void When_creating_an_invalid_entity_then_the_parameterized_bind_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = sut.Bind(ParameterizedIncrementEntityContentFunc, 5)
                        .Match(
                            Valid: v => v,
                            Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void When_creating_a_valid_entity_then_the_function_parameterized_bind_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = sut.Bind(ParameterizedIncrementEntityContentFunc, () => 5)
                        .Match(
                           Valid: v => v,
                           Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public void When_creating_an_invalid_entity_then_the_function_parameterized_bind_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = sut.Bind(ParameterizedIncrementEntityContentFunc, () => 5)
                        .Match(
                            Valid: v => v,
                            Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_entity_then_awaitable_function_parameterized_async_bind_fires_thevalid_function()
    {
        var sut = Entity<int>.Valid(5);

        var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_entity_then_awaitable_function_parameterized_async_bind_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_entity_then_the_async_bind_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = (await sut.BindAsync(AwaitableIncrementEntityContentFunc))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_entity_then_the_async_bind_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = (await sut.BindAsync(AwaitableIncrementEntityContentFunc))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_entity_then_async_parameterized_async_bind_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, 5))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_entity_then_async_parameterized_async_bind_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, 5))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_entity_then_the_awaitable_function_parameterized_async_bind_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => 5))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_entity_then_awaitable_function_parameterized_awaitable_bind_fires_the_valid_function()
    {
        var sut = Entity<int>.Valid(0);

        var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public async Task When_creating_an_invalid_entity_then_awaitable_function_parameterized_awaitable_bind_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }


    [TestMethod]
    public async Task When_creating_an_invalid_entity_then_the_function_parameterized_async_bind_fires_the_invalid_function()
    {
        var sut = Entity<int>.Invalid(new Error("Invalid entity"));

        var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => 5))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task When_creating_a_valid_awaitable_entity_then_awaitable_async_bind_fires_the_valid_function()
    {
        var sut = Task.FromResult(Entity<int>.Valid(0));

        var result = (await sut.BindAsync(IncrementEntityContentFunc))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public async Task WhenCreatingAValidAwaitableEntity_ThenAwaitableAsyncBindFiresTheInvalidFunction()
    {
        var sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));

        var result = (await sut.BindAsync(IncrementEntityContentFunc))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task WhenCreatingAValidAwaitableEntity_ThenAwaitableAsyncBindFiresTheValidFunction_1()
    {
        var sut = Task.FromResult(Entity<int>.Valid(0));

        var result = (await sut.BindAsync(AwaitableIncrementEntityContentFunc))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public async Task WhenCreatingAValidAwaitableEntity_ThenAwaitableAsyncBindFiresTheInvalidFunction_1()
    {
        var sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));

        var result = (await sut.BindAsync(AwaitableIncrementEntityContentFunc))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task WhenCreatingAValidAwaitableEntity_ThenAwaitableParameterizedBindFiresValidFunction()
    {
        var sut = Task.FromResult(Entity<int>.Valid(0));

        var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, 5))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public async Task WhenCreatingAnInvalidAwaitableEntity_ThenAwaitableParameterizedBindFiresInvalidFunction()
    {
        var sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));

        var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, 5))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task WhenCreatingAValidAwaitableEntity_ThenAwaitableFunctionParameterizedBindFiresValidFunction()
    {
        var sut = Task.FromResult(Entity<int>.Valid(0));

        var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, () => 5))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public async Task WhenCreatingAnInvalidAwaitableEntity_ThenFunctionParameterizedAwaitableBindFiresInvalidFunction()
    {
        var sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));

        var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, () => 5))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task WhenCreatingAValidAwaitableEntity_ThenAwaitableFunctionParameterizedAwaitableBindFiresValidFunction()
    {
        var sut = Task.FromResult(Entity<int>.Valid(0));

        var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public async Task WhenCreatingAnInvalidAwaitableEntity_ThenAwaitableFunctionParameterizedAwaitableBindFiresInvalidFunction()
    {
        var sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));

        var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task WhenCreatingAValidAwaitableEntity_ThenFunctionParameterizedAwaitableBindFiresValidFunction_1()
    {
        var sut = Task.FromResult(Entity<int>.Valid(0));

        var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public async Task WhenCreatingAnInvalidAwaitableEntity_ThenAwaitableFunctionParameterizedAwaitableBindFiresInvalidFunction_1()
    {
        var sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));

        var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                .Match(
                                   Valid: v => v,
                                   Invalid: e => 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task WhenEntityIsValid_ThenPersistFireSaveMethod()
    {
        Entity<int> sut = Entity<int>.Valid(5);
        bool saveFunctionIsFired = false;

        Task<int> saveFunc(int dbContext, int value)
        {
            saveFunctionIsFired = true;
            return Task.FromResult(value);
        }

        await sut.PersistAsync(saveFunc, 0);

        Assert.IsTrue(saveFunctionIsFired);
    }

    [TestMethod]
    public async Task WhenEntityIsInvalid_ThenPersistDoesNotFireSaveMethod()
    {
        Entity<int> sut = Entity<int>.Invalid(new Error("Invalid entity"));
        bool saveFunctionIsFired = false;

        Task<int> saveFunc(int dbContext, int value)
        {
            saveFunctionIsFired = true;
            return Task.FromResult(value);
        }

        await sut.PersistAsync(saveFunc, 0);

        Assert.IsFalse(saveFunctionIsFired);
    }

    [TestMethod]
    public async Task WhenAwaitableEntityIsValid_ThenPersistFireSaveMethod()
    {
        Task<Entity<int>> sut = Task.FromResult(Entity<int>.Valid(5));
        bool saveFunctionIsFired = false;

        Task<int> saveFunc(int dbContext, int value)
        {
            saveFunctionIsFired = true;
            return Task.FromResult(value);
        }

        await sut.PersistAsync(saveFunc, 0);

        Assert.IsTrue(saveFunctionIsFired);
    }

    [TestMethod]
    public async Task WhenAwaitableEntityIsInvalid_ThenPersistDoesNotFireSaveMethod()
    {
        Task<Entity<int>> sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));
        bool saveFunctionIsFired = false;

        Task<int> saveFunc(int dbContext, int value)
        {
            saveFunctionIsFired = true;
            return Task.FromResult(value);
        }

        await sut.PersistAsync(saveFunc, 0);

        Assert.IsFalse(saveFunctionIsFired);
    }
}
