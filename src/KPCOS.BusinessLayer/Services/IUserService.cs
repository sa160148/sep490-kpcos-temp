using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.Services;

public interface IUserService
{
    Task<bool> RegiterStaffAsync(UserRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<IEnumerable<StaffResponse>> GetsStaffAsync(PaginationFilter filter);
    Task<int> CountStaffAsync();
}