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

public static class DddContainerExtensions
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
        if (dddObjects == null)
            return DddContainer<IEnumerable<R>>.Valid(Enumerable.Empty<R>());

        var dddContainersInError = dddObjects.Where(e => !e.IsValid);

        if (dddContainersInError.Any())
            return DddContainer<IEnumerable<R>>.Invalid(dddContainersInError.SelectMany(e => e.Errors));
        else
            return DddContainer<IEnumerable<R>>.Valid(dddObjects.AsEnumerable(func));
    }

    private static IEnumerable<R> AsEnumerable<T, R>(this IEnumerable<DddContainer<T>> entities, Func<T, R> func)
    {
        foreach (var entity in entities)
        {
            yield return entity.Match(
                Invalid: e => throw new ArgumentException("There should be no invalid entities is this function."),
                Valid: v => func(v));
        }
    }

    public static DddContainer<IEnumerable<KeyValuePair<K, R>>> Traverse<K, T, R>(
        this IEnumerable<KeyValuePair<K, Entity<T>>> entitiesDictionary,
        Func<T, R> func)
    {
        if (entitiesDictionary == null)
            return DddContainer<IEnumerable<KeyValuePair<K, R>>>.Valid(Enumerable.Empty<KeyValuePair<K, R>>());

        var dddContainersInError = entitiesDictionary.Where(e => !e.Value.IsValid);

        if (dddContainersInError.Any())
            return DddContainer<IEnumerable<KeyValuePair<K, R>>>
                .Invalid(dddContainersInError.SelectMany(e => e.Value.Errors));
        else
            return DddContainer<IEnumerable<KeyValuePair<K, R>>>.Valid(entitiesDictionary.AsEnumerable(func));
    }

    private static IEnumerable<KeyValuePair<K, R>> AsEnumerable<K, T, R>(this IEnumerable<KeyValuePair<K, Entity<T>>> entities, Func<T, R> func)
    {
        foreach (var entity in entities)
        {
            yield return new KeyValuePair<K, R>(entity.Key, entity.Value.Match(
                Invalid: e => throw new Exception("There should be no invalid entities is this function."),
                Valid: v => func(v)));
        }
    }

    public static DddContainer<IEnumerable<KeyValuePair<K, R>>> Traverse<K, T, R>(
    this IEnumerable<KeyValuePair<K, ValueObject<T>>> valueObjectsDictionary,
    Func<T, R> func)
    {
        if (valueObjectsDictionary == null)
            return DddContainer<IEnumerable<KeyValuePair<K, R>>>.Valid(Enumerable.Empty<KeyValuePair<K, R>>());

        var dddContainersInError = valueObjectsDictionary.Where(e => !e.Value.IsValid);

        if (dddContainersInError.Any())
            return DddContainer<IEnumerable<KeyValuePair<K, R>>>
                .Invalid(dddContainersInError.SelectMany(e => e.Value.Errors));
        else
            return DddContainer<IEnumerable<KeyValuePair<K, R>>>.Valid(valueObjectsDictionary.AsEnumerable(func));
    }

    private static IEnumerable<KeyValuePair<K, R>> AsEnumerable<K, T, R>(this IEnumerable<KeyValuePair<K, ValueObject<T>>> valueObjects, Func<T, R> func)
    {
        foreach (var valueObject in valueObjects)
        {
            yield return new KeyValuePair<K, R>(valueObject.Key, valueObject.Value.Match(
                Invalid: e => throw new Exception("There should be no invalid value objects is this function."),
                Valid: v => func(v)));
        }
    }
}