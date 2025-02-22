using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EyeMezzexz.Services;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarcodeController : ControllerBase
    {
        private readonly WebServiceClient _webServiceClient;

        public BarcodeController(WebServiceClient webServiceClient)
        {
            _webServiceClient = webServiceClient;
        }

        // Endpoint to call GetCategoryALL
/*        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var categories = _webServiceClient.GetCategoryALL();
            return Ok(categories); // Return JSON data
        }*/

        // Endpoint to call GetCategoryALL asynchronously
        [HttpGet("categories/async")]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            var categories = await _webServiceClient.GetCategoryALLAsync();
            return Ok(categories); // Return JSON data
        }

        // Endpoint to call GetNoOfOrderAccordingCategory
        [HttpGet("orders")]
        public IActionResult GetOrdersByCategory()
        {
            var orders = _webServiceClient.GetNoOfOrderAccordingCategory();
            return Ok(orders); // Return JSON data
        }

        // Endpoint to call GetNoOfOrderAccordingCategory asynchronously
        [HttpGet("orders/async")]
        public async Task<IActionResult> GetOrdersByCategoryAsync()
        {
            var orders = await _webServiceClient.GetNoOfOrderAccordingCategoryAsync();
            return Ok(orders); // Return JSON data
        }
    }
}