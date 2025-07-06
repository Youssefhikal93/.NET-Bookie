using System.Linq.Expressions;

namespace Bookie.Repository.IRepository
{
    public interface IRepository<T> where T :class
    {
        //T : Category
        IEnumerable<T> GetAll();
        T Get(Expression<Func<T, bool>> filter);
        void Add(T entity);
        void Remove(T enitity);
        void RemoveRange(IEnumerable<T> entity);
    }
}
