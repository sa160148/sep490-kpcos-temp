using Microsoft.EntityFrameworkCore;

namespace KPCOS.DataAccessLayer.Repositories;

public interface IUnitOfWork
{
    DbContext DbContext { get; }
    IRepository<T> Repository<T>() where T : class;
    void SaveChanges();
    int SaveManualChanges();
    Task SaveChangesAsync();
    Task<int> SaveManualChangesAsync();

    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
}