namespace KPCOS.Common.Scheduler;
using System;
using System.Collections.Concurrent;
using Quartz;
using Quartz.Impl;

public class TriggerOptions
{
    public string Every { get; set; } // "minute", "hour", "day", "weekday", "mon", "tue", etc.
    public TimeOfDay? At { get; set; }
}

public class CronJobManager
{
    private static readonly ConcurrentDictionary<string, IJobDetail> CronList = new();
    private static readonly IScheduler Scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

    public CronJobManager()
    {
        Scheduler.Start().Wait();
    }

    private static string GenerateCronExpression(TriggerOptions trigger)
    {
        string every = trigger.Every.ToLower();
        var at = trigger.At;

        int hour = at?.Hour ?? 0;
        int minute = at?.Minute ?? 0;
        string cron;

        switch (every)
        {
            case "minute":
                cron = "0 * * * * ?"; // Every minute
                break;

            case "hour":
                cron = "0 0 * * * ?"; // Every hour
                break;

            case "day":
                cron = $"0 {minute} {hour} * * ?"; // Daily at specified time
                break;

            case "weekday":
                cron = $"0 {minute} {hour} ? * MON-FRI"; // Weekdays at specified time
                break;

            case "mon":
            case "tue":
            case "wed":
            case "thu":
            case "fri":
            case "sat":
            case "sun":
                cron = $"0 {minute} {hour} ? * {every.ToUpper()}"; // Specific day of the week
                break;

            default:
                cron = "0 * * * * ?"; // Default to every minute
                break;
        }

        return cron;
    }

    public void Create(string cronId, TriggerOptions trigger, Action callback)
    {
        try
        {
            string cronExpression = GenerateCronExpression(trigger);

            IJobDetail job = JobBuilder.Create<CronJob>()
                .WithIdentity(cronId)
                .UsingJobData("callback", callback.Method.Name) // Pass callback method name
                .Build();

            ITrigger cronTrigger = TriggerBuilder.Create()
                .WithIdentity(cronId)
                .WithCronSchedule(cronExpression)
                .Build();

            Scheduler.ScheduleJob(job, cronTrigger).Wait();
            CronList.TryAdd(cronId, job);
            Console.WriteLine($"Registered cron job {cronId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating cron job: {ex.Message}");
        }
    }

    public void Delete(string cronId)
    {
        try
        {
            if (CronList.TryRemove(cronId, out IJobDetail job))
            {
                Scheduler.DeleteJob(job.Key).Wait();
                Console.WriteLine($"Cron {cronId} has been deleted");
            }
            else
            {
                Console.WriteLine($"Cron id: {cronId} does not exist");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting cron job: {ex.Message}");
        }
    }
}

public class CronJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        // Execute the callback
        string callbackMethod = context.JobDetail.JobDataMap.GetString("callback");
        Console.WriteLine($"Executing job callback: {callbackMethod}");
        return Task.CompletedTask;
    }
}
