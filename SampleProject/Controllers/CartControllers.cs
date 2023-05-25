using Microsoft.AspNetCore.Mvc;
using SampleProject.Application;

namespace SampleProject.Controllers
{
    public static class GlobalCart
    {
        public static Option<Entity<Cart>> Value { get; set; } = None<Entity<Cart>>();
    }

    [ApiController]
    [Route("carts")]
    public class CartController : ControllerBase
    {
        /// <summary>
        /// Add a product to a cart. 
        /// When adding several times the same product Id, the cart total amount will raise up to a max amount of 1000 EUR.
        /// </summary>
        /// <param name="addProductToCartDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddProductToCart([FromBody] AddProductToCartDto addProductToCartDto)
        {
            var response = await CartOperations.AddProductToCart(addProductToCartDto,

                // Getting cart data from database function. Will be None one first call.
                GetCart: async (cartId) => await Task.FromResult(GlobalCart.Value),

                // Counting existing references from database function. Stubbed to 1. Try 0 to experiment error management.
                CountProductIds: async (productId) => await Task.FromResult(1),

                // Getting the product price from database function. Stubbed to 100.
                GetProductPrice: async (productId) => await Task.FromResult(Amount.Create(100, "EUR")),

                // Saving function.
                SaveCart: async (cart) => await Task.Run(() =>
                {
                    GlobalCart.Value = Some(Entity<Cart>.Valid(cart));
                    return cart;
                }));

            if (response.Success)
                return Ok(response.Data);
            else
                return UnprocessableEntity(response.Errors);
        }
    }
}
