using KPCOS.DataAccessLayer.Context;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories.Operations;

namespace KPCOS.DataAccessLayer.Repositories.Implements.BaseImplements;

public class UserRepository(KPCOSDBContext context, IUnitOfWork unitOfWork) :
    Repository<User>(context),
    IUserRepository
{
    
}