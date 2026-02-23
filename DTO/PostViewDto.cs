namespace where_we_go.Models;

public class PostDto
{
    public Guid PostId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string LocationName { get; set; }
    public string DateDeadlineFormatted { get; set; }
    public string CategoryName { get; set; }
}