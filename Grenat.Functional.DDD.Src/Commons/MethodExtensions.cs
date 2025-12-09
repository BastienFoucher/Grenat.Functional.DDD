using System.Linq.Expressions;
using System.Reflection;

namespace Grenat.Functional.DDD;

public static class MethodExtensions
{
    //public static Func<T, TResult> AsFunc<T, TResult>(this Func<T, TResult> method)
    //{
    //    return method;
    //}

    public static Func<T1, T2, TResult> AsFunc<T1, T2, TResult>(this Func<T1, T2, TResult> method)
    {
        return method;
    }
}