namespace SampleProject.Application;

public static class CartOperations
{
    public static async Task<OperationResultDto<CartDto>> AddProductToCart(AddProductToCartDto addProductToCartDto,
        AsyncFunc<string, Option<Entity<Cart>>> GetCart,
        AsyncFunc<string, int> CountProductIds,
        AsyncFunc<string, ValueObject<Amount>> GetProductPrice,
        AsyncFunc<Cart, Cart> SaveCart)
    {
        var verifyProduct = VerifyProduct;
        var setProductPrice = SetProductPrice;
        var addItemToCart = AddItemToCart;
       
        var cart = await addProductToCartDto.ToCartItemEntity()
            .BindAsync(verifyProduct.Apply(() => CountProductIds(addProductToCartDto.ProductId)))
            .BindAsync(setProductPrice.Apply(() => GetProductPrice(addProductToCartDto.ProductId)))
            .BindAsync(addItemToCart.Apply(() => GetCart(addProductToCartDto.CartId)))
            .MapAsync(SaveCart);

        return cart.Match(
            Valid: (v) => new OperationResultDto<CartDto>(v.ToCartDto()),
            Invalid: (e) => new OperationResultDto<CartDto>(e)
        );
    }
}
