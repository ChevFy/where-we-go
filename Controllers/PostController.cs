using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using where_we_go.DTO;
using where_we_go.Models;
using where_we_go.Database;

public class PostController(AppDbContext dbContext) : Controller
{
    private AppDbContext _dbContext { get; init; } = dbContext;

    public IActionResult PostView()
    {

        return View(); // จะหา Views/Post/PostView.cshtml
    }

    [HttpGet]
    public IActionResult PostCreate()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Post model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        model.UserId = userId;
        model.DateCreated = DateTime.UtcNow;
        model.Status = "Active";
        model.CurrentParticipants = 0;

        // TODO: Save to database later
        // _dbContext.Posts.Add(model);
        // await _dbContext.SaveChangesAsync();

        // For now, just redirect
        return RedirectToAction("PostView");
    }
}