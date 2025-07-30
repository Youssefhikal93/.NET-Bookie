using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Bookie.Models.ViewModels;
using Bookie.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BookieWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVm orderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
             orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId==orderId,includeProperties:"Product")
            };
            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles=SD.Role_Admin+","+SD.Role_Employee)]
         public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.State = orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }


            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order details updated successfully!";

            return RedirectToAction(nameof(Details),new {orderId=orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdatesStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();

           
            TempData["success"] = $"Order Status updated succssfully!";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });

        } 
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate =DateOnly.FromDateTime( DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = $"Order status updated succssfully to {orderHeaderFromDb.OrderStatus}";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntenId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
            _unitOfWork.OrderHeader.UpdatesStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }else
            {
                _unitOfWork.OrderHeader.UpdatesStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);

            }

            _unitOfWork.Save();
            TempData["success"] = $"Order status updated succssfully to {orderHeaderFromDb.OrderStatus}";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });

        }


        [HttpPost]
        public IActionResult PayNow()
        {

            orderVM.OrderHeader = _unitOfWork.OrderHeader
                .Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderVM.OrderDetail = _unitOfWork.OrderDetail
              .GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id, includeProperties: "Product");

            //StripeLogic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in orderVM.OrderDetail)
            {
                var sessionItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = (decimal)item.Price * 100,
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count

                };
                options.LineItems.Add(sessionItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.updateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }


        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader Placedorder = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId, includeProperties: "ApplicationUser");
            if (Placedorder.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(Placedorder.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdatesStatus(orderHeaderId, Placedorder.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.OrderHeader.updateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
              

            }
                return View(orderHeaderId);

        }
        #region API CALLS
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> OrderHeadersList;
            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                OrderHeadersList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            } else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                OrderHeadersList = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == userId,includeProperties: "ApplicationUser").ToList();

            }

            switch (status)
                {
                    case "pending":
                        OrderHeadersList = OrderHeadersList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment); break;
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
