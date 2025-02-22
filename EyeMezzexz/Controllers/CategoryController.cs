using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly WebServiceClient _webServiceClient;
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _webServiceClient = new WebServiceClient();
            _context = context;
        }

        [HttpGet("GetCategoryForm/{userId}")]
        public async Task<IActionResult> GetCategoryForm(int userId)
        {
            var computers = await _context.Computers.ToListAsync();
            var computerCategories = await _context.ComputerCategories.ToListAsync();

            // Fetch categories from WebService
            var categoriesJson = await _webServiceClient.GetCategoryALLAsync();
            var categoryList = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);

            // Fetch orders per category from WebService
            var ordersJson = await _webServiceClient.GetNoOfOrderAccordingCategoryAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(ordersJson);

            // Filter task assignments for the specific user
            var taskAssignments = await _context.TaskAssigned
                .Include(t => t.User)
                .Where(t => t.User.Id == userId)
                .ToListAsync();

            var taskAssignmentComputers = await _context.ComputerWiseTask
                .Where(tac => taskAssignments.Select(ta => ta.Id).Contains(tac.TaskAssignmentId))
                .ToListAsync();

            var computerViewModels = computers
                .Select(computer =>
                {
                    var assignedCategories = computerCategories
                        .Where(cc => cc.ComputerId == computer.Id)
                        .Select(cc => cc.CategoryId)
                        .ToList();

                    int totalOrders = orders
                        .Where(order => assignedCategories.Contains(order.CategoryID))
                        .Sum(order => order.TotalOrder);

                    var categoryNames = categoryList
                        .Where(c => assignedCategories.Contains(c.CategoryID))
                        .Select(c => c.Name)
                        .ToList();

                    // Check if the computer is assigned to the user
                    var isAssignedToUser = taskAssignmentComputers
                        .Any(tac => tac.ComputerId == computer.Id);

                    // Only include computers that have assigned categories, at least one order, and are assigned to the user
                    if (assignedCategories.Any() && totalOrders > 0 && isAssignedToUser)
                    {
                        return new ComputerViewModel
                        {
                            ComputerId = computer.Id,
                            ComputerName = computer.Name,
                            TotalOrders = totalOrders,
                            CategoryNames = categoryNames
                        };
                    }
                    return null; // Exclude computers that don't meet the condition
                })
                .Where(computer => computer != null) // Remove null entries
                .ToList();

            var viewModel = new CategoryViewModel
            {
                Computers = computerViewModels,
                Categories = categoryList
            };

            return Ok(viewModel);
        }

        [HttpGet("GetComputerCategoryAssignments/{userId}")]
        public async Task<IActionResult> GetComputerCategoryAssignments(int userId)
        {
            try
            {
                // Get current date (only date part, ignoring time)
                var today = DateTime.UtcNow.Date;

                // Fetch all computers and categories
                var computers = await _context.Computers.ToListAsync();
                var computerCategories = await _context.ComputerCategories.ToListAsync();

                // Fetch categories and orders from the web service
                var categoriesJson = await _webServiceClient.GetCategoryALLAsync();
                var categoryList = JsonConvert.DeserializeObject<List<Category>>(categoriesJson) ?? new List<Category>();

                var ordersJson = await _webServiceClient.GetNoOfOrderAccordingCategoryAsync();
                var orders = JsonConvert.DeserializeObject<List<Order>>(ordersJson) ?? new List<Order>();

                // Fetch user task assignments only for today's date
                var userTaskAssignments = await _context.TaskAssigned
                    .Include(ta => ta.User)
                    .Include(ta => ta.ComputerWiseTask)
                        .ThenInclude(tac => tac.Computer)
                    .Where(ta => ta.User.Id == userId && ta.AssignedDate.Date == today) // Filter today's data
                    .ToListAsync();

                // Get computers assigned to the user
                var userComputers = userTaskAssignments
                    .SelectMany(ta => ta.ComputerWiseTask)
                    .Select(tac => tac.Computer)
                    .Distinct()
                    .ToList();

                var result = new List<object>();
                foreach (var computer in userComputers)
                {
                    // Get assigned categories for this computer
                    var assignedCategoryIds = computerCategories
                        .Where(cc => cc.ComputerId == computer.Id)
                        .Select(cc => cc.CategoryId)
                        .ToList();

                    // Fetch category details with order count and order status
                    var categoryDetails = categoryList
                        .Where(c => assignedCategoryIds.Contains(c.CategoryID))
                        .Select(c => new
                        {
                            name = c.Name,
                            totalOrders = orders
                                .Where(o => o.CategoryID == c.CategoryID)
                                .Sum(o => o.TotalOrder),
                            orderStatusDetails = orders
                                .Where(o => o.CategoryID == c.CategoryID)
                                .GroupBy(o => o.OrderStatus)
                                .Select(g => new
                                {
                                    status = g.Key,
                                    count = g.Sum(o => o.TotalOrder)
                                })
                                .ToList()
                        })
                        .ToList();

                    // Calculate total order status details for the computer across all assigned categories
                    var computerOrderStatusDetails = orders
                        .Where(o => assignedCategoryIds.Contains(o.CategoryID))
                        .GroupBy(o => o.OrderStatus)
                        .Select(g => new
                        {
                            status = g.Key,
                            count = g.Sum(o => o.TotalOrder)
                        })
                        .ToList();

                    // Find all users assigned to this computer (including the current user) for today's date
                    var allTaskAssignmentsForComputer = await _context.ComputerWiseTask
                        .Include(tac => tac.TaskAssignment)
                            .ThenInclude(ta => ta.User)
                        .Where(tac => tac.ComputerId == computer.Id && tac.TaskAssignment.AssignedDate.Date == today) // Filter for today's date
                        .ToListAsync();

                    // Get partner users (excluding the current user) for today's date
                    var partnerUsers = allTaskAssignmentsForComputer
                        .Where(tac => tac.TaskAssignment.User.Id != userId)
                        .Select(tac => tac.TaskAssignment.User.FirstName + " " + tac.TaskAssignment.User.LastName)
                        .Distinct()
                        .ToList();

                    // Add the result for this computer
                    result.Add(new
                    {
                        computerName = computer.Name,
                        categories = categoryDetails,
                        computerOrderStatusDetails = computerOrderStatusDetails, // Add computer-level order status details
                        assignedUser = $"{userTaskAssignments.First().User.FirstName} {userTaskAssignments.First().User.LastName}",
                        partnerUsers = partnerUsers // List of partner users for today's date
                    });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class CategoryViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Order> Orders { get; set; }
        public List<ComputerViewModel> Computers { get; set; }
    }

    public class AssignedCategoryViewModel
    {
        public string Name { get; set; }
        public int TotalOrders { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
    }

    public class ComputerViewModel
    {
        public int ComputerId { get; set; }
        public string ComputerName { get; set; }
        public int TotalOrders { get; set; }
        public List<string> CategoryNames { get; set; }
        public List<AssignedCategoryViewModel> AssignedCategories { get; set; } = new List<AssignedCategoryViewModel>();
        public string AssignedUser { get; set; }
        public List<OrderStatusDetail> OrderStatusDetails { get; set; } // New property for order status details
    }
    public class OrderStatusDetail
    {
        public string Status { get; set; }
        public int TotalOrders { get; set; }
    }
    public class Order
    {
        public int TotalOrder { get; set; }
        public string OrderStatus { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string OrderType { get; set; }
    }
}