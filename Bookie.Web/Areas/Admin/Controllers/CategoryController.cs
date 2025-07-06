using Bookie.DataAccess.Data;
using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bookie.Web.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class CategoryController : Controller
    {
    private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork; 
        }
        public IActionResult Index()
        {
           
            List<Category> categoryList = _unitOfWork.Category.GetAll().ToList();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        } 
        
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {

                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created succssefully";

                return RedirectToAction ("Index");
            }
            return View();
        }

        public IActionResult Edit(int id)
        {
            Category? existingCategory = _unitOfWork.Category.Get(u=>u.Id==id);

            if (existingCategory== null) return NotFound();

            return View(existingCategory);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {

                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated succssefully";

                return RedirectToAction("Index");
            }
            return View();
        }


        public IActionResult Delete(int id)
        {
            Category? existingCategory = _unitOfWork.Category.Get(u => u.Id == id);

            if (existingCategory == null) return NotFound();

            return View(existingCategory);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {

            Category category = _unitOfWork.Category.Get(u => u.Id == id);

            if (category == null)
            {
                return NotFound();
                
            }
            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted succssefully";
            return RedirectToAction("Index");
        }
    }
}
