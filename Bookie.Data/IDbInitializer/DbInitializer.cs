//using Bookie.DataAccess.Data;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Bookie.Utilities;

//using Bookie.Models;
//namespace Bookie.DataAccess.DbIntializer
//{
//    public class DbIntializer : IDbIntializer
//    {
//        private readonly UserManager<IdentityUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;
//        private readonly ApplicationDbContext _db;

//        public DbIntializer(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
//        {
//            _userManager = userManager;
//            _roleManager = roleManager;
//            _db = db;
//        }


//        public async Task intializer()
//        {
//            try
//            {
//                // Apply pending migrations
//                if ((await _db.Database.GetPendingMigrationsAsync()).Any())
//                {
//                    await _db.Database.MigrateAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                // Log the error
//                Console.WriteLine($"Migration failed: {ex.Message}");
//                throw; // Re-throw if you want startup to fail
//            }

//            // Create roles if they don't exist
//            if (!await _roleManager.RoleExistsAsync(SD.Role_Customer))
//            {
//                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
//                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
//                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
//                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));

//                // Create admin user if no roles exist
//                var adminUser = new ApplicationUser
//                {
//                    UserName = "youssefhikal@gmail.com",
//                    Email = "youssefhikal@gmail.com",
//                    PhoneNumber = "760086267",
//                    StreetAddress = "",
//                    EmailConfirmed = true // Important for immediate access
//                };

//                var result = await _userManager.CreateAsync(adminUser, "Test@123");

//                await _userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
//            }

//        }


//    }
//}

using Bookie.DataAccess.Data;
using Bookie.DataAccess.DbIntializer;
using Bookie.Models;
using Bookie.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class DbInitializer : IDbInitializer
{

    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public DbInitializer(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
    }


    public void Initialize()
    {


        //migrations if they are not applied
        try
        {
            if (_db.Database.GetPendingMigrations().Count() > 0)
            {
                _db.Database.Migrate();
            }
        }
        catch (Exception ex) { }



        //create roles if they are not created
        if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();


            //if roles are not created, then we will create admin user as well
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "youssefhikal@gmail.com",
                Email = "youssefhikal@gmail.com",
                Name = "Youssef Hikal",
                PhoneNumber = "1112223333",
                StreetAddress = "test 123 Ave",
                State = "IL",
                PostalCode = "23422",
                City = "Chicago"
            }, "Admin123*").GetAwaiter().GetResult();


            ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "youssefhikal@gmail.com");
            _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

        }

        return;
    }
}