using System.Collections.Immutable;

namespace Grenat.Functional.DDD.Src;

public static class EntitySetters
{
    public static Entity<T> SetMember<T, V>(this T entity, V value, Action<T, V> setter)
    {
        return Entity<T>.Valid(entity).SetMember(value, setter);
    }

    public static Entity<T> SetMember<T, V>(this Entity<T> entity, V value, Action<T, V> setter)
    {
        var executor = new Executor<T, V>(setter);
        return entity.SetMember(value, executor);
    }

    public static Entity<T> SetMember<T, V>(this T entity, V value, Func<T, V, T> setter)
    {
        var executor = new Executor<T, V>(setter);
        return Entity<T>.Valid(entity).SetMember(value, executor);
    }

    public static Entity<T> SetMember<T, V>(this Entity<T> entity, V value, Func<T, V, T> setter)
    {
        var executor = new Executor<T, V>(setter);
        return entity.SetMember(value, executor);
    }

    public static Entity<T> SetMember<T, V>(this Entity<T> entity, V value, Executor<T, V> executor)
    {
        if (value is null) return entity;

        return entity.Match(
            Valid: t => executor.Invoke(t, value),
            Invalid: e => entity);
    }

    public static Entity<T> SetValueObject<T, V>(this T entity, ValueObject<V> valueObject, Action<T, V> setter)
    {
        return Entity<T>.Valid(entity).SetValueObject(valueObject, setter);
    }

    public static Entity<T> SetValueObject<T, V>(this Entity<T> entity, ValueObject<V> valueObject, Action<T, V> setter)
    {
        var executor = new Executor<T, V>(setter);
        return entity.SetValueObject(valueObject, executor);
    }

    public static Entity<T> SetValueObject<T, V>(this T entity, ValueObject<V> valueObject, Func<T, V, T> setter)
    {
        return Entity<T>.Valid(entity).SetValueObject(valueObject, setter);
    }

    public static Entity<T> SetValueObject<T, V>(this Entity<T> entity, ValueObject<V> valueObject, Func<T, V, T> setter)
    {
        var executor = new Executor<T, V>(setter);
        return entity.SetValueObject(valueObject, executor);
    }

    private static Entity<T> SetValueObject<T, V>(this Entity<T> entity, ValueObject<V> valueObject, Executor<T, V> executor)
    {
        if (valueObject is null) return entity;

        return valueObject.Match(
            Invalid: e => Entity<T>.Invalid(e.Concat(entity.Errors)),
            Valid: v => entity.Match(
                                Valid: t => executor.Invoke(t, v),
                                Invalid: e => entity));
    }

    public static Entity<T> SetValueObjectList<T, V>(this T entity, ImmutableList<ValueObject<V>> valueObjects, Action<T, ImmutableList<V>> setter)
    {
        return Entity<T>.Valid(entity).SetValueObjectList(valueObjects, setter);
    }

    public static Entity<T> SetValueObjectList<T, V>(this Entity<T> parentEntity, ImmutableList<ValueObject<V>> valueObjects, Action<T, ImmutableList<V>> setter)
    {
        var executor = new Executor<T, ImmutableList<V>>(setter);
        return parentEntity.SetValueObjectList(valueObjects, executor);
    }

    public static Entity<T> SetValueObjectList<T, V>(this T entity, ImmutableList<ValueObject<V>> valueObjects, Func<T, ImmutableList<V>, T> setter)
    {
        return Entity<T>.Valid(entity).SetValueObjectList(valueObjects, setter);
    }

    public static Entity<T> SetValueObjectList<T, V>(this Entity<T> parentEntity, ImmutableList<ValueObject<V>> valueObjects, Func<T, ImmutableList<V>, T> setter)
    {
        var executor = new Executor<T, ImmutableList<V>>(setter);
        return parentEntity.SetValueObjectList(valueObjects, executor);
    }

    private static Entity<T> SetValueObjectList<T, V>(this Entity<T> parentEntity, ImmutableList<ValueObject<V>> valueObjects, Executor<T, ImmutableList<V>> executor)
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
                Valid: v => executor.Invoke(v, validValues));
        }
    }

    public static Entity<T> SetEntity<T, E>(this T parentEntity, Entity<E> entity, Action<T, E> setter)
    {
        return Entity<T>.Valid(parentEntity).SetEntity(entity, setter);
    }

    public static Entity<T> SetEntity<T, E>(this Entity<T> parentEntity, Entity<E> entity, Action<T, E> setter)
    {
        var executor = new Executor<T, E>(setter);
        return parentEntity.SetEntity(entity, executor);
    }

    public static Entity<T> SetEntity<T, E>(this T parentEntity, Entity<E> entity, Func<T, E, T> setter)
    {
        var executor = new Executor<T, E>(setter);
        return Entity<T>.Valid(parentEntity).SetEntity(entity, executor);
    }

    public static Entity<T> SetEntity<T, E>(this Entity<T> parentEntity, Entity<E> entity, Func<T, E, T> setter)
    {
        var executor = new Executor<T, E>(setter);
        return parentEntity.SetEntity(entity, executor);
    }

    private static Entity<T> SetEntity<T, E>(this Entity<T> parentEntity, Entity<E> entity, Executor<T, E> executor)
    {
        if (entity is null) return parentEntity;

        return entity.Match(
            Invalid: e => Entity<T>.Invalid(e.Concat(parentEntity.Errors)),
            Valid: v => parentEntity.Match(
                                Valid: t => executor.Invoke(t, v),
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
}
