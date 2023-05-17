# Grenat.Functional.DDD
A set of C# functional containers for DDD-style programs :

````C#
Option<T>
ValueObject<T>
Entity<T>
````

The goals of this lightweight library are:
- Dealing with asynchrony, concurrency and parallelism (in progress!) challenges with very few and clear code thanks to the functional programming principles. 
- Writing very thin application and infrastructure layers to maximize the proportion of code written in the domain layer.
- Writing very few conditional logic. A maximum of conditional logic will be handled thanks to functional thinking and operations like `Bind`, `Map`.
- Performing error harvesting with a very little effort.
- Chaining operations to improve the reliability and lisibility of DDD-style C# programs.

# Article series
Before using this library, I recommend you to read my article series [here](https://grenat.hashnode.dev/functional-ddd-with-c-part-1-the-benefits-of-functional-thinking) first.

# Read the samples!
If you need more examples than those below to use the library, have a look at the [test project](https://github.com/BastienFoucher/Grenat.Functional.DDD/tree/main/Grenat.Functional.DDD.Tests) and at the [samples repository](https://github.com/BastienFoucher/Articles.Examples).

# `Option<T>`
`Option<T>` is a container to model the presence or absence of data instead of using `null`.

## Usage

### Elevating a value to `Option<T>`
````C#
// Defining some value
Option<string> someString = Some("foo");

// Defining the absence of value, instead of using null
Option<string> none = None<string>();
````

### Leaving the world of `Option<T>` and getting its inner value
Use `Match` to get the inner value of an `Option<T>` container. You must provide it two functions:

- **One for the case where the container holds a value (Some)**. This value will be injected.
- **One for the case where the contaniner doesn't hold a value (None)**. 

**Important**: the returned value of provided functions must be of the same type.

````C#
private string GetOptionValue<T>(Option<T> value)
{
    return value.Match(
            None: () => "Empty", // Returns a string
            Some: (value) => $"{value}"); // Returns a string
}
````

### Mapping
`Map` function is the same operator than `Select` in LINQ. It applies a function `Func<T,R>` to the inner value of `Option<T>` to transform its content.


````C#
[TestMethod]
public void When_mapping_a_function_on_some_value_then_it_is_applied()
{
    var addOneFun = (int i) => i + 1;
    var sut = Some(0);

    var result = sut.Map(addOneFun)
                    .Match(
                            Some: v => v,
                            None: () => 0);

    // Is equal to 1 because addOneFun was applied on a some value.
    Assert.AreEqual(1, result);
}

[TestMethod]
public void When_mapping_a_function_on_a_none_value_then_it_is_not_applied()
{
    var addOneFun = (int i) => i + 1;
    var sut = None<int>();

    var result = sut.Map(addOneFun)
                    .Match(
                            Some: v => v,
                            None: () => 0);

    // Is equal to zero because addOneFun was applied on a none value.
    Assert.AreEqual(0, result);
}
````

### Binding
Same usage as `Map`. The difference with `Map` is that `Bind` takes an `Option<R>` returning function, i.e `Func<T,Option<R>>`. Its is the same operator than `SelectMany` in LINQ. 

````C#
[TestMethod]
public void When_binding_three_AddOne_functions_on_Some_zero_value_then_the_result_is_three()
{
    var addOneFun = (int i) => Some(i + 1); //returns an Option<int> instead of an int with Map.
    var sut = Some(0);

    var result = sut.Bind(addOneFun)
                    .Bind(addOneFun)
                    .Bind(addOneFun)
                    .Match(
                            Some: v => v,
                            None: () => 0);

    Assert.AreEqual(3, result);
}
````

# Errors
This library contains an `Error` data structure to avoid the use of exceptions for domain problems.

```C#
public record Error
{
    public string Message { get; }
    public string Code { get; }
    public string TypeName { get; }

    public Error(string message) : this(message, string.Empty) { }

    public Error(string message, string code)
    {
        Message = message;
        TypeName = GetType().Name;
        Code = code;
    }
}
```

# `ValueObject<T>`
`ValueObject<T>` is a container for DDD value objects.

## A two states container
`ValueObject<T>` is a two states container:

- **A valid state**: the inner value of `ValueObject<T>` value is available.
- **An invalid state**: the inner value of `ValueObject<T>` is unaccessible and only an `IEnumerable<Error>` causing the invalid state is available.

## Declaration and construction
### Principles
- Value Objects must be declared as `records` to make them immutable.
- Make the Value Object constructor **private** and create a public static constructor that returns a `ValueObject<T>`. 

This is important to enjoy the benefits of this library and functional programming style.

### Creation of a `ValueObject` in a valid state
Use the Valid function:
```C#
ValueObject<T>.Valid(/*your value of type T*/)
```

### Creation of a `ValueObject<T>` in an invalid state
Use the invalid function:
```C#
ValueObject<T>.Invalid(new Error(/*... your message and code .... */));
```

### Implicit conversions
Implicit conversions have been added to create a `ValueObject<T>`:
- In an invalid state from an `Error`
- In a valid state from an object.

### Example
```C#
// Declaire a Value Object as a record to enforce its immutability
public record Quantity {

    public const int MAX_QUANTITY = 10;
    public readonly int Value;

    // Make standard constructor private
    private Quantity(int quantity)
    {
        Value = quantity;
        Unit = unit;
    }

    // Use instead a static public constructor that will return a ValueObject<Quantity>
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
}
```

## Getting the inner value of `ValueObject<T>` with `Match`
Like `Option<T>`, you need to use the `Match` function. You must provide it two functions:

- **One for the valid state**. The inner value of the `ValueObject<T>` will be injected.
- **One for the invalid state**. An `IEnumerable<Error>` containing the errors will be injected.

**Important**: the returned value of provided functions must be of the same type.

```C#
var quantity = Quantity.Create(10);

var value = quantity.Match(
    Valid: v => v.Value, // Returns an int
    Invalid: e => 0); // Returns an int

Console.WriteLine(value); // 10
```

In case of an Error, `ValueObject<T>` switches to an invalid state and the Error is stored in an `IEnumerable<Error>`.

```C#
var result = Quantity.Create(10, "Liters")
    .Bind((q) => q.Add(10))
    .Bind((q) => q.Add(100000)) //invalid
    .Bind((q) => q.Add(10))
    .Match(
        Valid: v => new { Value = v.Value, Errors = Enumerable.Empty<Error>()},
        Invalid: e => new { Value = 0, Errors = e});
```

## Chaining functions with `Map` and `Bind`
Like `Option<T>`, `Map` and `Bind` functions have been creating for function chaining:

```C#
var result = Quantity.Create(10, "Liters")
    .Bind((q) => q.Add(10))
    .Bind((q) => q.Add(30))
    .Match(
        Valid: v => new { value = v.Value, errors = Enumerable.Empty<Error>()},
        Invalid: e => new { value = 0, errors = e});

Console.WriteLine(result.value); // 50
```

# `Entity<T>`
`Entity<T>` is a container for DDD entities. A DDD entity should consist of value objects, list of value objects, other entities or list of other entities.

It has a lot more of extension methods than value objects. You will use them for:
- Defining in a functional way the properties of an Entity (value objects, list of entities, ...).
- Chaining functions.
- Dealing with aynchronism and parallelism challenges.

## A two states container
Like `ValueObject<T>`, `Entity<T>` is a two states container:

- **A valid state**: the inner value of `Entity<T>` value is available..
- **An invalid state**: the inner value of `Entity<T>` is unaccessible and only an `IEnumerable<Error>` causing the invalid state is available.

## Declaration and construction
### Principles
Like `ValueObject<T>`:
- Entities must be declared as `records` to make them immutable.
- Make the Entity constructor **private** and create a public static constructor that returns an `Entity<T>`.

This is important to enjoy the benefits of the library and functional programming style.

### Creation of an `Entity<T>` in a valid state
Use the Valid function:
```C#
Entity<T>.Valid(/*your value of type T*/)
```

### Creation of an `Entity<T>` in an invalid state
```C#
Entity<T>.Invalid(new Error(/* ... your message and error code ... */));
```

### Implicit conversions
Implicit conversions have been added to create a `Entity<T>`:
- In an invalid state from an `Error`
- In a valid state from an object.

### Example
```C#
// Declare an Entity as a record to enforce its immutability
public record CartItem 
{
    public Identifier Id { get; init; }
    public Identifier ProductId { get; init; }
      
    // Make standard constructor private
    private CartItem()
    {
        Id = new Identifier();
        ProductId = new Identifier();
    }

    // Create a static public constructor that will return an Entity<Item>
    // See "Builder pattern" further in this file for a more complete example.
    public static Entity<CartItem> Create(string id, string productId)
    {
		if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(productId))
	        return new Error("Id and ProductId cannot be null or empty.");

	    return Entity<CartItem>.Valid(new CartItem());
    }
}
```

## The challenges of container composition and immutability
Value objects will be created and containerized in a `ValueObject<T>` container thanks to their static constructor (see above). But they may be containerized in a parent `Entity<T>`. We face the problem of a container containing other containers, i.e. an `Entity<T>` containing some `ValueObject<T>` or some `Entity<T>`. This is not very convenient.

We don't want to write this:

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

What's more, whenever we need to change an entity's property, we have to recreate a new instance of it **to preserve immutability**. To solve these problems, this library contains some setters.

For value objects:
* `SetValueObject`
* `SetValueObjectList`
* `SetValueObjectOption`

For entities:
* `SetEntity`
* `SetEntityList`
* `SetEntityDictionary`
* `SetEntityOption`

Behind the scenes, they:
1. Unwrap the inner value of the `Entity<T>` modify.
2. Unwrap the inner value of the container (an `Entity<V>`, a `ValueObject<V>`, an `Option<V>`, ...) that is passed as a parameter.
3. Modify a target property of the `Entity<T>` thanks the setter function.
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

### SetValueObjectList, SetEntityList, SetEntityDictionary
As their name suggests, these setters do the same than the previous ones, but for immutable collections and immutable dictionaries of ValueObjects or Entities.

Here are their signatures:

```C#
public static Entity<T> SetValueObjectList<T, V>(
    this Entity<T> parentEntity, 
    ImmutableList<ValueObject<V>> valueObjects, 
    Func<T, ImmutableList<V>, T> setter) { /* ... */ }

public static Entity<T> SetEntityList<T, E>(
    this Entity<T> parentEntity, 
    ImmutableList<Entity<E>> entities,
    Func<T, ImmutableList<E>, T> setter) { /* ... */ }

public static Entity<T> SetEntityDictionary<T, E, K>(
    this Entity<T> parentEntity, 
    ImmutableDictionary<K, Entity<E>> entities, 
    Func<T, ImmutableDictionary<K, E>, T> setter) where K : notnull { /* ... */ }
```

### SetEntityOption
Use these functions to set a property of an `Entity<T>` that contains an `Option<V>`, i.e:

```C#
// First entity
public record LittleEntity
{
    public readonly int Value = 0;

    private LittleEntity(int value)
    {
        Value = value;
    }

    public static Entity<LittleEntity> Create(int value)
    {
        return Entity<LittleEntity>.Valid(new LittleEntity(value));
    }
}

// An other entity that contains the previous entity in an option prpoerty
public record ContainerEntity
{
    public Option<LittleEntity> LittleEntityOption { get; set; }

    /* ... */
}
```

Here is the signature of these setters:
```C#
// 1st setter: the inner value of the entity will be injected in the predicate
public static Entity<T> SetEntityOption<T, V>(this Entity<T> parentEntity,
    Entity<V> entity, Func<V, bool> predicate, 
    Func<T, Option<V>, T> setter)

// 2nd setter: Entity<V> can be constructed and set in the Entity container only if the predicate is verified.
public static Entity<T> SetEntityOption<T, V>(this Entity<T> parentEntity,
    Func<Entity<V>> entity,
    Func<bool> predicate,
    Func<T, Option<V>, T> setter)
```

If predicates are not verified, then `Option<V>` will be `None`. Else it will contain `Some(V)`.

Use them like this:
```C#
var containerEntity = ContainerEntity.Create();

containerEntity = containerEntity.SetEntityOption(
    () => LittleEntity.Create(0), 
    () => false,
    static (containerEntity, v) => containerEntity with { TestEntityOption = v }); 

// 2nd setter
// containerEntity.LittleEntityOption will be equal to None.
containerEntity = containerEntity.SetEntityOption(
    LittleEntity.Create(0),
    v => v.Value >= 1,
     static (containerEntity, v) => containerEntity with { TestEntityOption = v }); 
```

### SetValueObjectOption
Same than `SetEntityOption` but for Value Objects that are embedded in a `ValueObject<T>` container.

```C#
public static Entity<T> SetValueObjectOption<T, V>(this Entity<T> parentEntity,
    Func<ValueObject<V>> valueObject,
    Func<bool> predicate,
    Func<T, Option<V>, T> setter)
```

Here is an example:
```C#
public record CartItem
{
    /* See other declarations above */
    public Option<Discount> DiscountValue { get; init; }

    /* ... */
}

public static Entity<CartItem> WithDiscountValue(this Entity<CartItem> cartItem, int discountValue)
{
    return cartItem.SetValueObjectOption(
        () => Discount.Create(discountValue),
        () => discountValue > 0,
        (cartItem, discount) => cartItem with { DiscountValue = discount });
}
```

## Error harvesting
All the previous setters perform error harvesting. That is to say, if you try to set an invalid value object or an invalid entity, their errors are harvested and added to the ones already existing on the parent entity. It is very interesting for APIs: if the user types bad data, then all the errors will be returned.

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
    public const int DEFAULT_VALUE = 0;

    public readonly int Value;

    public Amount() { Value = DEFAULT_VALUE; }

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

    public static implicit operator int(Amount amount) => amount.Value;
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

# The application layer

## Using `Entity<T>`, `Map` and `Bind`
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

## Mapping an `Action` on `Entity<T>`
Sometimes you may want to call an action on the inner value of `Entity<T>` in an application layer pipeline for example. In that case, `Map()` will apply the action on the value and  will return the original `Entity<T>` after that.

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
`AsyncFunc<T>` delegates to simplify the writing of `Func<Task<T>>`. As a result :

- `Func<Task<T>>` is equivalent to `AsyncFunc<T>`.
- `Func<T, Task<R>>` is equivalent to `AsyncFunc<T, R>`.

### `AsyncAction<T>`
Like `AsyncFunc<T>` an `AsyncAction<T>` delegate have been added.

### BindAsync, MapAsync
This library contains asynchronous versions of `Entity<T>.Bind()` and `Entity<T>.Map()`: `Entity<T>.BindAsync()` and `Entity<T>.MapAsync()`. Here is an asynchronous version of the previous code:

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

## Dealing with parallelism

### MapParallel
This function allow you to run an `IEnumerable` of tasks on the inner value of an `Entity<T>` in parallel. When all tasks are achieved, an aggregation function will be fired and the results will be injected in an `ImmmutableList` argument.


The function's signature is as follows:
```C#
public static async Task<Entity<R>> MapParallel<T, R>(this Entity<T> entity,
    IEnumerable<AsyncFunc<T, R>> funcs,
    Func<ImmutableList<R>, R> aggregationFunc) {}
```

You can use it this way:
```C#
var sut = Entity<int>.Valid(5);
var funcs = new List<AsyncFunc<int, int>>()
{
    async (p) => await Task.FromResult(p + 1),
    async (p) => await Task.FromResult(p + 2),
    async (p) => await Task.FromResult(p + 3)
};

var result = (await sut.MapParallel(funcs, (values) => values.Sum()))
    .Match(
        Valid: v => v,
        Invalid: e => 0);

Assert.IsTrue(result == 21);
```

### BindParallel
The usage of `BindParallel` is the same than `MapParallel`. The signature of the function is slightly different:
```C#
// BindParallel takes a collection of asynchronous functions that return Entity<R> (R only for MapParallel)
public static async Task<Entity<R>> BindParallel<T, R>(this Entity<T> entity,
    IEnumerable<AsyncFunc<T, Entity<R>>> funcs, 
    Func<ImmutableList<R>, R> aggregationFunc) {}
```

If one of the result of the computations is invalid then the errors will be harvested in the original entity.

You can use it this way:
```C#
var sut = Entity<int>.Valid(5);
var funcs = new List<AsyncFunc<int, Entity<int>>>()
{
    async (p) => await Task.FromResult(Entity<int>.Valid(p + 1)),
    async (p) => await Task.FromResult(Entity<int>.Invalid(new Error("Error 1"))),
    async (p) => await Task.FromResult(Entity<int>.Invalid(new Error("Error 2")))
};

var result = await sut.BindParallel(funcs, (values) => values.Sum());

Assert.IsTrue(result.Errors.Count() == 2); // Errors are harvested
```


## Calling the Application Layer from an ASP.Net Core controller

The code to call an application layer's `AddProduct()` function (see above) can look like this:

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


# Some patterns

## Creating "ubiquitous language" setters

For better readability, you can create some setters using extension methods. They will call `SetValueObject`, `SetEntity`, `SetEntityList`, `SetEntityDictionary` functions under the hood.

Here is an example:

```csharp
public static class CartSetters
{
    public static Entity<Cart> SetId(this Entity<Cart> cart, string id) 
    {
        return cart.SetValueObject(Identifier.Create(id), static (cart, identifier) => cart with { Id = identifier });
	}

    public static Entity<Cart> SetTotalAmountWithoutTax(this Entity<Cart> cart, int totalAmount) 
    {
        return cart.SetValueObject(Amount.Create(totalAmount), static (cart, totalAmount) => cart with { TotalAmountWithoutTax = totalAmount });
    }

    /* ... */
}
```

## The problem of constructors

An entity with 6 fields requires a constructor with 6 parameters which is a lot. Moreover, some parameters could not be known depending on the call context. This would require creating several constructors or creating optional parameters.

To avoid multiplying the constructors, we can take inspiration from the builder pattern:

```C#
var cartItem = new CartItemBuilder()
    .WithId("45xxsDg1=")
    .WithProductId("ne252TJqAWk3")
    .WithAmount(25)
    .Build();
```

How to do that? First, let's create the CartItem Entity :

```csharp
public record CartItem 
{
    public Identifier Id { get; init; }
    public Identifier ProductId { get; init; }
    public Amount Amount { get; init; }
      
    private CartItem()
	{
        Id = new Identifier();
        ProductId = new Identifier();
        Amount = new Amount();
	}

    public static Entity<CartItem> Create(string id, string productId, int amount)
    {
       	if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(productId))
	        return new Error("Id and ProductId cannot be null or empty.");

        // Creating an empty Entity<CartItem>, then using setters (see below) 
        // to set the value object properties 
        return Entity<CartItem>.Valid(new CartItem())
            .SetId(id)
            .SetProductId(productId)
            .SetAmount(amount);
    }
}
```

Fore more clarity, let's create some setters:
```C#
public static class CartItemSetters 
{
    public static Entity<CartItem> SetId(this Entity<CartItem> cartItem, string id) 
    {
        return cartItem.SetValueObject(Identifier.Create(id), (cartItem, identifier) => cartItem with { Id = identifier });
    }
      
    public static Entity<CartItem> SetProductId(this Entity<CartItem> cartItem, string productId) 
    {
        return cartItem.SetValueObject(Identifier.Create(productId), (cartItem, productId) => cartItem with { ProductId = productId });
    }
      
    public static Entity<CartItem> SetAmount(this Entity<CartItem> cartItem, int amount) 
    {
        return cartItem.SetValueObject(Amount.Create(amount), (cartItem, amount) => cartItem with { Amount = amount });
    }
}
```

Finally, let's create the builder:
```csharp
public record CartItemBuilder
{
    private string _id { get; set; }
    private string _productId { get; set; }
    private int _amount { get; set; }
	
    public CartItemBuilder WithId(string id) { _id = id; return this; }
    public CartItemBuilder WithProductId(string productId) { _productId = productId; return this; }
    public CartItemBuilder WithAmount(int amount) { _amount = amount; return this; }

    public Entity<CartItem> Build() => Item.Create(_id, _productId, _amount);
}
```