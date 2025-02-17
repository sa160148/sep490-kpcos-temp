using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = "01234566789";

    public string? Avatar { get; set; }

    public string? Status { get; set; } = "YES";

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
