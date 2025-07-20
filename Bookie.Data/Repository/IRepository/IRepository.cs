using System.Linq.Expressions;

namespace Bookie.Repository.IRepository
{
    public interface IRepository<T> where T :class
    {
        //T : Category
        IEnumerable<T> GetAll(string? includeProperties = null);
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null);
        void Add(T entity);
        void Remove(T enitity);
        void RemoveRange(IEnumerable<T> entity);
    }
}
