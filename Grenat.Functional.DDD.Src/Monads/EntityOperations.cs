using System.Collections.Immutable;

namespace Grenat.Functional.DDD;

public static class EntityOperations
{
    public static Task<Entity<T>> MapAsync<T>(this Entity<T> entity, AsyncAction<T> action)
    {
        return entity.Match(
            Valid: async (value) =>
            {
                await action(value);
                return await Task.FromResult(entity);
            },
            Invalid: async (err) => Entity<T>.Invalid(await Task.FromResult(err)));
    }

    public static async Task<Entity<T>> MapAsync<T>(this Task<Entity<T>> entityTask, AsyncAction<T> action)
    {
        var entity = await entityTask;

        return await entity.Match(
            Valid: async (value) =>
            {
                await action(value);
                return await Task.FromResult(entity);
            },
            Invalid: async (err) => Entity<T>.Invalid(await Task.FromResult(err)));
    }

    public static Task<Entity<R>> MapAsync<T, R>(this Entity<T> entity, AsyncFunc<T, R> func)
    {
        return entity.Match(
            Valid: async (value) => Entity<R>.Valid(await func(value)),
            Invalid: async (err) => Entity<R>.Invalid(await Task.FromResult(err)));
    }

    public static async Task<Entity<R>> MapAsync<T, R>(this Task<Entity<T>> entityTask, AsyncFunc<T, R> func)
    {
        var entity = await entityTask;

        return await entity.Match(
            Valid: async (value) => Entity<R>.Valid(await func(value)),
            Invalid: async (err) => Entity<R>.Invalid(await Task.FromResult(err)));
    }

    public static async Task<Entity<R>> MapParallel<T, R>(this Entity<T> entity,
      IEnumerable<AsyncFunc<T, R>> funcs,
      Func<ImmutableList<R>, R> aggregationFunc)
    {
        var resultingValues = ImmutableList<R>.Empty;
        var errors = ImmutableList<Error>.Empty;

        return await entity.Match(
            Valid: async (value) =>
            {
                var tasks = new List<Task<R>>();
                foreach (var func in funcs) tasks.Add(func(value));

                await Task.WhenAll(tasks);

                foreach (var task in tasks) resultingValues = resultingValues.Add(await task);

                if (errors.Any()) return Entity<R>.Invalid(errors);
                else
                    return await Task.FromResult(Entity<R>.Valid(aggregationFunc(resultingValues)));

            },
            Invalid: async (err) => Entity<R>.Invalid(await Task.FromResult(err)));
    }

    public static async Task<Entity<R>> MapParallel<T, R>(this Task<Entity<T>> entityTask,
        IEnumerable<AsyncFunc<T, R>> funcs,
        Func<ImmutableList<R>, R> aggregationFunc)
    {
        var entity = await entityTask;

        return await MapParallel(entity, funcs, aggregationFunc);
    }

    public static async Task<Entity<R>> BindParallel<T, R>(this Entity<T> entity,
        IEnumerable<AsyncFunc<T, Entity<R>>> funcs,
        Func<ImmutableList<R>, R> aggregationFunc)
    {
        var resultingEntities = ImmutableList<Entity<R>>.Empty;
        var resultingValues = ImmutableList<R>.Empty;
        var errors = ImmutableList<Error>.Empty;

        return await entity.Match(
            Valid: async (value) =>
            {
                var tasks = new List<Task<Entity<R>>>();
                foreach (var func in funcs) tasks.Add(func(value));

                await Task.WhenAll(tasks);

                foreach (var task in tasks) resultingEntities = resultingEntities.Add(await task);

                foreach (var entity in resultingEntities)
                {
                    entity.Match(
                        Invalid: e =>
                        {
                            errors = errors.AddRange(e);
                            return resultingValues;
                        },
                        Valid: v =>
                        {
                            resultingValues = resultingValues.Add(v);
                            return resultingValues;
                        });
                }

                if (errors.Any()) return Entity<R>.Invalid(errors);
                else
                    return await Task.FromResult(Entity<R>.Valid(aggregationFunc(resultingValues)));

            },
            Invalid: async (err) => Entity<R>.Invalid(await Task.FromResult(err)));
    }

    public static async Task<Entity<R>> BindParallel<T, R>(this Task<Entity<T>> entityTask,
                IEnumerable<AsyncFunc<T, Entity<R>>> funcs,
                Func<ImmutableList<R>, R> aggregationFunc)
    {
        var entity = await entityTask;

        return await BindParallel(entity, funcs, aggregationFunc);
    }

    public static Entity<R> Bind<T, R>(this Entity<T> entity, Func<T, Entity<R>> func)
    {
        return entity.Match(
            Valid: (value) => func(value),
            Invalid: (err) => Entity<R>.Invalid(err));
    }

    public static Task<Entity<R>> BindAsync<T, R>(this Entity<T> entity, AsyncFunc<T, Entity<R>> func)
    {
        return entity.Match(
            Valid: async (value) => await func(value),
            Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
    }

    public static async Task<Entity<R>> BindAsync<T, R>(this Task<Entity<T>> entityTask, Func<T, Entity<R>> func)
    {
        var entity = await entityTask;

        return entity.Match(
            Valid: (value) => func(value),
            Invalid: (err) => Entity<R>.Invalid(err));
    }

    public static async Task<Entity<R>> BindAsync<T, R>(this Task<Entity<T>> entityTask, AsyncFunc<T, Entity<R>> func)
    {
        var entity = await entityTask;

        return await entity.Match(
            Valid: async (value) => await func(value),
            Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
    }
}
