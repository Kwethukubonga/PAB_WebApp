namespace PhilisaAbantuBethu.Models
{

    public class SupportRequest
    {

        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string AreaOfSupport { get; set; } = "";
        public string ContactMethod { get; set; } = "";
        public string SupportType { get; set; } = "";
        public string Situation { get; set; } = "";
        public string Urgent { get; set; } = "";
        public string Extra { get; set; } = "";
        public bool Declaration { get; set; } 
        public string Reference { get; set; } = "";
        public DateTime SubmittedAt { get; set; }

    }

}
