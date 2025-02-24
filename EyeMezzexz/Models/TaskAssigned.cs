using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class TaskAssigned
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TaskId { get; set; }

    public TimeOnly? AssignedDuration { get; set; }

    public int? TargetQuantity { get; set; }

    public string? Country { get; set; }

    public DateTime AssignedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<ComputerWiseTask> ComputerWiseTasks { get; set; } = new List<ComputerWiseTask>();

    public virtual TaskName Task { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
