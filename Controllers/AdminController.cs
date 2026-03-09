using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using where_we_go.DTO;
using where_we_go.Service;


[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController(
    IAdminUserService adminUserService,
    IAdminPostService adminPostService,
    ICategoryService categoryService) : Controller
{
    private IAdminUserService _adminUserService { get; init; } = adminUserService;
    private IAdminPostService _adminPostService { get; init; } = adminPostService;
    private ICategoryService _categoryService { get; init; } = categoryService;

    [HttpGet("index")]
    public IActionResult Index() => View();

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryDto query)
    {
        var result = await _adminUserService.GetUsersAsync(query);
        return Json(result);
    }

    [HttpPost("users/ban")]
    public async Task<IActionResult> BanUser([FromBody] BanUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var bannedBy = User.Identity?.Name ?? "System";
        var (success, error) = await _adminUserService.BanUserAsync(dto.UserId, dto.Reason, dto.DurationDays, bannedBy);

        if (!success)
        {
            if (error == "User not found")
                return NotFound(new { details = error });
            return BadRequest(new { details = error });
        }

        return Ok();
    }

    [HttpPost("users/unban")]
    public async Task<IActionResult> UnbanUser([FromBody] UnbanUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var (success, error) = await _adminUserService.UnbanUserAsync(dto.UserId);

        if (!success)
        {
            if (error == "User not found")
                return NotFound(new { details = error });
            return BadRequest(new { details = error });
        }

        return Ok();
    }

    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] UserQueryDto query)
    {
        if (string.IsNullOrWhiteSpace(query.NameFilter))
        {
            query.NameFilter = null;
        }

        return await GetUsers(query);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Json(categories);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        try
        {
            var category = await _categoryService.CreateAsync(dto);
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { details = ex.Message });
        }
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        try
        {
            var category = await _categoryService.UpdateAsync(id, dto);
            if (category == null)
            {
                return NotFound(new { details = "Category not found" });
            }
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { details = ex.Message });
        }
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { details = "Category not found" });
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { details = "Cannot delete category: " + ex.Message });
        }
    }

    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts([FromQuery] PostQueryDto query)
    {
        var result = await _adminPostService.GetPostsAsync(query);
        return Json(result);
    }

    [HttpGet("posts/{id:guid}")]
    public async Task<IActionResult> GetPostDetail([FromRoute] Guid id)
    {
        var post = await _adminPostService.GetPostDetailAsync(id);
        if (post == null)
        {
            return NotFound(new { details = "Post not found" });
        }
        return Json(post);
    }

    [HttpPost("posts/{id:guid}/delete")]
    public async Task<IActionResult> DeletePost([FromRoute] Guid id)
    {
        var (success, error) = await _adminPostService.DeletePostAsync(id);
        if (!success)
        {
            return NotFound(new { details = error });
        }
        return Ok();
    }

    [HttpPut("posts/{id:guid}")]
    public async Task<IActionResult> UpdatePost([FromRoute] Guid id, [FromBody] AdminPostUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var (success, error) = await _adminPostService.UpdatePostAsync(id, dto);
        if (!success)
        {
            if (error == "Post not found")
                return NotFound(new { details = error });
            return BadRequest(new { details = error });
        }

        return Ok(new { message = "Post updated successfully" });
    }

    [HttpPost("posts/{id:guid}/restore")]
    public async Task<IActionResult> RestorePost([FromRoute] Guid id)
    {
        var (success, error) = await _adminPostService.RestorePostAsync(id);
        if (!success)
        {
            return NotFound(new { details = error });
        }
        return Ok();
    }

    [HttpDelete("posts/{postId:guid}/participants/{participantId:guid}")]
    public async Task<IActionResult> RemoveParticipant([FromRoute] Guid postId, [FromRoute] Guid participantId)
    {
        var (success, error) = await _adminPostService.RemoveParticipantAsync(postId, participantId);
        if (!success)
        {
            return NotFound(new { details = error });
        }
        return Ok(new { message = "Participant removed successfully" });
    }

}
