namespace Grenat.Functional.DDD
{
    public delegate Task AsyncAction<in T>(T arg);
}
