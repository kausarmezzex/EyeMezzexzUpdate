using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class UploadedDatum
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? VideoUrl { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Username { get; set; } = null!;

    public string SystemName { get; set; } = null!;

    public string? TaskName { get; set; }

    public int? TaskTimerId { get; set; }

    public string? ClientTimeZone { get; set; }

    public virtual TaskTimer? TaskTimer { get; set; }
}
