using System;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;

namespace KPCOS.BusinessLayer.Services;

/// <summary>
/// Dịch vụ quản lý vấn đề dự án hồ Koi
/// </summary>
public interface IProjectIssueService
{
    /// <summary>
    /// Tạo mới một vấn đề cho dự án hồ Koi
    /// </summary>
    /// <param name="constructionItemId">ID của hạng mục xây dựng liên quan đến vấn đề</param>
    /// <param name="request">Thông tin chi tiết của vấn đề cần tạo</param>
    /// <param name="userId">ID của người dùng tạo vấn đề</param>
    /// <returns>Task hoàn thành khi tạo vấn đề thành công</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy hạng mục xây dựng với ID được cung cấp</exception>
    /// <exception cref="BadRequestException">Ném ra khi không tìm thấy hạng mục xây dựng cấp 1, tên vấn đề trống, ID loại vấn đề không được cung cấp, hoặc vấn đề trùng tên đã tồn tại</exception>
    Task CreateProjectIssueAsync(Guid constructionItemId, CommandProjectIssueRequest request, Guid userId);
    
    /// <summary>
    /// Cập nhật thông tin vấn đề dự án hồ Koi
    /// </summary>
    /// <param name="id">ID của vấn đề cần cập nhật</param>
    /// <param name="request">Thông tin cập nhật cho vấn đề</param>
    /// <returns>Task hoàn thành khi cập nhật vấn đề thành công</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy vấn đề với ID được cung cấp</exception>
    /// <exception cref="BadRequestException">Ném ra khi thiếu giải pháp khi đánh dấu đã giải quyết, URL hình ảnh trống, hoặc vấn đề trùng tên đã tồn tại</exception>
    /// <remarks>
    /// Trạng thái vấn đề được cập nhật tự động dựa trên các hành động:
    /// - Khi đánh dấu là đã giải quyết (isSolved=true), trạng thái chuyển thành "SOLVED"
    /// - Khi cung cấp giải pháp và trạng thái hiện tại là "OPENING", trạng thái chuyển thành "PROCESSING"
    /// </remarks>
    Task UpdateProjectIssueAsync(Guid id, CommandProjectIssueRequest request);
    
    /// <summary>
    /// Xóa hình ảnh của vấn đề dự án hồ Koi
    /// </summary>
    /// <param name="id">ID của hình ảnh cần xóa</param>
    /// <returns>Task hoàn thành khi xóa hình ảnh thành công</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy hình ảnh với ID được cung cấp</exception>
    Task DeleteIssueImageAsync(Guid id);
}
