using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grenat.Functional.DDD
{
    public delegate Task<T> AsyncFunc<T>();
    public delegate Task<R> AsyncFunc<T, R>(T arg);
    public delegate Task<R> AsyncFunc<T1, T2, R>(T1 arg1, T2 arg2);
    public delegate Task<R> AsyncFunc<T1, T2, T3, R>(T1 arg1, T2 arg2, T3 arg);
}
