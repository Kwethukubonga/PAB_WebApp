namespace PhilisaAbantuBethu.Localization;

public class NavStrings
{
	public string Home { get; init; } = "";
	public string Programmes { get; init; } = "";
	public string Resources { get; init; } = "";
	public string Contact { get; init; } = "";
	public string RequestSupport { get; init; } = "";
}

public class HomeStrings
{
	public string Badge { get; init; } = "";
	public string HeroHeading { get; init; } = "";
	public string HeroSub { get; init; } = "";
	public string HeroParagraph { get; init; } = "";
	public string RequestSupport { get; init; } = "";
	public string ExplorePrograms { get; init; } = "";
	public string ProgrammesSectionTitle { get; init; } = "";
	public string ProgrammesSectionSub { get; init; } = "";
	public string ViewAll { get; init; } = "";
	public string ResourcesTitle { get; init; } = "";
	public string ResourcesSub { get; init; } = "";
	public string ViewResources { get; init; } = "";
	public string ContactTitle { get; init; } = "";
	public string ContactSub { get; init; } = "";
	public string ContactUs { get; init; } = "";
	public string ServiceNote { get; init; } = "";
}

public class FormStrings
{
	public string Title { get; init; } = "";
	public string Subtitle { get; init; } = "";
	public string ServiceArea { get; init; } = "";
	public string Step1Label { get; init; } = "";
	public string Step2Label { get; init; } = "";
	public string Step3Label { get; init; } = "";
	public string FirstName { get; init; } = "";
	public string Surname { get; init; } = "";
	public string Phone { get; init; } = "";
	public string Email { get; init; } = "";
	public string Area { get; init; } = "";
	public string AreaPlaceholder { get; init; } = "";
	public string ContactMethod { get; init; } = "";
	public string ContactPhone { get; init; } = "";
	public string ContactWhatsApp { get; init; } = "";
	public string ContactEmail { get; init; } = "";
	public string ContactVisit { get; init; } = "";
	public string SupportType { get; init; } = "";
	public string SupportTypePlaceholder { get; init; } = "";
	public string Situation { get; init; } = "";
	public string SituationPlaceholder { get; init; } = "";
	public string Urgent { get; init; } = "";
	public string UrgentYes { get; init; } = "";
	public string UrgentNo { get; init; } = "";
	public string ExtraInfo { get; init; } = "";
	public string ExtraInfoPlaceholder { get; init; } = "";
	public string Declaration { get; init; } = "";
	public string SubmitRequest { get; init; } = "";
	public string Back { get; init; } = "";
	public string Next { get; init; } = "";
	public string Required { get; init; } = "";
}

public class ConfirmationStrings
{
	public string Title { get; init; } = "";
	public string Sub { get; init; } = "";
	public string RefLabel { get; init; } = "";
	public string DateLabel { get; init; } = "";
	public string StatusLabel { get; init; } = "";
	public string Status { get; init; } = "";
	public string WhatNext { get; init; } = "";
	public string[] Steps { get; init; } = Array.Empty<string>();
	public string BackHome { get; init; } = "";
	public string AnotherRequest { get; init; } = "";
}

public class SiteStrings
{
	public string Lang { get; init; } = "en";
	public NavStrings Nav { get; init; } = new();
	public HomeStrings Home { get; init; } = new();
	public FormStrings Form { get; init; } = new();
	public ConfirmationStrings Confirmation { get; init; } = new();
	public string[] SupportTypes { get; init; } = Array.Empty<string>();

	public bool IsEnglish => Lang == "en";
	public bool IsXhosa => Lang == "xh";
	public bool IsAfrikaans => Lang == "af";

	/// <summary>The text for a key in Resources/SharedResource.resx, in this language.</summary>
	public string this[string key] => Text.Get(key, Lang);

	/// <summary>The same, with values filled into its {0}, {1} placeholders.</summary>
	public string Format(string key, params object[] args) =>
		string.Format(Text.CultureFor(Lang), this[key], args);

	public static SiteStrings For(string lang) => lang switch
	{
		"xh" => Xh,
		"af" => Af,
		_ => En
	};

