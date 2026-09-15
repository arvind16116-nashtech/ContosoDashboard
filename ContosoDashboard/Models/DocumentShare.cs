using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public class DocumentShare
{
    [Key]
    public int DocumentShareId { get; set; }
    public int DocumentId { get; set; }
    public int? RecipientUserId { get; set; }
    [MaxLength(100)]
    public string? RecipientTeamKey { get; set; }
    public int SharedByUserId { get; set; }
    public DateTime SharedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Document Document { get; set; } = null!;
    public User? RecipientUser { get; set; }
    public User SharedByUser { get; set; } = null!;
}
