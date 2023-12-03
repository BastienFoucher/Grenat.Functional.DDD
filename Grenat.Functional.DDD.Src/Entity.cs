namespace Grenat.Functional.DDD;

public class Entity<T>
{
    public readonly IEnumerable<Error> Errors;
    private readonly T _value;

    public bool IsValid { get => !(Errors is not null && Errors.Any()); }

    private Entity(T t) => (Errors, _value) = (Enumerable.Empty<Error>(), t ?? throw new ArgumentNullException(nameof(t)));
    private Entity(IEnumerable<Error> errors) => (Errors, _value) = (errors, default(T)!);

    public static Entity<T> Valid(T t) => new(t);
    public static Entity<T> Invalid(Error error) => Invalid(new[] { error });
    public static Entity<T> Invalid(IEnumerable<Error> errors) => new(errors);

    public static implicit operator Entity<T>(T t) => Valid(t);
    public static implicit operator Entity<T>(Error error) => Invalid(new[] { error });
    public static implicit operator Entity<T>(Error[] errors) => Invalid(errors.ToArray());

    public R Match<R>(Func<Error[], R> Invalid, Func<T, R> Valid)
    {
        return IsValid ? Valid(_value!) : Invalid(Errors.ToArray()!);
    }
}
