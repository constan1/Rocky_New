using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    [Authorize(Roles=MC.AdminRole)]

    public class OrderController : Controller
    {


        private readonly IOrderHeaderRepository _order_header_repo;
        private readonly IOrderDetailsRepository _order_details_repo;
        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public OrderVM OrderVM { get; set; }


        public OrderController(
           IOrderHeaderRepository order_header_repo, IOrderDetailsRepository order_details_repo,
          IBrainTreeGate brain)
        {
          
            _order_header_repo = order_header_repo;
            _order_details_repo = order_details_repo;
            _brain = brain;
        }

        public IActionResult Index(string searchName = null,string searcEmail = null, string searcPhone = null, string Status = null)
        {

            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHList = _order_header_repo.GetAll(),
                StatusList = MC.listStatus.ToList().Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            if(!string.IsNullOrEmpty(searchName))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.FullName.ToLower().Contains(searchName.ToLower()));
            }
            if (!string.IsNullOrEmpty(searcEmail))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.Email.ToLower().Contains(searcEmail.ToLower()));
            }

            if (!string.IsNullOrEmpty(searcPhone))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.PhoneNumber.ToLower().Contains(searcPhone.ToLower()));
            }

            if (!string.IsNullOrEmpty(Status) && Status != "--Order Status--")
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower()));
            }


            return View(orderListVM);
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _order_header_repo.FirstOrDefault(u => u.Id == id),
                OrderDetails = _order_details_repo.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _order_header_repo.FirstOrDefault(u=>u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = MC.StatusInProcces;
            _order_header_repo.Save();


            TempData[MC.Success] = "Order Is In Process";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _order_header_repo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = MC.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _order_header_repo.Save();

            TempData[MC.Success] = "Order Shipped Successfully";
            return RedirectToAction(nameof(Index));

        }


        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _order_header_repo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            var gateway = _brain.GetGateWay();

            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);

            if (transaction.Status == TransactionStatus.AUTHORIZED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                //No refund
                Result<Transaction> resultvoid = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else
            {
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);
            }
            orderHeader.OrderStatus = MC.StatusRefunded;

            _order_header_repo.Save();


            TempData[MC.Success] = "Order Cancelled Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateOrderDetails()
        {
            OrderHeader orderHeaderFromDb = _order_header_repo.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            orderHeaderFromDb.FullName = OrderVM.OrderHeader.FullName;

            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.FullName;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            orderHeaderFromDb.Email = OrderVM.OrderHeader.Email;

            _order_header_repo.Save();

            TempData[MC.Success] = "Order Details Updated Successfully";

            return RedirectToAction("Details","Order",new { id = orderHeaderFromDb.Id });
        }

    }
}
