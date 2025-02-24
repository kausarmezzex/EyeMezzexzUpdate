using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class StaffRotum
{
    public int RotaId { get; set; }

    public int UserId { get; set; }

    public DateTime RotaDate { get; set; }

    public int? ShiftId { get; set; }

    public TimeOnly ShiftStartTime { get; set; }

    public TimeOnly ShiftEndTime { get; set; }

    public decimal AvailableHours { get; set; }

    public bool IsOff { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifyOn { get; set; }

    public string? ModifyBy { get; set; }

    public virtual ShiftType? Shift { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
