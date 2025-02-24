using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class TaskName
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? TaskCreatedBy { get; set; }

    public DateTime? TaskCreatedOn { get; set; }

    public string? TaskModifiedBy { get; set; }

    public DateTime? TaskModifiedOn { get; set; }

    public int? CountryId { get; set; }

    public int? ParentTaskId { get; set; }

    public bool? ComputerRequired { get; set; }

    public bool? IsDeleted { get; set; }

    public int? TargetQuantity { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<TaskName> InverseParentTask { get; set; } = new List<TaskName>();

    public virtual TaskName? ParentTask { get; set; }

    public virtual ICollection<TaskAssigned> TaskAssigneds { get; set; } = new List<TaskAssigned>();

    public virtual ICollection<TaskTimer> TaskTimers { get; set; } = new List<TaskTimer>();

    public virtual ICollection<TaskUser> TaskUsers { get; set; } = new List<TaskUser>();
}
