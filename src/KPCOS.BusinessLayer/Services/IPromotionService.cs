using System;
using KPCOS.BusinessLayer.DTOs.Request.Promotions;
using KPCOS.BusinessLayer.DTOs.Response.Promotions;

namespace KPCOS.BusinessLayer.Services;

/// <summary>
/// Interface dịch vụ quản lý khuyến mãi trong hệ thống xây dựng và bảo trì hồ cá Koi
/// </summary>
public interface IPromotionService
{
    /// <summary>
    /// Lấy danh sách khuyến mãi phân trang theo điều kiện lọc
    /// </summary>
    /// <param name="filter">Thông số lọc và phân trang</param>
    /// <returns>Tuple chứa danh sách khuyến mãi và tổng số bản ghi</returns>
    Task<(IEnumerable<GetAllPromotionResponse> data, int total)> GetAllPromotions(GetAllPromotionFilterRequest filter);
    
    /// <summary>
    /// Lấy thông tin chi tiết khuyến mãi theo ID
    /// </summary>
    /// <param name="id">ID của khuyến mãi</param>
    /// <returns>Thông tin chi tiết khuyến mãi</returns>
    /// <exception cref="Exception">Ném ra khi không tìm thấy khuyến mãi hoặc khuyến mãi không hoạt động</exception>
    Task<GetAllPromotionResponse> GetPromotionById(Guid id);
    
    /// <summary>
    /// Tạo mới khuyến mãi với thông tin được cung cấp
    /// </summary>
    /// <param name="request">Yêu cầu tạo khuyến mãi chứa thông tin như tên, mã, phần trăm giảm giá, v.v.</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    /// <exception cref="Exception">Ném ra khi giảm giá không hợp lệ hoặc mã đã tồn tại</exception>
    Task CreatePromotion(CommandPromotionRequest request);
    
    /// <summary>
    /// Cập nhật khuyến mãi với thông tin mới
    /// </summary>
    /// <param name="id">ID của khuyến mãi cần cập nhật</param>
    /// <param name="request">Thông tin khuyến mãi đã cập nhật</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    /// <exception cref="Exception">Ném ra khi không tìm thấy khuyến mãi, giảm giá không hợp lệ, hoặc mã bị trùng</exception>
    Task UpdatePromotion(Guid id, CommandPromotionRequest request);
    
    /// <summary>
    /// Xóa khuyến mãi hoặc đánh dấu là không hoạt động nếu đang được sử dụng
    /// </summary>
    /// <param name="id">ID của khuyến mãi cần xóa</param>
    /// <returns>Task đại diện cho thao tác bất đồng bộ</returns>
    /// <exception cref="Exception">Ném ra khi không tìm thấy khuyến mãi</exception>
    Task DeletePromotion(Guid id);
}
