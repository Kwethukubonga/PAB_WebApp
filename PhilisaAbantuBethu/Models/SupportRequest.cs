namespace PhilisaAbantuBethu.Models
{

    public class SupportRequest
    {

        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Area { get; set; } = "";
        public string ContactMethod { get; set; } = "";
        public string SupportType { get; set; } = "";
        public string Situation { get; set; } = "";
        public string Urgent { get; set; } = "";
        public string Extra { get; set; } = "";
        public bool Declaration { get; set; }
        public string Reference { get; set; } = "";
        public DateTime SubmittedAt { get; set; }

        // Where PAB staff are with the request. Changed on the staff pages only.
        public string Status { get; set; } = SupportRequestStatus.New;
        public DateTime? StatusUpdatedAt { get; set; }
        public string? HandledBy { get; set; }

        // What staff have done about it. Written on the staff detail page, never shown publicly.
        public string? StaffNotes { get; set; }

        public bool IsUrgent => Urgent == "yes";

    }

    public static class SupportRequestStatus
    {
        public const string New = "new";
        public const string InProgress = "in-progress";
        public const string Closed = "closed";

        public static readonly string[] All = { New, InProgress, Closed };
    }

}
