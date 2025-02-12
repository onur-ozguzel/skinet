using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services;

public class CouponService(IConfiguration config) : ICouponService
{
    public async Task<AppCoupon?> GetCouponFromPromoCode(string code)
    {
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];

        var options = new PromotionCodeListOptions { Code = code };
        var service = new PromotionCodeService();
        var result = (await service.ListAsync(options)).FirstOrDefault();

        if (result == null) return null;

        return new AppCoupon
        {
            AmountOff = result.Coupon.AmountOff,
            PercentOff = result.Coupon.PercentOff,
            CouponId = result.Coupon.Id,
            Name = result.Coupon.Name,
            PromotionCode = result.Code
        };
    }
}
