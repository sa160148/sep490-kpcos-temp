using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.DataAccessLayer.Repositories.Implements;

public class Repository<T> : IRepository<T> where T : class
{
    public Repository(DbContext dbContext)
    {
        DbContext = dbContext;
    }

    public DbContext DbContext { get; }
    public DbSet<T?> Entities => DbContext.Set<T>();

    public IQueryable<T?> Get()
    {
        return Entities.AsQueryable();
    }

    public IQueryable<T> GetPagingQueryable(int pageNumber, int pageSize)
    {
        var query = Entities.AsQueryable();
        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    public virtual IEnumerable<T> Get(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int? pageIndex = null,
        int? pageSize = null)
    {
        IQueryable<T> query = Entities;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
                     (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Implementing pagination
        if (pageIndex.HasValue && pageSize.HasValue)
        {
            // Ensure the pageIndex and pageSize are valid
            int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
            int validPageSize =
                pageSize.Value > 0
                    ? pageSize.Value
                    : 10; // Assuming a default pageSize of 10 if an invalid value is passed
            if (pageSize.Value > 0)
            {
                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }
        }

        return query.ToList();
    }

    public virtual (IEnumerable<T> Data, int Count) GetWithCount(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int? pageIndex = null,
        int? pageSize = null)
    {
        IQueryable<T> query = Entities;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Get the total count before pagination
        int count = query.Count();

        // Implementing pagination
        if (pageIndex.HasValue && pageSize.HasValue)
        {
            int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
            int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Default pageSize of 10 if invalid
            query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
        }

        // Returning data with count
        return (Data: query.ToList(), Count: count);
    }

    public IQueryable<T?> Where(Expression<Func<T?, bool>> predic = null)
    {
        return Entities.Where(predic).AsQueryable();
    }

    public void Add(T? entity)
    {
        Entities.Add(entity);
    }

    public T? SingleOrDefault(Expression<Func<T?, bool>> predicate)
    {
        return Entities.SingleOrDefault(predicate);
    }

    public T? Find(params object?[]? keyValues)
    {
        return Entities.Find(keyValues);
    }

    public async Task<IQueryable<T>> GetAsync()
    {
        return await (Task<IQueryable<T>>)Entities.AsQueryable();
    }

    public async Task<IQueryable<T>> WhereAsync(Expression<Func<T?, bool>> predic = null)
    {
        return await (Task<IQueryable<T>>)Entities.Where(predic).AsQueryable();
    }

    public async Task AddAsync(T? entity, bool saveChanges = true)
    {
        await Entities.AddAsync(entity);
        if (saveChanges) await DbContext.SaveChangesAsync();
    }

    public async Task AddRangeAsync(List<T> entities, bool saveChanges = true)
    {
        await Entities.AddRangeAsync(entities);
        if (saveChanges) await DbContext.SaveChangesAsync();
    }

    public void Update(T? entity)
    {
        Entities.Update(entity);
    }

    public async Task UpdateAsync(T? entity, bool saveChanges = true)
    {
        Entities.Update(entity);
        if (saveChanges) await DbContext.SaveChangesAsync();
    }

    public async Task RemoveAsync(T? entity, bool saveChanges = true)
    {
        Entities.Remove(entity);
        if (saveChanges) await DbContext.SaveChangesAsync();
    }

    public int Count()
    {
        return Entities.Count();
    }

    public T? FirstOrDefault()
    {
        return Entities.FirstOrDefault();
    }

    public T? LastOrDefault()
    {
        return Entities.LastOrDefault();
    }

    public T? FirstOrDefault(Expression<Func<T?, bool>> predicate)
    {
        return Entities.FirstOrDefault(predicate);
    }

    public async Task<T?> FirstOrDefaultAsync()
    {
        return await Entities.FirstOrDefaultAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T?, bool>> predicate)
    {
        return await Entities.FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T?, bool>> predicate)
    {
        return await Entities.SingleOrDefaultAsync(predicate);
    }

    public int SaveChanges()
    {
        return DbContext.SaveChanges();
    }

    public async Task<int> SaveAsync()
    {
        return await DbContext.SaveChangesAsync();
    }

    public async Task<T?> FindAsync(params object?[]? keyValues)
    {
        return await Entities.FindAsync(keyValues);
    }

    public T? Find(Expression<Func<T, bool>> expression)
    {
        return Entities.Find(expression);
    }
}