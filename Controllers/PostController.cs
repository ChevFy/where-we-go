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
}