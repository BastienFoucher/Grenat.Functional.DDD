namespace Grenat.Functional.DDD.Tests
{
    [TestClass]
    public class ValueObject
    {
        [TestMethod]
        public void When_creating_a_valid_value_object_then_its_state_is_valid()
        {
            var sut = ValueObject<int>.Valid(5);

            Assert.IsTrue(sut.IsValid);
        }

        [TestMethod]
        public void When_creating_an_invalid_value_object_then_its_state_is_invalid()
        {
            var sut = ValueObject<int>.Invalid(new Error("Invalid value object"));

            Assert.IsFalse(sut.IsValid);
        }

        [TestMethod]
        public void When_creating_a_valid_value_object_then_the_match_valid_function_is_fired()
        {
            var sut = ValueObject<int>.Valid(0);
            var countValid = 0;

            countValid = sut.Match(
                            Valid: v => v + 1,
                            Invalid: e => 0);

            Assert.AreEqual(1, countValid);
        }

        [TestMethod]
        public void When_creating_an_invalid_value_object_then_the_match_invalid_function_is_fired()
        {
            var sut = ValueObject<int>.Invalid(new Error("Invalid value object"));
            var countInvalid = 0;

            countInvalid = sut.Match(
                            Valid: v => 0,
                            Invalid: e => countInvalid + 1);

            Assert.AreEqual(1, countInvalid);
        }

        [TestMethod]
        public void When_converting_a_valid_value_object_to_an_entity_then_the_entity_state_is_the_same_than_the_value_object()
        {
            var sut = ValueObject<int>.Valid(5).ToEntity();

            Assert.IsTrue(sut.IsValid);
        }

        public void When_converting_an_invalid_value_object_to_an_entity_then_the_entity_state_is_the_same_than_the_value_object()
        {
            var sut = ValueObject<int>.Invalid(new Error("Invalid value object")).ToEntity();

            Assert.IsFalse(sut.IsValid);
        }

        [TestMethod]
        public void When_creating_a_valid_value_object_the_the_binded_function_is_fired()
        {
            var sut = ValueObject<int>.Valid(0);
            var func = (int x) => ValueObject<int>.Valid(x + 1);

            var count = sut.Bind(func);
            var result = count.Match(
                   Valid: v => v,
                   Invalid: e => 0);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void When_creating_an_invalid_value_object_then_the_binded_function_is_not_fired()
        {
            var sut = ValueObject<int>.Invalid(new Error("Invalid value object"));
            var func = (int x) => ValueObject<int>.Valid(x + 1);

            var count = sut.Bind(func);
            var result = count.Match(
                   Valid: v => v,
                   Invalid: e => 0);

            Assert.AreEqual(0, result);
        }
    }
}
