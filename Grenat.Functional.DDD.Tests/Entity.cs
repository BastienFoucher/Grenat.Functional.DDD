namespace Grenat.Functional.DDD.Tests
{
    [TestClass]
    public class Entity
    {
        public Func<int, Entity<int>> IncrementEntityContentFunc = (int x) => Entity<int>.Valid(x + 1);
        public Func<int, int, Entity<int>> ParameterizedIncrementEntityContentFunc = (int x, int y) => Entity<int>.Valid(x + y);

        public AsyncFunc<int, Entity<int>> AwaitableIncrementEntityContentFunc = (int x) => Task.FromResult(Entity<int>.Valid(x + 1));
        public AsyncFunc<int, int, Entity<int>> AwaitableParameterizedIncrementEntityContentFunc = (int x, int y) => Task.FromResult(Entity<int>.Valid(x + y));

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
        public async Task WhenCreatingAnValidEntity_ThenTheAwaitableFunctionParameterizedAsyncBindFiresTheValidFunction()
        {
            var sut = Entity<int>.Valid(0);

            var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => 5))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task WhenCreatingAValidEntity_ThenAwaitableFunctionParameterizedAwaitableBindFiresTheValidFunction()
        {
            var sut = Entity<int>.Valid(0);

            var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task WhenCreatingAInvalidEntity_ThenAwaitableFunctionParameterizedAwaitableBindFiresTheInvalidFunction()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));

            var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(0, result);
        }


        [TestMethod]
        public async Task WhenCreatingAnInvalidEntity_ThenTheFunctionParameterizedAsyncBindFiresTheInvalidFunction()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));

            var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, () => 5))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task WhenCreatingAValidAwaitableEntity_ThenAwaitableAsyncBindFiresTheValidFunction()
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

            AsyncFunc<int, int, int> saveFunc = (dbContext, value) =>
            {
                saveFunctionIsFired = true;
                return Task.FromResult(value);
            };

            await sut.PersistAsync(saveFunc, 0);

            Assert.IsTrue(saveFunctionIsFired);
        }

        [TestMethod]
        public async Task WhenEntityIsInvalid_ThenPersistDoesNotFireSaveMethod()
        {
            Entity<int> sut = Entity<int>.Invalid(new Error("Invalid entity"));
            bool saveFunctionIsFired = false;

            AsyncFunc<int, int, int> saveFunc = (dbContext, value) =>
            {
                saveFunctionIsFired = true;
                return Task.FromResult(value);
            };

            await sut.PersistAsync(saveFunc, 0);

            Assert.IsFalse(saveFunctionIsFired);
        }

        [TestMethod]
        public async Task WhenAwaitableEntityIsValid_ThenPersistFireSaveMethod()
        {
            Task<Entity<int>> sut = Task.FromResult(Entity<int>.Valid(5));
            bool saveFunctionIsFired = false;

            AsyncFunc<int, int, int> saveFunc = (dbContext, value) =>
            {
                saveFunctionIsFired = true;
                return Task.FromResult(value);
            };

            await sut.PersistAsync(saveFunc, 0);

            Assert.IsTrue(saveFunctionIsFired);
        }

        [TestMethod]
        public async Task WhenAwaitableEntityIsInvalid_ThenPersistDoesNotFireSaveMethod()
        {
            Task<Entity<int>> sut = Task.FromResult(Entity<int>.Invalid(new Error("Invalid entity")));
            bool saveFunctionIsFired = false;

            AsyncFunc<int, int, int> saveFunc = (dbContext, value) =>
            {
                saveFunctionIsFired = true;
                return Task.FromResult(value);
            };

            await sut.PersistAsync(saveFunc, 0);

            Assert.IsFalse(saveFunctionIsFired);
        }
    }
}
