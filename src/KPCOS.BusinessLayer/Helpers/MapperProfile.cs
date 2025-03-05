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
using KPCOS.BusinessLayer.DTOs.Response.Contracts;
using KPCOS.BusinessLayer.DTOs.Response.Designs;
using KPCOS.BusinessLayer.DTOs.Response.Payments;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Users;
using KPCOS.DataAccessLayer.Enums;
using ContractRequest = KPCOS.BusinessLayer.DTOs.Request.Contracts.ContractRequest;

namespace KPCOS.BusinessLayer.Helpers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<AuthRequest, User>();
        CreateMap<SignupRequest, User>();
        CreateMap<User, UserResponse>();
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
                opt => opt.MapFrom(src => src.Position));

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
                opt => opt.MapFrom(project => project.Package.Name));
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
            .ForMember(dest => dest.TemplateConstructionId, opt => opt.MapFrom(src => src.Idtemplate));

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
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => 1))
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
                opt => opt.MapFrom(src => 
                    src.DesignImages.FirstOrDefault()!.ImageUrl))
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
    }
}