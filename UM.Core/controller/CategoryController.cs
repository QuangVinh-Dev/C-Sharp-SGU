using Microsoft.AspNetCore.Mvc;
using UM.Core.dto;
using UM.Core.sercurity;
using UM.Core.service;

namespace UM.Core.controller
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ICurrentUserService _currentUserService;

        public CategoryController(ICategoryService categoryService, ICurrentUserService currentUserService)
        {
            _categoryService = categoryService;
            _currentUserService = currentUserService;
        }

        [HttpPost("api/v1/servers/{serverId}/categories")]
        public async Task<ActionResult<CategoryResponseDto>> CreateCategory(long serverId, [FromBody] CreateCategoryRequest request)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _categoryService.CreateCategoryAsync(serverId, request, currentUserId);
            return StatusCode(201, result);
        }

        [HttpGet("api/v1/servers/{serverId}/categories")]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetCategories(long serverId)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _categoryService.GetCategoriesByServerAsync(serverId, currentUserId);
            return Ok(result);
        }

        [HttpPatch("api/v1/categories/{categoryId}")]
        public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(long categoryId, [FromBody] UpdateCategoryRequest request)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _categoryService.UpdateCategoryAsync(categoryId, request, currentUserId);
            return Ok(result);
        }

        [HttpDelete("api/v1/categories/{categoryId}")]
        public async Task<IActionResult> DeleteCategory(long categoryId)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            await _categoryService.DeleteCategoryAsync(categoryId, currentUserId);
            return NoContent();
        }
    }
}
