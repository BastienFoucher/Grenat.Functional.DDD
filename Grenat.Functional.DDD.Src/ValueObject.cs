namespace Grenat.Functional.DDD
{
    public class ValueObject<T>
    {
        public readonly IEnumerable<Error> Errors;

        public bool IsValid { get => !(Errors is not null && Errors.Any()); }

        private readonly T _value;

        private ValueObject(T t) => (Errors, _value) = (Enumerable.Empty<Error>(), t ?? throw new ArgumentNullException(nameof(t)));

        private ValueObject(IEnumerable<Error> errors) => (Errors, _value) = (errors, default(T)!);

        public static ValueObject<T> Valid(T t) => new(t);
        public static ValueObject<T> Invalid(IEnumerable<Error> errors) => new(errors);
        public static ValueObject<T> Invalid(Error error) => Invalid(new[] { error });

        public static implicit operator ValueObject<T>(T t) => Valid(t);
        public static implicit operator ValueObject<T>(Error error) => Invalid(new[] { error });

        public Entity<T> ToEntity()
        {
            return IsValid ? Entity<T>.Valid(_value) : Entity<T>.Invalid(Errors);
        }

        public R Match<R>(Func<IEnumerable<Error>, R> Invalid, Func<T, R> Valid)
        {
            return IsValid ? Valid(_value!) : Invalid(Errors!);
        }

    }

    public static class ValueObject
    {
        public static ValueObject<R> Bind<T, R>(this ValueObject<T> valueObject, Func<T, ValueObject<R>> func)
        {
            return valueObject.Match(
                Valid: (value) => func(value),
                Invalid: (err) => ValueObject<R>.Invalid(err));
        }

        public static ValueObject<R> Map<T, R>(this ValueObject<T> valueObject, Func<T, R> func)
        {
            return valueObject.Match(
                Valid: (value) => ValueObject<R>.Valid(func(value)),
                Invalid: (err) => ValueObject<R>.Invalid(err));
        }
    }
}
