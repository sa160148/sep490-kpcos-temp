using System;

namespace KPCOS.DataAccessLayer.Entities;

public class MaintenanceTaskItem
{
    public Guid MaintenanceRequestTaskId { get; set; }
    public MaintenanceRequestTask MaintenanceRequestTask { get; set; }
    public Guid? MaintenanceItemId { get; set; }
    public MaintenanceItem MaintenanceItem { get; set; }
}