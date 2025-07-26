using Bookie.Models;
using Bookie.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookie.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository: IRepository<OrderHeader>
    {
        void update(OrderHeader obj);
        void UpdatesStatus(int id, string orderStatus, string? paymentIntenId=null);
        void updateStripePaymentId(int id, string sessionId, string paymentIntenId);
    }
}
