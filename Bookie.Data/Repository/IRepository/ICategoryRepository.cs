using Bookie.Models;
using Bookie.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookie.DataAccess.Repository.IRepository
{
   public interface ICategoryRepository :IRepository<Category>
    {
        void Update(Category obj);
        void Save();
    }
}
