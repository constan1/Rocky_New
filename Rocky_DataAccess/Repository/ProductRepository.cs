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
   public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDBContext _db;

        public ProductRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetAllDropDown(string obj)
        {
            if(obj ==MC.Category)
            {
                return _db.Category.Select(i => new SelectListItem
                {

                    Text = i.Name,
                    Value = i.Id.ToString()
                });
            }

            if(obj == MC.Application)
            {
                return _db.Application.Select(i => new SelectListItem
                {

                    Text = i.Name,
                    Value = i.Id.ToString()
                });
            };

            return null;
        }
      

        public void Update(Product obj)
        {
            _db.Product.Update(obj);
        }

        
    }
    }
