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
    class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        //public void Save()
        //{
        //    _db.SaveChanges();
        //}

        public void update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
