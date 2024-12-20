using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace KPCOS.DataAccessLayer.Repositories.Implements;

public class CacheRepository<T> : IRepository<T> where T : class
{
    private readonly IRepository<T?> _repository;
    private readonly IDistributedCache _cache;

    public CacheRepository(DbContext dbContext, IRepository<T?> repository, IDistributedCache cache)
    {
        DbContext = dbContext;
        _repository = repository;
        _cache = cache;
    }

    public DbContext DbContext { get; }
    public DbSet<T?> Entities => DbContext.Set<T>();

    public IQueryable<T?> Get()
    {
        return _repository.Get();
    }

    public IQueryable<T?> Where(Expression<Func<T?, bool>> predic = null)
    {
        return _repository.Where(predic);
    }

    public void Add(T? entity)
    {
        _repository.Add(entity);

        _cache.SetString( /*key*/ "key", JsonSerializer.Serialize(entity));
    }

    public void Update(T? entity)
    {
        _repository.Update(entity);

        if (!string.IsNullOrEmpty(_cache.GetString( /*key*/ "key")))
        {
            _cache.Remove( /*key*/ "key");
        }

        _cache.SetString( /*key*/ "key", JsonSerializer.Serialize(entity));
    }

    public int Count()
    {
        return _repository.Count();
    }

    public T? FirstOrDefault()
    {
        return _repository.FirstOrDefault();
    }

    public T? LastOrDefault()
    {
        return _repository.LastOrDefault();
    }

    public int SaveChanges()
    {
        return _repository.SaveChanges();
    }

    public T? FirstOrDefault(Expression<Func<T?, bool>> predicate)
    {
        return _repository.FirstOrDefault(predicate);
    }

    public T? SingleOrDefault(Expression<Func<T?, bool>> predicate)
    {
        var cacheEntity = _cache.GetString(predicate.Body.ToString());
        if (!string.IsNullOrEmpty(cacheEntity))
        {
            return JsonSerializer.Deserialize<T>(cacheEntity);
        }

        var entity = _repository.SingleOrDefault(predicate);
        _cache.SetString( /*key*/ "key", JsonSerializer.Serialize(entity));
        return entity;
    }

    public T? Find(params object[] keyValues)
    {
        var cacheEntity = _cache.GetString(keyValues.ToString()!);
        if (!string.IsNullOrEmpty(cacheEntity))
        {
            return JsonSerializer.Deserialize<T>(cacheEntity);
        }

        return _repository.Find(keyValues);
    }

    public async Task<IQueryable<T>> GetAsync()
    {
        return await _repository.GetAsync();
    }

    public async Task<IQueryable<T>> WhereAsync(Expression<Func<T?, bool>> predic = null)
    {
        return await _repository.WhereAsync(predic);
    }

    public async Task AddAsync(T? entity, bool saveChanges = true)
    {
        await _repository.AddAsync(entity, saveChanges);

        await _cache.SetStringAsync( /*key*/ "key", JsonSerializer.Serialize(entity));
    }

    public async Task AddRangeAsync(List<T> entities, bool saveChanges = true)
    {
        await _repository.AddRangeAsync(entities, saveChanges);
    }

    public async Task UpdateAsync(T? entity, bool saveChanges = true)
    {
        await _repository.UpdateAsync(entity, saveChanges);

        var cacheEntity = await _cache.GetStringAsync( /*key*/ "key");
        if (string.IsNullOrEmpty(cacheEntity))
        {
            await _cache.SetStringAsync( /*key*/ "key", JsonSerializer.Serialize(entity));
        }
    }

    public async Task RemoveAsync(T? entity, bool saveChanges = true)
    {
        await _cache.RemoveAsync( /*key*/ "key");

        await _repository.RemoveAsync(entity, saveChanges);
    }

    public async Task<T?> FirstOrDefaultAsync()
    {
        return await _repository.FirstOrDefaultAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T?, bool>> predicate)
    {
        return await _repository.FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T?, bool>> predicate)
    {
        return await _repository.SingleOrDefaultAsync(predicate);
    }

    public async Task<T?> FindAsync(params object[] keyValues)
    {
        var cacheEntity = await _cache.GetStringAsync(keyValues.ToString());
        if (!string.IsNullOrEmpty(cacheEntity))
        {
            return JsonSerializer.Deserialize<T>(cacheEntity);
        }

        return await _repository.FindAsync(keyValues);
    }

    public async Task<int> SaveAsync()
    {
        return await _repository.SaveAsync();
    }
}