	// Built once per language from the .resx files. The pages keep reading s.Home.HeroHeading and
	// the rest as before; only where the words are kept has changed (A8).
	public static readonly SiteStrings En = Build("en");
	public static readonly SiteStrings Xh = Build("xh");
	public static readonly SiteStrings Af = Build("af");

	private static SiteStrings Build(string lang)
	{
		string T(string key) => Text.Get(key, lang);
		string[] List(string prefix) => Text.List(prefix, lang);

		return new SiteStrings
		{
			Lang = lang,
			Nav = new NavStrings
			{
				Home = T("Nav.Home"),
				Programmes = T("Nav.Programmes"),
				Resources = T("Nav.Resources"),
				Contact = T("Nav.Contact"),
				RequestSupport = T("Nav.RequestSupport")
			},
			Home = new HomeStrings
			{
				Badge = T("Home.Badge"),
				HeroHeading = T("Home.HeroHeading"),
				HeroSub = T("Home.HeroSub"),
				HeroParagraph = T("Home.HeroParagraph"),
				RequestSupport = T("Home.RequestSupport"),
				ExplorePrograms = T("Home.ExplorePrograms"),
				ProgrammesSectionTitle = T("Home.ProgrammesSectionTitle"),
				ProgrammesSectionSub = T("Home.ProgrammesSectionSub"),
				ViewAll = T("Home.ViewAll"),
				ResourcesTitle = T("Home.ResourcesTitle"),
				ResourcesSub = T("Home.ResourcesSub"),
				ViewResources = T("Home.ViewResources"),
				ContactTitle = T("Home.ContactTitle"),
				ContactSub = T("Home.ContactSub"),
				ContactUs = T("Home.ContactUs"),
				ServiceNote = T("Home.ServiceNote")
			},
			Form = new FormStrings
			{
				Title = T("Form.Title"),
				Subtitle = T("Form.Subtitle"),
				ServiceArea = T("Form.ServiceArea"),
				Step1Label = T("Form.Step1Label"),
				Step2Label = T("Form.Step2Label"),
				Step3Label = T("Form.Step3Label"),
				FirstName = T("Form.FirstName"),
				Surname = T("Form.Surname"),
				Phone = T("Form.Phone"),
				Email = T("Form.Email"),
				Area = T("Form.Area"),
				AreaPlaceholder = T("Form.AreaPlaceholder"),
				ContactMethod = T("Form.ContactMethod"),
				ContactPhone = T("Form.ContactPhone"),
				ContactWhatsApp = T("Form.ContactWhatsApp"),
				ContactEmail = T("Form.ContactEmail"),
				ContactVisit = T("Form.ContactVisit"),
				SupportType = T("Form.SupportType"),
				SupportTypePlaceholder = T("Form.SupportTypePlaceholder"),
				Situation = T("Form.Situation"),
				SituationPlaceholder = T("Form.SituationPlaceholder"),
				Urgent = T("Form.Urgent"),
				UrgentYes = T("Form.UrgentYes"),
				UrgentNo = T("Form.UrgentNo"),
				ExtraInfo = T("Form.ExtraInfo"),
				ExtraInfoPlaceholder = T("Form.ExtraInfoPlaceholder"),
				Declaration = T("Form.Declaration"),
				SubmitRequest = T("Form.SubmitRequest"),
				Back = T("Form.Back"),
				Next = T("Form.Next"),
				Required = T("Form.Required")
			},
			Confirmation = new ConfirmationStrings
			{
				Title = T("Confirmation.Title"),
				Sub = T("Confirmation.Sub"),
				RefLabel = T("Confirmation.RefLabel"),
				DateLabel = T("Confirmation.DateLabel"),
				StatusLabel = T("Confirmation.StatusLabel"),
				Status = T("Confirmation.Status"),
				WhatNext = T("Confirmation.WhatNext"),
				Steps = List("Confirmation.Steps"),
				BackHome = T("Confirmation.BackHome"),
				AnotherRequest = T("Confirmation.AnotherRequest")
			},
			SupportTypes = List("SupportTypes")
		};
	}
}
