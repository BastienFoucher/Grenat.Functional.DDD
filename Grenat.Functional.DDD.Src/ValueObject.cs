namespace Grenat.Functional.DDD
{
    public record ValueObject<T> : DddContainer<T>
    {
        private ValueObject(T t) : base(t) { }
        private ValueObject(IEnumerable<Error> errors) : base(errors) { }

        public new static ValueObject<T> Valid(T t) => new(t);
        public new static ValueObject<T> Invalid(IEnumerable<Error> errors) => new(errors);
        public new static ValueObject<T> Invalid(Error error) => Invalid(new[] { error });

        public static implicit operator ValueObject<T>(T t) => Valid(t);
        public static implicit operator ValueObject<T>(Error error) => Invalid(new[] { error });
        public static implicit operator ValueObject<T>(Error[] errors) => Invalid(errors.ToArray());

        public Entity<T> ToEntity()
        {
            return IsValid ? Entity<T>.Valid(_value) : Entity<T>.Invalid(Errors);
        }

        //public R Match<R>(Func<Error[], R> Invalid, Func<T, R> Valid)
        //{
        //    return IsValid ? Valid(_value!) : Invalid(Errors.ToArray()!);
        //}

    }

    //public static class ValueObject
    //{
    //    public static ValueObject<R> Bind<T, R>(this ValueObject<T> valueObject, Func<T, ValueObject<R>> func)
    //    {
    //        return valueObject.Match(
    //            Valid: (value) => func(value),
    //            Invalid: (err) => ValueObject<R>.Invalid(err));
    //    }

    //    public static ValueObject<R> Map<T, R>(this ValueObject<T> valueObject, Func<T, R> func)
    //    {
    //        return valueObject.Match(
    //            Valid: (value) => ValueObject<R>.Valid(func(value)),
    //            Invalid: (err) => ValueObject<R>.Invalid(err));
    //    }
    //}
}
