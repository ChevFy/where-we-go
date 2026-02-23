namespace where_we_go.Models;

public class PostDto
{
    public Guid PostId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string LocationName { get; set; }
    public string DateDeadlineFormatted { get; set; } // We can format the date here!
    public string CategoryName { get; set; } // Send a string instead of an object
}