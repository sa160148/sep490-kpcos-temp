using KPCOS.DataAccessLayer.Context;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories.Operations;

namespace KPCOS.DataAccessLayer.Repositories.Implements.BaseImplements;

public class ServiceRepository(KPCOSDBContext context, IUnitOfWork unitOfWork) :
    Repository<Service>(context),
    IServiceRepository
{
    
} 