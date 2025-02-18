using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.DataAccessLayer.Repositories;

public interface IRepository<T> where T : class
{
    DbSet<T?> Entities { get; }
    DbContext DbContext { get; }

    #region Sync

    public IQueryable<T?> Get();
    public IQueryable<T> GetPagingQueryable(int pageNumber, int pageSize);
    public IQueryable<T?> Where(Expression<Func<T?, bool>> predic = null);
    public void Add(T? entity);
    public void Update(T? entity);
    public int Count();
    public int SaveChanges();
    public T? FirstOrDefault(Expression<Func<T?, bool>> predicate = null);
    public T? SingleOrDefault(Expression<Func<T?, bool>> predicate = null);
    public T? Find(params object?[]? keyValues);

    #endregion

    #region Async

    /// <summary>
    ///     unsafe to use, ill
    /// </summary>
    /// <returns>Task<IQueryable<T>></returns>
    public Task<IQueryable<T>> GetAsync();

    public Task<IQueryable<T>> WhereAsync(Expression<Func<T?, bool>> predic = null);
    public Task AddAsync(T? entity, bool saveChanges = true);
    public Task AddRangeAsync(List<T> entities, bool saveChanges = true);
    public Task UpdateAsync(T? entity, bool saveChanges = true);
    public Task RemoveAsync(T? entity, bool saveChanges = true);
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T?, bool>> predicate = null);
    
    public Task<T?> SingleOrDefaultAsync(Expression<Func<T?, bool>> predicate = null);
    
    /// <summary>
    /// use when know primary key
    /// </summary>
    /// <param name="keyValues">id</param>
    /// <returns>entity?</returns>
    public Task<T?> FindAsync(params object?[]? keyValues);
    public Task<int> SaveAsync();

    #endregion
}