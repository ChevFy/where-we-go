using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Database;
using where_we_go.Service;

public class PostController(IPostService postService) : Controller
{
    private IPostService _postService { get; init; } = postService;
    [HttpGet]
    public async Task<IActionResult> PostDetail(Guid id)
    {
        var postDto = await _postService.GetPostDetailAsync(id);

        if (postDto == null) return NotFound();

        return View(postDto);
    }

    [HttpGet]
    public IActionResult PostCreate()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostCreate(PostCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return NotFound();

        await _postService.CreatePostAsync(dto, userId);

        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostDelete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return NotFound();

        var result = await _postService.DeletePostAsync(id, userId);
        if (result)
        {
            TempData["AlertMessage"] = "Success: Post deleted successfully!";
            return RedirectToAction("Index", "Home");
        }
        else
        {
            TempData["AlertMessage"] = "Error: You do not have permission or the post was not found.";
            return RedirectToAction("PostDetail", "Post", new { id = id });
        }

    }
}