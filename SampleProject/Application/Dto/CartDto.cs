namespace SampleProject.Application.Dto;

public record CartDto(string CartId, List<CartItemDto> Items, int TotalAmount);

public static class CartDtoExtensions
{
    public static CartDto ToCartDto(this Cart cart)
    {
        return new CartDto(
            cart.Id.Value,
            ToCartItemsDto(cart.Items),
            cart.TotalAmount);

    }

    private static List<CartItemDto> ToCartItemsDto(ImmutableDictionary<string, CartItem> cartItems)
    {
        return cartItems
            .Select((item, i) => new CartItemDto(
                item.Value.Id.Value,
                item.Value.ProductId.Value,
                item.Value.Amount.Value)).ToList();
    }
}

public record AddProductToCartDto(string CartId, string ProductId);

public static class AddProductToCartDtoExtensions
{
    public static Entity<CartItem> ToCartItemEntity(this AddProductToCartDto addProductToCart)
    {
        return VerifyDto(addProductToCart, () => new CartItemBuilder()
            .WithId(Guid.NewGuid().ToString())
            .WithCartId(addProductToCart.CartId)
            .WithProductId(addProductToCart.ProductId)
            .Build());
    }
}



