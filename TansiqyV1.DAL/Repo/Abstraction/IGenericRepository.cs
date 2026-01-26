using System.Linq.Expressions;
using TansiqyV1.DAL.Entities;

namespace TansiqyV1.DAL.Repo.Abstraction;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> GetQueryable();
}





