using System;
using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request.Docs;
using KPCOS.BusinessLayer.DTOs.Response.Docs;
using KPCOS.Common.Exceptions;
using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using KPCOS.DataAccessLayer.Enums;
using KPCOS.DataAccessLayer.Repositories;

namespace KPCOS.BusinessLayer.Services.Implements;

public class DocService : IDocService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IFirebaseService _firebaseService;
    private readonly IBackgroundService _backgroundService;

    public DocService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IFirebaseService firebaseService, IBackgroundService backgroundService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _firebaseService = firebaseService;
        _backgroundService = backgroundService;
    }
    
    /// <summary>
    /// Creates a new document for a project
    /// </summary>
    /// <param name="request">Document data</param>
    /// <returns>Task representing the operation</returns>
    public async Task CreateDocAsync(CommandDocRequest request)
    {
        if (!request.ProjectId.HasValue)
        {
            throw new BadRequestException("ProjectId is required");
        }
        
        // Validate project exists
        var project = await ValidateAndGetProject(request.ProjectId.Value);
        
        // Validate DocType exists
        if (!request.DocTypeId.HasValue)
        {
            throw new BadRequestException("DocTypeId is required");
        }
        
        var docType = await _unitOfWork.Repository<DocType>().FindAsync(request.DocTypeId.Value);
        if (docType == null)
        {
            throw new NotFoundException($"DocType with ID {request.DocTypeId} not found");
        }
        
        // Validate document name is unique within project
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Document name is required");
        }
        
        // Check for duplicate name in the same project
        var existingDoc = await _unitOfWork.Repository<Doc>()
            .FirstOrDefaultAsync(d => d.ProjectId == request.ProjectId.Value && 
                                       d.Name == request.Name && 
                                       d.IsActive == true);
        
        if (existingDoc != null)
        {
            throw new BadRequestException($"Document with name '{request.Name}' already exists in this project");
        }
        
        // Validate URL is provided
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new BadRequestException("Document URL is required");
        }
        
        // Create and save the document
        var doc = new Doc
        {
            Name = request.Name,
            Url = request.Url,
            DocTypeId = request.DocTypeId.Value,
            ProjectId = request.ProjectId.Value,
            Status = EnumDocStatus.PROCESSING.ToString()
        };
        
        await _unitOfWork.Repository<Doc>().AddAsync(doc);
    }
    
    /// <summary>
    /// Updates a document for a project
    /// </summary>
    /// <param name="projectId">Project ID the document belongs to</param>
    /// <param name="docId">Document ID to update</param>
    /// <param name="request">Updated document data</param>
    /// <returns>Task representing the operation</returns>
    public async Task UpdateDocAsync(Guid docId, CommandDocRequest request)
    {
        // Validate project exists
        var project = await ValidateAndGetProject(request.ProjectId.Value);
        
        // Find the document
        var doc = await _unitOfWork.Repository<Doc>().FirstOrDefaultAsync(d => 
            d.Id == docId && 
            d.ProjectId == request.ProjectId.Value && 
            d.IsActive == true);
        
        if (doc == null)
        {
            throw new NotFoundException($"Document with ID {docId} not found in project {request.ProjectId.Value}");
        }
        
        // Check if we need to validate DocType
        if (request.DocTypeId.HasValue && request.DocTypeId.Value != doc.DocTypeId)
        {
            var docType = await _unitOfWork.Repository<DocType>().FindAsync(request.DocTypeId.Value);
            if (docType == null)
            {
                throw new NotFoundException($"DocType with ID {request.DocTypeId} not found");
            }
            
            doc.DocTypeId = request.DocTypeId.Value;
        }
        
        // Check if name is being updated and needs validation
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != doc.Name)
        {
            // Check for duplicate name in the same project
            var existingDoc = await _unitOfWork.Repository<Doc>()
                .FirstOrDefaultAsync(d => d.ProjectId == request.ProjectId.Value && 
                                           d.Name == request.Name && 
                                           d.Id != docId &&
                                           d.IsActive == true);
            
            if (existingDoc != null)
            {
                throw new BadRequestException($"Document with name '{request.Name}' already exists in this project");
            }
            
            doc.Name = request.Name;
        }
        
        // Update URL if provided
        if (!string.IsNullOrWhiteSpace(request.Url))
        {
            doc.Url = request.Url;
        }
        
        // Save changes
        await _unitOfWork.Repository<Doc>().UpdateAsync(doc);
    }

    /// <summary>
    /// Initiates the document acceptance process by generating an OTP
    /// </summary>
    /// <param name="docId">Document ID to accept</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task AcceptDocAsync(Guid docId)
    {
        // Find the document
        var docRepo = _unitOfWork.Repository<Doc>();
        var doc = await docRepo.FindAsync(docId);
        if (doc == null || doc.IsActive != true)
        {
            throw new NotFoundException("Tài liệu không tồn tại");
        }
        
        // Check if document is in PROCESSING status
        if (doc.Status != EnumDocStatus.PROCESSING.ToString())
        {
            throw new BadRequestException("Tài liệu này không ở trạng thái chờ xử lý");
        }
        
        // Get the project
        var project = await ValidateAndGetProject(doc.ProjectId);
        
        // Get the project's customer
        var customer = await _unitOfWork.Repository<Customer>().FirstOrDefaultAsync(c => c.Id == project.CustomerId);
        if (customer == null)
        {
            throw new NotFoundException("Không tìm thấy thông tin khách hàng cho dự án này");
        }
        
        // Get user associated with the customer
        var user = await _unitOfWork.Repository<User>().FindAsync(customer.UserId);
        if (user == null)
        {
            throw new NotFoundException("Không tìm thấy thông tin người dùng cho dự án này");
        }
        
        // Generate OTP code
        int otpCode = new Random().Next(1000, 9999);
        
        // Create OTP response
        var otpResponse = new DocOtpResponse
        {
            DocId = docId.ToString(),
            OtpCode = otpCode.ToString()
        };
        
        // Save OTP to Firebase
        await _firebaseService.SaveDocOtpAsync(otpResponse);
        
        // Send OTP to user's email
        await _emailService.SendVerifyDocOtpAsync(user.Email, doc.Name, otpCode, otpResponse.ExpiresAt);
        
        // Schedule job to delete OTP after expiration
        _backgroundService.DelayedCancelDocOtpJob(5, docId.ToString());
    }
    
    /// <summary>
    /// Verifies the document using the OTP received in email
    /// </summary>
    /// <param name="docId">Document ID to verify</param>
    /// <param name="otp">OTP code received in email</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task VerifyDocAsync(Guid docId, string otp)
    {
        // Get OTP from Firebase
        var otpVerify = await _firebaseService.GetDocOtpAsync(docId.ToString());
        if (otpVerify == null || otpVerify.OtpCode != otp)
        {
            throw new NotFoundException("Mã OTP không hợp lệ hoặc đã hết hạn");
        }
        
        // Find the document
        var docRepo = _unitOfWork.Repository<Doc>();
        var doc = await docRepo.FindAsync(docId);
        if (doc == null || doc.IsActive != true)
        {
            throw new NotFoundException("Tài liệu không tồn tại");
        }
        
        // Update OTP status in Firebase to mark it as used
        await _firebaseService.UpdateDocOtpAsync(docId.ToString());
        
        // Update document status to ACTIVE
        doc.Status = EnumDocStatus.ACTIVE.ToString();
        await docRepo.UpdateAsync(doc);
    }
    
    /// <summary>
    /// Invalidates document OTP in Firebase
    /// </summary>
    /// <param name="docId">Document ID</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvalidDocOtpAsync(string docId)
    {
        await _firebaseService.DeleteDocOtpAsync(docId);
    }
    
    private async Task<Project> ValidateAndGetProject(Guid projectId)
    {
        var project = await _unitOfWork.Repository<Project>().FindAsync(projectId);
        if (project == null)
        {
            throw new NotFoundException("Project không tồn tại");
        }
        return project;
    }
}