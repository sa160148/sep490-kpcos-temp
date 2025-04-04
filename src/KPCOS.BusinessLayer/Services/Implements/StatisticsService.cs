using System;
using System.Linq;
using System.Threading.Tasks;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.BusinessLayer.DTOs.Response.Statistics;
using KPCOS.BusinessLayer.DTOs.Request.Statistics;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Repositories;
using KPCOS.DataAccessLayer.Enums;
using System.Linq.Expressions;

namespace KPCOS.BusinessLayer.Services.Implements;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public StatisticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetUserStatisticResponse> GetUserStatisticsAsync()
    {
        // Get all users and their status
        var userRepo = _unitOfWork.Repository<User>();
        var (users, totalUsers) = userRepo.GetWithCount();
        var inactiveUsers = users.Count(u => u.IsActive == false);

        // Get all customers
        var customerRepo = _unitOfWork.Repository<Customer>();
        var (customers, totalCustomers) = customerRepo.GetWithCount();

        // Get all staff with their project assignments
        var staffRepo = _unitOfWork.Repository<Staff>();
        var (staffUsers, totalStaff) = staffRepo.GetWithCount(
            includeProperties: "ProjectStaffs.Project,MaintenanceStaffs.MaintenanceRequestTask"
        );

        // Get all non-finished projects
        var projectRepo = _unitOfWork.Repository<Project>();
        var (activeProjects, _) = projectRepo.GetWithCount(
            filter: p => p.Status != "FINISHED",
            includeProperties: "ProjectStaffs"
        );

        // Get total transaction count
        var transactionRepo = _unitOfWork.Repository<Transaction>();
        var totalTransactions = transactionRepo.Count();

        // Calculate idle staff (staff not assigned to any active project or maintenance task)
        var assignedStaffIds = activeProjects
            .SelectMany(p => p.ProjectStaffs)
            .Select(ps => ps.StaffId)
            .Distinct()
            .ToList();

        var idleStaffCount = staffUsers.Count(s => 
            !assignedStaffIds.Contains(s.Id) && 
            !s.MaintenanceStaffs.Any(ms => 
                ms.MaintenanceRequestTask.Status != "DONE" && 
                ms.MaintenanceRequestTask.ParentId == null
            )
        );

        return new GetUserStatisticResponse
        {
            TotalUser = totalUsers,
            TotalInactiveUser = inactiveUsers,
            ToltalCustomer = totalCustomers,
            TotalCustomerTransaction = totalTransactions,
            TotalStaff = totalStaff,
            TotalIdleStaff = idleStaffCount
        };
    }

    public async Task<(IEnumerable<GetStatisticsResponse> data, int totalRecords)> GetStatisticsAsync(
        GetStatisticFilterRequest request)
    {
        // Get transactions based on filter
        var transactionRepo = _unitOfWork.Repository<Transaction>();
        var (transactions, totalCount) = transactionRepo.GetWithCount(
            filter: request.GetExpressions(),
            orderBy: request.GetOrder(),
            includeProperties: "Customer",
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );

        // Get payment batches and maintenance requests for transaction mapping
        var paymentBatchRepo = _unitOfWork.Repository<PaymentBatch>();
        var maintenanceRepo = _unitOfWork.Repository<MaintenanceRequest>();

        var paymentBatches = paymentBatchRepo.Get(includeProperties: "Contract").ToList();
        var maintenanceRequests = maintenanceRepo.Get().ToList();

        // Get current year and month
        var today = DateTime.Now;
        int currentYear = today.Year;
        int currentMonth = today.Month;

        // Generate list of years to show
        var years = string.IsNullOrEmpty(request.Year) 
            ? Enumerable.Range(0, request.PageSize)
                .Select(i => currentYear - i)
                .OrderBy(y => y)
                .ToList()
            : request.Year.Split(',')
                .Select(int.Parse)
                .OrderBy(y => y)
                .ToList();

        // Group transactions by year
        var transactionsByYear = transactions
            .Where(t => t.CreatedAt.HasValue)
            .GroupBy(t => t.CreatedAt.Value.Year)
            .ToDictionary(g => g.Key, g => g.ToList());

        var response = new List<GetStatisticsResponse>();

        // Create response for each year
        foreach (var year in years)
        {
            var yearTransactions = transactionsByYear.GetValueOrDefault(year, new List<Transaction>());
            
            // Determine number of months to show
            int monthsToShow = year < currentYear ? 12 : currentMonth;

            var yearlyStats = new GetStatisticsResponse
            {
                Year = year.ToString(),
                Data = new List<GetStatisticDetailResponse>
                {
                    // Construction project transactions
                    new GetStatisticDetailResponse
                    {
                        Name = "Đơn xây dựng",
                        Data = Enumerable.Range(1, monthsToShow)
                            .Select(month => yearTransactions
                                .Where(t => t.CreatedAt.Value.Month == month &&
                                          paymentBatches.Any(pb => pb.Id == t.No && pb.Contract != null))
                                .Sum(t => t.Amount))
                            .ToList()
                    },
                    // Maintenance request transactions
                    new GetStatisticDetailResponse
                    {
                        Name = "Đơn bảo trì/bảo dưỡng",
                        Data = Enumerable.Range(1, monthsToShow)
                            .Select(month => yearTransactions
                                .Where(t => t.CreatedAt.Value.Month == month &&
                                          maintenanceRequests.Any(mr => mr.Id == t.No))
                                .Sum(t => t.Amount))
                            .ToList()
                    }
                }
            };

            response.Add(yearlyStats);
        }

        // Update total count if we're generating default years
        if (string.IsNullOrEmpty(request.Year))
        {
            totalCount = currentYear - (currentYear - request.PageSize) + 1;
        }

        return (response, totalCount);
    }

    public async Task<(IEnumerable<GetStatisticsResponse> data, int totalRecords)> GetTotalTransactionStatisticsAsync(
        GetStatisticFilterRequest request)
    {
        // Get transactions based on filter
        var transactionRepo = _unitOfWork.Repository<Transaction>();
        var (transactions, totalCount) = transactionRepo.GetWithCount(
            filter: request.GetExpressions(),
            orderBy: request.GetOrder(),
            includeProperties: "Customer",
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );

        // Get payment batches and maintenance requests for transaction mapping
        var paymentBatchRepo = _unitOfWork.Repository<PaymentBatch>();
        var maintenanceRepo = _unitOfWork.Repository<MaintenanceRequest>();

        var paymentBatches = paymentBatchRepo.Get(includeProperties: "Contract").ToList();
        var maintenanceRequests = maintenanceRepo.Get().ToList();

        // Get current year and month
        var today = DateTime.Now;
        int currentYear = today.Year;
        int currentMonth = today.Month;

        // Generate list of years to show
        var years = string.IsNullOrEmpty(request.Year) 
            ? Enumerable.Range(0, request.PageSize)
                .Select(i => currentYear - i)
                .OrderBy(y => y)
                .ToList()
            : request.Year.Split(',')
                .Select(int.Parse)
                .OrderBy(y => y)
                .ToList();

        // Group transactions by year
        var transactionsByYear = transactions
            .Where(t => t.CreatedAt.HasValue)
            .GroupBy(t => t.CreatedAt.Value.Year)
            .ToDictionary(g => g.Key, g => g.ToList());

        var response = new List<GetStatisticsResponse>();

        // Create response for each year
        foreach (var year in years)
        {
            var yearTransactions = transactionsByYear.GetValueOrDefault(year, new List<Transaction>());
            
            // Determine number of months to show
            int monthsToShow = year < currentYear ? 12 : currentMonth;

            var yearlyStats = new GetStatisticsResponse
            {
                Year = year.ToString(),
                Data = new List<GetStatisticDetailResponse>
                {
                    // Total transactions (construction + maintenance)
                    new GetStatisticDetailResponse
                    {
                        Name = "Tổng",
                        Data = Enumerable.Range(1, monthsToShow)
                            .Select(month => yearTransactions
                                .Where(t => t.CreatedAt.Value.Month == month &&
                                          (paymentBatches.Any(pb => pb.Id == t.No && pb.Contract != null) ||
                                           maintenanceRequests.Any(mr => mr.Id == t.No)))
                                .Sum(t => t.Amount))
                            .ToList()
                    }
                }
            };

            response.Add(yearlyStats);
        }

        // Update total count if we're generating default years
        if (string.IsNullOrEmpty(request.Year))
        {
            totalCount = currentYear - (currentYear - request.PageSize) + 1;
        }

        return (response, totalCount);
    }

    public async Task<GetGrowthRateStatisticResponse> GetTransactionCountGrowthRateAsync()
    {
        var currentYear = DateTime.Now.Year;
        var lastYear = currentYear - 1;

        var transactionRepo = _unitOfWork.Repository<Transaction>();

        // Get transactions count for current and last year
        var currentYearCount = transactionRepo.Get(
            filter: t => t.CreatedAt.HasValue && t.CreatedAt.Value.Year == currentYear
        ).Count();

        var lastYearCount = transactionRepo.Get(
            filter: t => t.CreatedAt.HasValue && t.CreatedAt.Value.Year == lastYear
        ).Count();

        var response = new GetGrowthRateStatisticResponse
        {
            CurrentValue = currentYearCount,
            PreviousValue = lastYearCount,
            IsNewActivity = lastYearCount == 0 && currentYearCount > 0
        };

        // If both years have no transactions, return 0% growth
        if (currentYearCount == 0 && lastYearCount == 0)
        {
            response.GrowthRate = 0;
            return response;
        }

        // If last year had no transactions, growth rate is undefined
        if (lastYearCount == 0)
        {
            response.GrowthRate = null;
            return response;
        }

        // Calculate growth rate
        response.GrowthRate = ((double)(currentYearCount - lastYearCount) / lastYearCount) * 100;
        return response;
    }

    public async Task<GetGrowthRateStatisticResponse> GetTransactionAmountGrowthRateAsync()
    {
        var currentYear = DateTime.Now.Year;
        var lastYear = currentYear - 1;

        var transactionRepo = _unitOfWork.Repository<Transaction>();

        // Get total amount for current and last year
        var currentYearAmount = transactionRepo.Get(
            filter: t => t.CreatedAt.HasValue && t.CreatedAt.Value.Year == currentYear
        ).Sum(t => t.Amount);

        var lastYearAmount = transactionRepo.Get(
            filter: t => t.CreatedAt.HasValue && t.CreatedAt.Value.Year == lastYear
        ).Sum(t => t.Amount);

        var response = new GetGrowthRateStatisticResponse
        {
            CurrentValue = currentYearAmount,
            PreviousValue = lastYearAmount,
            IsNewActivity = lastYearAmount == 0 && currentYearAmount > 0
        };

        // If both years have no transactions, return 0% growth
        if (currentYearAmount == 0 && lastYearAmount == 0)
        {
            response.GrowthRate = 0;
            return response;
        }

        // If last year had no transactions, growth rate is undefined
        if (lastYearAmount == 0)
        {
            response.GrowthRate = null;
            return response;
        }

        // Calculate growth rate
        response.GrowthRate = ((double)(currentYearAmount - lastYearAmount) / lastYearAmount) * 100;
        return response;
    }
}
