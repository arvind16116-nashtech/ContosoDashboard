using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public class Document
{
    [Key]
    public int DocumentId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required, MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Tags { get; set; }

    [Required, MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string StoredFilePath { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    public int UploadedByUserId { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }

    [Required, MaxLength(30)]
    public string ScanStatus { get; set; } = DocumentScanStatuses.Clean;

    [MaxLength(100)]
    public string ScanCorrelationId { get; set; } = Guid.NewGuid().ToString("N");

    public int ScanAttemptCount { get; set; }
    public bool IsAvailable { get; set; } = true;

    public User UploadedByUser { get; set; } = null!;
    public Project? Project { get; set; }
    public TaskItem? Task { get; set; }
    public ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
    public ICollection<DocumentActivity> Activities { get; set; } = new List<DocumentActivity>();
}

public static class DocumentScanStatuses
{
    public const string Pending = "Pending";
    public const string Clean = "Clean";
    public const string Infected = "Infected";
    public const string Failed = "Failed";
    public const string Quarantined = "Quarantined";
}
