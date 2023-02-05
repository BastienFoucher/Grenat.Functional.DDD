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
        public void WhenCreatingAValidEntity_ThenItsStateIsValid()
        {
            var sut = Entity<int>.Valid(5);

            Assert.IsTrue(sut.IsValid);
        }

        [TestMethod]
        public void WhenCreatingAnInvalidEntity_ThenItsStateIsInvalid()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));

            Assert.IsFalse(sut.IsValid);
        }

        public void WhenCreatingAValidEntity_ThenTheMatchValidFunctionIsFired()
        {
            var sut = Entity<int>.Valid(0);
            var countValid = 0;

            countValid = sut.Match(
                            Valid: v => v + 1,
                            Invalid: e => 0);

            Assert.AreEqual(1, countValid);
        }

        [TestMethod]
        public void WhenCreatingAInvalidEntity_ThenTheMatchInvalidFunctionIsFired()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));
            var countInvalid = 0;

            countInvalid = sut.Match(
                            Valid: v => 0,
                            Invalid: e => countInvalid + 1);

            Assert.AreEqual(1, countInvalid);
        }

        [TestMethod]
        public void WhenCreatingAValidEntity_ThenMapFiresTheValidFunction()
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
        public void WhenCreatingAnInvalidEntity_ThenMapFiresTheInvalidFunction()
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
        public void WhenCreatingAValidEntity_ThenBindFiresTheValidFunction()
        {
            var sut = Entity<int>.Valid(0);

            var result = sut.Bind(IncrementEntityContentFunc)
                            .Match(
                               Valid: v => v,
                               Invalid: e => 0);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void WhenCreatingAnInvalidEntity_ThenBindFiresTheInvalidFunction()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));

            var result = sut.Bind(IncrementEntityContentFunc)
                            .Match(
                                Valid: v => v,
                                Invalid: e => 0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void WhenCreatingAValidEntity_ThenTheParameterizedBindFiresTheValidFunction()
        {
            var sut = Entity<int>.Valid(0);

            var result = sut.Bind(ParameterizedIncrementEntityContentFunc, 5)
                            .Match(
                               Valid: v => v,
                               Invalid: e => 0);

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void WhenCreatingAnInvalidEntity_ThenTheParameterizedBindFiresTheInvalidFunction()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));

            var result = sut.Bind(ParameterizedIncrementEntityContentFunc, 5)
                            .Match(
                                Valid: v => v,
                                Invalid: e => 0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void WhenCreatingAValidEntity_ThenTheFunctionParameterizedBindFiresTheValidFunction()
        {
            var sut = Entity<int>.Valid(0);

            var result = sut.Bind(ParameterizedIncrementEntityContentFunc, () => 5)
                            .Match(
                               Valid: v => v,
                               Invalid: e => 0);

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void WhenCreatingAnInvalidEntity_ThenTheFunctionParameterizedBindFiresTheInvalidFunction()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));

            var result = sut.Bind(ParameterizedIncrementEntityContentFunc, () => 5)
                            .Match(
                                Valid: v => v,
                                Invalid: e => 0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task WhenCreatingAValidEntity_ThenAwaitableFunctionParameterizedAsyncBindFiresTheValidFunction()
        {
            var sut = Entity<int>.Valid(5);

            var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public async Task WhenCreatingAnInvalidEntity_ThenAwaitableFunctionParameterizedAsyncBindFiresTheInvalidFunction()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));

            var result = (await sut.BindAsync(ParameterizedIncrementEntityContentFunc, () => Task.FromResult(5)))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task WhenCreatingAValidEntity_ThenTheAsyncBindFiresTheValidFunction()
        {
            var sut = Entity<int>.Valid(0);

            var result = (await sut.BindAsync(AwaitableIncrementEntityContentFunc))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public async Task WhenCreatingAnInvalidEntity_ThenTheAsyncBindFiresTheInvalidFunction()
        {
            var sut = Entity<int>.Invalid(new Error("Invalid entity"));

            var result = (await sut.BindAsync(AwaitableIncrementEntityContentFunc))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task WhenCreatingAnInvalidEntity_ThenAsyncParameterizedAsyncBindFiresTheValidFunction()
        {
            var sut = Entity<int>.Valid(0);

            var result = (await sut.BindAsync(AwaitableParameterizedIncrementEntityContentFunc, 5))
                                    .Match(
                                       Valid: v => v,
                                       Invalid: e => 0);

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task WhenCreatingAnInvalidEntity_ThenAsyncParameterizedAsyncBindFiresTheInvalidFunction()
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
