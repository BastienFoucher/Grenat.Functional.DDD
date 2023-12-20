using Grenat.Functional.DDD;

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
