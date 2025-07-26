using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Bookie.Models.ViewModels;
using Bookie.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Security.Claims;

namespace BookieWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty] // will automaticully populate the model once we post (in nput fields)
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentoty = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentoty.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentoty = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentoty.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                .Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }
            return View(ShoppingCartVM);
        }

        //[HttpPost]
        //public IActionResult Summary(ShoppingCartVM shoppingCartVM)
        //{
        //    var claimsIdentoty = (ClaimsIdentity)User.Identity;
        //    var userId = claimsIdentoty.FindFirst(ClaimTypes.NameIdentifier).Value;

        //    // Use the parameter shoppingCartVM instead of ShoppingCartVM
        //    shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
        //        u => u.ApplicationUserId == userId,
        //        includeProperties: "Product");

        //    var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
        //    shoppingCartVM.OrderHeader.ApplicationUser = applicationUser;
        //    shoppingCartVM.OrderHeader.Name = applicationUser.Name;
        //    shoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
        //    shoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
        //    shoppingCartVM.OrderHeader.City = applicationUser.City;
        //    shoppingCartVM.OrderHeader.State = applicationUser.State;
        //    shoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;
        //    shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
        //    shoppingCartVM.OrderHeader.ApplicationUserId = userId;

        //    // Initialize OrderTotal before the loop
        //    shoppingCartVM.OrderHeader.OrderTotal = 0;

        //    foreach (var cart in shoppingCartVM.ShoppingCartList)
        //    {
        //        cart.Price = GetPriceBasedOnQuantity(cart);
        //        shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        //    }

        //    if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        //    {
        //        shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
        //        shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
        //    }
        //    else
        //    {
        //        shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
        //        shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        //    }

        //    _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
        //    _unitOfWork.Save();

        //    foreach (var cart in shoppingCartVM.ShoppingCartList)
        //    {
        //        OrderDetail orderDetail = new()
        //        {
        //            ProductId = cart.ProductId,
        //            OrderHeaderId = shoppingCartVM.OrderHeader.Id,
        //            Price = cart.Price,
        //            Count = cart.Count
        //        };
        //        _unitOfWork.OrderDetail.Add(orderDetail);
        //    }
        //    _unitOfWork.Save();

        //    if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        //    {
        //        //StripeLogic
        //    }

        //    return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingCartVM.OrderHeader.Id });
        //}

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentoty = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentoty.FindFirst(ClaimTypes.NameIdentifier).Value;

           

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");


            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


            var applicationUser = ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                 .Get(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //means logged in user is not a company
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //StripeLogic
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }




        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);

            if (cartFromDb == null)
            {
                TempData["error"] = "Shopping cart item not found";
                return RedirectToAction(nameof(Index));
            }

            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Item quantity increased";
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            if (cartFromDb == null)
            {
                TempData["error"] = "Shopping cart item not found";
                return RedirectToAction(nameof(Index));
            }
            if (cartFromDb.Count == 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {

                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {

            var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            if (cartFromDb == null)
            {
                TempData["error"] = "Shopping cart item not found";
                return RedirectToAction(nameof(Index));
            }
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}

