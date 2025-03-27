using System;
using System.ComponentModel.DataAnnotations;
using KPCOS.DataAccessLayer.Enums;

namespace KPCOS.BusinessLayer.DTOs.Request.Feedbacks;

public class CommandFeedbackRequest
{
    public Guid? No { get; set; }
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [Range(0, 5, ErrorMessage = "Điểm đánh giá phải nằm trong khoảng từ 0 đến 5")]
    public int? Rating { get; set; }

    [EnumDataType(typeof(EnumFeedbackType), ErrorMessage = "Loại đánh giá không hợp lệ")]
    public string? Type { get; set; }
}
