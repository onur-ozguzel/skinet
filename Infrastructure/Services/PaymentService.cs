using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unit;

    public PaymentService(IConfiguration config, ICartService cartService, IUnitOfWork unit)
    {
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];
        _cartService = cartService;
        _unit = unit;
    }

    public async Task<string> RefundPayment(string paymentIntentId)
    {
        var refundOptions = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId
        };

        var refundService = new RefundService();
        var result = await refundService.CreateAsync(refundOptions);

        return result.Status;
    }

    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        var cart = await _cartService.GetCartAsync(cartId)
            ?? throw new Exception("Cart unavailable");

        var shippingPrice = await GetShippingPriceAsync(cart) ?? 0;

        await ValidateCartItemsInCartAsync(cart);

        var subtotal = CalculateSubtotal(cart);

        if (cart.Coupon != null)
        {
            subtotal = await ApplyDiscountAsync(cart, subtotal);
        }

        var total = subtotal + shippingPrice;

        await CreateUpdatePaymentIntentAsync(cart, total);
        await _cartService.SetCartAsync(cart);
        return cart;
    }

    private static async Task CreateUpdatePaymentIntentAsync(ShoppingCart cart, long amount)
    {
        var service = new PaymentIntentService();

        if (string.IsNullOrEmpty(cart.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "eur",
                PaymentMethodTypes = ["card"]
            };
            var intent = await service.CreateAsync(options);
            cart.PaymentIntentId = intent.Id;
            cart.ClientSecret = intent.ClientSecret;
        }
        else
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = amount,
            };

            await service.UpdateAsync(cart.PaymentIntentId, options);
        }
    }

    private static async Task<long> ApplyDiscountAsync(ShoppingCart cart, long amount)
    {
        var service = new Stripe.CouponService();
        var coupon = await service.GetAsync(cart.Coupon!.CouponId);
        if (!coupon.PercentOff.HasValue) return amount;

        var discount = amount * (coupon.PercentOff.Value / 100);
        amount -= (long)discount;

        return amount;
    }

    private static long CalculateSubtotal(ShoppingCart cart)
    {
        return (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100));
    }

    private async Task ValidateCartItemsInCartAsync(ShoppingCart cart)
    {
        foreach (var item in cart.Items)
        {
            var productItem = await _unit.Repository<Core.Entities.Product>().GetByIdAsync(item.ProductId);
            if (productItem == null) throw new Exception("Product unavailable");

            if (item.Price != productItem.Price)
            {
                item.Price = productItem.Price;
            }
        }
    }

    private async Task<long?> GetShippingPriceAsync(ShoppingCart cart)
    {
        if (!cart.DeliveryMethodId.HasValue) return null;

        var deliveryMethod = await _unit.Repository<DeliveryMethod>().GetByIdAsync(cart.DeliveryMethodId.Value);

        if (deliveryMethod == null) return null;
        return (long)deliveryMethod.Price * 100;
    }
}
