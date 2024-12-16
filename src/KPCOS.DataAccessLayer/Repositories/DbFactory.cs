using KPCOS.DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.DataAccessLayer.Repositories;

public class DbFactory
{
    private DbContext _dbContext;
    private bool _disposed;
    private readonly Func<KPCOSDBContext> _instanceFunc;

    public DbFactory(Func<KPCOSDBContext> dbContextFactory)
    {
        _instanceFunc = dbContextFactory;
    }

    public DbContext DbContext => _dbContext ?? (_dbContext = _instanceFunc.Invoke());

    public void Dispose()
    {
        if (!_disposed && _dbContext != null)
        {
            _disposed = true;
            _dbContext.Dispose();
        }
    }
}