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
        if (cache.Get(entity!.Id.ToString()) != null)
        {
            cache.Remove(entity.Id.ToString());
        }
        cache.SetString(entity.Id.ToString(), JsonSerializer.Serialize(entity));
    }

    public void Update(User? entity)
    {
        repository.Update(entity);
        if (cache.Get(entity!.Id.ToString()) != null)
        {
            cache.Remove(entity.Id.ToString());
        }
        cache.SetString(entity.Id.ToString(), JsonSerializer.Serialize(entity));
    }

    public int Count()
    {
        return repository.Count();
    }
    
    public int SaveChanges()
    {
        return repository.SaveChanges();
    }

    public User? FirstOrDefault(Expression<Func<User?, bool>> predicate = null)
    {
        var cacheValue = cache.GetString(predicate.Body.ToString());
        if (cacheValue != null)
        {
            return JsonSerializer.Deserialize<User>(cacheValue);
        }
        var repoValue = repository.FirstOrDefault(predicate);
        if (repoValue != null)
        {
            cache.SetString(predicate.Body.ToString(), JsonSerializer.Serialize(repoValue));
        }
        return repoValue;
    }

    public User? SingleOrDefault(Expression<Func<User?, bool>> predicate = null)
    {
        var cacheValue = cache.GetString(predicate.Body.ToString());
        if (cacheValue != null)
        {
            return JsonSerializer.Deserialize<User>(cacheValue);
        }
        var repoValue = repository.SingleOrDefault(predicate);
        if (repoValue != null)
        {
            cache.SetString(predicate.Body.ToString(), JsonSerializer.Serialize(repoValue));
        }
        return repoValue;
    }

    public User? Find(params object?[]? keyValues)
    {
        var cacheValue = cache.GetString(keyValues[0].ToString());
        if (cacheValue != null)
        {
            return JsonSerializer.Deserialize<User>(cacheValue);
        }
        var repoValue = repository.Find(keyValues);
        if (repoValue != null)
        {
            cache.SetString(keyValues[0].ToString(), JsonSerializer.Serialize(repoValue));
        }
        return repoValue;
    }

    public async Task<IQueryable<User>> GetAsync()
    {
        return await repository.GetAsync();
    }

    public async Task<IQueryable<User>> WhereAsync(Expression<Func<User?, bool>> predic = null)
    {
        return await repository.WhereAsync(predic);
    }

    public async Task AddAsync(User? entity, bool saveChanges = true)
    {
        await repository.AddAsync(entity, saveChanges);
        if (await cache.GetStringAsync(entity!.Id.ToString()) != null)
        {
            await cache.RemoveAsync(entity.Id.ToString());
            return;
        }
        await cache.SetStringAsync(entity.Id.ToString(), JsonSerializer.Serialize(entity));
    }

    public async Task AddRangeAsync(List<User> entities, bool saveChanges = true)
    {
        await repository.AddRangeAsync(entities, saveChanges);
    }

    public async Task UpdateAsync(User? entity, bool saveChanges = true)
    {
        await repository.UpdateAsync(entity, saveChanges);
        if (await cache.GetStringAsync(entity.Id.ToString()) != null )
        {
            await cache.RemoveAsync(entity.Id.ToString());
        }
        await cache.SetStringAsync(entity.Id.ToString(), JsonSerializer.Serialize(entity));
    }

    public async Task RemoveAsync(User? entity, bool saveChanges = true)
    {
        await repository.RemoveAsync(entity, saveChanges);
        if (await cache.GetStringAsync(entity.Id.ToString()) != null)
        {
            await cache.RemoveAsync(entity.Id.ToString());
        }
    }

    public async Task<User?> FirstOrDefaultAsync(Expression<Func<User?, bool>> predicate)
    {
        var cacheValue = await cache.GetStringAsync(predicate.Body.ToString());
        if (cacheValue != null)
        {
            return JsonSerializer.Deserialize<User>(cacheValue);
        }
        var result = await repository.FirstOrDefaultAsync(predicate);
        if (result != null)
        {
            await cache.SetStringAsync(predicate.Body.ToString(), JsonSerializer.Serialize(result));
        }
        return result;
    }

    public async Task<User?> SingleOrDefaultAsync(Expression<Func<User?, bool>> predicate)
    {
        var cacheValue = await cache.GetStringAsync(predicate.Body.ToString());
        if (cacheValue != null)
        {
            return JsonSerializer.Deserialize<User>(cacheValue);
        }
        var result = await repository.SingleOrDefaultAsync(predicate);
        if (result != null)
        {
            await cache.SetStringAsync(predicate.Body.ToString(), JsonSerializer.Serialize(result));
        }
        return result;
    }

    public async Task<User?> FindAsync(params object?[]? keyValues)
    {
        var cacheValue = await cache.GetStringAsync(keyValues[0].ToString());
        if (cacheValue != null)
        {
            return JsonSerializer.Deserialize<User>(cacheValue);
        }
        var result = await repository.FindAsync(keyValues);
        if (result != null)
        {
            await cache.SetStringAsync(keyValues[0].ToString(), JsonSerializer.Serialize(result));
        }
        return result;
    }

    public async Task<int> SaveAsync()
    {
        return await repository.SaveAsync();
    }
}