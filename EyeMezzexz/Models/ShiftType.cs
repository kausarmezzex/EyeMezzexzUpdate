using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class ShiftType
{
    public int ShiftId { get; set; }

    public string ShiftName { get; set; } = null!;

    public TimeOnly FromTime { get; set; }

    public TimeOnly ToTime { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public int CountryId { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<ShiftAssigned> ShiftAssigneds { get; set; } = new List<ShiftAssigned>();

    public virtual ICollection<StaffRotum> StaffRota { get; set; } = new List<StaffRotum>();
}
