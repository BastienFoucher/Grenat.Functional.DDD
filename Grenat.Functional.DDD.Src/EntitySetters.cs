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
        ImmutableList<DddContainer<E>> dddContainers, 
        Func<T, ImmutableList<E>, T> setter)
    {
        return Entity<T>.Valid(parentEntity).SetImmutableList(dddContainers, setter);
    }

    public static Entity<T> SetImmutableList<T, E>(
        this Entity<T> parentEntity, 
        ImmutableList<DddContainer<E>> dddContainers, 
        Func<T, ImmutableList<E>, T> setter)
    {
        var validValues = ImmutableList<E>.Empty;
        var errors = new List<Error>();

        dddContainers ??= ImmutableList<DddContainer<E>>.Empty;

        foreach (var item in dddContainers)
        {
            if (item.IsValid)
            {
                validValues = validValues.Add(
                    item.Match(
                        Valid: v => v,
                        Invalid: e => default!
                    ));
            }
            else
            {
                errors.AddRange(
                    item.Match(
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

    public static Entity<T> SetImmutableDictionary<T, E, K>(
        this T parentEntity,
        ImmutableDictionary<K, DddContainer<E>> dddObjects,
        Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull
    {
        return Entity<T>.Valid(parentEntity).SetImmutableDictionary(dddObjects, setter);
    }

    public static Entity<T> SetImmutableDictionary<T, E, K>(
        this Entity<T> parentEntity,
        ImmutableDictionary<K, DddContainer<E>> dddObjects,
        Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull
    {
        var validValues = ImmutableDictionary<K, E>.Empty;
        var errors = new List<Error>();

        dddObjects ??= ImmutableDictionary<K, DddContainer<E>>.Empty;

        foreach (var entity in dddObjects)
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
