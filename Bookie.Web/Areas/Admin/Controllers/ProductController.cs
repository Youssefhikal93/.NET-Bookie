using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Bookie.Models.ViewModels;
using Bookie.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookieWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
             var productsList =_unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
           

            return View(productsList);
        }

        public IActionResult Upsert(int? id)
        {
            var categoryList = _unitOfWork.Category.GetAll()
               .Select(u => new SelectListItem
               {
                   Text = u.Name,
                   Value = u.Id.ToString(),
               });
            //ViewBag.CategoryList = categoryList;
            ProductVM productVM = new()
            {
                CategoryList = categoryList,
                Product = new Product(),
               
            };
            if(id==null || id == 0)
            {
                //Create
            return View(productVM);
            }else
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension
                        (file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images/product");

                    //Delete the image if exixts on update case!
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldPathName = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldPathName))
                        {
                            System.IO.File.Delete(oldPathName);
                        }
                    }


                    using(var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"/images/product/" + fileName;

                }

                if(productVM.Product.Id == 0)
                {
                _unitOfWork.Product.Add(productVM.Product);

                }
                else
                {
                    _unitOfWork.Product.update(productVM.Product);
                }

                    _unitOfWork.Save();
                TempData["success"] = "Book created succssfully!";
                return RedirectToAction("Index");
            }
            return View();
        }

        //public IActionResult Edit(int id)
        //{
        //    Product existingProduct = _unitOfWork.Product.Get(p => p.Id == id);
        //    if (existingProduct == null) return NotFound();
        //    return View(existingProduct);
        //}

        //[HttpPost]
        //public IActionResult Edit(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //    _unitOfWork.Product.update(product);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Book updated succssefully";
        //    return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        //public IActionResult Delete(int id )
            
        //{
        //    var existingProduct = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (existingProduct == null) return NotFound();
        //    return View(existingProduct);
        //}

        //[HttpPost]
        //public IActionResult Delete(Product product)
        //{
        //    _unitOfWork.Product.Remove(product);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Book deleted succssefully";

        //    return RedirectToAction("Index");
        //}

        #region API CALLS
        public IActionResult GetAll()
        {
            var productsList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();


            return Json(new {data=productsList});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u=>u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deletinig!" });
            }
            var oldPathName = Path.Combine(_webHostEnvironment.WebRootPath,
                productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldPathName))
            {
                System.IO.File.Delete(oldPathName);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted successfully!" });
        }
        #endregion
    }
}
