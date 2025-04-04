using System;

namespace KPCOS.BusinessLayer.DTOs.Response.Users;

public class GetUserStatisticResponse
{
    /// <summary>
    /// Tổng số user
    /// </summary>
    public int TotalUser { get; set; }
    /// <summary>
    /// Tổng số user không hoạt động
    /// </summary>
    public int TotalInactiveUser { get; set; }
    /// <summary>
    /// Tổng số khách hàng
    /// </summary>
    public int ToltalCustomer { get; set; }
    /// <summary>
    /// Tổng số giao dịch của khách hàng
    /// </summary>
    public int TotalCustomerTransaction { get; set; }
    /// <summary>
    /// Tổng số nhân viên
    /// </summary>
    public int TotalStaff { get; set; }
    /// <summary>
    /// Tổng số nhân viên đang không có trong các dự án xây dựng hoặc dự án bảo trì đang hoạt động
    /// </summary>
    public int TotalIdleStaff { get; set; }
}
