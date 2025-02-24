using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class ShiftAssigned
{
    public int AssignmentId { get; set; }

    public int ShiftId { get; set; }

    public int UserId { get; set; }

    public DateTime AssignedOn { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ShiftType Shift { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
