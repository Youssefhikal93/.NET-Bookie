using Bookie.DataAccess.Data;
using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Bookie.Models.ViewModels;
using Bookie.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookieWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(); 
        }

        public IActionResult RoleMangement(string userId)
        {
            string RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

            RoleMangementVM roleVM = new()
            {
                ApplicationUser = _db.ApplicationUsers.Include(u => u.Company)
                .FirstOrDefault(u => u.Id == userId),
                RoleList = _db.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _db.Compaines.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            roleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;
           
            return View(roleVM);
        }

        [HttpPost]
        public IActionResult RoleMangement(RoleMangementVM roleVM)
        {
            string RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleVM.ApplicationUser.Id).RoleId;
            string oldeRole = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;
           
            if(roleVM.ApplicationUser.Role != oldeRole)
            {
                // a role is differnet
                var applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleVM.ApplicationUser.Id);
               if(roleVM.ApplicationUser.Role == SD.Role_Company)
                applicationUser.CompanyId = roleVM.ApplicationUser.CompanyId;

                if (oldeRole == SD.Role_Company)
                    applicationUser.CompanyId = null;

            _db.SaveChanges();
            _userManager.RemoveFromRoleAsync(applicationUser, oldeRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }

            TempData["success"] = "User updated succssefully!";
            return RedirectToAction(nameof(Index));
        }


        #region API CALLS
        public IActionResult GetAll()
        {
            var usersList = _db.ApplicationUsers.Include(u=>u.Company).ToList();
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach(var user in usersList)
            {

                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
               user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                if (user.Company == null)
                {
                    user.Company = new Company { Name = "N/A" };
                }
            }
            return Json(new { data = usersList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {

            var userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

            if(userFromDb == null)
            {
                return Json(new { success = false, message = "Error while Lokcing" });

            }

            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
            {
                // user is currently locked
                userFromDb.LockoutEnd = DateTime.Now;
            }else
            {
                userFromDb.LockoutEnd = DateTime.Now.AddYears(100);

            }
            _db.SaveChanges();
            return Json(new { success = true, message = "User locked/unlocked succssedfully" });
        }

        #endregion
    }
}
