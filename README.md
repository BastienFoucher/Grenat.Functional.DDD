# Grenat.Functional.DDD
A set of C# monads for C# DDD-style programs :

````C#
Option<T>
ValueObject<T>
Entity<T> // <-- doc is in progress!
````

They are very useful for chaining operations thus improving the reliability and lisibility of our DDD-style C# programs.

# Article series
Before using this library, I recommend you to read my series of articles [here](https://grenat.hashnode.dev/functional-ddd-with-c-part-1-the-benefits-of-functional-thinking) first.

# Read the test project!
If you need more examples than those below to use the library, have a look at the test project: `Grenat.Functional.DDD.Tests`.

# Option<T>
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

### Binding a function an Option<T>

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

# ValueObject<T>
ValueObject<T> is a container for ValueObjects.

## A two states container
ValueObject<T> is a two states container:

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

### Getting the inner value of ValueObject<T> with Match
Like Option<T>, you need to use the `Match` function. Because a `ValueObject<T>` has two states, one valid and one invalid, use `Match` to retrieve the inner value by providing it two functions: 

- **One for the valid state**. The inner value of the ValueObject<T> will be injected into it.

- **One for the invalid state**. An IEnumerable<Error> containing the errors will be injected into it. 

**Note that the returned value of provided functions for Valid and Invalid cases must be of the same type.**

```C#
var quantity = Quantity.Create(10);
var value = quantity.Match(
				Valid: v => v.Value,
				Invalid: e => 0);
Console.WriteLine(value); // 10
```

## Chaining functions with Bind
Like Option<T>, a `Bind` function has been defined to chain functions :

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


