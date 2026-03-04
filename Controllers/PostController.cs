using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Service;

public class PostController(IPostService postService , AppDbContext dbContext) : Controller
{
    private IPostService _postService { get; init; } = postService;
    [HttpGet]
    public async Task<IActionResult> PostDetail(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var postDto = await _postService.GetPostDetailAsync(id, userId);

        if (postDto == null) return NotFound();

        return View(postDto);
    }

    [HttpGet]
    public IActionResult PostCreate()
    {
        ViewBag.Categories = dbContext.Categories
            .Select(c => new CategorySelectDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.Name
            }).ToList();

        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostCreate(PostCreateDto dto )
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = dbContext.Categories
                .Select(c => new CategorySelectDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Name
                }).ToList();
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
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> JoinPost(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var result = await _postService.JoinPostAsync(id, userId);

        if (result == "Success")
        {
            TempData["AlertMessage"] = "You have successfully joined the activity!";
        }
        else if (result == "Pending")
        {
            TempData["AlertMessage"] = "This activity is full. You have been added to the waitlist (Pending).";
        }
        else
        {
            TempData["AlertMessage"] = result;
        }

        return RedirectToAction("PostDetail", new { id = id });
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LeavePost(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var result = await _postService.LeavePostAsync(id, userId);

        if (result == "Success")
        {
            TempData["AlertMessage"] = "You have successfully left the activity.";
        }
        else
        {
            TempData["AlertMessage"] = result;
        }

        return RedirectToAction("PostDetail", new { id = id });
    }

    [HttpPost]
    [Route("api/post/locationsave")]
    public async Task<IActionResult> PostLocationSave([FromBody] PostLocationSaveDto model )
    {
        if(!ModelState.IsValid)
        {
            return BadRequest();
        }

        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.PostId == Guid.Parse(model.PostId));
        if(post is null)
            return NotFound();

        post.LocationLat = float.Parse(model.LocationLat);
        post.LocationLon = float.Parse(model.LocationLon);


        return Ok(new { message = "Suceess"});
    }

    
}