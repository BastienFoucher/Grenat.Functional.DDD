using System.Collections.Immutable;

namespace Grenat.Functional.DDD;

public static class EntitySetters
{
    public static Entity<T> Set<T, V>(this T entity, V value, Action<T, V> setter)
    {
        return Entity<T>.Valid(entity).Set(value, setter);
    }

    public static Entity<T> Set<T, V>(this Entity<T> entity, V value, Action<T, V> setter)
    {
        var executor = new Executor<T, V>(setter);
        return entity.Set(value, executor);
    }

    public static Entity<T> Set<T, V>(this T entity, V value, Func<T, V, T> setter)
    {
        var executor = new Executor<T, V>(setter);
        return Entity<T>.Valid(entity).Set(value, executor);
    }

    public static Entity<T> Set<T, V>(this Entity<T> entity, V value, Func<T, V, T> setter)
    {
        var executor = new Executor<T, V>(setter);
        return entity.Set(value, executor);
    }

    private static Entity<T> Set<T, V>(this Entity<T> entity, V value, Executor<T, V> executor)
    {
        if (value is null) return entity;

        return entity.Match(
            Valid: t => executor.Invoke(t, value),
            Invalid: e => entity);
    }

    public static Entity<T> Set<T, E>(this T parentEntity, DddContainer<E> dddContainer, Action<T, E> setter)
    {
        return Entity<T>.Valid(parentEntity).Set(dddContainer, setter);
    }

    public static Entity<T> Set<T, E>(this Entity<T> parentEntity, DddContainer<E> dddContainer, Action<T, E> setter)
    {
        var executor = new Executor<T, E>(setter);
        return parentEntity.Set(dddContainer, executor);
    }

    public static Entity<T> Set<T, E>(this T parentEntity, DddContainer<E> dddContainer, Func<T, E, T> setter)
    {
        var executor = new Executor<T, E>(setter);
        return Entity<T>.Valid(parentEntity).Set(dddContainer, executor);
    }

    public static Entity<T> Set<T, E>(this Entity<T> parentEntity, DddContainer<E> dddContainer, Func<T, E, T> setter)
    {
        var executor = new Executor<T, E>(setter);
        return parentEntity.Set(dddContainer, executor);
    }

    private static Entity<T> Set<T, E>(this Entity<T> parentEntity, DddContainer<E> dddContainer, Executor<T, E> executor)
    {
        if (dddContainer is null) return parentEntity;

        return dddContainer.Match(
            Invalid: e => Entity<T>.Invalid(e.Concat(parentEntity.Errors)),
            Valid: v => parentEntity.Match(
                                Valid: t => executor.Invoke(t, v),
                                Invalid: e => parentEntity));
    }

    public static Entity<T> SetImmutableList<T, E>(
        this T parentEntity, 
        ImmutableList<Entity<E>> entities, 
        Func<T, ImmutableList<E>, T> setter)
    {
        return Entity<T>.Valid(parentEntity)
            .SetImmutableList(entities, setter);
    }

    public static Entity<T> SetImmutableList<T, E>(
        this Entity<T> parentEntity,
        ImmutableList<Entity<E>> entities,
        Func<T, ImmutableList<E>, T> setter)
    {
        return parentEntity.Set(entities.Traverse(), (e, o) => setter(e, ToEmptyImmutableListIfNull(o)));
    }

    private static ImmutableList<E> ToEmptyImmutableListIfNull<E>(IEnumerable<E> entities)
    {
        return entities == null ? ImmutableList<E>.Empty : entities.ToImmutableList();
    }


    public static Entity<T> SetImmutableList<T, E>(
        this T parentEntity,
        ImmutableList<ValueObject<E>> valueObjects,
        Func<T, ImmutableList<E>, T> setter)
    {
        return Entity<T>.Valid(parentEntity).SetImmutableList(valueObjects, setter);
    }

    public static Entity<T> SetImmutableList<T, E>(
        this Entity<T> parentEntity,
        ImmutableList<ValueObject<E>> valueObjects,
        Func<T, ImmutableList<E>, T> setter)
    {
        return parentEntity.Set(valueObjects.Traverse(), (e, o) => setter(e, o.ToImmutableList()));
    }


    public static Entity<T> SetImmutableDictionary<T, E, K>(
        this T parentEntity,
        ImmutableDictionary<K, ValueObject<E>> dddObjects,
        Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull
    {
        return Entity<T>.Valid(parentEntity).SetImmutableDictionary(dddObjects, setter);
    }

    public static Entity<T> SetImmutableDictionary<T, E, K>(
        this Entity<T> parentEntity,
        ImmutableDictionary<K, ValueObject<E>> dddObjects,
        Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull
    {
        return parentEntity.SetImmutableDictionary(dddObjects, setter);
    }

    public static Entity<T> SetImmutableDictionary<T, E, K>(
        this T parentEntity,
        ImmutableDictionary<K, Entity<E>> dddObjects,
        Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull
    {
        return Entity<T>.Valid(parentEntity).SetImmutableDictionary(dddObjects, setter);
    }

    public static Entity<T> SetImmutableDictionary<T, E, K>(
        this Entity<T> parentEntity,
        ImmutableDictionary<K, Entity<E>> dddObjects,
        Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull
    {
        return parentEntity.Set(dddObjects.Traverse(e => e), (e, o) => setter(e, o.ToImmutableDictionary()));
    }

    public static Entity<T> SetOption<T, V>(
        this T parentEntity,
        Option<ValueObject<V>> valueObjectOption,
        Func<T, Option<V>, T> setter)
    {
        return Entity<T>.Valid(parentEntity).SetOption(valueObjectOption, setter);
    }

    public static Entity<T> SetOption<T, V>(
        this Entity<T> parentEntity,
        Option<ValueObject<V>> valueObjectOption,
        Func<T, Option<V>, T> setter)
    {
        return parentEntity.Set(valueObjectOption.Traverse(r => r), (e, o) => setter(e, o));
    }

    public static Entity<T> SetOption<T, V>(
        this T parentEntity,
        Option<Entity<V>> entityOption,
        Func<T, Option<V>, T> setter)
    {
        return Entity<T>.Valid(parentEntity).SetOption(entityOption, setter);
    }

    public static Entity<T> SetOption<T, V>(
        this Entity<T> parentEntity,
        Option<Entity<V>> entityOption,
        Func<T, Option<V>, T> setter)
    {
        return parentEntity.Set(entityOption.Traverse(r => r), (e, o) => setter(e, o));
    }

    public static Entity<T> SetOption<T, V>(
        this Entity<T> parentEntity,
        Func<DddContainer<V>> dddContainer,
        Func<bool> predicate,
        Func<T, Option<V>, T> setter)
    {
        return parentEntity.Match(
            Invalid: Entity<T>.Invalid,
            Valid: v =>
            {
                if (predicate())
                {
                    var entityResult = dddContainer();
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

    public static Entity<T> SetOption<T, V>(
        this Entity<T> parentEntity,
        DddContainer<V> entity,
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
}
