using Bookie.DataAccess.Repository;
using Bookie.DataAccess.Repository.IRepository;
using Bookie.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookieWeb.ViewComponents
{
    public class ShoppingCartViewComponent :ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWOrk)
        {
            _unitOfWork = unitOfWOrk;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentoty = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentoty.FindFirst(ClaimTypes.NameIdentifier);

            if (claim?.Value != null)
            {

                if (HttpContext.Session.GetInt32(SD.sessionCart) == null)
                {
                    HttpContext.Session.SetInt32(SD.sessionCart, _unitOfWork.ShoppingCart
                                  .GetAll(s => s.ApplicationUserId == claim.Value).Count());
                }

               
                return View(HttpContext.Session.GetInt32(SD.sessionCart));
            } else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }

    }
}
