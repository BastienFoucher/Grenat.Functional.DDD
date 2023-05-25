using static Grenat.Functional.DDD.Option;

namespace Grenat.Functional.DDD.Tests
{
    [TestClass]
    public class Option
    {
        private string GetOptionValue<T>(Option<T> value)
        {
            return value.Match(
                    None: () => "Empty",
                    Some: (value) => $"{value}"
                    );
        }

        [TestMethod]
        public void When_creating_a_some_option_object_then_we_can_get_its_value()
        {
            Option<string> sut = Some("foo");

            Assert.AreEqual("foo", GetOptionValue(sut));
        }

        [TestMethod]
        public void When_mapping_a_function_on_some_value_then_it_is_fired()
        {
            var func = (int i) => i + 1;
            var sut = Some(0);

            var result = sut.Map(func)
                            .Match(
                                    Some: v => v,
                                    None: () => 0);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void When_mapping_a_function_on_none_value_then_it_is_not_fired()
        {
            var func = (int i) => i + 1;
            var sut = None<int>();

            var result = sut.Map(func)
                            .Match(
                                    Some: v => v,
                                    None: () => 0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void When_binding_a_function_on_some_value_then_it_is_fired()
        {
            var func = (int i) => Some(i + 1);
            var sut = Some(0);

            var result = sut.Bind(func)
                            .Match(
                                    Some: v => v,
                                    None: () => 0);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void When_binding_three_AddOne_functions_on_Some_zero_value_then_the_result_is_three()
        {
            var func = (int i) => Some(i + 1);
            var sut = Some(0);

            var result = sut.Bind(func)
                            .Bind(func)
                            .Bind(func)
                            .Match(
                                    Some: v => v,
                                    None: () => 0);

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void When_binding_a_function_that_returns_None_then_the_result_is_none()
        {
            var func = (int i) => Some(i + 1);
            var funcNone = (int i) => None<int>();
            var sut = Some(0);

            var result = sut.Bind(func)
                            .Bind(funcNone)
                            .Bind(func);

            Assert.IsFalse(result.IsSome);
        }

        [TestMethod]
        public void When_binding_a_function_on_a_none_value_then_it_is_not_fired()
        {
            var func = (int i) => Some(i + 1);
            var sut = None<int>();

            var result = sut.Bind(func).Match(
                                    Some: v => v,
                                    None: () => 0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void When_binding_a_function_returning_a_none_value_then_orelse_is_not_fired()
        {
            var func = (int i) => Some(i + 1);
            var sut = Some(0);

            var result = sut
                .Bind(func)
                .OrElse(() => Some(1000))
                .Match(
                    Some: v => v,
                    None: () => 0);

            Assert.IsTrue(result == 1);
        }

        [TestMethod]
        public void When_binding_a_function_returning_a_none_value_then_orelse_is_fired()
        {
            var func = (int i) => None<int>();
            var sut = Some(0);

            var result = sut
                .Bind(func)
                .OrElse(() => Some(1000))
                .Match(
                    Some: v => v,
                    None: () => 0);

            Assert.IsTrue(result == 1000);
        }

        [TestMethod]
        public void When_binding_a_function_on_a_none_value_then_orelse_is_fired()
        {
            var func = (int i) => None<int>();
            var sut = None<int>();

            var result = sut
                .Bind(func)
                .OrElse(() => Some(1000))
                .Match(
                    Some: v => v,
                    None: () => 1);

            Assert.IsTrue(result == 1000);
        }

        [TestMethod]
        public void LeftIdentity()
        {
            var t = Some(5);

            var result = t.Bind(Some);
            Assert.AreEqual(t, result);
        }

        [TestMethod]
        public void When_getting_a_value_that_exist_in_a_dictionary_then_the_result_is_some()
        {
            var dictionary = ImmutableDictionary<int, int>.Empty;
            dictionary = dictionary.Add(1, 1);

            Assert.IsTrue(dictionary.GetValue(1).IsSome);
        }

        [TestMethod]
        public void When_getting_a_value_that_dont_exist_in_a_dictionary_then_the_result_is_none()
        {
            var dictionary = ImmutableDictionary<int, int>.Empty;
            dictionary = dictionary.Add(1, 1);

            Assert.IsFalse(dictionary.GetValue(2).IsSome);
        }
    }
}