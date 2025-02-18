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

        CreateMap<ProjectRequest, Project>();
        CreateMap<Project, ProjectResponse>();
    }
}