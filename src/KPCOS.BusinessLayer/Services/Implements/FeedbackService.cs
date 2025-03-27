using System;
using AutoMapper;
using KPCOS.DataAccessLayer.Repositories;

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
}
