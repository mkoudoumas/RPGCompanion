using System;
using System.Collections.Generic;

namespace RPGCompanion.Server.Infrastructure.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
