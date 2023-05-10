# Grenat.Functional.DDD
A set of C# containers for C# DDD-style programs :

````C#
Option<T>
ValueObject<T>
Entity<T>
````

The goals of this lightweight library are as follow:
- Dealing with asynchrony, concurrency and parallelism (in progress!) challenges with very few and clear code thanks to the functional programming principles. 
- Writing very thin application and infrastructure layers thus maximizing the proportion of code written in the domain layer.
- Chaining operations thus improving the reliability and lisibility of our DDD-style C# programs.

# Article series
Before using this library, I recommend you to read my series of articles [here](https://grenat.hashnode.dev/functional-ddd-with-c-part-1-the-benefits-of-functional-thinking) first.

# Read the samples!
If you need more examples than those below to use the library, have a look at the test project and the Samples directory.

# `Option<T>`
Use it to model the presence or absence of data, instead of using `null`.

## Usage

### Elevating a value to `Option<T>`

````C#
//define some value
Option<string> someString = Some("foo");
//define the absence of value, instead of using null
Option<string> none = None<string>();
````

### Leaving the world of `Option<T>` and getting its inner value
Use `Match` to get the inner value of an `Option<T>` container.

````C#
private string GetOptionValue<T>(Option<T> value)
{
    return value.Match(
            None: () => "Empty",
            Some: (value) => $"{value}");
}
````

### Mapping a function on `Option<T>`

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

## Construction
Make the object constructor **private** and create a public static constructor that returns a `ValueObject<T>`. 

**This is important to enjoy the benefits of this library and functional programming style.**

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

## Getting the inner value of `ValueObject<T>` with Match
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
			.Match(
                Valid: v => new { value = v.Value, errors = Enumerable.Empty<Error>()},
                Invalid: e => new { value = 0, errors = e});
					
Console.WriteLine(result.value); // 50
```

## Dealing with the invalid state
In case of an Error, `ValueObject<T>` switches to an invalid state and the Error is stored in an `IEnumerable<Error>`.

Here is an example on how to use it. 
```C#
var result = Quantity.Create(10, "Liters")
            .Bind((q) => q.Add(10))
            .Bind((q) => q.Add(100000)) //invalid
            .Bind((q) => q.Add(10))
            .Match(
                Valid: v => new { Value = v.Value, Errors = Enumerable.Empty<Error>()},
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

## Construction
Like `ValueObject<T>`, make the object constructor **private** and create a public static constructor that returns an `Entity<T>`. 

**This is important to enjoy the benefits of the library and functional programming style.**

```C#
public record Item 
{
	public Identifier Id { get; init; }
	public Identifier ProductId { get; init; }
	public Amount Amount { get; init; }
      
	private Item()
	{
	    Id = new Identifier();
		ProductId = new Identifier();
		Amount = new Amount();
	}

	public static Entity<Item> Create(string id, string productId, int amount)
	{
        /* ... */
	}
}
```

## The challenges of container composition and immutability

Value objects will be created and containerized in a `ValueObject<T>` container thanks to their static constructor (see above). And they will be themselves containerized in a parent `Entity<T>` container. But we face the problem of a container containing other containers, i.e. an `Entity<T>` containing some `ValueObject<T>` or some `Entity<T>`. This is not very convenient.

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

What's more, whenever we need to change an entity's property, we have to recreate a new instance of it **to preserve immutability**.

To solve these problems, this library contains some setters:
* `SetValueObject`
* `SetEntity`
* `SetEntityList`
* `SetEntityDictionary`
* `SetEntityOption`
* `SetValueObjectOption`


Behind the scenes, these setters :
1. Unwrap the inner value of the `Entity<T>` to modify. Then :
2. Unwrap the inner value of the container (an `Entity<V>`, a `ValueObject<V>`, an `Option<V>`, ...) that is passed as a parameter,
3. They modify the targeted property of `Entity<T>` thanks the setter function,
4. And they wrap the result in an new instance of `Entity<T>`. 

If one of the `Entity` or `ValueObject` parameter is invalid, then the resulting `Entity<T>` will become invalid too.
 Finally, `Error`s are harvested into this resulting entity.
    

### SetValueObject

Here is the function's signature:

```C#
public static Entity<T> SetValueObject<T, V>(this Entity<T> entity, ValueObject<V> valueObject, Func<T, V, T> setter) { /*...*/ }
```

It takes as arguments:

* The entity update.
* The value object to set into the entity.
* A setter function.
    

You can use it this way:

```C#
public static Entity<Cart> SetTotalAmount(this Entity<Cart> cart, ValueObject<Amount> totalAmount)
{
     return cart.SetValueObject(totalAmount, (cart, totalAmount) => cart with { TotalAmount = totalAmount });
}
```

### SetEntity

`SetEntity` is the same as `SetValueObject` but instead of accepting a `ValueObject<T>` as a parameter, it accepts an `Entity<T>`. Here is its signature:

```C#
public static Entity<T> SetEntity<T, E>(this Entity<T> parentEntity, Entity<E> entity, Func<T, E, T> setter) { /* ... */ }
```

### SetEntityCollection, SetEntityDictionary

As their name suggests, these setters do the same than the previous ones, but for immutable collections and immutable dictionaries.

Here are their signatures:

```C#
public static Entity<T> SetEntityList<T, E>(
    this Entity<T> parentEntity, 
    ImmutableList<Entity<E>> entities, Func<T, ImmutableList<E>, T> setter) { /* ... */ }

public static Entity<T> SetEntityDictionary<T, E, K>(
    this Entity<T> parentEntity, 
    ImmutableDictionary<K, Entity<E>> entities, 
    Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull { /* ... */ }
```

### SetEntityOption

Use this function to set a property of an `Entity<T>` that contains an `Option<V>`, i.e:

```C#
public record TestEntity
{
    public readonly int Value = 0;

    private TestEntity(int value)
    {
        Value = value;
    }

    public static Entity<TestEntity> Create(int value)
    {
        return Entity<TestEntity>.Valid(new TestEntity(value));
    }
}

public record ContainerEntity
{
    public Option<TestEntity> TestEntityOption { get; set; }

    /* ... */
}
```

The signature of this Setter is as follow:
```C#
public static Entity<T> SetEntityOption<T, V>(this Entity<T> parentEntity, Entity<V> entity, Func<V, bool> predicate, Func<T, Option<V>, T> setter)
```

If the predicate is not verified, then the value Option<V> will be `None`. Else it will contain `Some`.

Use it like this:
```C#
var testEntity = TestEntity.Create(0);

var containerEntity = ContainerEntity.Create();
containerEntity = containerEntity.SetEntityOption(entity, v => v.Value >= 1, static (e, v) => e with { TestEntityOption = v });
```

### SetValueObjectOption

This function has the same behaviour than `SetEntityOption` but for objects that are embedded in a `ValueObject<T>` container.

### Error harvesting

All these setters perform error harvesting. That is to say, if you try to set an invalid value object or an invalid entity, their errors are harvested and added to the ones already existing on the parent entity. It is very interesting for APIs: if the user types bad data, then all the errors will be returned.

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

An entity with 6 fields requires a constructor with 6 parameters which is a lot. Moreover, some parameters could not be known depending on the call context. This would require creating several constructors or creating optional parameters.

To avoid multiplying the constructors, we will take inspiration from the builder pattern. Here is what I want to write to create a cart item. 

```C#
var cartItem = new CartItemBuilder()
	.WithId("45xxsDg1=")
	.WithProductId("ne252TJqAWk3")
	.WithAmount(25)
	.Build();
```

How to do that? First, let's create the CartItem Entity :

```csharp
public record Item 
{
	public Identifier Id { get; init; }
	public Identifier ProductId { get; init; }
	public Amount Amount { get; init; }
      
	private Item()
	{
	    Id = new Identifier();
		ProductId = new Identifier();
		Amount = new Amount();
	}

	public static Entity<Item> Create(string id, string productId, int amount)
	{
		/* Add some verifications here if needed by returning an Error 
		 * which will be automatically converted in an invalid Entity.
		 * Eg: return new Error("Error !!")
		 */

	    return Entity<Item>.Valid(new Item())
			.SetId(id)
			.SetProductId(productId)
			.SetAmount(amount);
	}
}
```

Fore more clarity, let's create some setters:
```C#
public static class ItemSetters 
{
      public static Entity<Item> SetId(this Entity<Item> item, string id) 
      {
            return item.SetValueObject(Identifier.Create(id), (item, identifier) => item with { Id = identifier });
      }
      
      public static Entity<Item> SetProductId(this Entity<Item> item, string productId) 
      {
            return item.SetValueObject(Identifier.Create(productId), (item, productId) => item with { ProductId = productId });
      }
      
      public static Entity<Item> SetAmount(this Entity<Item> item, int amount) 
      {
            return item.SetValueObject(Amount.Create(amount), (item, amount) => item with { Amount = amount });
      }
}
```

Finally, let's create the builder:

```csharp
public record ItemBuilder
{
	private string _id { get; set; }
	private string _productId { get; set; }
	private int _amount { get; set; }
	
	public ItemBuilder WithId(string id) { _id = id; return this; }
	public ItemBuilder WithProductId(string productId) { _productId = productId; return this; }
	public ItemBuilder WithAmount(int amount) { _amount = amount; return this; }

	public Entity<Item> Build() => Item.Create(_id, _productId, _amount);
}
```

# The application layer

## Using `Entity<T>`, `Map()` and `Bind()`

You can write an application layer in a functional style like this:
```C#
public static OperationResultDto<ProductDto> AddProduct(
    AddProductDto addProductDto,
    Func<string, int> CountProductReferences
    Func<Product, Product> SaveProduct)
{
    var p = addProductDto.ToProductEntity()
        .Bind(VerifyProductReference, CountProductReferences(addProductDto.Reference))
        .Map(SaveProduct);

    return p.Match(
        Valid: (v) => new OperationResultDto<ProductDto>(v.ToProductDto()),
        Invalid: (e) => new OperationResultDto<ProductDto>(e)
    );
}
```

## Mapping an action on `Entity<T>`

Sometimes you may need to call an action on the inner value of `Entity<T>` in an application layer pipeline for example. In that case, `Map()` will apply the action on the value and  will return the original `Entity<T>` after that.

Let's say you need to send a message after an operation:
```C#
public static OperationResultDto<ProductDto> AddProduct(
    AddProductDto addProductDto,
    Func<Product, Product> SaveProduct
    Action<Product> ProductAddedMessage)
{
    var p = addProductDto.ToProductEntity()
        .Map(SaveProduct)
        .Map(ProductAddedMessage);

    return p.Match(
        Valid: (v) => new OperationResultDto<ProductDto>(v.ToProductDto()),
        Invalid: (e) => new OperationResultDto<ProductDto>(e)
    );
}
```

## Dealing with asynchrony

### `AsyncFunc<T>`

I added `AsyncFunc<T>` delegates to simplify the writing of `Func<Task<T>>`. As a result :

- `Func<Task<T>>` is equivalent to `AsyncFunc<T>`.
- `Func<T, Task<R>>` is equivalent to `AsyncFunc<T, R>`.

### `AsyncAction<T>`

Like `AsyncFunc<T>` an `AsyncAction<T>` delegate have been added.

### `BindAsync()`, `MapAsync()`

Grenat.Functional.DDD contains asynchronous versions of `Bind()` and `Map()` : `BindAsync()` and `MapAsync()`. Here is an asynchronous version of the previous code:

```C#
public static async Task<OperationResultDto<ProductDto>> AddProduct(
    AddProductDto addProductDto,
    AsyncFunc<string, int> CountProductReferences,
    AsyncFunc<Product, Product> SaveProduct)
{
    var p = await addProductDto.ToProductEntity()
        .BindAsync(VerifyProductReference, () => CountProductReferences(addProductDto.Reference))
        .MapAsync(SaveProduct);

    return p.Match(
        Valid: (v) => new OperationResultDto<ProductDto>(v.ToProductDto()),
        Invalid: (e) => new OperationResultDto<ProductDto>(e)
    );
}
```

## Calling from an ASP.Net Core controller

The code to call `AddProduct()` can look like this:

```C#
[HttpPost]
public async Task<ActionResult> AddProduct(AddProductDto addProductDto)
{
    var response = await CatalogOperations.AddProduct(
         addProductDto,
         _productRepository.CountExistingProductReferences,
         _productRepository.AddProduct);

    if (response.Success)
        return Ok(response.Data);
    else
        return UnprocessableEntity(response.Errors);
}
```

## More information about a functional application layer

I suggest you read this [post](https://grenat.hashnode.dev/functional-ddd-with-c-part-6-the-application-layer) on my blog.

