using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public class DocumentActivity
{
    [Key]
    public int DocumentActivityId { get; set; }
    public int DocumentId { get; set; }
    public int ActorUserId { get; set; }
    [Required, MaxLength(30)]
    public string ActivityType { get; set; } = string.Empty;
    public DateTime OccurredDate { get; set; } = DateTime.UtcNow;
    [MaxLength(1000)]
    public string? Details { get; set; }

    public Document Document { get; set; } = null!;
    public User ActorUser { get; set; } = null!;
}
