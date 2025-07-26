using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Bookie.Models.ViewModels;
using Bookie.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookieWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> companiesList = _unitOfWork.Company.GetAll().ToList();
            return View(companiesList); 
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                Company Company = _unitOfWork.Company.Get(u => u.Id == id);
                return View(Company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {


                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);

                }
                else
                {
                    _unitOfWork.Company.update(company);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company created succssfully!";
                return RedirectToAction("Index");
            }
            
            return View(company);
        }

        #region API CALLS
        public IActionResult GetAll()
        {
            var CompanysList = _unitOfWork.Company.GetAll().ToList();


            return Json(new { data = CompanysList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                var CompanyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
                if (CompanyToBeDeleted == null)
                {

                    return Json(new { success = false, message = $"Error while Deletinig! {Response.Body}" });
                }

                _unitOfWork.Company.Remove(CompanyToBeDeleted);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Deleted successfully!" });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = $"Error while deleteing {Response.Body} {ex.Message}" });
                //return StatusCode(500, new { success = false, message = $"Error while deleting: {ex.Message}" });

            }

        }
        #endregion
    }
}
