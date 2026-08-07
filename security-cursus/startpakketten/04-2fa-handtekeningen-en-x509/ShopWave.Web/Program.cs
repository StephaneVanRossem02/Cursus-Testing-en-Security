using ShopWave;
using ShopWave.Security;
using ShopWave.Web.Infrastructure;

namespace ShopWave.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();

            // Demo-infrastructuur. Deze concrete implementaties staan niet in de cursus;
            // ze bestaan alleen zodat de webshop de domeinklassen kan tonen.
            builder.Services.AddSingleton<DemoProductCatalog>();
            builder.Services.AddSingleton<IPaymentGateway, DemoPaymentGateway>();
            builder.Services.AddSingleton<IStockService, DemoStockService>();
            builder.Services.AddSingleton<IShippingService, DemoShippingService>();

            // Domeinklassen uit de cursus.
            builder.Services.AddSingleton<DiscountCalculator>();
            builder.Services.AddSingleton<OrderService>();
            builder.Services.AddSingleton<CheckoutService>();

            // Les 2: versleutelde opslag.
            builder.Services.AddSingleton<OrderEncryptor>();
            builder.Services.AddSingleton<OrderRepository>();

            // Les 4: 2FA, wachtwoordreset en digitale handtekeningen.
            // DemoCodeHolder vangt de gegenereerde codes op via de callback-techniek,
            // zodat de demo ze op het scherm kan tonen.
            builder.Services.AddSingleton<DemoCodeHolder>();
            builder.Services.AddSingleton<TwoFactorService>(sp =>
            {
                DemoCodeHolder holder = sp.GetRequiredService<DemoCodeHolder>();

                return new TwoFactorService(
                    onCodeGenerated: (mail, code) => holder.Store(mail, code));
            });
            builder.Services.AddSingleton<AccountRepository>();
            builder.Services.AddSingleton<PasswordResetService>();
            builder.Services.AddSingleton<OrderSigner>(sp =>
            {
                return new OrderSigner(
                    CertificateHelper.CreateSelfSignedCertificate("ShopWave"));
            });

            // Les 3: winkelmandje en coupons.
            builder.Services.AddSingleton<CouponService>();
            builder.Services.AddSingleton<ICouponService>(sp => sp.GetRequiredService<CouponService>());
            builder.Services.AddSingleton<CartService>();
            builder.Services.AddSingleton<DemoCartView>();

            WebApplication app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.MapRazorPages();

            app.Run();
        }
    }
}
