using System.Linq.Expressions;
using System.Text.Json;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace KPCOS.DataAccessLayer.Repositories.Implements.CacheImplements;

public class UserCacheRepository(IUserRepository repository, IDistributedCache cache) : IUserRepository
{
    public DbSet<User?> Entities => DbContext.Set<User>();
    public DbContext DbContext { get; }

    public IQueryable<User?> Get()
    {
        return repository.Get();
    }

    public IQueryable<User?> Where(Expression<Func<User?, bool>> predic = null)
    {
        return repository.Where(predic);
    }

    public void Add(User? entity)
    {
        repository.Add(entity);
        if (cache.Get(entity?.Id.ToString()) != null || entity == null)
        {
            cache.Remove(entity?.Id.ToString());
        }
        cache.SetString(entity?.Id.ToString(), JsonSerializer.Serialize(entity));
    }

    public void Update(User? entity)
    {
        repository.Update(entity);
        if (cache.Get(entity?.Id.ToString()) != null || entity == null)
        {
            cache.Remove(entity?.Id.ToString());
        }
        cache.SetString(entity?.Id.ToString(), JsonSerializer.Serialize(entity));
    }

    public int Count()
    {
        return repository.Count();
    }
    
    public int SaveChanges()
    {
        throw new NotImplementedException();
    }

    public User? FirstOrDefault(Expression<Func<User?, bool>> predicate = null)
    {
        throw new NotImplementedException();
    }

    public User? SingleOrDefault(Expression<Func<User?, bool>> predicate = null)
    {
        throw new NotImplementedException();
    }

    public User? Find(params object?[]? keyValues)
    {
        throw new NotImplementedException();
    }

    public Task<IQueryable<User>> GetAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IQueryable<User>> WhereAsync(Expression<Func<User?, bool>> predic = null)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(User? entity, bool saveChanges = true)
    {
        throw new NotImplementedException();
    }

    public Task AddRangeAsync(List<User> entities, bool saveChanges = true)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(User? entity, bool saveChanges = true)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(User? entity, bool saveChanges = true)
    {
        throw new NotImplementedException();
    }

    public Task<User?> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User?> FirstOrDefaultAsync(Expression<Func<User?, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<User?> SingleOrDefaultAsync(Expression<Func<User?, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<User?> FindAsync(params object?[]? keyValues)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveAsync()
    {
        throw new NotImplementedException();
    }
}