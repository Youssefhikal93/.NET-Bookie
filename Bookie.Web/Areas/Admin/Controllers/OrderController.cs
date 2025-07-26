using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Bookie.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookieWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
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
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> OrderHeadersList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

            switch (status)
            {
                case "pending": 
                    OrderHeadersList = OrderHeadersList.Where(u=>u.PaymentStatus==SD.PaymentStatusDelayedPayment); break;
                case "inprocess":
                    OrderHeadersList = OrderHeadersList.Where(u => u.PaymentStatus == SD.StatusInProcess); break;
                case "completed":
                    OrderHeadersList = OrderHeadersList.Where(u => u.PaymentStatus == SD.StatusShipped); break;
                case "approved":
                    OrderHeadersList = OrderHeadersList.Where(u => u.PaymentStatus == SD.StatusApproved); break;
                default: break;
            }

            return Json(new { data = OrderHeadersList });
        }
        #endregion
    }
}
