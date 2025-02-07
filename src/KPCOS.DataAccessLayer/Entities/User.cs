using System;
using System.Collections.Generic;

namespace KPCOS.DataAccessLayer;

public partial class User
{
    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsActive { get; set; }

    public string Fullname { get; set; } = null!;

    public DateOnly? Birthdate { get; set; }

    public string? Address { get; set; }

    public string? Gender { get; set; }

    public string? Avatar { get; set; }

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
