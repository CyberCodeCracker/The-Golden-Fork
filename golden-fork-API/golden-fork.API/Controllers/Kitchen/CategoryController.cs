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
        public async Task<ActionResult<CategoryWithItemsResponse>> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return category != null ? Ok(category) : NotFound();
        }

        [HttpPost]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<ActionResult> Create([FromBody] CategoryRequest request)
        {
            var (success, message, categoryId) = await _categoryService.CreateAsync(request);

            if (success)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = categoryId },
                    new { message, categoryId }  // ← ADD categoryId HERE
                );
            }

            return BadRequest(new { message });
        }

        [HttpPut("{id}")]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<ActionResult> Update(int id, [FromBody] CategoryUpdate request)
        {
            var (success, message) = await _categoryService.UpdateAsync(id, request);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpDelete("{id}")]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<ActionResult> Delete(int id)
        {
            var (success, message) = await _categoryService.DeleteAsync(id);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpPost("{categoryId}/upload-photo")]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<ActionResult> UploadPhoto(int categoryId, IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest(new { message = "No photo uploaded." });

            var (success, message, imageUrl) = await _categoryService.UploadPhotoAsync(categoryId, photo);

            return success
                ? Ok(new { message, imageUrl })
                : BadRequest(new { message });
        }

        [HttpPost("{categoryId}/items/{itemId}")]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<IActionResult> AddItemToCategory(int categoryId, int itemId)
        {
            var result = await _categoryService.AddItemToCategoryAsync(categoryId, itemId);
            return result.success ? Ok() : BadRequest(new { result.message });
        }

        [HttpDelete("{categoryId}/items/{itemId}")]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<IActionResult> RemoveItemFromCategory(int categoryId, int itemId)
        {
            var result = await _categoryService.RemoveItemFromCategoryAsync(categoryId, itemId);
            return result.success ? Ok() : BadRequest(new { result.message });
        }
    }
}