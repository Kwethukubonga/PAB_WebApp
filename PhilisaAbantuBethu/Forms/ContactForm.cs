using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Forms;

/// <summary>What someone typed into the Contact form, before it is checked.</summary>
public record ContactInput(string? Name, string? Email, string? Phone, string? Subject, string? Message);

/// <summary>Checks a contact message on the server. Messages come back keyed by form field.</summary>
public static class ContactValidator
{
    public const int NameMax = 100;
    public const int MessageMax = 3000;

    /// <summary>The subjects offered in each language. The page shows these and the server accepts only these.</summary>
    public static string[] SubjectsFor(string lang) => lang switch
    {
        "xh" => new[] { "Umbuzo Jikelele", "Ukuba Sisithi Ngobuyisi", "Iminikelo Nemibambano", "Abezindaba", "Enye into" },
        "af" => new[] { "Algemene Navraag", "Vrywilligerwerk", "Skenkings & Vennootskappe", "Media", "Ander" },
        _ => new[] { "General Enquiry", "Volunteering", "Donations & Partnerships", "Media", "Other" }
    };

    private static readonly HashSet<string> AllSubjects =
        SubjectsFor("en").Concat(SubjectsFor("xh")).Concat(SubjectsFor("af")).ToHashSet();

    public static Dictionary<string, string> Validate(ContactInput input, SiteStrings s)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(input.Name))
            errors["Name"] = s.Pick("Please enter your name.", "Nceda ubhale igama lakho.", "Vul asseblief jou naam in.");
        else if (input.Name.Trim().Length > NameMax)
            errors["Name"] = SupportRequestValidator.TooLong(s, NameMax);

        var hasEmail = !string.IsNullOrWhiteSpace(input.Email);
        var hasPhone = !string.IsNullOrWhiteSpace(input.Phone);

        if (hasEmail && !SupportRequestValidator.IsEmail(input.Email!))
            errors["Email"] = SupportRequestValidator.InvalidEmail(s);

        if (hasPhone && !PhoneNumber.IsValid(input.Phone))
            errors["Phone"] = SupportRequestValidator.InvalidPhone(s);

        // Either one is enough, but without both the team has no way to reply.
        if (!hasEmail && !hasPhone)
            errors["Email"] = s.Pick(
                "Please give an email address or phone number so we can reply.",
                "Nceda unike idilesi ye-imeyile okanye inombolo yefowuni ukuze sikuphendule.",
                "Gee asseblief 'n e-posadres of foonnommer sodat ons kan antwoord.");

        if (string.IsNullOrWhiteSpace(input.Subject) || !AllSubjects.Contains(input.Subject))
            errors["Subject"] = s.Pick("Please choose a subject.", "Nceda ukhethe isihloko.", "Kies asseblief 'n onderwerp.");

        if (string.IsNullOrWhiteSpace(input.Message))
            errors["Message"] = s.Pick("Please write your message.", "Nceda ubhale umyalezo wakho.", "Skryf asseblief jou boodskap.");
        else if (input.Message.Trim().Length > MessageMax)
            errors["Message"] = SupportRequestValidator.TooLong(s, MessageMax);

        return errors;
    }
}
