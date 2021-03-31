using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky_DataAccess.Repository
{
   public class ApplicationTypeRepository : Repository<Application>, IApplicationRepository
    {
        public readonly ApplicationDBContext _db;

        public ApplicationTypeRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Application obj)
        {
            var objFromDb = base.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
               
            }
        }
    }
}
