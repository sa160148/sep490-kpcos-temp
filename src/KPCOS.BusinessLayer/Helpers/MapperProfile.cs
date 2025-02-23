using AutoMapper;
using KPCOS.BusinessLayer.DTOs.Request;
using KPCOS.BusinessLayer.DTOs.Response;
using KPCOS.DataAccessLayer.Entities;

namespace KPCOS.BusinessLayer.Helpers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<AuthRequest, User>();
        CreateMap<SignupRequest, User>();

        CreateMap<ProjectRequest, Project>()
            .ForMember(dest => dest.Name,
                opt => 
                    opt.MapFrom(cust => cust.CustomerName + " project"))
            .ForMember(dest => dest.Note,
                opt => opt.MapFrom(cust => cust.Note ?? ""))
                ;
        CreateMap<Project, ProjectResponse>();
        CreateMap<Project, ProjectForListResponse>()
            .ForMember(dest => dest.PackageName,
                opt => opt.MapFrom(project => project.Package.Name));
    }
}