using AutoMapper;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;

namespace golden_fork.API.Controllers.Kitchen
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : BaseController
    {

        private readonly IMenuService _service;

        public MenuController(IUnitOfWork unitOfWork, IMapper mapper, IMenuService service)
            : base(unitOfWork, mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllMenus(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] bool ascending = true)
        {
            try
            {
                // Validate inputs
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _service.GetAllMenusAsync(pageNumber, pageSize, searchTerm, sortBy, ascending);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while processing your request." });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuById(int id)
        {
            try
            {
                var menu = await _service.GetMenuByIdAsync(id);
                if (menu == null)
                {
                    return NotFound();
                }
                return Ok(menu);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize]
        [HttpPost("Create")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)]
        public async Task<IActionResult> CreateMenu(MenuRequest request)
        {
            try
            {
                var createdMenu = await _service.CreateMenuAsync(request);
                return CreatedAtAction(nameof(GetMenuById), new { id = createdMenu.Id }, createdMenu);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize]
        [HttpPut("Update/{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)] 
        public async Task<IActionResult> UpdateMenu(int id, MenuRequest request)
        {
            try
            {
                var updatedMenu = await _service.UpdateMenuAsync(id, request);
                if (updatedMenu == null)
                {
                    return NotFound();
                }
                return Ok(updatedMenu);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize]
        [HttpDelete("Delete/{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Chef)] 
        public async Task<IActionResult> DeleteMenuById(int id)
        {
            try
            {
                var result = await _service.DeleteMenuByIdAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
