using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EyeMezzexz.Models;
using X.PagedList;
using X.PagedList.Mvc.Core;
using EyeMezzexz.Data;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Net.Http;
using EyeMezzexz.Services;

namespace MezzexEye.Controllers
{
    [Authorize]
    public class ComputerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WebServiceClient _webServiceClient;
        private readonly IHttpClientFactory _httpClientFactory; // For direct API call if WebServiceClient is unavailable

        public ComputerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, WebServiceClient webServiceClient, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _webServiceClient = webServiceClient;
            _httpClientFactory = httpClientFactory;
        }

        private async Task<List<Category>> GetCategoriesFromApi()
        {
            try
            {
                // Using WebServiceClient if it has the method
                var categoriesJson = await _webServiceClient.GetCategoryALLAsync();
                return JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
            }
            catch (Exception)
            {
                // Fallback to direct HTTP call if WebServiceClient fails or method not implemented
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync("https://smapi.mezzex.com/api/Barcode/categories/async");
                return JsonConvert.DeserializeObject<List<Category>>(response);
            }
        }
        public async Task<IActionResult> Index(int? page, string searchQuery, string statusFilter)
        {
            const int pageSize = 10;
            int pageNumber = page ?? 1;

            var computersQuery = _context.Computers
                .Include(c => c.UserComputers).ThenInclude(uc => uc.User)
                .Include(c => c.ComputerCategories) // Ensure this is correctly loading
                .AsNoTracking();

            if (string.IsNullOrEmpty(statusFilter))
            {
                statusFilter = "Active";
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                computersQuery = statusFilter switch
                {
                    "Active" => computersQuery.Where(c => c.IsActive),
                    "Inactive" => computersQuery.Where(c => !c.IsActive),
                    _ => computersQuery
                };
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                computersQuery = computersQuery.Where(c =>
                    c.Name.ToLower().Contains(searchQuery) ||
                    (c.CreatedBy != null && c.CreatedBy.ToLower().Contains(searchQuery)) ||
                    (c.ModifyBy != null && c.ModifyBy.ToLower().Contains(searchQuery)));
            }

            var computers = await computersQuery
                .OrderByDescending(c => c.CreatedOn)
                .ToPagedListAsync(pageNumber, pageSize);

            // Fetch all categories from the database or API to map IDs to names
            var categories = await GetCategoriesFromApi();

            // Create a dictionary or list to map CategoryId to Name for quick lookup
            var categoryDict = categories.ToDictionary(c => c.CategoryID, c => c.Name);

            // Store category names for each computer in ViewBag
            var categoryNamesForComputers = new Dictionary<int, string>(); // ComputerId -> Comma-separated category names

            foreach (var computer in computers)
            {
                if (computer.ComputerCategories != null && computer.ComputerCategories.Any())
                {
                    var categoryNames = computer.ComputerCategories
                        .Select(cc => categoryDict.ContainsKey(cc.CategoryId) ? categoryDict[cc.CategoryId] : $"Unknown Category (ID: {cc.CategoryId})")
                        .Where(name => name != null)
                        .ToList();

                    categoryNamesForComputers[computer.Id] = string.Join(", ", categoryNames);

                    Console.WriteLine($"Computer ID: {computer.Id}, Categories: {string.Join(", ", computer.ComputerCategories.Select(cc => cc.CategoryId))}");
                    Console.WriteLine($"Category Names: {categoryNamesForComputers[computer.Id]}");
                }
                else
                {
                    categoryNamesForComputers[computer.Id] = "No categories assigned";
                    Console.WriteLine($"Computer ID: {computer.Id} has no categories or ComputerCategories is null");
                }
            }

            ViewBag.Users = await _userManager.Users
                .Select(u => new { u.Id, FullName = $"{u.FirstName} {u.LastName}" })
                .ToListAsync();

            ViewBag.Categories = categories; // Keep the full list of categories for other uses (e.g., dropdowns)

            ViewBag.CategoryNamesForComputers = categoryNamesForComputers; // Pass category names to the view
            ViewBag.SearchQuery = searchQuery;
            ViewBag.StatusFilter = statusFilter;

            return View(computers);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Computer computer, int[] selectedUserIds, int[] selectedCategoryIds)
        {
            if (!ModelState.IsValid)
            {
                if (selectedUserIds != null && selectedUserIds.Length > 0)
                {
                    foreach (var userId in selectedUserIds)
                    {
                        var userComputers = await _context.UserComputers
                            .Where(uc => uc.UserId == userId && uc.ComputerId != computer.Id)
                            .FirstOrDefaultAsync();
                        if (userComputers != null)
                        {
                            HttpContext.Session.SetString("NotificationMessage", "One or more selected users are already assigned to another computer.");
                            HttpContext.Session.SetString("NotificationType", "danger");
                            return RedirectToAction("Index");
                        }
                    }
                }

                var user = await _userManager.GetUserAsync(User);
                computer.CreatedOn = DateTime.UtcNow;
                computer.CreatedBy = $"{user.FirstName} {user.LastName}";
                _context.Add(computer);
                await _context.SaveChangesAsync();

                if (selectedUserIds != null)
                {
                    foreach (var userId in selectedUserIds)
                    {
                        _context.UserComputers.Add(new UserComputer { UserId = userId, ComputerId = computer.Id });
                    }
                }

                if (selectedCategoryIds != null)
                {
                    foreach (var categoryId in selectedCategoryIds)
                    {
                        _context.ComputerCategories.Add(new ComputerCategory { ComputerId = computer.Id, CategoryId = categoryId });
                    }
                }

                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("NotificationMessage", "Computer created successfully!");
                HttpContext.Session.SetString("NotificationType", "success");
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString("NotificationMessage", "Invalid computer details provided.");
            HttpContext.Session.SetString("NotificationType", "danger");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Computer computer, int[] selectedUserIds, int[] selectedCategoryIds)
        {
            if (ModelState.IsValid)
            {
                if (selectedUserIds != null && selectedUserIds.Length > 0)
                {
                    foreach (var userId in selectedUserIds)
                    {
                        var userComputers = await _context.UserComputers
                            .Where(uc => uc.UserId == userId && uc.ComputerId != computer.Id)
                            .FirstOrDefaultAsync();
                        if (userComputers != null)
                        {
                            HttpContext.Session.SetString("NotificationMessage", "One or more selected users are already assigned to another computer.");
                            HttpContext.Session.SetString("NotificationType", "danger");
                            return RedirectToAction("Index");
                        }
                    }
                }

                var existing = await _context.Computers
                    .Include(c => c.UserComputers)
                    .Include(c => c.ComputerCategories)
                    .FirstOrDefaultAsync(c => c.Id == computer.Id);

                if (existing == null)
                {
                    HttpContext.Session.SetString("NotificationMessage", "Computer not found.");
                    HttpContext.Session.SetString("NotificationType", "danger");
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                existing.Name = computer.Name;
                existing.TargetQuantity = computer.TargetQuantity;
                existing.IsActive = computer.IsActive;
                existing.ModifyOn = DateTime.UtcNow;
                existing.ModifyBy = $"{user.FirstName} {user.LastName}";

                // Update UserComputers
                _context.UserComputers.RemoveRange(existing.UserComputers);
                if (selectedUserIds != null)
                {
                    foreach (var userId in selectedUserIds)
                    {
                        _context.UserComputers.Add(new UserComputer { UserId = userId, ComputerId = existing.Id });
                    }
                }

                // Update ComputerCategories
                _context.ComputerCategories.RemoveRange(existing.ComputerCategories);
                if (selectedCategoryIds != null)
                {
                    foreach (var categoryId in selectedCategoryIds)
                    {
                        _context.ComputerCategories.Add(new ComputerCategory { ComputerId = existing.Id, CategoryId = categoryId });
                    }
                }

                _context.Update(existing);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("NotificationMessage", "Computer updated successfully!");
                HttpContext.Session.SetString("NotificationType", "success");
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString("NotificationMessage", "Invalid computer details provided.");
            HttpContext.Session.SetString("NotificationType", "danger");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var computer = await _context.Computers
                .Include(c => c.UserComputers)
                .Include(c => c.ComputerCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
            {
                HttpContext.Session.SetString("NotificationMessage", "Computer not found.");
                HttpContext.Session.SetString("NotificationType", "danger");
                return NotFound();
            }

            _context.UserComputers.RemoveRange(computer.UserComputers);
            _context.ComputerCategories.RemoveRange(computer.ComputerCategories);
            _context.Computers.Remove(computer);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("NotificationMessage", "Computer deleted successfully!");
            HttpContext.Session.SetString("NotificationType", "success");
            return RedirectToAction("Index");
        }
    }
}