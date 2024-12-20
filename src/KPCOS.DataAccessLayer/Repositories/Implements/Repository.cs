﻿using System.Linq.Expressions;
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