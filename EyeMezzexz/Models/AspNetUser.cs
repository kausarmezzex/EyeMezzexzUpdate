using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models;

public partial class AspNetUser
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public bool Active { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public DateTime? LastLogoutTime { get; set; }

    public string? CountryName { get; set; }

    public string? SystemName { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    public virtual ICollection<ShiftAssigned> ShiftAssigneds { get; set; } = new List<ShiftAssigned>();

    public virtual ICollection<StaffAssignToTeam> StaffAssignToTeams { get; set; } = new List<StaffAssignToTeam>();

    public virtual ICollection<StaffInOut> StaffInOuts { get; set; } = new List<StaffInOut>();

    public virtual ICollection<StaffRotum> StaffRota { get; set; } = new List<StaffRotum>();

    public virtual ICollection<TaskAssigned> TaskAssigneds { get; set; } = new List<TaskAssigned>();

    public virtual ICollection<TaskTimer> TaskTimers { get; set; } = new List<TaskTimer>();

    public virtual ICollection<TaskUser> TaskUsers { get; set; } = new List<TaskUser>();

    public virtual ICollection<UserAccountDetail> UserAccountDetails { get; set; } = new List<UserAccountDetail>();

    public virtual ICollection<Computer> Computers { get; set; } = new List<Computer>();

    public virtual ICollection<PermissionsName> Permissions { get; set; } = new List<PermissionsName>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
