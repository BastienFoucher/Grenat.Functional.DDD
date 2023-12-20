namespace Grenat.Functional.DDD;

public record DddContainer<T>
{
    public readonly IEnumerable<Error> Errors;

    public bool IsValid { get => !(Errors is not null && Errors.Any()); }

    protected readonly T _value;

    protected DddContainer(T t) => (Errors, _value) = (Enumerable.Empty<Error>(), t ?? throw new ArgumentNullException(nameof(t)));

    protected DddContainer(IEnumerable<Error> errors) => (Errors, _value) = (errors, default(T)!);

    public static DddContainer<T> Valid(T t) => new DddContainer<T>(t);
    public static DddContainer<T> Invalid(IEnumerable<Error> errors) => new(errors);
    public static DddContainer<T> Invalid(Error error) => Invalid(new[] { error });

    public R Match<R>(Func<Error[], R> Invalid, Func<T, R> Valid)
    {
        return IsValid ? Valid(_value!) : Invalid(Errors.ToArray()!);
    }

}

public static class DddContainerBase
{
    public static DddContainer<R> Bind<T, R>(this DddContainer<T> dddContainer, Func<T, DddContainer<R>> func)
    {
        return dddContainer.Match(
            Valid: value => func(value),
            Invalid: DddContainer<R>.Invalid);
    }

    public static DddContainer<R> Map<T, R>(this DddContainer<T> dddContainer, Func<T, R> func)
    {
        return dddContainer.Match(
            Valid: value => DddContainer<R>.Valid(func(value)),
            Invalid: DddContainer<R>.Invalid);
    }

    public static DddContainer<T> Map<T>(this DddContainer<T> dddContainer, Action<T> action)
    {
        return dddContainer.Match(
            Valid: (value) =>
            {
                action(value);
                return dddContainer;
            },
            Invalid: Entity<T>.Invalid);
    }

    public static DddContainer<IEnumerable<T>> Traverse<T>(this IEnumerable<DddContainer<T>> dddContainers)
    {
        return dddContainers.Traverse(t => t);
    }

    public static DddContainer<IEnumerable<R>> Traverse<T, R>(this IEnumerable<DddContainer<T>> dddObjects, Func<T, R> func)
    {
        var dddContainersInError = dddObjects.Where(e => !e.IsValid);

        if (dddContainersInError.Any())
            return DddContainer<IEnumerable<R>>.Invalid(dddContainersInError.SelectMany(e => e.Errors));
        else
            return DddContainer<IEnumerable<R>>.Valid(dddObjects.AsEnumerable(func));
    }

    // FIXME : comment factoriser ?
    public static ValueObject<Option<R>> Traverse<T, R>(this Option<ValueObject<T>> option, Func<T, R> func)
    {
        return option.Match(
            Some: v => v.Match(
                Valid: t => ValueObject<Option<R>>.Valid(Some(func(t))),
                Invalid: e => ValueObject<Option<R>>.Invalid(e)),
            None: () => ValueObject<Option<R>>.Valid(None<R>()));
    }

    private static IEnumerable<R> AsEnumerable<T, R>(this IEnumerable<DddContainer<T>> entities, Func<T, R> func)
    {
        foreach (var entity in entities)
        {
            yield return entity.Match(
                Invalid: e => throw new Exception("Invalid entities should not be this function."),
                Valid: v => func(v));
        }
    }
}