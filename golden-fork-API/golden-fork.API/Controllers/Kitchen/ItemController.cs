using golden_fork.core.DTOs.Kitchen;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
namespace golden_fork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        public ItemController(IItemService itemService)
        {
            _itemService = itemService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<ItemResponse>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] bool ascending = true)
        {
            try
            {
                var result = await _itemService.GetAllItemsAsync(
                    pageNumber, pageSize, searchTerm, sortBy, ascending);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching items.", error = ex.Message });
            }
        }
        // GET: api/item/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ItemResponse>> GetById(int id)
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = $"Item with ID {id} not found." });
            return Ok(item);
        }
        [HttpPost]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<ActionResult> Create([FromBody] ItemRequest request)
        {
            try
            {
                var (success, message, itemId) = await _itemService.CreateAsync(request);
                if (!success)
                    return BadRequest(new { message });
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = itemId },
                    new { message = "Item created successfully", itemId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create item.", error = ex.Message });
            }
        }
        [HttpPut("{id}")]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<ActionResult> Update(int id, [FromBody] ItemUpdate request)
        {
            try
            {
                var (success, message) = await _itemService.UpdateAsync(id, request);
                return success
                    ? Ok(new { message })
                    : BadRequest(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update item.", error = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var (success, message) = await _itemService.DeleteAsync(id);
                return success
                    ? Ok(new { message })
                    : NotFound(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete item.", error = ex.Message });
            }
        }
        [HttpPost("{itemId}/upload-photo")]
        [Authorize]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<ActionResult> UploadPhoto(int itemId, IFormFile photo, [FromQuery] bool isMain = false)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest(new { message = "No photo uploaded." });
            try
            {
                // Fix 1: Explicitly declare the tuple variables
                var (success, message, imageUrl) = await _itemService.UploadPhotoAsync(itemId, photo, isMain);
                return success
                    ? Ok(new { message, imageUrl })
                    : BadRequest(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to upload photo.", error = ex.Message });
            }
        }
    }
}