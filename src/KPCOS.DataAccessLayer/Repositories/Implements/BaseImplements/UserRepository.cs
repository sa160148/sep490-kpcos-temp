using KPCOS.DataAccessLayer.Repositories.Operations;

namespace KPCOS.DataAccessLayer.Repositories.Implements.BaseImplements;

public class UserRepository(KpcosContext context, IUnitOfWork unitOfWork) :
    Repository<User>(context),
    IUserRepository
{
    
}