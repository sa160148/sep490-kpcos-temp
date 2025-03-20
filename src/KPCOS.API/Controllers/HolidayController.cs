using KPCOS.BusinessLayer.DTOs.Response.Holiday;
using KPCOS.WebFramework.Api;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using KPCOS.Common.Exceptions;

namespace KPCOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HolidayController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public HolidayController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<ApiResult<HolidayReponse[]>> GetHolidaysAsync()
    {
        string apiUrl = "https://api.11holidays.com/v1/holidays?country=VN";
        var response = await _httpClient.GetAsync(apiUrl);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var holidays = JsonConvert.DeserializeObject<HolidayReponse[]>(content);
            return Ok(holidays);
        }
        else
        {
            throw new BadRequestException("Error when get holidays", HttpStatusCode.InternalServerError);
        }
    
    }
}