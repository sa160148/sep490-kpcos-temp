using System;

namespace KPCOS.DataAccessLayer.Entities;

public class MaintenanceStaff
{
    public Guid MaintenanceRequestTaskId { get; set; }
    public Guid StaffId { get; set; }

    public virtual MaintenanceRequestTask MaintenanceRequestTask { get; set; } = null!;
    public virtual Staff Staff { get; set; } = null!;
}
