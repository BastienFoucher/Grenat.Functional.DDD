namespace SampleProject.Domain.Services
{
    public static class CartItemServices
    {
        public static Entity<CartItem> VerifyProduct(int productIdsCount, CartItem cartItem)
        {
            if (productIdsCount == 0)
                return new IdentifierError($"The product {cartItem.ProductId.Value} does not exist.");
            else
                return cartItem;
        }

        public static Entity<CartItem> SetProductPrice(ValueObject<Amount> price, CartItem cartItem)
        {
            return cartItem.Set(price, (item, price) => item with { Amount = price });
        }
    }
}
