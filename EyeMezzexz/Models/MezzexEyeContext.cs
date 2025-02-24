using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EyeMezzexz.Models;

public partial class MezzexEyeContext : DbContext
{
    public MezzexEyeContext()
    {
    }

    public MezzexEyeContext(DbContextOptions<MezzexEyeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Computer> Computers { get; set; }

    public virtual DbSet<ComputerCategory> ComputerCategories { get; set; }

    public virtual DbSet<ComputerWiseTask> ComputerWiseTasks { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<OfficeLeave> OfficeLeaves { get; set; }

    public virtual DbSet<PermissionsName> PermissionsNames { get; set; }

    public virtual DbSet<ShiftAssigned> ShiftAssigneds { get; set; }

    public virtual DbSet<ShiftType> ShiftTypes { get; set; }

    public virtual DbSet<StaffAssignToTeam> StaffAssignToTeams { get; set; }

    public virtual DbSet<StaffInOut> StaffInOuts { get; set; }

    public virtual DbSet<StaffRotum> StaffRota { get; set; }

    public virtual DbSet<TaskAssigned> TaskAssigneds { get; set; }

    public virtual DbSet<TaskName> TaskNames { get; set; }

    public virtual DbSet<TaskTimer> TaskTimers { get; set; }

    public virtual DbSet<TaskUser> TaskUsers { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<UploadedDatum> UploadedData { get; set; }

    public virtual DbSet<UserAccountDetail> UserAccountDetails { get; set; }

    public virtual DbSet<UserLog> UserLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=KAUSAR_MEZZEX\\SQLEXPRESS;Database=MezzexEye;Trusted_Connection=True;TrustServerCertificate=Yes;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<PermissionsName>().WithMany().HasForeignKey("PermissionId"),
                    l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId");
                        j.ToTable("RolePermissions");
                        j.HasIndex(new[] { "PermissionId" }, "IX_RolePermissions_PermissionId");
                    });
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Computers).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserComputer",
                    r => r.HasOne<Computer>().WithMany().HasForeignKey("ComputerId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "ComputerId");
                        j.ToTable("UserComputers");
                        j.HasIndex(new[] { "ComputerId" }, "IX_UserComputers_ComputerId");
                    });

            entity.HasMany(d => d.Permissions).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserPermission",
                    r => r.HasOne<PermissionsName>().WithMany().HasForeignKey("PermissionId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "PermissionId");
                        j.ToTable("UserPermissions");
                        j.HasIndex(new[] { "PermissionId" }, "IX_UserPermissions_PermissionId");
                    });

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ComputerCategory>(entity =>
        {
            entity.HasKey(e => new { e.ComputerId, e.CategoryId });

            entity.HasOne(d => d.Computer).WithMany(p => p.ComputerCategories).HasForeignKey(d => d.ComputerId);
        });

        modelBuilder.Entity<ComputerWiseTask>(entity =>
        {
            entity.ToTable("ComputerWiseTask");

            entity.HasIndex(e => e.ComputerId, "IX_ComputerWiseTask_ComputerId");

            entity.HasIndex(e => e.TaskAssignmentId, "IX_ComputerWiseTask_TaskAssignmentId");

            entity.HasOne(d => d.Computer).WithMany(p => p.ComputerWiseTasks).HasForeignKey(d => d.ComputerId);

            entity.HasOne(d => d.TaskAssignment).WithMany(p => p.ComputerWiseTasks).HasForeignKey(d => d.TaskAssignmentId);
        });

        modelBuilder.Entity<OfficeLeave>(entity =>
        {
            entity.HasKey(e => e.LeaveId);

            entity.ToTable("OfficeLeave");

            entity.Property(e => e.CountryName).HasMaxLength(100);
            entity.Property(e => e.LeaveType).HasMaxLength(100);
        });

        modelBuilder.Entity<PermissionsName>(entity =>
        {
            entity.ToTable("PermissionsName");
        });

        modelBuilder.Entity<ShiftAssigned>(entity =>
        {
            entity.HasKey(e => e.AssignmentId);

            entity.ToTable("ShiftAssigned");

            entity.HasIndex(e => e.ShiftId, "IX_ShiftAssigned_ShiftId");

            entity.HasIndex(e => e.UserId, "IX_ShiftAssigned_UserId");

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);

            entity.HasOne(d => d.Shift).WithMany(p => p.ShiftAssigneds).HasForeignKey(d => d.ShiftId);

            entity.HasOne(d => d.User).WithMany(p => p.ShiftAssigneds).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ShiftType>(entity =>
        {
            entity.HasKey(e => e.ShiftId);

            entity.ToTable("ShiftType");

            entity.HasIndex(e => e.CountryId, "IX_ShiftType_CountryId");

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            entity.Property(e => e.ShiftName).HasMaxLength(50);

            entity.HasOne(d => d.Country).WithMany(p => p.ShiftTypes).HasForeignKey(d => d.CountryId);
        });

        modelBuilder.Entity<StaffAssignToTeam>(entity =>
        {
            entity.ToTable("StaffAssignToTeam");

            entity.HasIndex(e => e.CountryId, "IX_StaffAssignToTeam_CountryId");

            entity.HasIndex(e => e.TeamId, "IX_StaffAssignToTeam_TeamId");

            entity.HasIndex(e => e.UserId, "IX_StaffAssignToTeam_UserId");

            entity.HasOne(d => d.Country).WithMany(p => p.StaffAssignToTeams).HasForeignKey(d => d.CountryId);

            entity.HasOne(d => d.Team).WithMany(p => p.StaffAssignToTeams).HasForeignKey(d => d.TeamId);

            entity.HasOne(d => d.User).WithMany(p => p.StaffAssignToTeams).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<StaffInOut>(entity =>
        {
            entity.ToTable("StaffInOut");

            entity.HasIndex(e => e.UserId, "IX_StaffInOut_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.StaffInOuts).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<StaffRotum>(entity =>
        {
            entity.HasKey(e => e.RotaId);

            entity.HasIndex(e => e.ShiftId, "IX_StaffRota_ShiftId");

            entity.HasIndex(e => e.UserId, "IX_StaffRota_UserId");

            entity.Property(e => e.AvailableHours).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Shift).WithMany(p => p.StaffRota)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.User).WithMany(p => p.StaffRota).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<TaskAssigned>(entity =>
        {
            entity.ToTable("TaskAssigned");

            entity.HasIndex(e => e.TaskId, "IX_TaskAssigned_TaskId");

            entity.HasIndex(e => e.UserId, "IX_TaskAssigned_UserId");

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);

            entity.HasOne(d => d.Task).WithMany(p => p.TaskAssigneds).HasForeignKey(d => d.TaskId);

            entity.HasOne(d => d.User).WithMany(p => p.TaskAssigneds).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<TaskName>(entity =>
        {
            entity.HasIndex(e => e.CountryId, "IX_TaskNames_CountryId");

            entity.HasIndex(e => e.ParentTaskId, "IX_TaskNames_ParentTaskId");

            entity.HasOne(d => d.Country).WithMany(p => p.TaskNames)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ParentTask).WithMany(p => p.InverseParentTask).HasForeignKey(d => d.ParentTaskId);
        });

        modelBuilder.Entity<TaskTimer>(entity =>
        {
            entity.HasIndex(e => e.TaskId, "IX_TaskTimers_TaskId");

            entity.HasIndex(e => e.UserId, "IX_TaskTimers_UserId");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskTimers).HasForeignKey(d => d.TaskId);

            entity.HasOne(d => d.User).WithMany(p => p.TaskTimers).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<TaskUser>(entity =>
        {
            entity.HasIndex(e => e.TaskId, "IX_TaskUsers_TaskId");

            entity.HasIndex(e => e.UserId, "IX_TaskUsers_UserId");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskUsers).HasForeignKey(d => d.TaskId);

            entity.HasOne(d => d.User).WithMany(p => p.TaskUsers).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasIndex(e => e.CountryId, "IX_Teams_CountryId");

            entity.HasOne(d => d.Country).WithMany(p => p.Teams).HasForeignKey(d => d.CountryId);
        });

        modelBuilder.Entity<UploadedDatum>(entity =>
        {
            entity.HasIndex(e => e.TaskTimerId, "IX_UploadedData_TaskTimerId");

            entity.HasOne(d => d.TaskTimer).WithMany(p => p.UploadedData).HasForeignKey(d => d.TaskTimerId);
        });

        modelBuilder.Entity<UserAccountDetail>(entity =>
        {
            entity.HasKey(e => e.AccountDetailsId);

            entity.ToTable("UserAccountDetail");

            entity.HasIndex(e => e.UserId, "IX_UserAccountDetail_UserId");

            entity.Property(e => e.AgreementType).HasMaxLength(50);
            entity.Property(e => e.CashHoursRate).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.FixedSalaryAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Ninumber).HasColumnName("NINumber");
            entity.Property(e => e.PayBackAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PayrollHoursRate).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.UserAccountDetails).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<UserLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("UserLog");

            entity.Property(e => e.Url).HasColumnName("URL");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
