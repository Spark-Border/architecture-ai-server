using System.Linq.Expressions;

namespace ArchitectureAI.Application.Interfaces.Repositories;

public class PagedResult<T>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; } = new List<T>();
}

public interface IGenericRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
    Task<T> FindAsync(Expression<Func<T, bool>> expression);
    Task<IEnumerable<T>> FindAndIncludeAsync(Expression<Func<T, bool>> expression, params string[] includeProperties);
    Task<IEnumerable<T>> GetAllAsync(params string[] includeProperties);
    Task<T> GetByIdAsync(string id);
    Task<int> RemoveAsync(T entity);
    Task RemoveRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>> expression);
    Task<int> CountAsync();
    Task<bool> SaveAsync();
    Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params string[] includeProperties);
}
