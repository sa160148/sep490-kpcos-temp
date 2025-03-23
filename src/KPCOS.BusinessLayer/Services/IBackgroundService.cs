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
    
    /// <summary>
    /// Delay cancel doc otp job.
    /// <para>This function will delete the doc otp document in firebase after the minute that put in</para>
    /// </summary>
    /// <param name="timespanMinutes">int, the lifetime of a document otp</param>
    /// <param name="docId">string</param>
    void DelayedCancelDocOtpJob(int timespanMinutes, string docId);
}