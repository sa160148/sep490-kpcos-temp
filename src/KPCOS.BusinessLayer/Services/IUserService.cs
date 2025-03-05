using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.Common.Pagination;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.Services;

public interface IUserService
{
    Task RegiterStaffAsync(UserRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<IEnumerable<StaffResponse>> GetsStaffAsync(PaginationFilter filter);
    Task<int> CountStaffAsync();
    
    Task<(IEnumerable<StaffResponse> Data, int TotalRecords)> GetsConsultantAsync(PaginationFilter filter);
    
    Task<(IEnumerable<StaffResponse> data, int total)> GetsManagerAsync(PaginationFilter filter);
    Task<(IEnumerable<StaffResponse> data, int total)> GetsDesignerAsync(PaginationFilter filter);
    Task<(IEnumerable<StaffResponse> data, int total)> GetsConstructorAsync(PaginationFilter filter);
}