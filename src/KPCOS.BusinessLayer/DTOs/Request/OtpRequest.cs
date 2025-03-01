using System.ComponentModel;

namespace KPCOS.BusinessLayer.DTOs.Request;

public class OtpRequest
{
    [DefaultValue("1000")]
    public string OtpCode { get; set; }
}