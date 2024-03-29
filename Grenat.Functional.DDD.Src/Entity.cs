﻿using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

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
        public static implicit operator Entity<T>(Error[] errors) => Invalid(errors.ToArray());

        public R Match<R>(Func<Error[], R> Invalid, Func<T, R> Valid)
        {
            return IsValid ? Valid(_value!) : Invalid(Errors.ToArray()!);
        }
    }


    public static class Entity
    {

        public static Entity<T> SetValueObject<T, V>(this T entity, ValueObject<V> valueObject, Func<T, V, T> setter)
        {
            return Entity<T>.Valid(entity).SetValueObject(valueObject, setter);
        }

        public static Entity<T> SetValueObject<T, V>(this Entity<T> entity, ValueObject<V> valueObject, Func<T, V, T> setter)
        {
            if (valueObject is null) return entity;

            return valueObject.Match(
                Invalid: e => Entity<T>.Invalid(e.Concat(entity.Errors)),
                Valid: v => entity.Match(
                                    Valid: t => Entity<T>.Valid(setter(t, v)),
                                    Invalid: e => entity));
        }

        public static Entity<T> SetValueObjectList<T, V>(this T entity, ImmutableList<ValueObject<V>> valueObjects, Func<T, ImmutableList<V>, T> setter)
        {
            return Entity<T>.Valid(entity).SetValueObjectList(valueObjects, setter);
        }

        public static Entity<T> SetValueObjectList<T, V>(this Entity<T> parentEntity, ImmutableList<ValueObject<V>> valueObjects, Func<T, ImmutableList<V>, T> setter)
        {
            var validValues = ImmutableList<V>.Empty;
            var errors = new List<Error>();

            valueObjects ??= ImmutableList<ValueObject<V>>.Empty;

            foreach (var valueObject in valueObjects)
            {
                if (valueObject.IsValid)
                {
                    validValues = validValues.Add(
                        valueObject.Match(
                            Valid: v => v,
                            Invalid: e => default!
                        ));
                }
                else
                {
                    errors.AddRange(
                        valueObject.Match(
                            Valid: v => default!,
                            Invalid: e => e)
                    );
                }
            }

            if (errors.Count > 0) return Entity<T>.Invalid(errors);
            else
            {
                return parentEntity.Match(
                    Invalid: e => Entity<T>.Invalid(e.Concat(parentEntity.Errors)),
                    Valid: v => Entity<T>.Valid(setter(v, validValues)));
            }
        }

        public static Entity<T> SetEntity<T, E>(this T parentEntity, Entity<E> entity, Func<T, E, T> setter)
        {
            return Entity<T>.Valid(parentEntity).SetEntity(entity, setter);
        }

        public static Entity<T> SetEntity<T, E>(this Entity<T> parentEntity, Entity<E> entity, Func<T, E, T> setter)
        {
            if (entity is null) return parentEntity;

            return entity.Match(
                Invalid: e => Entity<T>.Invalid(e.Concat(parentEntity.Errors)),
                Valid: v => parentEntity.Match(
                                    Valid: t => Entity<T>.Valid(setter(t, v)),
                                    Invalid: e => parentEntity));
        }

        public static Entity<T> SetEntityList<T, E>(this T parentEntity, ImmutableList<Entity<E>> entities, Func<T, ImmutableList<E>, T> setter)
        {
            return Entity<T>.Valid(parentEntity).SetEntityList(entities, setter);
        }

        public static Entity<T> SetEntityList<T, E>(this Entity<T> parentEntity, ImmutableList<Entity<E>> entities, Func<T, ImmutableList<E>, T> setter)
        {
            var validValues = ImmutableList<E>.Empty;
            var errors = new List<Error>();

            entities ??= ImmutableList<Entity<E>>.Empty;

            foreach (var entity in entities)
            {
                if (entity.IsValid)
                {
                    validValues = validValues.Add(
                        entity.Match(
                            Valid: v => v,
                            Invalid: e => default!
                        ));
                }
                else
                {
                    errors.AddRange(
                        entity.Match(
                            Valid: v => default!,
                            Invalid: e => e)
                    );
                }
            }

            if (errors.Count > 0) return Entity<T>.Invalid(errors);
            else
            {
                return parentEntity.Match(
                    Invalid: e => Entity<T>.Invalid(e.Concat(parentEntity.Errors)),
                    Valid: v => Entity<T>.Valid(setter(v, validValues)));
            }
        }

        public static Entity<T> SetEntityDictionary<T, E, K>(this T parentEntity,
            ImmutableDictionary<K, Entity<E>> entities,
            Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull
        {
            return Entity<T>.Valid(parentEntity).SetEntityDictionary(entities, setter);
        }

        public static Entity<T> SetEntityDictionary<T, E, K>(this Entity<T> parentEntity,
            ImmutableDictionary<K, Entity<E>> entities,
            Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull
        {
            var validValues = ImmutableDictionary<K, E>.Empty;
            var errors = new List<Error>();

            entities ??= ImmutableDictionary<K, Entity<E>>.Empty;

            foreach (var entity in entities)
            {
                if (entity.Value.IsValid)
                {
                    validValues = validValues.Add(entity.Key,
                        entity.Value.Match(
                            Valid: v => v,
                            Invalid: e => default!
                        ));
                }
                else
                {
                    errors.AddRange(
                        entity.Value.Match(
                            Valid: v => default!,
                            Invalid: e => e)
                    );
                }
            }

            if (errors.Count > 0) return Entity<T>.Invalid(errors);
            else
            {
                return parentEntity.Match(
                    Invalid: e => Entity<T>.Invalid(e.Concat(parentEntity.Errors)),
                    Valid: v => Entity<T>.Valid(setter(v, validValues)));
            }
        }

        public static Entity<T> SetValueObjectOption<T, V>(this Entity<T> parentEntity,
            Func<ValueObject<V>> valueObject,
            Func<bool> predicate,
            Func<T, Option<V>, T> setter)
        {
            return parentEntity.Match(
                Invalid: pe => Entity<T>.Invalid(pe),
                Valid: v =>
                {
                    if (predicate())
                    {
                        var valueObjectResult = valueObject();
                        if (valueObjectResult is null)
                            return Entity<T>.Valid(setter(v, None<V>()));
                        else
                            return valueObject().Match(
                                Valid: vo => Entity<T>.Valid(setter(v, Some(vo))),
                                Invalid: e => Entity<T>.Invalid(e));
                    }
                    else
                        return Entity<T>.Valid(setter(v, None<V>()));
                });
        }

        public static Entity<T> SetValueObjectOption<T, V>(this T parentEntity,
            Option<ValueObject<V>> valueObjectOption,
            Func<T, Option<V>, T> setter)
        {
            return Entity<T>.Valid(parentEntity).SetValueObjectOption(valueObjectOption, setter);
        }

        public static Entity<T> SetValueObjectOption<T, V>(this Entity<T> parentEntity,
            Option<ValueObject<V>> valueObjectOption,
            Func<T, Option<V>, T> setter)
        {
            return parentEntity.Match(
                Invalid: error => Entity<T>.Invalid(error),
                Valid: entity =>
                {
                    if (valueObjectOption is null)
                        return Entity<T>.Valid(setter(entity, None<V>()));
                    else
                    {
                        return valueObjectOption.Match(
                            Some: valueObject =>
                            {
                                if (valueObject is null)
                                    return Entity<T>.Valid(setter(entity, None<V>()));
                                else
                                {
                                    return valueObject.Match(
                                                    Valid: v => Entity<T>.Valid(setter(entity, Some(v))),
                                                    Invalid: e => Entity<T>.Invalid(e));
                                }
                            },
                            None: () => Entity<T>.Valid(setter(entity, None<V>())));
                    }
                });
        }

        public static Entity<T> SetEntityOption<T, V>(this Entity<T> parentEntity,
            Func<Entity<V>> entity,
            Func<bool> predicate,
            Func<T, Option<V>, T> setter)
        {
            return parentEntity.Match(
                Invalid: e => Entity<T>.Invalid(e),
                Valid: v =>
                {
                    if (predicate())
                    {
                        var entityResult = entity();
                        if (entityResult is null)
                            return Entity<T>.Valid(setter(v, None<V>()));
                        else
                            return entityResult.Match(
                                Valid: en => Entity<T>.Valid(setter(v, Some(en))),
                                Invalid: err => Entity<T>.Invalid(err));
                    }
                    else
                        return Entity<T>.Valid(setter(v, None<V>()));
                });
        }

        public static Entity<T> SetEntityOption<T, V>(this Entity<T> parentEntity,
            Entity<V> entity,
            Func<V, bool> predicate,
            Func<T, Option<V>, T> setter)
        {
            return parentEntity.Match(
                Invalid: e => Entity<T>.Invalid(e.Concat(parentEntity.Errors)),
                Valid: v =>
                {
                    if (entity is null)
                        return Entity<T>.Valid(setter(v, None<V>()));
                    else
                        return entity.Match(
                            Valid: t =>
                            {
                                if (t is null)
                                    return Entity<T>.Valid(setter(v, None<V>()));
                                else if (predicate(t))
                                    return Entity<T>.Valid(setter(v, Some(t)));
                                else
                                    return Entity<T>.Valid(setter(v, None<V>()));
                            },
                            Invalid: err => Entity<T>.Invalid(err));
                });
        }

        public static Entity<T> SetEntityOption<T, V>(this T parentEntity,
            Option<Entity<V>> entityOption,
            Func<T, Option<V>, T> setter)
        {
            return Entity<T>.Valid(parentEntity).SetEntityOption(entityOption, setter);
        }


        public static Entity<T> SetEntityOption<T, V>(this Entity<T> parentEntity,
            Option<Entity<V>> entityOption,
            Func<T, Option<V>, T> setter)
        {
            return parentEntity.Match(
                Invalid: error => Entity<T>.Invalid(error),
                Valid: entity =>
                {
                    if (entityOption is null)
                        return Entity<T>.Valid(setter(entity, None<V>()));
                    else
                    {
                        return entityOption.Match(
                            Some: value =>
                            {
                                if (value is null)
                                    return Entity<T>.Valid(setter(entity, None<V>()));
                                else
                                {
                                    return value.Match(
                                        Valid: v => Entity<T>.Valid(setter(entity, Some(v))),
                                        Invalid: e => Entity<T>.Invalid(e));
                                }
                            },
                            None: () => Entity<T>.Valid(setter(entity, None<V>())));
                    }
                });
        }

        public static Entity<R> Map<T, R>(this Entity<T> Entity, Func<T, R> func)
        {
            return Entity.Match(
                Valid: (value) => Entity<R>.Valid(func(value)),
                Invalid: (err) => Entity<R>.Invalid(err));
        }

        public static Entity<T> Map<T>(this Entity<T> entity, Action<T> action)
        {
            return entity.Match(
                Valid: (value) =>
                {
                    action(value);
                    return entity;
                },
                Invalid: (err) => Entity<T>.Invalid(err));
        }

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
