using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Linq;
using System;
using KPCOS.BusinessLayer.DTOs.Request.Contracts;
using KPCOS.BusinessLayer.DTOs.Request.Projects;
using KPCOS.BusinessLayer.DTOs.Response.Projects;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.BusinessLayer.Helpers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<AuthRequest, User>();
        CreateMap<SignupRequest, User>();
        CreateMap<User, UserResponse>();

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
    }
}