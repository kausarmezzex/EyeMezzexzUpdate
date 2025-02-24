using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class AspNetRole
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? NormalizedName { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();

    public virtual ICollection<PermissionsName> Permissions { get; set; } = new List<PermissionsName>();

    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
