using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess;
using Rocky_DataAccess.Repository;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Controllers
{

    [Authorize]

    public class CartController : Controller
    {

        private readonly IProductRepository _prodRepo;
        private readonly IApplicationUserRepository _appRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IInquiryHeaderRepository _headerRepo;
        private readonly IInquiryDetailsRepository _detailRepo;
        private readonly IOrderHeaderRepository _order_header_repo;
        private readonly IOrderDetailsRepository _order_details_repo;
        private readonly IEmailSender _emailSender;
        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public ProductUserViewModel ProductUserViewModel { get; set; }

        public CartController(IWebHostEnvironment webHostEnvironment, IProductRepository prodRepo, IApplicationUserRepository appRepo, IInquiryHeaderRepository headerRepo,
           IOrderHeaderRepository order_header_repo, IOrderDetailsRepository order_details_repo,
              IInquiryDetailsRepository detailRepo, 
            IEmailSender emailSender, IBrainTreeGate brain)
        {
            _prodRepo = prodRepo;
            _appRepo = appRepo;
            _headerRepo = headerRepo;
            _detailRepo = detailRepo;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _order_header_repo = order_header_repo;
            _order_details_repo = order_details_repo;
            _brain = brain;
        }



        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if(HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart)!=null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart).Count() > 0 )
            {
                //session exists

                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(MC.SesssionCart);


            }
            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();

            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> prodList = new List<Product>();




            foreach(var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFT = cartObj.SqFt;
                prodList.Add(prodTemp);
            }
    
            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFT });

            }

            HttpContext.Session.Set(MC.SesssionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            ApplicationUser applicationUser;

            if(User.IsInRole(MC.AdminRole))
            {
                if (HttpContext.Session.Get<int>(MC.SesssionInquiryId) != 0)
                {
                    //cart has been loaded with an inquiry.

                    InquiryHeader inquiryHeader = _headerRepo.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(MC.SesssionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhhoneNumber
                    };
                
             
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }

                var gateway = _brain.GetGateWay();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                applicationUser = _appRepo.FirstOrDefault(u => u.Id == claim.Value);
            }
    
            //var userId = User.FindFirstValue(ClaimTypes.Name);

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart).Count() > 0)
            {
                //session exists

                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(MC.SesssionCart);


            }
            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserViewModel = new ProductUserViewModel()
            {
                ApplicationUser = applicationUser,
            
        };

            foreach(var cartObj in shoppingCartList)
            {
                Product prodTemp = _prodRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFT = cartObj.SqFt;
                ProductUserViewModel.ProductList.Add(prodTemp);

            }
            return View(ProductUserViewModel);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(MC.SesssionCart).Count() > 0)
            {
                //session exists

                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(MC.SesssionCart);


            }
            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));

            HttpContext.Session.Set(MC.SesssionCart, shoppingCartList);



            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]

        public  async Task<IActionResult> SummaryPost(IFormCollection collection, ProductUserViewModel ProductUserVM)
        {


            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(MC.AdminRole))
            {
                //create order.

                

          
                OrderHeader orderheader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = ProductUserViewModel.ProductList.Sum(x=>x.TempSqFT*x.Price),
                    City = ProductUserViewModel.ApplicationUser.City,
                    StreetAddress = ProductUserViewModel.ApplicationUser.StreetAddress,
                    State = ProductUserViewModel.ApplicationUser.State,
                    PostalCode = ProductUserViewModel.ApplicationUser.Postal,
                    FullName = ProductUserViewModel.ApplicationUser.FullName,
                    Email = ProductUserViewModel.ApplicationUser.Email,
                    PhoneNumber = ProductUserViewModel.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = MC.StatusPending
                };
                _order_header_repo.Add(orderheader);
                _order_header_repo.Save();


                foreach (var prod in ProductUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                       OrderHeaderId = orderheader.Id,
                       PricePerSqFt = prod.Price,
                       Sqft = prod.TempSqFT,
                       ProductId =  prod.Id
                    };

                    _order_details_repo.Add(orderDetail);
                   

                }

                _order_details_repo.Save();

                string nonceFromTheClient = collection["payment_method_nonce"];

                var request = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(orderheader.FinalOrderTotal),
                    PaymentMethodNonce = nonceFromTheClient,
                    OrderId = orderheader.Id.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };

                var gateway = _brain.GetGateWay();
                Result<Transaction> result = gateway.Transaction.Sale(request);

                if(result.Target.ProcessorResponseText == "Approved")
                {
                    orderheader.TransactionId = result.Target.Id;
                    orderheader.OrderStatus = MC.StatusApproved;
                }
                else
                {
                    orderheader.OrderStatus = MC.StatusCancelled;
                }
                _order_header_repo.Save();
                return RedirectToAction(nameof(InquiryConfirmation),new { id = orderheader.Id });
            }

            else
            {
                //create inquiry.


                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "Templates" + Path.DirectorySeparatorChar.ToString()
                    + "Inquiry.html";

                var subject = "New Inquiry";

                string HtmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }


                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in ProductUserViewModel.ProductList)
                {
                    productListSB.Append($" - Name: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br/>");
                }
                string messageBdy = string.Format(HtmlBody,
                    ProductUserVM.ApplicationUser.FullName,
                    ProductUserVM.ApplicationUser.Email,
                    ProductUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString());


                await _emailSender.SendEmailAsync(MC.EmailAdmin, subject, messageBdy);

                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claim.Value,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.Now
                };

                _headerRepo.Add(inquiryHeader);
                _headerRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    InquiryDetails inquiryDetail = new InquiryDetails()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id
                    };

                    _detailRepo.Add(inquiryDetail);

                }

                TempData[MC.Success] = "Inquiry Sent Successfully";
                _detailRepo.Save();
                return RedirectToAction(nameof(InquiryConfirmation));
            }
        }
        public IActionResult InquiryConfirmation(int id=0)
        {
            OrderHeader orderHeader = _order_header_repo.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            foreach(Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFT });

            }

            HttpContext.Session.Set(MC.SesssionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));

           

        }

        public IActionResult Clear(int id)
        {

            HttpContext.Session.Clear();

            return RedirectToAction("Index","Home");
        }
    }
}
