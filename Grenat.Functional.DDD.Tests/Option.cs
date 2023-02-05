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
                    Some: (name) => $"{name}"
                   );
        }

        [TestMethod]
        public void WhenCreatingAValidOptionObject_ThenWeCanGetItsValue()
        {
            Option<string> sut = Some("foo");

            Assert.AreEqual("foo", GetOptionValue(sut));
        }

        [TestMethod]
        public void WhenBindingAFunctionOnSomeValue_ThenItIsNotFired()
        {
            var func = (int i) => Some(i + 1);
            var sut = Some(0);

            var result = sut.Bind(func).Match(
                                    Some: v => v,
                                    None: () => 0);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void WhenBindingAFunctionOnANoneValue_ThenItIsNotFired()
        {
            var func = (int i) => Some(i + 1);
            var sut = None<int>();

            var result = sut.Bind(func).Match(
                                    Some: v => v,
                                    None: () => 0);

            Assert.AreEqual(0, result);
        }
    }
}