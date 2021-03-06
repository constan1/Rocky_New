using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky_DataAccess.Repository
{
   public  class OrderDetailsRepository: Repository<OrderDetail>, IOrderDetailsRepository
    {

       private readonly ApplicationDBContext _db;

        public OrderDetailsRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }
       
        public void update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
