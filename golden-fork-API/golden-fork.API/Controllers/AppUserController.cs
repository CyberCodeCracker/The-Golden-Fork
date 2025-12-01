using AutoMapper;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace golden_fork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUserController : BaseController
    {

        private readonly IUnitOfWork _unitOfWork;   
        private readonly IConfiguration _mapper;
        public AppUserController(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


    }
}
