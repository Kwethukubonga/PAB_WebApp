using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhilisaAbantuBethu.Data;
using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Pages
{
    public class ContactModel : PageModel
    {

        private readonly AppDbContext _db;

        public ContactModel(AppDbContext db)
        {

            _db = db;

        }

        [BindProperty] public string? Name { get; set; }
        [BindProperty] public string? Email { get; set; }
        [BindProperty] public string? Phone { get; set; }
        [BindProperty] public string? Subject { get; set; }
        [BindProperty] public string? Message { get; set; }

        public void OnGet() { }

        /// <summary>
        /// Handles the POST request for the contact form submission. 
        /// Validates the model state, creates a new ContactInquiry object with the submitted data, saves it to the database, 
        /// and redirects to the same page with a query parameter indicating that the message was sent successfully.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult.</returns>
        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid) 
                return Page();

            var inquiry = new ContactInquiry
            {

                Name = Name ?? "",
                Email = Email ?? "",
                Phone = Phone ?? "",
                Subject = Subject ?? "",
                Message = Message ?? "",
                SubmittedAt = DateTime.Now

            };

            _db.ContactInquiries.Add(inquiry);
            await _db.SaveChangesAsync();

            return RedirectToPage(new { sent = true });

        }

    }

}
