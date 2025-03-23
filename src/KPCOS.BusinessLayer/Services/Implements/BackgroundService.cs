using Hangfire;
using KPCOS.DataAccessLayer.Repositories;

namespace KPCOS.BusinessLayer.Services.Implements;

public class BackgroundService : IBackgroundService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IFirebaseService _firebaseService;

    public BackgroundService(IBackgroundJobClient backgroundJobClient, IUnitOfWork unitOfWork, IFirebaseService firebaseService)
    {
        _backgroundJobClient = backgroundJobClient;
        _unitOfWork = unitOfWork;
        _firebaseService = firebaseService;
    }

    public Task DelayedJob()
    {
        throw new NotImplementedException();
    }

    public void DelayedCancelOtpJob(int timespanMinutes, string contractId)
    { 
        _backgroundJobClient.Schedule(() => _firebaseService.DeleteContractOtpAsync(contractId), TimeSpan.FromMinutes(timespanMinutes));
    }
    
    public void DelayedCancelDocOtpJob(int timespanMinutes, string docId)
    { 
        _backgroundJobClient.Schedule(() => _firebaseService.DeleteDocOtpAsync(docId), TimeSpan.FromMinutes(timespanMinutes));
    }
}