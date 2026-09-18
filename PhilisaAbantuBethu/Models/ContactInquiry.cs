namespace PhilisaAbantuBethu.Models
{
    public class ContactInquiry
    {

        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime SubmittedAt { get; set; }

    }
}
