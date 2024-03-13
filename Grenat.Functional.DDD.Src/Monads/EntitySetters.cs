using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public static Entity<T> SetCollection<T, E>(
        this T parentEntity,
        ICollection<Entity<E>> entities, 
        Func<T, ICollection<E>, T> setter)
    {
        return Entity<T>.Valid(parentEntity)
            .SetCollection(entities, setter);
    }

    public static Entity<T> SetCollection<T, E>(
        this Entity<T> parentEntity,
        ICollection<Entity<E>> entities,
        Func<T, ICollection<E>, T> setter)
    {
        return parentEntity.Set(entities.Traverse(), (e, o) => setter(e, ToEmptyImmutableListIfNull(o)));
    }

    private static ImmutableList<E> ToEmptyImmutableListIfNull<E>(IEnumerable<E> entities)
    {
        return entities == null ? ImmutableList<E>.Empty : entities.ToImmutableList();
    }


    public static Entity<T> SetCollection<T, V>(
        this T parentEntity,
        ICollection<ValueObject<V>> valueObjects,
        Func<T, ICollection<V>, T> setter)
    {
        return Entity<T>.Valid(parentEntity).SetCollection(valueObjects, setter);
    }

    public static Entity<T> SetCollection<T, V>(
        this Entity<T> parentEntity,
        ICollection<ValueObject<V>> valueObjects,
        Func<T, ICollection<V>, T> setter)
    {
        return parentEntity.Set(valueObjects.Traverse(), (e, o) => setter(e, o.ToCollection(valueObjects)));
    }

    private static ICollection<T> ToCollection<T,C>(this IEnumerable<T> enumerator, C collectionTargetedType)
    {
        // Is there a better way for all of this code ?
        if (collectionTargetedType == null)
            return null!;
        else if (collectionTargetedType.GetType().Name.StartsWith(nameof(ImmutableList)))
            return enumerator.ToImmutableList();
        else if (collectionTargetedType!.GetType().Name.StartsWith(nameof(ImmutableArray)))
            return enumerator.ToImmutableArray();
        else if (collectionTargetedType.GetType().Name.StartsWith(nameof(ImmutableHashSet)))
            return enumerator.ToImmutableHashSet();
        else if (collectionTargetedType.GetType().IsArray)
            return enumerator.ToArray();
        else if (collectionTargetedType.GetType().Name.StartsWith("HashSet"))
            return enumerator.ToHashSet();
        else if (collectionTargetedType.GetType().Name.StartsWith("List"))
            return enumerator.ToList();
        else
            throw new ArgumentException($"Collection type {collectionTargetedType.GetType().Name} is not supported.");
    }

    public static Entity<T> SetDictionary<T, E, K>(
        this T parentEntity,
        IDictionary<K, ValueObject<E>> dddObjects,
        Func<T, IDictionary<K, E>, T> setter) where K : notnull
    {
        return Entity<T>.Valid(parentEntity).SetDictionary(dddObjects, setter);
    }

    public static Entity<T> SetDictionary<T, E, K>(
        this Entity<T> parentEntity,
        IDictionary<K, ValueObject<E>> dddObjects,
        Func<T, IDictionary<K, E>, T> setter) where K : notnull
    {
        return parentEntity.SetDictionary(dddObjects, setter);
    }

    public static Entity<T> SetDictionary<T, E, K>(
        this T parentEntity,
        IDictionary<K, Entity<E>> dddObjects,
        Func<T, IDictionary<K, E>, T> setter) where K : notnull
    {
        return Entity<T>.Valid(parentEntity).SetDictionary(dddObjects, setter);
    }

    public static Entity<T> SetDictionary<T, E, K>(
        this Entity<T> parentEntity,
        IDictionary<K, Entity<E>> dddObjects,
        Func<T, IDictionary<K, E>, T> setter) where K : notnull
    {
        return parentEntity.Set(dddObjects.Traverse(e => e), (e, o) => setter(e, o.ToImmutableDictionary()));
    }

    private static IDictionary<K, E> ToDictionaryCollection<K, E, C>(
        this IEnumerable<KeyValuePair<K, E>> enumerator,
        C dictionaryTargetedType)
        where K : notnull
    {
        if (dictionaryTargetedType == null)
            return null!;
        else if (dictionaryTargetedType.GetType().Name.StartsWith(nameof(ImmutableDictionary)))
            return enumerator.ToImmutableDictionary();
        else if (dictionaryTargetedType.GetType().Name.StartsWith("Dictionary"))
            return enumerator.ToDictionary(e => e.Key, e => e.Value);
        else
            throw new ArgumentException($"Dictionary type {typeof(C).Name} is not supported.");
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
