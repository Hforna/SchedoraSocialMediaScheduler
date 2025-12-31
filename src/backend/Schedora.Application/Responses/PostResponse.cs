namespace Schedora.Application.Responses;

public class PostResponse
{
    public long UserId { get; private set; }
    public string Content { get; private set; }
    public PostStatus Status { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public long? QueueId { get; private set; }
    public long? TemplateId { get; private set; }
    public string? Notes { get; private set; }
    public long? CreatedBy { get; private set; }
    public long? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    public string? RejectionReason { get; private set; }
}