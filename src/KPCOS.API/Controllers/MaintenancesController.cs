using KPCOS.BusinessLayer.Services;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KPCOS.API.Controllers
{
    [Route("api/[controller]")]
    public class MaintenancesController : BaseController
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenancesController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        
    }
}
