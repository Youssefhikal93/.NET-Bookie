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

        public void UpdatesStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var currentOrder = _db.OrderHeaders.FirstOrDefault(o => o.Id==id);
            if(currentOrder != null)
            {
                currentOrder.OrderStatus = orderStatus;

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    currentOrder.PaymentStatus = paymentStatus;
                }
            }
        }

        public void updateStripePaymentId(int id, string sessionId, string paymentIntenId)
        {
            OrderHeader currentOrder = _db.OrderHeaders.FirstOrDefault(o=>o.Id==id);
            if(currentOrder!= null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {

                currentOrder.SessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntenId))
                {

                    currentOrder.PaymentIntenId = paymentIntenId;
                    currentOrder.PaymentDate = DateTime.Now;
                }
            }
        }
    }
}
