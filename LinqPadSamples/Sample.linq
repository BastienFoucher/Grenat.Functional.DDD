<Query Kind="Statements">
  <IncludeUncapsulator>false</IncludeUncapsulator>
</Query>

using System.Collections.Immutable;

var cart = Cart.CreateEmpty()
               .WithId("1ds3d!bM")
               .WithItem(Item.CreateEmpty()
                                          .WithId("45xxsDg1=")
                                          .WithProductId("ne252TJqAWk3")
                                          .WithAmount(25))
                  .WithTotalAmountWithoutTax(25)
                  .Match(
                        Valid: c => { Console.WriteLine(c); return Entity<Cart>.Valid(c); },
                        Invalid: e => { Console.WriteLine(e); return Entity<Cart>.Invalid(e); });

public record Cart
{
    public Identifier Id { get; init; }
    public ImmutableList<Item> Items { get; init; }
    public Amount TotalAmountWithoutTax { get; init; }
    public Amount TotalAmountWithTax { get; init; }

    private Cart()
    {
        Id = new Identifier();
        Items = ImmutableList.Create<Item>();
        TotalAmountWithoutTax = new Amount();
        TotalAmountWithTax = new Amount();
    }

    public static Entity<Cart> CreateEmpty()
    {
        return new Cart();
    }
}

public static class CartBuilder
{
      public static Entity<Cart> WithId(this Entity<Cart> cart, string id) 
      {
            return cart.SetValueObject(Identifier.Create(id), (cart, identifier) => cart with { Id = identifier });
      }
      
      public static Entity<Cart> WithTotalAmountWithoutTax(this Entity<Cart> cart, int totalAmount) 
      {
            return cart.SetValueObject(Amount.Create(totalAmount), (cart, totalAmount) => cart with { TotalAmountWithoutTax = totalAmount });
      }
      
      public static Entity<Cart> WithTotalAmountWithTax(this Entity<Cart> cart, int totalAmountWithTax) 
      {
            return cart.SetValueObject(Amount.Create(totalAmountWithTax), (cart, totalAmountWithTax) => cart with { TotalAmountWithTax = totalAmountWithTax });
      }
      
      public static Entity<Cart> WithItem(this Entity<Cart> cart, Entity<Item> item) 
      {
            return cart.SetEntity(item, (cart, item) => cart with { Items = cart.Items.Add(item) });
      }
}

public record Item 
{
      public Identifier Id { get; init; }
      public Identifier ProductId { get; init; }
      public Amount Amount { get; init; }
      
    public Item()
    {
        Id = new Identifier();
            ProductId = new Identifier();
            Amount = new Amount();
    }
      
    public static Entity<Item> CreateEmpty()
    {
        return new Item();
    }
}

public static class ItemBuilder 
{
      public static Entity<Item> WithId(this Entity<Item> item, string id) 
      {
            return item.SetValueObject(Identifier.Create(id), (item, identifier) => item with { Id = identifier });
      }
      
      public static Entity<Item> WithProductId(this Entity<Item> item, string productId) 
      {
            return item.SetValueObject(Identifier.Create(productId), (item, productId) => item with { ProductId = productId });
      }
      
      public static Entity<Item> WithAmount(this Entity<Item> item, int amount) 
      {
            return item.SetValueObject(Amount.Create(amount), (item, amount) => item with { Amount = amount });
      }
}

public record Identifier
{
    public const string DEFAULT_VALUE = "";
    public readonly string Value = "";
      
      public Identifier() { Value = DEFAULT_VALUE; }

    private Identifier(string identifier)
    {
        Value = identifier;
    }

    public static ValueObject<Identifier> Create(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return new Error("An identifier cannot be null or empty.");
        
        return new Identifier(identifier);
    }
}

public record Amount
{
    public const int MAX_AMOUNT = 2500;
    public const int DEFAULT_VALUE = 0;
    public const string DEFAULT_CURRENCY = "EUR";

    public readonly int Value;
    public readonly string Currency = DEFAULT_CURRENCY;

    public Amount() { Value = DEFAULT_VALUE; }

    private Amount(int value, string currency = DEFAULT_CURRENCY)
    {
        Value = value;
        Currency = currency;
    }

    public static ValueObject<Amount> Create(int amount)
    {
        if (amount < 0)
            return new Error("An amount cannot be negative.");
        if (amount > MAX_AMOUNT)
            return new Error(String.Format($"An amount cannot be more than {MAX_AMOUNT}"));
                
        return new Amount(amount);
    }

    public static implicit operator int(Amount amount) => amount.Value;
}


public record Error
{
    public string Message { get; }
    public string TypeName { get; }

    public Error(string message)
    {
        Message = message;
        TypeName = this.GetType().Name;
    }
}


public class ValueObject<T>
{
    public readonly IEnumerable<Error> Errors;

    public bool IsValid { get => !(Errors is not null && Errors.Any()); }

    private readonly T _value;

    private ValueObject(T t) => (Errors, _value) = (Enumerable.Empty<Error>(), t ?? throw new ArgumentNullException(nameof(t)));

    private ValueObject(IEnumerable<Error> errors) => (Errors, _value) = (errors, default(T)!);

    public static ValueObject<T> Valid(T t) => new(t);
    public static ValueObject<T> Invalid(IEnumerable<Error> errors) => new(errors);
    public static ValueObject<T> Invalid(Error error) => Invalid(new[] { error });

    public static implicit operator ValueObject<T>(T t) => Valid(t);
    public static implicit operator ValueObject<T>(Error error) => Invalid(new[] { error });

    public Entity<T> ToEntity()
    {
        return IsValid ? Entity<T>.Valid(_value) : Entity<T>.Invalid(Errors);
    }

    public R Match<R>(Func<IEnumerable<Error>, R> Invalid, Func<T, R> Valid)
    {
        return IsValid ? Valid(_value!) : Invalid(Errors!);
    }
}


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
}