using System.Reflection.Metadata.Ecma335;

namespace Grenat.Functional.DDD.Src;

public class Executor<T, V>
{
    private Option<Func<T, V, T>> Func { get; set; }

    private Option<Action<T, V>> Action { get; set; }

    public Executor(Func<T, V, T> func)
    {
        Func = Some(func);

        Action = None<Action<T, V>>();
    }

    public Executor(Action<T, V> action)
    {
        Func = None<Func<T, V, T>>();
        Action = Some(action);
    }

    public Entity<T> Invoke(T obj, V arg)
    {
        return Action.Match(
            Some: a => { a(obj, arg); return Entity<T>.Valid(obj); },
            None: () =>
            {
                return Func.Match(
                    Some: f => Entity<T>.Valid(f(obj, arg)),
                    None: () => obj);
            });
    }
}
