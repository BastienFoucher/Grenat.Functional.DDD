namespace SampleProject.Domain.Services;

public static class CartServices
{
    public static Entity<Cart> AddItemToCart(CartItem cartItem, Option<Entity<Cart>> cart)
    {
        return cart.Match(
            None: () => new CartBuilder()
                .WithId(Guid.NewGuid().ToString())
                .Build(),
            Some: c => c)
        .Bind(CreateOrUpdateCartItem, cartItem)
        .Bind(CalculateTotalPrice);
    }

    private static Entity<Cart> CreateOrUpdateCartItem(Cart cart, CartItem cartItem)
    {
        return cart.Items.GetValue(cartItem.ProductId.Value).Match(
            Some: v => UpdateItem(cart, v, cartItem),
            None: () => NewCartItem(cart, cartItem));
    }

    private static Entity<Cart> UpdateItem(Cart cart, CartItem newCartItem, CartItem existingCartItem)
    {
        var totalItemAmount = Amount.Create(newCartItem.Amount.Value + existingCartItem.Amount.Value, "EUR");

        return Entity<CartItem>.Valid(existingCartItem)
            .Set(totalItemAmount, (item, totalItemAmount) => item with { Amount = totalItemAmount })
            .Match(
                Invalid: e => e,
                Valid: i => Entity<Cart>.Valid(cart with { Items = cart.Items.SetItem(i.ProductId.Value, i) }));
    }

    private static Entity<Cart> NewCartItem(Cart cart, CartItem cartItem)
    {
        return cart with { Items = cart.Items.Add(cartItem.ProductId.Value, cartItem) };
    }

    private static Entity<Cart> CalculateTotalPrice(Cart cart)
    {
        var totalAmount = Amount.Create(cart.Items.Sum(cartItem => cartItem.Value.Amount.Value), "EUR");
        return cart.Set(totalAmount, (cart, totalAmount) => cart with { TotalAmount = totalAmount });
    }
}
