using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EyeMezzexz.Models;

namespace MezzexEye.Services
{
    public interface IShiftAssignmentApiService
    {
        Task<List<ShiftAssignment>> GetShiftAssignmentsAsync(); // Fetch all shift assignments
        Task<List<ShiftAssignment>> GetShiftAssignmentsAsync(DateTime startOfWeek, DateTime endOfWeek, string selectedCountry); // Fetch shift assignments by week and country

        Task<ShiftAssignment> GetShiftAssignmentByIdAsync(int assignmentId); // Fetch assignment by ID
        Task<bool> AddShiftAssignmentAsync(ShiftAssignment assignment); // Create a new shift assignment
        Task<bool> UpdateShiftAssignmentAsync(ShiftAssignment updatedAssignment); // Update assignment
        Task<bool> DeleteShiftAssignmentAsync(int assignmentId); // Delete assignment
        Task RemoveAssignmentsForUserInWeekAsync(int userId, DateTime weekStart, DateTime weekEnd);
    }
}
