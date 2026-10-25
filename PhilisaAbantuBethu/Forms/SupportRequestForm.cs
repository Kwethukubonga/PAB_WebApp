using System.ComponentModel.DataAnnotations;
using PhilisaAbantuBethu.Localization;
using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Forms;

/// <summary>What someone typed into the Request Support form, before it is checked.</summary>
public record SupportRequestInput(
    string? FirstName,
    string? Surname,
    string? Phone,
    string? Email,
    string? Area,
    string? ContactMethod,
    string? SupportType,
    string? Situation,
    string? Urgent,
    string? Extra,
    bool Declaration)
{
    /// <summary>The same input with invisible control and direction characters removed (G7).</summary>
    public SupportRequestInput Cleaned() => this with
    {
        FirstName = TextInput.Clean(FirstName), Surname = TextInput.Clean(Surname), Phone = TextInput.Clean(Phone),
        Email = TextInput.Clean(Email), Area = TextInput.Clean(Area), ContactMethod = TextInput.Clean(ContactMethod),
        SupportType = TextInput.Clean(SupportType), Situation = TextInput.Clean(Situation), Urgent = TextInput.Clean(Urgent),
        Extra = TextInput.Clean(Extra)
    };
}

/// <summary>
/// Checks a support request on the server. The browser checks the same things, but it can be
/// skipped, so nothing is saved unless it passes here. Messages come back in the visitor's language,
/// keyed by the form field they belong to.
/// </summary>
public static class SupportRequestValidator
{
    public const int NameMax = 60;
    public const int EmailMax = 254;
    public const int AreaMax = 100;
    public const int SituationMax = 3000;
    public const int ExtraMax = 2000;

    /// <summary>What staff may write against a request on the staff pages.</summary>
    public const int StaffNotesMax = 2000;

    public static readonly string[] ContactMethods = { "phone", "whatsapp", "email", "visit" };
    public static readonly string[] UrgentValues = { "yes", "no" };

    // The form sends the support type in the visitor's language, so any language's label is accepted.
    private static readonly HashSet<string> SupportTypes = SiteStrings.En.SupportTypes
        .Concat(SiteStrings.Xh.SupportTypes)
        .Concat(SiteStrings.Af.SupportTypes)
        .ToHashSet();

    public static Dictionary<string, string> Validate(SupportRequestInput input, SiteStrings s)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(input.FirstName))
            errors["FirstName"] = s["SupportRequestForm.PleaseEnterFirstName"];
        else if (TextInput.Length(input.FirstName.Trim()) > NameMax)
            errors["FirstName"] = TooLong(s, NameMax);

        if (string.IsNullOrWhiteSpace(input.Surname))
            errors["Surname"] = s["SupportRequestForm.PleaseEnterSurname"];
        else if (TextInput.Length(input.Surname.Trim()) > NameMax)
            errors["Surname"] = TooLong(s, NameMax);

        if (string.IsNullOrWhiteSpace(input.Phone))
            errors["Phone"] = s["SupportRequestForm.PleaseEnterPhoneNumber"];
        else if (!PhoneNumber.IsValid(input.Phone))
            errors["Phone"] = InvalidPhone(s);

        if (!string.IsNullOrWhiteSpace(input.Email) && !IsEmail(input.Email))
            errors["Email"] = InvalidEmail(s);

        if (string.IsNullOrWhiteSpace(input.Area))
            errors["Area"] = s["SupportRequestForm.PleaseTellUsArea"];
        else if (TextInput.Length(input.Area.Trim()) > AreaMax)
            errors["Area"] = TooLong(s, AreaMax);

        if (!ContactMethods.Contains(input.ContactMethod))
            errors["ContactMethod"] = s["SupportRequestForm.PleaseChooseHowShould"];
        else if (input.ContactMethod == "email" && string.IsNullOrWhiteSpace(input.Email) && !errors.ContainsKey("Email"))
            errors["Email"] = s["SupportRequestForm.AddEmailAddressChoose"];

        if (string.IsNullOrWhiteSpace(input.SupportType) || !SupportTypes.Contains(input.SupportType))
            errors["SupportType"] = s["SupportRequestForm.PleaseChooseTypeSupport"];

        if (string.IsNullOrWhiteSpace(input.Situation))
            errors["Situation"] = s["SupportRequestForm.PleaseTellUsLittle"];
        else if (TextInput.Length(input.Situation.Trim()) > SituationMax)
            errors["Situation"] = TooLong(s, SituationMax);

        if (!UrgentValues.Contains(input.Urgent))
            errors["Urgent"] = s["SupportRequestForm.PleaseTellUsWhether"];

        if (TextInput.Length(input.Extra?.Trim() ?? "") > ExtraMax)
            errors["Extra"] = TooLong(s, ExtraMax);

        if (!input.Declaration)
            errors["Declaration"] = s["SupportRequestForm.PleaseTickBoxConfirm"];

        return errors;
    }

    // The attribute alone accepts "name@home", so the part after @ must also have a dot in it.
    internal static bool IsEmail(string value)
    {
        var email = value.Trim();
        return email.Length <= EmailMax
            && new EmailAddressAttribute().IsValid(email)
            && email[(email.LastIndexOf('@') + 1)..].Contains('.');
    }

    internal static string TooLong(SiteStrings s, int max) => s.Format("SupportRequestForm.PleaseKeepUnderCharacters", max);

    internal static string InvalidEmail(SiteStrings s) => s["SupportRequestForm.PleaseCheckEmailAddress"];

    internal static string InvalidPhone(SiteStrings s) => s["SupportRequestForm.EnterSouthAfricanCellphone"];
}
