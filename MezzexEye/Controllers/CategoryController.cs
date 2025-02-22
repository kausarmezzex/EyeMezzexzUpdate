using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MezzexEye.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class CategoryController : Controller
    {
        private readonly WebServiceClient _webServiceClient;
        private readonly ApplicationDbContext context;

        public CategoryController(ApplicationDbContext context)
        {
            _webServiceClient = new WebServiceClient();
            this.context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategoryForm()
        {
            var computers = await context.Computers.ToListAsync();
            var computerCategories = await context.ComputerCategories.ToListAsync();
            ViewBag.Computers = computers;
            // Fetch categories from WebService
            var categoriesJson = await _webServiceClient.GetCategoryALLAsync();
            var categoryList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Category>>(categoriesJson);

            // Fetch orders per category from WebService
            var ordersJson = await _webServiceClient.GetNoOfOrderAccordingCategoryAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(ordersJson);

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

                    // Only include computers that have assigned categories AND at least one order
                    if (assignedCategories.Any() && totalOrders > 0)
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

            return PartialView("_CategoryForm", viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> GetComputerCategoryAssignments()
        {
            try
            {
                var computers = await context.Computers.ToListAsync();
                var computerCategories = await context.ComputerCategories.ToListAsync();
                var users = await context.Users.ToListAsync();
                var categoriesJson = await _webServiceClient.GetCategoryALLAsync();
                var categoryList = JsonConvert.DeserializeObject<List<Category>>(categoriesJson) ?? new List<Category>();
                var ordersJson = await _webServiceClient.GetNoOfOrderAccordingCategoryAsync();
                var orders = JsonConvert.DeserializeObject<List<Order>>(ordersJson) ?? new List<Order>();
                var taskAssignments = await context.TaskAssigned.Include(t => t.User).ToListAsync();
                var taskAssignmentComputers = await context.ComputerWiseTask.ToListAsync();

                var result = computers
                    .Select(computer =>
                    {
                        // 🛠 Assigned Categories IDs for this Computer
                        var assignedCategoryIds = computerCategories
                            .Where(cc => cc.ComputerId == computer.Id)
                            .Select(cc => cc.CategoryId)
                            .ToList();

                        // 🛠 Fetch Categories with Order Count
                        var categoryDetails = categoryList
                            .Where(c => assignedCategoryIds.Contains(c.CategoryID))
                            .Select(c => new
                            {
                                name = c.Name,
                                totalOrders = orders
                                    .Where(o => o.CategoryID == c.CategoryID)
                                    .Sum(o => o.TotalOrder)
                            })
                            .ToList();

                        // 🛠 Find Assigned User from TaskAssignments Table
                        var assignedUser = (
                            from ta in taskAssignments
                            join tac in taskAssignmentComputers on ta.Id equals tac.TaskAssignmentId
                            where tac.ComputerId == computer.Id
                            select ta.User.FirstName + " " + ta.User.LastName
                        ).FirstOrDefault() ?? "Unassigned"; // Default if no user found

                        // 🛠 Only return computers with assigned categories
                        if (categoryDetails.Any())
                        {
                            return new
                            {
                                computerName = computer.Name,
                                categories = categoryDetails,
                                assignedUser = assignedUser
                            };
                        }
                        return null; // Exclude if no categories assigned
                    })
                    .Where(computer => computer != null) // Remove null entries
                    .ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save(int computerId, List<int> categoryIds)
        {
            try
            {
                // Pehle se assigned categories ko check karein
                var existingAssignments = await context.ComputerCategories
                    .Where(cc => categoryIds.Contains(cc.CategoryId))
                    .ToListAsync();



                // 🔥 Step 2: Add new assignments for the selected computer
                foreach (var categoryId in categoryIds)
                {
                    var computerCategory = new ComputerCategory
                    {
                        ComputerId = computerId,
                        CategoryId = categoryId,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = User.Identity.Name, // Assuming authentication
                        IsDeleted = false
                    };

                    context.ComputerCategories.Add(computerCategory);
                }

                // 🔥 Step 3: Save changes in database
                await context.SaveChangesAsync();

                return Json(new { success = true, message = "Category assignments updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
        public List<string> CategoryNames { get; set; } // New field to store category names
        public List<AssignedCategoryViewModel> AssignedCategories { get; set; } = new List<AssignedCategoryViewModel>();  // ✅ Default empty list
        public string AssignedUser { get; set; }
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
