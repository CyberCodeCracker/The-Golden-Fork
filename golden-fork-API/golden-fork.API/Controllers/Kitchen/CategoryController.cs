// golden_fork.API/Controllers/CategoryController.cs
using golden_fork.core.DTOs.Kitchen;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace golden_fork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<CategoryResponse>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] bool ascending = true)
        {
            var result = await _categoryService.GetAllAsync(pageNumber, pageSize, searchTerm, sortBy, ascending);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryResponse>> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return category != null ? Ok(category) : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] CategoryRequest request)
        {
            var (success, message, categoryId) = await _categoryService.CreateAsync(request);
            return success
                ? CreatedAtAction(nameof(GetById), new { id = categoryId }, new { message })
                : BadRequest(new { message });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] CategoryUpdate request)
        {
            var (success, message) = await _categoryService.UpdateAsync(id, request);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var (success, message) = await _categoryService.DeleteAsync(id);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}