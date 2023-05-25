namespace SampleProject.Application;

public static class CartOperations
{
    public static async Task<OperationResultDto<CartDto>> AddProductToCart(AddProductToCartDto addProductToCartDto,
        AsyncFunc<string, Option<Entity<Cart>>> GetCart,
        AsyncFunc<string, int> CountProductIds,
        AsyncFunc<string, ValueObject<Amount>> GetProductPrice,
        AsyncFunc<Cart, Cart> SaveCart)
    {
        var cart = await addProductToCartDto.ToCartItemEntity()
            .BindAsync(VerifyProduct, () => CountProductIds(addProductToCartDto.ProductId))
            .BindAsync(SetProductPrice, () => GetProductPrice(addProductToCartDto.ProductId))
            .BindAsync(AddItemToCart, () => GetCart(addProductToCartDto.CartId))
            .MapAsync(SaveCart);

        return cart.Match(
            Valid: (v) => new OperationResultDto<CartDto>(v.ToCartDto()),
            Invalid: (e) => new OperationResultDto<CartDto>(e)
        );
    }
}
