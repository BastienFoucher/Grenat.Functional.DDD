namespace Grenat.Functional.DDD.Src.Commons;

public static class Applicatives
{
    public static Func<R> Apply<P, R>(this Func<P, R> func, P arg)
    {
        return () => func(arg);
    }

    public static Func<P2, R> Apply<P1, P2, R>(this Func<P1, P2, R> func, P1 arg)
    {
        return p2 => func(arg, p2);
    }

    public static Func<P3, R> Apply<P1, P2, P3, R>(this Func<P1, P2, P3, R> func, P1 arg1, P2 arg2)
    {
        return p3 => func(arg1, arg2, p3);
    }
}
