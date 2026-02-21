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

    public IActionResult PostView(Guid id)
    {
        // Search the mock database for the post with this specific ID
        var post = _dbContext.Posts.FirstOrDefault(p => p.PostId == id);

        // If someone types a random ID in the URL that doesn't exist, return a 404 Not Found
        if (post == null)
        {
            return NotFound();
        }

        // Pass post to the detail view
        return View(post);
    }

    [HttpGet]
    public IActionResult PostCreate()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(PostCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var post = new Post
        {
            PostId = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            LocationName = dto.LocationName,
            DateDeadline = dto.DateDeadline,
            MinParticipants = dto.MinParticipants,
            MaxParticipants = dto.MaxParticipants,
            DateCreated = DateTime.UtcNow,
            Status = "Active",
            CurrentParticipants = 0,
            InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper() // Generate simple invite code
        };

        // TODO: Save to database later
        // _dbContext.Posts.Add(post);
        // await _dbContext.SaveChangesAsync();

        // For now, just redirect
        return RedirectToAction("PostView");
    }
}