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
    bool Declaration);

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
            errors["FirstName"] = s.Pick("Please enter your first name.", "Nceda ubhale igama lakho.", "Vul asseblief jou voornaam in.");
        else if (input.FirstName.Trim().Length > NameMax)
            errors["FirstName"] = TooLong(s, NameMax);

        if (string.IsNullOrWhiteSpace(input.Surname))
            errors["Surname"] = s.Pick("Please enter your surname.", "Nceda ubhale ifani yakho.", "Vul asseblief jou van in.");
        else if (input.Surname.Trim().Length > NameMax)
            errors["Surname"] = TooLong(s, NameMax);

        if (string.IsNullOrWhiteSpace(input.Phone))
            errors["Phone"] = s.Pick(
                "Please enter a phone number we can reach you on.",
                "Nceda ubhale inombolo yefowuni esinokukufumana kuyo.",
                "Vul asseblief 'n foonnommer in waarop ons jou kan bereik.");
        else if (!PhoneNumber.IsValid(input.Phone))
            errors["Phone"] = InvalidPhone(s);

        if (!string.IsNullOrWhiteSpace(input.Email) && !IsEmail(input.Email))
            errors["Email"] = InvalidEmail(s);

        if (string.IsNullOrWhiteSpace(input.Area))
            errors["Area"] = s.Pick(
                "Please tell us your area or suburb.",
                "Nceda usixelele indawo ohlala kuyo.",
                "Sê asseblief vir ons in watter area of voorstad jy woon.");
        else if (input.Area.Trim().Length > AreaMax)
            errors["Area"] = TooLong(s, AreaMax);

        if (!ContactMethods.Contains(input.ContactMethod))
            errors["ContactMethod"] = s.Pick(
                "Please choose how we should contact you.",
                "Nceda ukhethe indlela esinokuqhagamshelana nawe ngayo.",
                "Kies asseblief hoe ons jou moet kontak.");
        else if (input.ContactMethod == "email" && string.IsNullOrWhiteSpace(input.Email) && !errors.ContainsKey("Email"))
            errors["Email"] = s.Pick(
                "Add an email address, or choose another way for us to contact you.",
                "Yongeza idilesi ye-imeyile, okanye ukhethe enye indlela yokuqhagamshelana nawe.",
                "Voeg 'n e-posadres by, of kies 'n ander manier om jou te kontak.");

        if (string.IsNullOrWhiteSpace(input.SupportType) || !SupportTypes.Contains(input.SupportType))
            errors["SupportType"] = s.Pick(
                "Please choose the type of support you need.",
                "Nceda ukhethe uhlobo lwenkxaso oyidingayo.",
                "Kies asseblief die soort ondersteuning wat jy nodig het.");

        if (string.IsNullOrWhiteSpace(input.Situation))
            errors["Situation"] = s.Pick(
                "Please tell us a little about your situation.",
                "Nceda usixelele kancinci ngemeko yakho.",
                "Vertel ons asseblief 'n bietjie van jou situasie.");
        else if (input.Situation.Trim().Length > SituationMax)
            errors["Situation"] = TooLong(s, SituationMax);

        if (!UrgentValues.Contains(input.Urgent))
            errors["Urgent"] = s.Pick(
                "Please tell us whether this is urgent.",
                "Nceda usixelele ukuba oku kungxamisekile na.",
                "Sê asseblief of dit dringend is.");

        if ((input.Extra?.Trim().Length ?? 0) > ExtraMax)
            errors["Extra"] = TooLong(s, ExtraMax);

        if (!input.Declaration)
            errors["Declaration"] = s.Pick(
                "Please tick the box to confirm and give consent.",
                "Nceda uphawule ibhokisi ukuze uqinisekise kwaye unike imvume.",
                "Merk asseblief die blokkie om te bevestig en toestemming te gee.");

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

    internal static string TooLong(SiteStrings s, int max) => s.Pick(
        $"Please keep this under {max} characters.",
        $"Nceda ungadluli koonobumba abayi-{max}.",
        $"Hou dit asseblief onder {max} karakters.");

    internal static string InvalidEmail(SiteStrings s) => s.Pick(
        "Please check the email address.",
        "Nceda ujonge idilesi ye-imeyile.",
        "Kontroleer asseblief die e-posadres.");

    internal static string InvalidPhone(SiteStrings s) => s.Pick(
        "Enter a South African cellphone number, like 082 123 4567, or an international number starting with +.",
        "Faka inombolo yeselfowuni yaseMzantsi Afrika, efana no-082 123 4567, okanye inombolo yamanye amazwe eqala ngo-+.",
        "Vul 'n Suid-Afrikaanse selfoonnommer in, soos 082 123 4567, of 'n internasionale nommer wat met + begin.");
}
