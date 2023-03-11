# Grenat.Functional.DDD
A set of C# monads for C# DDD-style programs :

````C#
Option<T>
ValueObject<T>
Entity<T>
````

They are very useful for chaining operations thus improving the reliability and lisibility of our DDD-style C# programs.

# Article series
Before using this library, I recommend you to read my series of articles [here](https://grenat.hashnode.dev/functional-ddd-with-c-part-1-the-benefits-of-functional-thinking) first.

# Read the test project!
If you need more examples than those below to use the library, have a look at the test project: `Grenat.Functional.DDD.Tests`.

# `Option<T>`
Use it to model the presence or absence of data, instead of using `null`.

## Usage

### Elevating a value to Option<T>

````C#
//define some value
Option<string> someString = Some("foo");
//define the absence of value, instead of using null
Option<string> none = None<string>();
````

### Leaving Option<T> and get the inner value
Use `Match` to get the inner value of an Option<T> container.

````C#
private string GetOptionValue<T>(Option<T> value)
{
    return value.Match(
            None: () => "Empty",
            Some: (value) => $"{value}");
}
````

### Mapping a function on Option<T>

`Map` function is the same operator than `Select` in LINQ. It applies a function `Func<T,R>` to the inner value of `Option<T>` to transform its content.


````C#
[TestMethod]
public void When_mapping_a_function_on_some_value_then_it_is_fired()
{
    var addOne = (int i) => i + 1;
    var sut = Some(0);

    var result = sut.Map(addOne)
                    .Match(
                            Some: v => v,
                            None: () => 0);

    Assert.AreEqual(1, result);
}

[TestMethod]
public void When_mapping_a_function_on_none_value_then_it_is_not_fired()
{
    var addOne = (int i) => i + 1;
    var sut = None<int>();

    var result = sut.Map(addOne)
                    .Match(
                            Some: v => v,
                            None: () => 0);

    Assert.AreEqual(0, result);
}
````

### Binding a function an `Option<T>`

Same usage as `Map`. The difference with `Map` is that `Bind` takes an `Option<R>` returning function, i.e `Func<T,Option<R>>`. Its is the same operator than `SelectMany` in LINQ. 

````C#
[TestMethod]
public void When_binding_three_AddOne_functions_on_Some_zero_value_then_the_result_is_three()
{
    var addOne = (int i) => Some(i + 1); //returns an Option<int> instead of an int with Map.
    var sut = Some(0);

    var result = sut.Bind(addOne)
                    .Bind(addOne)
                    .Bind(addOne)
                    .Match(
                            Some: v => v,
                            None: () => 0);

    Assert.AreEqual(3, result);
}
````

# `ValueObject<T>`
`ValueObject<T>` is a container for value objects.

## A two states container
`ValueObject<T>` is a two states container:

- **A valid state**: the inner value of `ValueObject<T>` value is accessible.

- **An invalid state**: the inner value of `ValueObject<T>` is unaccessible and only the error causing this invalid state is available.


To benefit of it, you will need to create an static constructor for the inner class :

```C#
// Make standard constructor private
private Quantity(int quantity)
{
    Value = quantity;
	Unit = unit;
}

// Use instead a public constructor
public static ValueObject<Quantity> Create(int quantity)
{
    if (quantity > MAX_QUANTITY)
    {
        // The Error class is embeded in Grenat.Functionnal.DDD.
        // An implicit conversion is also defined in the library to convert an Error to an invalid ValueObject<T>.
        return new Error($"Quantity cannot be more than {MAX_QUANTITY}");
    }
    else 
    {
        // An implicit conversion is defined in the library ton convert a T to a valid ValueObject<T>.
        return new Quantity(quantity);
    }
}
```

### Getting the inner value of `ValueObject<T>` with Match
Like `Option<T>`, you need to use the `Match` function. Because a `ValueObject<T>` has two states, one valid and one invalid, use `Match` to retrieve the inner value by providing it two functions: 

- **One for the valid state**. The inner value of the `ValueObject<T>` will be injected into it.

- **One for the invalid state**. An `IEnumerable<Error>` containing the errors will be injected into it. 

**Note that the returned value of provided functions for Valid and Invalid cases must be of the same type.**

```C#
var quantity = Quantity.Create(10);
var value = quantity.Match(
				Valid: v => v.Value,
				Invalid: e => 0);
Console.WriteLine(value); // 10
```

## Chaining functions with Bind
Like `Option<T>`, a `Bind` function has been defined to chain functions :

```C#
var result = Quantity.Create(10, "Liters")
			.Bind((q) => q.Add(10))
			.Bind((q) => q.Add(30))
			.Match(Valid: v => new { value = v.Value, errors = Enumerable.Empty<Error>()},
					Invalid: e => new { value = 0, errors = e});
					
Console.WriteLine(result.value); // 50
```

## Dealing with the invalid state
In case of an Error, `ValueObject<T>` switches to an invalid state and the Error is stored in an `IEnumerable<Error>`.

Here is an example on how to use it. 
```C#
var result = Quantity.Create(10, "Liters")
            .Bind((q) => q.Add(10))
            .Bind((q) => q.Add(100))
            .Bind((q) => q.Add(10))
            .Match(Valid: v => new { Value = v.Value, Errors = Enumerable.Empty<Error>()},
                    Invalid: e => new { Value = 0, Errors = e});

// Back to the normal world : handle the result
if (result.Errors.Count() > 0)
    /* ... */
else {
    /* ... */
}
```

# `Entity<T>`
`Entity<T>` is a container for value objects, list of value objects, other entities or list of other entities.

## A two states container
`Entity<T>` is a two states container:

- **A valid state**: the inner value of `Entity<T>` value is accessible.

- **An invalid state**: the inner value of `Entity<T>` is unaccessible and only the errors causing this invalid state is available.

## The problem of container composition

Value objects should be containerized in a `ValueObject<T>` container. They also should be containerized in an `Entity<T>` container. But we face the problem of a container containing other containers, i.e. an `Entity<T>` containing some `ValueObject<T>` or some `Entity<T>`. This is not very convenient when we write code using the data of these containers.

For example, because the following `Cart` object should be constructed as an `Entity<Cart>` (see further), we don't want to write this:

```C#
public record Cart
{
    public ValueObject<Identifier> Id { get; init; }
    public ImmutableList<Entity<Item>> Items { get; init; }
    public ValueObject<Amount> TotalAmount { get; init; }

	/*...*/
}
```

We want to write this:

```C#
public record Cart
{
    public Identifier Id { get; init; }
    public ImmutableList<Item> Items { get; init; }
    public Amount TotalAmount { get; init; }
	
	/*...*/
}
```

To solve this problem, I added two functions in my library Grenat.Functional.DDD:

* `SetValueObject`
* `SetEntity`
    

### SetValueObject

Here is the function's signature :

```C#
public static Entity<T> SetValueObject<T, V>(this Entity<T> entity, ValueObject<V> valueObject, Func<T, V, T> setter) { /*...*/ }
```

It takes as arguments:

* The entity
* The value object to set into the entity.
* A setter function.
    

You can use it this way:

```C#
public static Entity<Cart> SetTotalAmount(this Entity<Cart> cart, ValueObject<Amount> totalAmount)
{
     return cart.SetValueObject(totalAmount, (cart, totalAmount) => cart with { TotalAmount = totalAmount });
}
```

Behind the scenes, `SetValueObject` unwraps the inner value object, unwraps the entity, sets the value object in the entity with the setter function, and wraps the result in an `Entity<T>`. Of course, if the `ValueObject<T>` parameter is invalid, then the resulting `Entity<T>` will become invalid too.

### SetEntity

`SetEntity` is the same as `SetValueObject` but instead of accepting a `ValueObject<T>` as a parameter, it accepts an `Entity<T>`. Here is its signature:

```C#
public static Entity<T> SetEntity<T, E>(this Entity<T> parentEntity, Entity<E> entity, Func<T, E, T> setter) { /* ... */ }
```

### Error harvesting

Both `SetValueObject` and `SetEntity` functions perform error harvesting. That is to say, if you try to set an invalid value object or an invalid entity, their errors are harvested and added to the ones already existing on the parent entity. It is very interesting for APIs: if the user types bad data, then all the errors will be returned.

Here is an `Identifier` value object:

```C#
public record Identifier
{
    public const string DEFAULT_VALUE = "";
    public readonly string Value = "";

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
```

Here is an `Amount` value object:

```C#
public record Amount
{
    public const int MAX_AMOUNT = 2500;
    public readonly int Value;

    private Amount(int value)
    {
        Value = value;
    }

    public static ValueObject<Amount> Create(int amount)
    {
        if (amount < 0)
            return new Error("An amount cannot be negative.");
        if (amount > MAX_AMOUNT)
            return new Error(String.Format($"An amount cannot be more than {MAX_AMOUNT}"));
				
        return new Amount(amount);
    }
}
```

Let's create a cart by setting invalid data and let's see the errors harvesting in action. `Cart.Create` is a static constructor returning an empty `Entity<Cart>`:

```C#
var cart = Cart.Create()
			.SetValueObject(Identifier.Create(null), (cart, identifier) => cart with { Id = identifier })
			.SetValueObject(Amount.Create(-1), (cart, totalAmount) => cart with { TotalAmount = totalAmount })
			.SetValueObject(Amount.Create(3000), (cart, totalAmountWithTax) => cart with { TotalAmountWithTax = totalAmountWithTax });
			
Console.WriteLine(cart.Errors);
// Output :
// An identifier cannot be null or empty.
// An amount cannot be negative.
// An amount cannot be more than 2500.
```

## The problem of constructors

Implementing immutability means using constructors a lot: for instance, if you need to modify just one field of an entity, you need to recreate a copy of the entity by calling a constructor. This is not practical: an entity with 6 fields requires a constructor with 6 parameters which is a lot. Moreover, some parameters could not be known depending on the call context. This would require creating several constructors or creating optional parameters.

To avoid multiplying the constructors, we will take inspiration from the builder pattern. A working implementation might look like this:

1. Create a static constructor returning an empty Entity.
    
2. Create setters returning a new instance of the Entity, using the `SetValueObject` or `SetEntity` functions we have just seen.
    

Let's first create an empty `Entity<Cart>`. In the `CreateEntity` function below, Grenat.Functional.DDD will automatically wrap the `Cart` object in a valid `Entity<Cart>` thanks to an implicit operator.

```C#
public record Cart
{
    public Identifier Id { get; init; }
    public ImmutableList<Item> Items { get; init; }
    public Amount TotalAmount { get; init; }

    private Cart()
    {
        Id = new Identifier();
        Items = ImmutableList.Create<Item>();
        TotalAmount = new Amount();
    }

    public static Entity<Cart> CreateEmpty()
    {
        return new Cart();
    }
}
```

Now, we just have to write construction methods thanks to the `SetValueObject` function we saw before:
```C#
public static class CartBuilder
{
	public static Entity<Cart> WithId(this Entity<Cart> cart, string id) 
	{
		return cart.SetValueObject(Identifier.Create(id), (cart, identifier) => cart with { Id = identifier });
	}
	/* ... */
	public static Entity<Cart> WithItem(this Entity<Cart> cart, Entity<Item> item) 
	{
		return cart.SetEntity(item, (cart, item) => cart with { Items = cart.Items.Add(item) });
	}
}
```

Now we can write the following:
```C#
var cart = Cart.CreateEmpty()
	         .WithId("1ds3d!bM")
	         .WithItem(Item.CreateEmpty()
			 				.WithId("45xxsDg1=")
							.WithProductId("ne252TJqAWk3")
							.WithAmount(25));
```