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

            // Les 2: accounts en versleutelde opslag.
            builder.Services.AddSingleton<AccountRepository>();
            builder.Services.AddSingleton<OrderEncryptor>();
            builder.Services.AddSingleton<OrderRepository>();

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
