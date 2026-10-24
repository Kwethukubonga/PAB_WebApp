using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhilisaAbantuBethu.Forms;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Pages
{
    // The longest field here is the 3000-character message, so 64KB is ample. See the note on
    // RequestSupportModel for why the 30MB Kestrel default is worth narrowing on a public form.
    [RequestSizeLimit(64 * 1024)]
    public class ContactModel : PageModel
    {

        private readonly IContactService _contact;

        public ContactModel(IContactService contact)
        {

            _contact = contact;

        }

        [BindProperty] public string? Name { get; set; }
        [BindProperty] public string? Email { get; set; }
        [BindProperty] public string? Phone { get; set; }
        [BindProperty] public string? Subject { get; set; }
        [BindProperty] public string? Message { get; set; }

        // Hidden from people, so only bots fill it in.
        [BindProperty] public string? Website { get; set; }

        /// <summary>A message for the whole form, shown above the fields.</summary>
        public string? FormError { get; private set; }

        public void OnGet() { }

        /// <summary>
        /// Checks the message on the server, saves it, and redirects back with ?sent=true so a refresh
        /// can't send it twice. When something is wrong the form comes back with what was typed.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {

            var s = HttpContext.Strings();

            if (!string.IsNullOrEmpty(Website))
            {
                FormError = s["Contact.CouldntSendMessagePlease"];
                return Page();
            }

            var input = new ContactInput(Name, Email, Phone, Subject, Message);

            foreach (var (field, message) in ContactValidator.Validate(input, s))
            {
                ModelState.AddModelError(field, message);
            }

            if (!ModelState.IsValid)
            {
                FormError = s["RequestSupport.PleaseCheckHighlightedFields"];
                return Page();
            }

            await _contact.SubmitAsync(input, HttpContext.RequestAborted);

            return RedirectToPage(new { sent = true });

        }

        /// <summary>The message for one field, or null when it has none.</summary>
        public string? ErrorFor(string field) =>
            ModelState.TryGetValue(field, out var entry) && entry.Errors.Count > 0
                ? entry.Errors[0].ErrorMessage
                : null;

    }

}
