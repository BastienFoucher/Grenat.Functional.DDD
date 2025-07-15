namespace Grenat.Functional.DDD;

public static class PartialApplication
{
    #region Non function arguments
    public static R Apply<P, R>(this Func<P, R> func, P arg)
    {
        return func(arg);
    }

    public static Func<P2, R> Apply<P1, P2, R>(this Func<P1, P2, R> func, P1 arg)
    {
        return p2 => func(arg, p2);
    }

    public static Func<P2, P3, R> Apply<P1, P2, P3, R>(this Func<P1, P2, P3, R> func, P1 arg)
    {
        return (p2, p3) => func(arg, p2, p3);
    }

    public static Func<P2, P3, P4, R> Apply<P1, P2, P3, P4, R>(this Func<P1, P2, P3, P4, R> func, P1 arg)
    {
        return (p2, p3, p4) => func(arg, p2, p3, p4);
    }
    #endregion

    #region Function arguments
    public static R Apply<P, R>(this Func<P, R> func, Func<P> argFunc)
    {
        return func(argFunc());
    }

    public static Func<P2, R>Apply<P1, P2, R>(this Func<P1, P2, R> func, Func<P1> argFunc)
    {
        return p2 => func(argFunc(), p2);
    }

    public static Func<P2, P3, R> Apply<P1, P2, P3, R>(this Func<P1, P2, P3, R> func, Func<P1> argFunc)
    {
        return (p2, p3) => func(argFunc(), p2, p3);
    }

    public static Func<P2, P3, P4, R> Apply<P1, P2, P3, P4, R>(this Func<P1, P2, P3, P4, R> func, Func<P1> argFunc)
    {
        return (p2, p3, p4) => func(argFunc(), p2, p3, p4);
    }
    #endregion

    #region Async function arguments
    #region 1 argument
    public static async Task<R> Apply<P, R>(this Func<P, R> func, AsyncFunc<P> argFunc)
    {
        return func(await argFunc());
    }

    public static async Task<R> Apply<P, R>(this AsyncFunc<P, R> func, P arg)
    {
        return await func(arg);
    }
    #endregion

    #region 2 arguments
    public static AsyncFunc<P2, R> Apply<P1, P2, R>(this AsyncFunc<P1, P2, R> func, P1 arg)
    {
        return async p2 => await func(arg, p2);
    }

    public static AsyncFunc<P2, R> Apply<P1, P2, R>(this AsyncFunc<P1, P2, R> func, Func<P1> argFunc)
    {
        return async p2 => await func(argFunc(), p2);
    }

    public static AsyncFunc<P2, R> Apply<P1, P2, R>(this AsyncFunc<P1, P2, R> func, AsyncFunc<P1> argFunc)
    {
        return async p2 => await func(await argFunc(), p2);
    }

    public static AsyncFunc<P2, R> Apply<P1, P2, R>(this Func<P1, P2, R> func, AsyncFunc<P1> argFunc)
    {
        return async p2 => func(await argFunc(), p2);
    }
    #endregion

    #region 3 arguments
    public static AsyncFunc<P2, P3, R> Apply<P1, P2, P3, R>(this AsyncFunc<P1, P2, P3, R> func, P1 arg)
    {
        return async (p2, p3) => await func(arg, p2, p3);
    }

    public static AsyncFunc<P2, P3, R> Apply<P1, P2, P3, R>(this AsyncFunc<P1, P2, P3, R> func, Func<P1> argFunc)
    {
        return async (p2, p3) => await func(argFunc(), p2, p3);
    }

    public static AsyncFunc<P2, P3, R> Apply<P1, P2, P3, R>(this AsyncFunc<P1, P2, P3, R> func, AsyncFunc<P1> argFunc)
    {
        return async (p2, p3) => await func(await argFunc(), p2, p3);
    }

    public static AsyncFunc<P2, P3, R> Apply<P1, P2, P3, R>(this Func<P1, P2, P3, R> func, AsyncFunc<P1> argFunc)
    {
        return async (p2, p3) => func(await argFunc(), p2, p3);
    }
    #endregion
    #endregion
}
