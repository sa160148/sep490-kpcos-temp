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
    /// Lấy danh sách nhân viên
    /// </summary>
    /// <param name="filter">Bộ lọc phân trang</param>
    /// <returns>Danh sách nhân viên</returns>
    Task<IEnumerable<StaffResponse>> GetsStaffAsync(PaginationFilter filter);
    Task<int> CountStaffAsync();
    
    Task<(IEnumerable<StaffResponse> Data, int TotalRecords)> GetsConsultantAsync(PaginationFilter filter);
    
    Task<(IEnumerable<StaffResponse> data, int total)> GetsManagerAsync(PaginationFilter filter);
    Task<(IEnumerable<StaffResponse> data, int total)> GetsDesignerAsync(PaginationFilter filter);
    Task<(IEnumerable<StaffResponse> data, int total)> GetsConstructorAsync(PaginationFilter filter);

    /// <summary>
    /// Lấy thông tin chi tiết người dùng theo ID
    /// </summary>
    /// <param name="id">ID người dùng</param>
    /// <returns>Thông tin chi tiết người dùng</returns>
    Task<GetDetailUserResponse> GetUserByIdAsync(Guid id);

    /// <summary>
    /// Lấy danh sách tất cả người dùng
    /// </summary>
    /// <param name="filter">Bộ lọc phân trang</param>
    /// <returns>Danh sách người dùng</returns>
    Task<(IEnumerable<GetDetailUserResponse> Data, int TotalRecords)> GetAllUsersAsync(
        GetAllUserFilterRequest filter);

    /// <summary>
    /// Cập nhật thông tin người dùng
    /// </summary>
    /// <param name="id">ID người dùng</param>
    /// <param name="request">Thông tin cập nhật</param>
    Task UpdateUserAsync(Guid id, CommandUserRequest request);
}