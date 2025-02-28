namespace KPCOS.BusinessLayer.Services;

public interface IBackgroundService
{
    Task DelayedJob();
    /// <summary>
    /// Delay cancel otp job.
    /// <para>This function will delete the otp document in firebase after the minute that put in</para>
    /// </summary>
    /// <param name="timespanMinutes">int, the lifetime of a contract otp</param>
    /// <param name="contractId">string</param>
    void DelayedCancelOtpJob(int timespanMinutes, string contractId);
}