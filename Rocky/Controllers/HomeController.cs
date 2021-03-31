using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocky_DataAccess;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IProductRepository _prodRepo;

        private readonly ICategoryRepository _catRepo;




        public HomeController(ILogger<HomeController> logger, IProductRepository prodRepo, 
            ICategoryRepository catRepo, ApplicationDBContext db)
        {
            _logger = logger;
            _prodRepo = prodRepo;
            _catRepo = catRepo;
        }

        public IActionResult Index()
        {

            HomeViewModel HVM = new HomeViewModel()
            {

                Products = _prodRepo.GetAll(includeProperties: "Category,ApplicationType"),

                Categories = _catRepo.GetAll()
            };

            return View(HVM);
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

        public IActionResult Details(int? id)
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart) != null

                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(MC.SesssionCart);

            }


            DetailsVM DetailsVM= new DetailsVM()
            {
                Product = _prodRepo.FirstOrDefault(u=>u.Id == id,includeProperties: "Category,ApplicationType"),
                ExistsInCart = false
            };

            foreach(var item in shoppingCartList)
            {
                if(item.ProductId==id)
                {
                    DetailsVM.ExistsInCart = true;

                }
            }
            return View(DetailsVM);

        }

        [HttpPost,ActionName("Details")]

        public IActionResult DetailsPost(int id, DetailsVM detailsVM)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if(HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart)!=null
                
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(MC.SesssionCart);
          
            }
            shoppingCartList.Add(new ShoppingCart { ProductId = id, SqFt=detailsVM.Product.TempSqFT });
            HttpContext.Session.Set(MC.SesssionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));

        }
    

    public IActionResult RemoveFromCart(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart) != null

            && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart).Count() > 0)
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(MC.SesssionCart);

        }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);

            if(itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }

        HttpContext.Session.Set(MC.SesssionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));

    }
}
}
