using Bookie.DataAccess.Data;
using Bookie.Utilities;
using Bookie.DataAccess.Repository;
using Bookie.DataAccess.Repository.IRepository;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Bookie.DataAccess.DbIntializer;

namespace BookieWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<ApplicationDbContext>(options=>options
            .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //to inject the secrets in app.settings.json into the genric class
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            
            //To like the role with the users
            builder.Services.AddIdentity<IdentityUser,IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            //when you have issue with the routings due to areas
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/login";
                options.LogoutPath = $"/Identity/Account/logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                
            });

            //Adding facebook 
            builder.Services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "2809349919266513";
                options.AppSecret = "96dbd0a24a143975c3f58df58ebf6140";

            });

            //Adding session and cookie
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(100);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            }
            );


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();
            app.UseSession();

            //inject dbintalizer 
            SeedDatabase();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.Run();

            void SeedDatabase()
            {
                using(var scope = app.Services.CreateScope())
                {
                  var dbIntalizer =  scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                    dbIntalizer.Initialize();
                }
            }
        }
    }
}
