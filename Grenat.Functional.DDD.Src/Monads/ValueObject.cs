namespace Grenat.Functional.DDD;

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
}

public static class ValueObjectExtensions
{
    public static ValueObject<Option<R>> Traverse<T, R>(this Option<ValueObject<T>> option, Func<T, R> func)
    {
        return option.Match(
            Some: v => v.Match(
                Valid: t => ValueObject<Option<R>>.Valid(Some(func(t))),
                Invalid: e => ValueObject<Option<R>>.Invalid(e)),
            None: () => ValueObject<Option<R>>.Valid(None<R>()));
    }
}
