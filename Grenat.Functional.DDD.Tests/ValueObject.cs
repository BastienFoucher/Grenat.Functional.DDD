namespace Grenat.Functional.DDD.Tests
{
    [TestClass]
    public class ValueObject
    {
        [TestMethod]
        public void WhenCreatingAValidValueObject_ThenItsStateIsValid()
        {
            var sut = ValueObject<int>.Valid(5);

            Assert.IsTrue(sut.IsValid);
        }

        [TestMethod]
        public void WhenCreatingAnInvalidValueObject_ThenItsStateIsInvalid()
        {
            var sut = ValueObject<int>.Invalid(new Error("Invalid value object"));

            Assert.IsFalse(sut.IsValid);
        }

        [TestMethod]
        public void WhenCreatingAValidValueObject_ThenTheMatchValidFunctionIsFired()
        {
            var sut = ValueObject<int>.Valid(0);
            var countValid = 0;

            countValid = sut.Match(
                            Valid: v => v + 1,
                            Invalid: e => 0);

            Assert.AreEqual(1, countValid);
        }

        [TestMethod]
        public void WhenCreatingAInvalidValueObject_ThenTheMatchInvalidFunctionIsFired()
        {
            var sut = ValueObject<int>.Invalid(new Error("Invalid value object"));
            var countInvalid = 0;

            countInvalid = sut.Match(
                            Valid: v => 0,
                            Invalid: e => countInvalid + 1);

            Assert.AreEqual(1, countInvalid);
        }

        [TestMethod]
        public void WhenConvertingAValidValueObjectToAnEntity_ThenTheEntityStateIsTheSameThanTheValueObject()
        {
            var sut = ValueObject<int>.Valid(5).ToEntity();

            Assert.IsTrue(sut.IsValid);
        }

        public void WhenConvertingAnInvalidValueObjectToAnEntity_ThenTheEntityStateIsTheSameThanTheValueObject()
        {
            var sut = ValueObject<int>.Invalid(new Error("Invalid value object")).ToEntity();

            Assert.IsFalse(sut.IsValid);
        }

        [TestMethod]
        public void WhenCreatingAValidValueObject_TheTheBindedFunctionIsFired()
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
        public void WhenCreatingAnInvValidValueObject_TheTheBindedFunctionIsNotFired()
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
