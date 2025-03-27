using System;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.Feedbacks;
using KPCOS.BusinessLayer.DTOs.Response.Feedbacks;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using KPCOS.Common.Exceptions;
using LinqKit;

namespace KPCOS.BusinessLayer.Services.Implements;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Tạo đánh giá mới cho dự án hoặc yêu cầu bảo trì
    /// </summary>
    /// <param name="request">Thông tin đánh giá bao gồm điểm đánh giá và mô tả tùy chọn</param>
    /// <param name="userId">ID người dùng của khách hàng tạo đánh giá</param>
    /// <returns>Task đại diện cho hoạt động bất đồng bộ</returns>
    /// <exception cref="ArgumentException">Ném ra khi thiếu điểm đánh giá hoặc không có ID tham chiếu</exception>
    /// <exception cref="InvalidOperationException">Ném ra khi đánh giá đã tồn tại cho dự án/yêu cầu bảo trì hoặc khi dự án/yêu cầu bảo trì chưa hoàn thành</exception>
    /// <exception cref="KeyNotFoundException">Ném ra khi không tìm thấy dự án hoặc yêu cầu bảo trì được tham chiếu</exception>
    public async Task CreateFeedbackAsync(CommandFeedbackRequest request, Guid userId)
    {
        if (request.Rating == null)
        {
            throw new BadRequestException("Điểm đánh giá là bắt buộc");
        }

        // Validate that the target (project or maintenance request) exists
        if (request.No == null)
        {
            throw new BadRequestException("ID tham chiếu (No) là bắt buộc");
        }

        var feedbackRepo = _unitOfWork.Repository<Feedback>();

        var customer = await _unitOfWork.Repository<Customer>()
            .SingleOrDefaultAsync(u => u.UserId == userId) ??
            throw new NotFoundException("Không tìm thấy khách hàng hợp lệ");

        // Check if feedback already exists for this target
        var existingFeedback = await feedbackRepo
            .FirstOrDefaultAsync(f => f.No == request.No && f.Type == request.Type && f.CustomerId == customer.Id);
        
        if (existingFeedback != null)
        {
            if (request.Type == EnumFeedbackType.PROJECT.ToString())
            {
                throw new BadRequestException("Đã tồn tại đánh giá cho dự án này");
            }
            if (request.Type == EnumFeedbackType.MAINTENANCE.ToString())
            {
                throw new BadRequestException("Đã tồn tại đánh giá cho yêu cầu bảo trì này");
            }
        }

        // Validate that the project or maintenance request is in DONE status
        if (request.Type == EnumFeedbackType.PROJECT.ToString())
        {
            var project = await _unitOfWork.Repository<Project>()
                .FirstOrDefaultAsync(p => p.Id == request.No);
            
            if (project == null)
            {
                throw new NotFoundException("Không tìm thấy dự án");
            }
            
            if (project.Status != EnumProjectStatus.FINISHED.ToString())
            {
                throw new BadRequestException("Chỉ có thể đánh giá dự án đã hoàn thành");
            }
            
            // Check if the project belongs to the customer
            if (project.CustomerId != customer.Id)
            {
                throw new BadRequestException("Bạn không có quyền đánh giá dự án này");
            }
        }
        else if (request.Type == EnumFeedbackType.MAINTENANCE.ToString())
        {
            var maintenance = await _unitOfWork.Repository<MaintenanceRequest>()
                .FirstOrDefaultAsync(m => m.Id == request.No);
                
            if (maintenance == null)
            {
                throw new NotFoundException("Không tìm thấy yêu cầu bảo trì");
            }
            
            if (maintenance.Status != EnumMaintenanceRequestStatus.DONE.ToString())
            {
                throw new BadRequestException("Chỉ có thể đánh giá yêu cầu bảo trì đã hoàn thành");
            }
            
            // Check if the maintenance request belongs to the customer
            if (maintenance.CustomerId != customer.Id)
            {
                throw new BadRequestException("Bạn không có quyền đánh giá yêu cầu bảo trì này");
            }
        }

        var feedback = _mapper.Map<Feedback>(request);
        feedback.CustomerId = customer.Id;

        // If name is null, generate it based on project or maintenance request
        if (string.IsNullOrEmpty(feedback.Name))
        {
            if (feedback.Type == EnumFeedbackType.PROJECT.ToString())
            {
                var project = await _unitOfWork.Repository<Project>()
                    .FirstOrDefaultAsync(p => p.Id == feedback.No);
                if (project != null)
                {
                    feedback.Name = $"Đánh giá {project.Name}";
                }
            }
            else if (feedback.Type == EnumFeedbackType.MAINTENANCE.ToString())
            {
                var maintenance = await _unitOfWork.Repository<MaintenanceRequest>()
                    .FirstOrDefaultAsync(m => m.Id == feedback.No);
                if (maintenance != null)
                {
                    feedback.Name = $"Đánh giá {maintenance.Name}";
                }
            }
        }

        await feedbackRepo.AddAsync(feedback, false);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Cập nhật đánh giá hiện có với thông tin mới
    /// </summary>
    /// <param name="id">ID của đánh giá cần cập nhật</param>
    /// <param name="request">Thông tin đánh giá mới</param>
    /// <param name="userId">ID người dùng của khách hàng cập nhật đánh giá</param>
    /// <returns>Task đại diện cho hoạt động bất đồng bộ</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy đánh giá hoặc khách hàng</exception>
    /// <exception cref="BadRequestException">Ném ra khi người dùng không có quyền cập nhật đánh giá này</exception>
    public async Task UpdateFeedbackAsync(Guid id, CommandFeedbackRequest request, Guid userId)
    {
        var feedbackRepo = _unitOfWork.Repository<Feedback>();
        
        // Get customer based on userId
        var customer = await _unitOfWork.Repository<Customer>()
            .SingleOrDefaultAsync(u => u.UserId == userId);
        
        if (customer == null)
        {
            throw new NotFoundException("Không tìm thấy khách hàng hợp lệ");
        }
        
        // Find the feedback
        var feedback = await feedbackRepo
            .FirstOrDefaultAsync(f => f.Id == id);
            
        if (feedback == null)
        {
            throw new NotFoundException("Không tìm thấy đánh giá");
        }
        
        // Verify that the feedback belongs to this customer
        if (feedback.CustomerId != customer.Id)
        {
            throw new BadRequestException("Bạn không có quyền cập nhật đánh giá này");
        }
        
        // If No property is in the request and it's different from current feedback.No, verify ownership
        if (request.No.HasValue && request.No != feedback.No)
        {
            // Validate that the new target exists and belongs to this customer
            if (request.Type == EnumFeedbackType.PROJECT.ToString())
            {
                var project = await _unitOfWork.Repository<Project>()
                    .FirstOrDefaultAsync(p => p.Id == request.No);
                    
                if (project == null)
                {
                    throw new NotFoundException("Không tìm thấy dự án");
                }
                
                // Verify project is complete
                if (project.Status != EnumProjectStatus.FINISHED.ToString())
                {
                    throw new BadRequestException("Chỉ có thể đánh giá dự án đã hoàn thành");
                }
                
                // Verify ownership
                if (project.CustomerId != customer.Id)
                {
                    throw new BadRequestException("Bạn không có quyền đánh giá dự án này");
                }
            }
            else if (request.Type == EnumFeedbackType.MAINTENANCE.ToString())
            {
                var maintenance = await _unitOfWork.Repository<MaintenanceRequest>()
                    .FirstOrDefaultAsync(m => m.Id == request.No);
                    
                if (maintenance == null)
                {
                    throw new NotFoundException("Không tìm thấy yêu cầu bảo trì");
                }
                
                // Verify maintenance is complete
                if (maintenance.Status != EnumMaintenanceRequestStatus.DONE.ToString())
                {
                    throw new BadRequestException("Chỉ có thể đánh giá yêu cầu bảo trì đã hoàn thành");
                }
                
                // Verify ownership
                if (maintenance.CustomerId != customer.Id)
                {
                    throw new BadRequestException("Bạn không có quyền đánh giá yêu cầu bảo trì này");
                }
            }
        }

        // Use reflection to update only non-null properties
        ReflectionUtil.UpdateProperties(request, feedback, new List<string> { "No", "Type" });
        
        await feedbackRepo.UpdateAsync(feedback, false);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Lấy danh sách đánh giá dựa trên tiêu chí lọc với phân trang
    /// </summary>
    /// <param name="request">Thông số lọc và phân trang</param>
    /// <param name="userId">ID người dùng để lọc đánh giá theo khách hàng (tùy chọn)</param>
    /// <returns>Tập hợp các phản hồi đánh giá với tổng số lượng</returns>
    /// <exception cref="NotFoundException">Ném ra khi không tìm thấy khách hàng với userId được cung cấp</exception>
    public async Task<(IEnumerable<GetAllFeedbackResponse> data, int total)> GetAllFeedbackAsync(
        GetAllFeedbackFilterRequest request, 
        Guid? userId = null)
    {
        // Using repository's Get method with filter, include, and pagination
        var includeProperties = "Customer";
        
        // Base expression from request filters
        var filterExpression = request.GetExpressions();
        
        // If userId is provided, filter feedback by customer
        if (userId.HasValue)
        {
            // Find the customer with the given userId
            var customer = await _unitOfWork.Repository<Customer>()
                .SingleOrDefaultAsync(c => c.UserId == userId.Value);
                
            if (customer == null)
            {
                throw new NotFoundException("Không tìm thấy khách hàng hợp lệ");
            }
            
            // Use LinqKit to combine the original filter with the customer filter
            var customerPredicate = PredicateBuilder.New<Feedback>(f => f.CustomerId == customer.Id);
            filterExpression = filterExpression.And(customerPredicate);
        }
        
        // Get data with count in one operation
        var (feedbacks, total) = _unitOfWork.Repository<Feedback>().GetWithCount(
            filter: filterExpression,
            includeProperties: includeProperties,
            pageIndex: request.PageNumber,
            pageSize: request.PageSize
        );

        // Map entities to response DTOs
        var response = _mapper.Map<IEnumerable<GetAllFeedbackResponse>>(feedbacks);

        // For each feedback, load related entities based on type
        foreach (var item in response)
        {
            var feedbackEntity = feedbacks.FirstOrDefault(f => f.Id == item.Id);
            if (feedbackEntity?.No != null)
            {
                if (feedbackEntity.Type == EnumFeedbackType.PROJECT.ToString())
                {
                    var project = await _unitOfWork.Repository<Project>()
                        .FirstOrDefaultAsync(p => p.Id == feedbackEntity.No);
                    
                    if (project != null)
                    {
                        item.Project = _mapper.Map<ProjectResponse>(project);
                    }
                }
                else if (feedbackEntity.Type == EnumFeedbackType.MAINTENANCE.ToString())
                {
                    var maintenance = await _unitOfWork.Repository<MaintenanceRequest>()
                        .FirstOrDefaultAsync(m => m.Id == feedbackEntity.No);
                    
                    if (maintenance != null)
                    {
                        item.MaintenanceRequest = _mapper.Map<GetMaintenanceRequestResponse>(maintenance);
                    }
                }
            }
        }

        return (response, total);
    }
}
