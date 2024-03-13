namespace Grenat.Functional.DDD;

public record Entity<T> : DddContainer<T>
{
    private Entity(T t) : base(t) { }
    private Entity(IEnumerable<Error> errors) : base(errors) { }

    public new static Entity<T> Valid(T t) => new(t);
    public new static Entity<T> Invalid(Error error) => Invalid(new[] { error });
    public new static Entity<T> Invalid(IEnumerable<Error> errors) => new(errors);

    public static implicit operator Entity<T>(T t) => Valid(t);
    public static implicit operator Entity<T>(Error error) => Invalid(new[] { error });
    public static implicit operator Entity<T>(Error[] errors) => Invalid(errors.ToArray());
}

public static class EntityExtensions
{
    public static Entity<Option<R>> Traverse<T, R>(this Option<Entity<T>> option, Func<T, R> func)
    {
        return option.Match(
            Some: v => v.Match(
                Valid: t => Entity<Option<R>>.Valid(Some(func(t))),
                Invalid: e => Entity<Option<R>>.Invalid(e)),
            None: () => Entity<Option<R>>.Valid(None<R>()));
    }
}
