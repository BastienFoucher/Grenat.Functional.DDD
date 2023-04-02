namespace Grenat.Functional.DDD
{
    public class Entity<T>
    {
        public readonly IEnumerable<Error> Errors;
        private readonly T _value;

        public bool IsValid { get => !(Errors is not null && Errors.Any()); }

        private Entity(T t) => (Errors, _value) = (Enumerable.Empty<Error>(), t ?? throw new ArgumentNullException(nameof(t)));
        private Entity(IEnumerable<Error> errors) => (Errors, _value) = (errors, default(T)!);

        public static Entity<T> Valid(T t) => new(t);
        public static Entity<T> Invalid(Error error) => Invalid(new[] { error });
        public static Entity<T> Invalid(IEnumerable<Error> errors) => new(errors);

        public static implicit operator Entity<T>(T t) => Valid(t);
        public static implicit operator Entity<T>(Error error) => Invalid(new[] { error });

        public R Match<R>(Func<IEnumerable<Error>, R> Invalid, Func<T, R> Valid)
        {
            return IsValid ? Valid(_value!) : Invalid(Errors!);
        }
    }


    public static class Entity
    {
        public static Entity<T> SetValueObject<T, V>(this Entity<T> entity, ValueObject<V> valueObject, Func<T, V, T> setter)
        {
            return valueObject.Match(
                Invalid: e => Entity<T>.Invalid(e.Concat(entity.Errors)),
                Valid: v => entity.Match(
                                    Valid: t => Entity<T>.Valid(setter(t, v)),
                                    Invalid: e => entity));
        }

        public static Entity<T> SetEntity<T, E>(this Entity<T> parentEntity, Entity<E> entity, Func<T, E, T> setter)
        {
            return entity.Match(
                Invalid: e => Entity<T>.Invalid(e.Concat(parentEntity.Errors)),
                Valid: v => parentEntity.Match(
                                    Valid: t => Entity<T>.Valid(setter(t, v)),
                                    Invalid: e => parentEntity));
        }

        public static Entity<R> Map<T, R>(this Entity<T> Entity, Func<T, R> func)
        {
            return Entity.Match(
                Valid: (value) => Entity<R>.Valid(func(value)),
                Invalid: (err) => Entity<R>.Invalid(err));
        }

        public static Task<Entity<R>> MapAsync<T, R>(this Entity<T> Entity, AsyncFunc<T, R> func)
        {
            return Entity.Match(
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

        public static Entity<R> Bind<T, R>(this Entity<T> entity, Func<T, Entity<R>> func)
        {
            return entity.Match(
                Valid: (value) => func(value),
                Invalid: (err) => Entity<R>.Invalid(err));
        }

        public static Entity<R> Bind<T, P, R>(this Entity<T> entity, Func<T, P, Entity<R>> func, P arg)
        {
            return entity.Match(
                Valid: (value) => func(value, arg),
                Invalid: (err) => Entity<R>.Invalid(err));
        }

        public static Entity<R> Bind<T, P, R>(this Entity<T> entity, Func<T, P, Entity<R>> func, Func<P> arg)
        {
            return entity.Match(
                Valid: (value) => func(value, arg()),
                Invalid: (err) => Entity<R>.Invalid(err));
        }

        public static async Task<Entity<R>> BindAsync<T, P, R>(this Entity<T> entity, Func<T, P, Entity<R>> func, AsyncFunc<P> arg)
        {
            return await entity.Match(
                Valid: async (value) => func(value, await arg()),
                Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
        }

        public static Task<Entity<R>> BindAsync<T, R>(this Entity<T> entity, AsyncFunc<T, Entity<R>> func)
        {
            return entity.Match(
                Valid: async (value) => await func(value),
                Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
        }

        public static Task<Entity<R>> BindAsync<T, P, R>(this Entity<T> entity, AsyncFunc<T, P, Entity<R>> func, P arg)
        {
            return entity.Match(
                Valid: async (value) => await func(value, arg),
                Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
        }

        public static Task<Entity<R>> BindAsync<T, P, R>(this Entity<T> entity, AsyncFunc<T, P, Entity<R>> func, Func<P> arg)
        {
            return entity.Match(
                Valid: async (value) => await func(value, arg()),
                Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
        }

        public static async Task<Entity<R>> BindAsync<T, P, R>(this Entity<T> entity, AsyncFunc<T, P, Entity<R>> func, AsyncFunc<P> arg)
        {
            return await entity.Match(
                Valid: async (value) => await func(value, await arg()),
                Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
        }

        //ok
        public static async Task<Entity<R>> BindAsync<T, R>(this Task<Entity<T>> entityTask, Func<T, Entity<R>> func)
        {
            var entity = await entityTask;

            return entity.Match(
                Valid: (value) => func(value),
                Invalid: (err) => Entity<R>.Invalid(err));
        }

        //ok
        public static async Task<Entity<R>> BindAsync<T, R>(this Task<Entity<T>> entityTask, AsyncFunc<T, Entity<R>> func)
        {
            var entity = await entityTask;

            return await entity.Match(
                Valid: async (value) => await func(value),
                Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
        }

        //ok
        public static async Task<Entity<R>> BindAsync<T, P, R>(this Task<Entity<T>> entityTask, Func<T, P, Entity<R>> func, P arg)
        {
            var entity = await entityTask;

            return entity.Match(
                Valid: (value) => func(value, arg),
                Invalid: (err) => Entity<R>.Invalid(err));
        }

        //ok
        public static async Task<Entity<R>> BindAsync<T, P, R>(this Task<Entity<T>> entityTask, Func<T, P, Entity<R>> func, Func<P> arg)
        {
            var entity = await entityTask;

            return entity.Match(
                Valid: (value) => func(value, arg()),
                Invalid: (err) => Entity<R>.Invalid(err));
        }

        //ok
        public static async Task<Entity<R>> BindAsync<T, P, R>(this Task<Entity<T>> entityTask, Func<T, P, Entity<R>> func, AsyncFunc<P> arg)
        {
            var entity = await entityTask;

            return await entity.Match(
                Valid: async (value) => func(value, await arg()),
                Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
        }

        public static async Task<Entity<R>> BindAsync<T, P, R>(this Task<Entity<T>> entityTask, AsyncFunc<T, P, Entity<R>> func, AsyncFunc<P> arg)
        {
            var entity = await entityTask;

            return await entity.Match(
                Valid: async (value) => await func(value, await arg()),
                Invalid: async (err) => await Task.FromResult(Entity<R>.Invalid(err)));
        }

        public static async Task<Entity<R>> PersistAsync<TDbContext, T, R>(this Entity<T> entity, AsyncFunc<TDbContext, T, R> save, TDbContext dbContext)
        {
            return await entity.Match(
                Valid: async v => Entity<R>.Valid(await save(dbContext, v)),
                Invalid: async e => Entity<R>.Invalid(await Task.FromResult(e)));
        }

        public static async Task<Entity<R>> PersistAsync<TDbContext, T, R>(this Task<Entity<T>> entityTask, AsyncFunc<TDbContext, T, R> save, TDbContext dbContext)
        {
            var entity = await entityTask;

            return await entity.Match(
                Valid: async v => Entity<R>.Valid(await save(dbContext, v)),
                Invalid: async e => Entity<R>.Invalid(await Task.FromResult(e)));
        }
    }
}
