using Bookie.DataAccess.Data;
using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bookie.DataAccess.Repository
{
    class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository 
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        //public void Save()
        //{
        //    _db.SaveChanges();
        //}

        public void update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }
    }
}
