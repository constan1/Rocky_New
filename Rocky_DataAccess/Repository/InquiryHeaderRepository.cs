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
   public  class InquiryHeaderRepository : Repository<InquiryHeader>, IInquiryHeaderRepository
    {

       private readonly ApplicationDBContext _db;

        public InquiryHeaderRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }
       
        public void update(InquiryHeader obj)
        {
            _db.InquiryHeaders.Update(obj);
        }
    }
}
