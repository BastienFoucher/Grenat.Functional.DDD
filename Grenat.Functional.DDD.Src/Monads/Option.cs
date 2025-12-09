using System.Collections.Immutable;

namespace Grenat.Functional.DDD;

public record Option<T>
{
    public readonly bool IsSome;

    private readonly T? _value;

    private Option(T v, bool isSome) => (_value, IsSome) = (v, isSome);

    internal static Option<T> CreateSome(T v) => new(v, true);
    internal static Option<T> CreateNone() => new(default!, false);

    public R Match<R>(Func<R> None, Func<T, R> Some) => IsSome ? Some(_value!) : None();
}

public static class Option
{
    public static Option<T> Some<T>(T value) => Option<T>.CreateSome(value);

    public static Option<T> None<T>() => Option<T>.CreateNone();

    public static Option<R> Map<T, R>(this Option<T> option, Func<T, R> func)
    {
        return option.Match(
                 Some: v => Some(func(v)),
                 None: () => None<R>());
    }

    public static Option<R> Bind<T, R>(this Option<T> option, Func<T, Option<R>> func)
    {
        return option.Match(
                 Some: v => func(v),
                 None: () => None<R>());
    }

    public static Option<T> OrElse<T>(this Option<T> option, Func<Option<T>> ifNoneFunc)
    {
        return option.Match(
            Some: v => option,
            None: () => ifNoneFunc());
    }
}

public static class IEnumerableExtensions
{
    public static Option<T> GetValue<T, K>(this ImmutableDictionary<K, T> dictionary, K key) where K : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return Some(value);
        else
            return None<T>();
    }
}