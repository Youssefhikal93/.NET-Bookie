using System.Diagnostics;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Bookie.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookie.Web.Areas.Customer.Controllers;
[Area("Customer")]

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    public HomeController(ILogger<HomeController> logger , IUnitOfWork unitOfWork, IEmailSender emailSender)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
        return View(productList);
    }

    [HttpPost]
    public async Task<IActionResult> Index(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["error"] = "Please enter a valid email address";
            return RedirectToAction(nameof(Index));
        }
        TempData["success"] = "Welcom to our family ?? !";
        await _emailSender.SendEmailAsync(
           email,
           "Subscription Confirmation",
           "Thank you for subscribing to our newsletter!");

        TempData["success"] = "Your email subscribed successfully";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        ShoppingCart cart = new ShoppingCart()
        {
            Product = _unitOfWork.Product.Get(p => p.Id == id, includeProperties: "Category"),
            Count = 1,
            ProductId = id
        };
        //var product = _unitOfWork.Product.Get(p=>p.Id==id, includeProperties: "Category");
        return View(cart);
    }
    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        shoppingCart.ApplicationUserId = userId;
        shoppingCart.Id = 0; // Ensure ID is reset for new entries

        var existingShoppingCart = _unitOfWork.ShoppingCart
            .Get(s => s.ApplicationUserId == userId && s.ProductId == shoppingCart.ProductId);

        if (existingShoppingCart != null)
        {
            existingShoppingCart.Count += shoppingCart.Count;
            _unitOfWork.ShoppingCart.Update(existingShoppingCart);
            _unitOfWork.Save();

            TempData["success"] = $"{existingShoppingCart.Count} items now in your cart!";
        }
        else
        {
            _unitOfWork.ShoppingCart.Add(shoppingCart);
            _unitOfWork.Save();

            TempData["success"] = $"{shoppingCart.Count} item(s) added to cart successfully!";
            //Add to session 
            HttpContext.Session.SetInt32(SD.sessionCart, _unitOfWork.ShoppingCart
            .GetAll(s => s.ApplicationUserId == userId).Count());
        }

        _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
