using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Repository;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{

    [Authorize(Roles = MC.AdminRole)]
    public class InquiryController : Controller
    {

        private readonly IInquiryHeaderRepository _inquiryHeader;
        private readonly IInquiryDetailsRepository _inquiryDetails;

        [BindProperty]
        public InquiryVM InquiryVM { get; set; }

        public InquiryController(IInquiryDetailsRepository inquiryDetails, IInquiryHeaderRepository inquiryHeader)
        {
            _inquiryHeader = inquiryHeader;
            _inquiryDetails = inquiryDetails;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            InquiryVM = new InquiryVM()
            {
                InquiryHeader = _inquiryHeader.FirstOrDefault(u => u.Id == id),
                InquiryDetails = _inquiryDetails.GetAll(u => u.InquiryHeaderId == id, includeProperties: "Product")
            };

            return View(InquiryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Details()
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            InquiryVM.InquiryDetails = _inquiryDetails.GetAll(u => u.InquiryHeaderId == InquiryVM.InquiryHeader.Id);
     
            foreach(var detail in InquiryVM.InquiryDetails)
            {
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    ProductId = detail.ProductId
                };
                shoppingCartList.Add(shoppingCart);
            }
            HttpContext.Session.Clear();
            HttpContext.Session.Set(MC.SesssionCart, shoppingCartList);
            HttpContext.Session.Set(MC.SesssionInquiryId, InquiryVM.InquiryHeader.Id);

            return RedirectToAction("Index","Cart");
        }

        [HttpPost]
        public IActionResult Delete()
        {

            InquiryHeader inquiryHeader = _inquiryHeader.FirstOrDefault(u => u.Id == InquiryVM.InquiryHeader.Id);
            IEnumerable<InquiryDetails> inquiryDetails = _inquiryDetails.GetAll(u => u.InquiryHeaderId == InquiryVM.InquiryHeader.Id);

            _inquiryDetails.RemoveRange(inquiryDetails);
            _inquiryHeader.Remove(inquiryHeader);
            _inquiryHeader.Save();

            return RedirectToAction(nameof(Index));
        }



        #region API CALLS

        [HttpGet]
        public IActionResult GetInquiryList()
        {
            return Json(new { data = _inquiryHeader.GetAll() });
        }
        #endregion
    }
}
