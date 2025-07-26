using Bookie.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BookieWeb.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS
        public IActionResult GetAll()
        {
            var OrderHeadersList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();


            return Json(new { data = OrderHeadersList });
        }
        #endregion
    }
}
