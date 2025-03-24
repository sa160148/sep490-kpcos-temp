using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Linq;
using System;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Request.Designs;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Request.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Quotations;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.DataAccessLayer.Enums;
using ContractRequest = KPCOS.BusinessLayer.DTOs.Request.Contracts.ContractRequest;
using KPCOS.BusinessLayer.DTOs.Response.Services;
using KPCOS.BusinessLayer.DTOs.Response.Equipments;
using KPCOS.BusinessLayer.DTOs.Response.Constructions;
using KPCOS.BusinessLayer.DTOs.Request.Constructions;
using KPCOS.Common.Utilities;
using KPCOS.BusinessLayer.DTOs.Response.ProjectIssues;
using KPCOS.BusinessLayer.DTOs.Response.Docs;
using KPCOS.BusinessLayer.DTOs.Response.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Request.MaintenancePackages;
using KPCOS.BusinessLayer.DTOs.Response.Maintenances;
using KPCOS.BusinessLayer.DTOs.Request.Maintenances;

namespace KPCOS.BusinessLayer.Helpers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<AuthRequest, User>();
        CreateMap<SignupRequest, User>();
        CreateMap<User, UserResponse>();
        CreateMap<Staff, GetAllStaffResponse>()
            .ForMember(dest => dest.Avatar, 
                opt => opt.MapFrom(src => src.User.Avatar))
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Position,
                opt => opt.MapFrom(src => src.Position))
                ;
        CreateMap<Staff, GetAllStaffForDesignResponse>()
            .ForMember(dest => dest.Avatar, 
                opt => opt.MapFrom(src => src.User.Avatar))
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Position,
                opt => opt.MapFrom(src => src.Position))
                ;
        CreateMap<Customer, GetAllStaffResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => RoleEnum.CUSTOMER.ToString()))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))
            ;

        CreateMap<ProjectRequest, Project>()
            .ForMember(dest => dest.Name,
                opt => 
                    opt.MapFrom(cust => cust.CustomerName + " project"))
            .ForMember(dest => dest.Note,
                opt => opt.MapFrom(cust => cust.Note ?? ""))
                ;
        CreateMap<Project, ProjectResponse>()
            .ForMember(dest => dest.Staff, 
                opt => opt.MapFrom(src => new List<StaffResponse>()));
        CreateMap<Project, ProjectForListResponse>()
            .ForMember(dest => dest.PackageName,
                opt => opt.MapFrom(project => project.Package.Name))
                .ForMember(dest => dest.Staffs,
                opt => opt.MapFrom(project => project.ProjectStaffs.Select(ps => ps.Staff)))
                ;
        CreateMap<Project, GetAllProjectForQuotationResponse>();
        CreateMap<Project, GetAllProjectForDesignResponse>();
        
        CreateMap<Staff, StaffResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId));

        CreateMap<Package, PackageResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PackageDetails))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => 
                Enumerable.Range(0, 5).Select(i => (int)(src.Price * Math.Pow(0.95, i))).ToList()));

        CreateMap<PackageDetail, PackageResponse.PackageItem>()
            .ForMember(dest => dest.IdPackageItem, opt => opt.MapFrom(src => src.PackageItemId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PackageItem.Name))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<Quotation, QuotationForProjectResponse>()
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.TemplateConstructionId, opt => opt.MapFrom(src => src.Idtemplate))
            .ForMember(dest => dest.Staffs,
                opt => opt.MapFrom(src => src.Project.ProjectStaffs.Select(ps => ps.Staff)))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => "Báo giá " + src.Version))
            .ForMember(dest => dest.Reason,
                opt => opt.MapFrom(src => src.Reason ?? string.Empty))
            ;

        CreateMap<Staff, GetAllStaffResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar));

        CreateMap<Quotation, QuotationResponse>()
            .ForMember(dest => dest.Services, 
            opt => opt.MapFrom(src => src.QuotationDetails))
            .ForMember(dest => dest.Equipments, 
            opt => opt.MapFrom(src => src.QuotationEquipments))
            .ForMember(dest => dest.TemplateConstructionId, 
            opt => opt.MapFrom(src => src.Idtemplate))
            .ForMember(dest => dest.Reason, 
            opt => opt.MapFrom(src => src.Reason ?? string.Empty))
            ;
        CreateMap<QuotationDetail, GetAllServiceResponse>()
            .ForMember(dest => dest.Id, 
            opt => opt.MapFrom(src => src.ServiceId))
            .ForMember(dest => dest.Name, 
            opt => opt.MapFrom(src => src.Service.Name))
            .ForMember(dest => dest.Description, 
            opt => opt.MapFrom(src => src.Service.Description))
            .ForMember(dest => dest.Price, 
            opt => opt.MapFrom(src => src.Service.Price))
            .ForMember(dest => dest.Unit,
            opt => opt.MapFrom(src => src.Service.Unit))
            .ForMember(dest => dest.Type,
            opt => opt.MapFrom(src => src.Service.Type))
        ;
        CreateMap<QuotationEquipment, GetAllEquipmentResponse>()
            .ForMember(dest => dest.Id, 
            opt => opt.MapFrom(src => src.EquipmentId))
            .ForMember(dest => dest.Name, 
            opt => opt.MapFrom(src => src.Equipment.Name))
            .ForMember(dest => dest.Description, 
            opt => opt.MapFrom(src => src.Equipment.Description))
            .ForMember(dest => dest.Price, 
            opt => opt.MapFrom(src => src.Price))
            ;

        CreateMap<ContractRequest, Contract>()
            .ForMember(dest => dest.Name, opt =>
                opt.MapFrom(src => src.Name ?? " "))
            .ForMember(dest => dest.Note, opt =>
                opt.MapFrom(src => src.Note ?? " "))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => EnumContractStatus.PROCESSING.ToString()))
            .ForMember(dest => dest.ContractValue, opt =>
                opt.MapFrom(src => src.ContractValue ?? 0)
            );
        CreateMap<Contract, GetAllContractResponse>();
        CreateMap<Contract, GetContractDetailResponse>();
        CreateMap<PaymentBatch, GetAllPaymentBatchesResponse>();

        CreateMap<CreateDesignRequest, Design>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EnumDesignStatus.OPENING.ToString()))
            .ForMember(dest => dest.DesignImages,
                opt => opt.MapFrom(src => 
                    src.DesignImages.Select(img => new DesignImage
                    {
                        ImageUrl = img.ImageUrl,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    })))
            ;
        CreateMap<UpdateDesignRequest, Design>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EnumDesignStatus.OPENING.ToString()))
            .ForMember(dest => dest.DesignImages,
                opt => opt.MapFrom(src => 
                    src.DesignImages.Select(url => new DesignImage
                    {
                        ImageUrl = url.ImageUrl,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    })))
            ;
        CreateMap<Design, GetAllDesignResponse>()
            .ForMember(dest => dest.ImageUrl, 
                opt =>
                    opt.MapFrom(src => src.DesignImages.FirstOrDefault()!.ImageUrl))
            .ForMember(dest => dest.Staffs,
                opt => opt.MapFrom(src => src.Project.ProjectStaffs.Select(ps => ps.Staff)))
            ;
        CreateMap<DesignImage, GetAllDesignImageResponse>()
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
        CreateMap<Design, GetDesignDetailResponse>()
           .ForMember(dest => dest.CustomerName, 
                opt => opt.MapFrom(src => src.Project.CustomerName))
            .ForMember(dest => dest.Reason, 
                opt => opt.MapFrom(src => src.Reason ?? ""))
            ;

        // Add mapping for Staff to GetAllStaffForDesignResponse
        CreateMap<Staff, GetAllStaffForDesignResponse>()
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position));

        CreateMap<ConstructionItem, GetAllConstructionItemResponse>()
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
        ;
        CreateMap<ConstructionItem, GetAllConstructionItemChildResponse>()
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
        ;
        CreateMap<ConstructionItem, GetConstructionItemDetailResponse>()
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
        ;
        CreateMap<ConstructionItem, GetConstructionItemParentDetailResponse>()
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
        .ForMember(dest => dest.ConstructionTasks, 
        opt => opt.MapFrom(src => src.ConstructionTasks ?? null))
        ;
        CreateMap<ConstructionItem, GetConstructionItemForTaskResponse>()
        ;

        CreateMap<ConstructionTask, GetAllConstructionTaskResponse>()
        .ForMember(dest => dest.Staff, opt => opt.MapFrom(src => src.Staff))
        ;

        CreateMap<ConstructionTask, GetConstructionTaskDetailResponse>()
        .ForMember(dest => dest.ImageUrl, 
        opt => opt.MapFrom(src => src.ImageUrl ?? string.Empty))
        .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason ?? string.Empty))
        ;

        // Mapping for construction task creation
        CreateMap<CreateConstructionTaskRequest, ConstructionTask>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EnumConstructionTaskStatus.OPENING.ToString()))
        .ForMember(dest => dest.DeadlineAt, opt => opt.MapFrom(src => 
            GlobalUtility.ConvertToSEATimeForPostgres(src.DeadlineAt)))
        ;

        // Add mapping for Transaction to GetPaymentDetailResponse
        // Transaction mappings for payment
        CreateMap<Transaction, GetTransactionDetailResponse>()
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer));

        // Add mappings for payment-related entities
        CreateMap<PaymentBatch, GetPaymentForTransactionResponse>()
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => (int)src.TotalValue))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.IsPaid))
            .ForMember(dest => dest.PaymentAt, opt => opt.MapFrom(src => src.PaymentAt));
            
        CreateMap<Contract, GetContractForPaymentBatchResponse>()
            .ForMember(dest => dest.ContractValue, opt => opt.MapFrom(src => src.ContractValue))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName));
            
        CreateMap<Project, GetProjectForTransactionResponse>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            
        CreateMap<Doc, GetDocResponse>();

        // Project Issue mappings
        CreateMap<CommandProjectIssueRequest, ProjectIssue>()
        ;
            
        CreateMap<ProjectIssue, GetAllProjectIssueResponse>()
            .ForMember(dest => dest.Staff, opt => opt.MapFrom(src => src.Staff))
            .ForMember(dest => dest.IssueType, opt => opt.MapFrom(src => src.IssueType.Name));
        
        CreateMap<IssueType, GetIssueTypeResponse>()
            ;
        
        // Direct mapping from User to GetAllStaffResponse for ProjectIssue
        CreateMap<User, GetAllStaffResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => 
                src.Staff.Any() ? src.Staff.FirstOrDefault().Position : RoleEnum.CUSTOMER.ToString()));
                
        // Document mappings
        CreateMap<Doc, GetAllDocResponse>()
            .ForMember(dest => dest.DocType, opt => opt.MapFrom(src => src.DocType))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.MinValue))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? DateTime.MinValue));
            
        CreateMap<DocType, GetDocTypeResponse>();

        // Maintenance item mappings
        CreateMap<MaintenanceItem, GetAllMaintenanceItemResponse>();
        
        CreateMap<CommandMaintenanceItemRequest, MaintenanceItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            ;
        
        // Maintenance package mappings
        CreateMap<CommandMaintenancePackageRequest, MaintenancePackage>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.HasValue ? src.Price.Value : 0))
            ;
        
        CreateMap<MaintenancePackage, GetAllMaintenancePackageResponse>()
            .ForMember(dest => dest.PriceList, 
                opt => opt.MapFrom(src => 
                    Enumerable.Range(0, 5).Select(i => (int)(src.Price * Math.Pow(0.95, i))).ToList()))
            .ForMember(dest => dest.MaintenanceItems,
                opt => opt.MapFrom(src => src.MaintenancePackageItems.Select(mpi => mpi.MaintenanceItem)))
            ;
            
        // Maintenance request mappings
        CreateMap<MaintenanceRequest, GetAllMaintenanceRequestResponse>()
            .ForMember(dest => dest.MaintenancePackage, 
                opt => opt.MapFrom(src => src.MaintenancePackage));
                
        // Transaction and maintenance mappings for payment
        CreateMap<MaintenanceRequest, GetMaintenanceRequestForTransactionResponse>();
        CreateMap<MaintenancePackage, GetMaintenancePackageForTransactionResponse>();
                
        // Add specific mapping from Customer to UserResponse
        CreateMap<Customer, UserResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => RoleEnum.CUSTOMER.ToString()));
    }
}