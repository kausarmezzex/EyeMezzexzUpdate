using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class PermissionsName
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();

    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
