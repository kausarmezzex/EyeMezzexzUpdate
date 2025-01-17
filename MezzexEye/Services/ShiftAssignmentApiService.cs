using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MezzexEye.Services
{
    public class ShiftAssignmentApiService : IShiftAssignmentApiService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShiftAssignmentApiService> _logger;

        public ShiftAssignmentApiService(ApplicationDbContext context, ILogger<ShiftAssignmentApiService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Fetch all shift assignments
        public async Task<List<ShiftAssignment>> GetShiftAssignmentsAsync()
        {
            try
            {
                return await _context.ShiftAssignments
                                     .Include(sa => sa.Shift)
                                     .Include(sa => sa.User)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve shift assignments.");
                return new List<ShiftAssignment>();
            }
        }

        public async Task RemoveAssignmentsForUserInWeekAsync(int userId, DateTime weekStart, DateTime weekEnd)
        {
            var assignmentsToDelete = _context.ShiftAssignments
                .Where(a => a.UserId == userId && a.AssignedOn >= weekStart && a.AssignedOn <= weekEnd);

            _context.ShiftAssignments.RemoveRange(assignmentsToDelete);
            await _context.SaveChangesAsync();
        }

        // Fetch shift assignments for a specific week and country
        public async Task<List<ShiftAssignment>> GetShiftAssignmentsAsync(DateTime startOfWeek, DateTime endOfWeek, string selectedCountry)
        {
            try
            {
                return await _context.ShiftAssignments
                                     .Include(sa => sa.Shift)
                                     .Include(sa => sa.User)
                                     .Where(sa => EF.Functions.DateDiffDay(startOfWeek.Date, sa.AssignedOn.Date) >= 0 &&
                                                  EF.Functions.DateDiffDay(endOfWeek.Date, sa.AssignedOn.Date) <= 0 &&
                                                  sa.User.CountryName == selectedCountry)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve shift assignments for the specified week and country.");
                return new List<ShiftAssignment>();
            }
        }

        // Fetch shift assignment by ID
        public async Task<ShiftAssignment> GetShiftAssignmentByIdAsync(int assignmentId)
        {
            try
            {
                return await _context.ShiftAssignments
                                     .Include(sa => sa.Shift)
                                     .Include(sa => sa.User)
                                     .FirstOrDefaultAsync(sa => sa.AssignmentId == assignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve shift assignment with ID {assignmentId}.");
                return null;
            }
        }

        public async Task<bool> AddShiftAssignmentAsync(ShiftAssignment assignment)
        {
            try
            {
                if (assignment == null) return false;

                assignment.CreatedOn = DateTime.Now;

                // Check if the shift and user exist in the database
                bool shiftExists = await _context.Shifts.AnyAsync(s => s.ShiftId == assignment.ShiftId);
                bool userExists = await _context.Users.AnyAsync(u => u.Id == assignment.UserId);

                if (!shiftExists || !userExists) return false;

                // Directly add the shift assignment without checking for an existing assignment for the day
                _context.ShiftAssignments.Add(assignment);
                _logger.LogInformation("New shift assignment created for user {UserId} on {Date} for shift {ShiftId}", assignment.UserId, assignment.AssignedOn, assignment.ShiftId);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process shift assignment.");
                return false;
            }
        }


        // Update an existing shift assignment
        public async Task<bool> UpdateShiftAssignmentAsync(ShiftAssignment updatedAssignment)
        {
            try
            {
                var existingAssignment = await GetShiftAssignmentByIdAsync(updatedAssignment.AssignmentId);
                if (existingAssignment == null)
                {
                    _logger.LogError($"Shift assignment with ID {updatedAssignment.AssignmentId} does not exist.");
                    return false;
                }

                existingAssignment.ShiftId = updatedAssignment.ShiftId;
                existingAssignment.UserId = updatedAssignment.UserId;
                existingAssignment.AssignedOn = updatedAssignment.AssignedOn;
                existingAssignment.ModifiedBy = updatedAssignment.ModifiedBy;
                existingAssignment.ModifiedOn = DateTime.Now;

                _context.ShiftAssignments.Update(existingAssignment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Shift assignment successfully updated.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update shift assignment.");
                return false;
            }
        }

        // Delete a shift assignment by ID
        public async Task<bool> DeleteShiftAssignmentAsync(int assignmentId)
        {
            try
            {
                var existingAssignment = await GetShiftAssignmentByIdAsync(assignmentId);
                if (existingAssignment == null)
                {
                    _logger.LogError($"Shift assignment with ID {assignmentId} does not exist.");
                    return false;
                }

                _context.ShiftAssignments.Remove(existingAssignment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Shift assignment successfully deleted.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete shift assignment with ID {assignmentId}.");
                return false;
            }
        }
    }
}
