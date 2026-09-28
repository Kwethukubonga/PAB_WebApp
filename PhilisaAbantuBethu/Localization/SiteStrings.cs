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

	/// <summary>Picks between the English, isiXhosa, and Afrikaans value, the way the
	/// `lang === 'en' ? a : b` ternaries did in the .tsx files.</summary>
	public string Pick(string en, string xh, string af) => Lang switch
	{
		"xh" => xh,
		"af" => af,
		_ => en
	};

	public static SiteStrings For(string lang) => lang switch
	{
		"xh" => Xh,
		"af" => Af,
		_ => En
	};

	public static readonly SiteStrings En = new()
	{
		Lang = "en",
		Nav = new NavStrings
		{
			Home = "Home",
			Programmes = "Our Programmes",
			Resources = "Resources",
			Contact = "Contact",
			RequestSupport = "Request Support"
		},
		Home = new HomeStrings
		{
			Badge = "Philisa Abafazi Bethu: Community Support",
			HeroHeading = "Do you need support?",
			HeroSub = "We are here to help.",
			HeroParagraph = "Philisa Abafazi Bethu supports women, children and families across Cape Town communities. If you need support, this platform gives you a simple and private way to reach out and ask for help.",
			RequestSupport = "Request Support",
			ExplorePrograms = "Explore Programmes",
			ProgrammesSectionTitle = "Our Programmes",
			ProgrammesSectionSub = "We offer a range of free programmes for community members who need support.",
			ViewAll = "View All Programmes",
			ResourcesTitle = "Resources & Information",
			ResourcesSub = "Emergency contacts, community services, and helpful information for you and your family.",
			ViewResources = "View Resources",
			ContactTitle = "Not sure where to start?",
			ContactSub = "Call or email us and we will help guide you to the right support.",
			ContactUs = "Contact Us",
			ServiceNote = "Philisa primarily serves Lavender Hill, Steenberg and surrounding Cape Flats communities in Cape Town."
		},
		Form = new FormStrings
		{
			Title = "Request Support",
			Subtitle = "You do not need to create an account. Simply fill in the form and we will be in touch.",
			ServiceArea = "Philisa primarily serves Lavender Hill, Steenberg and surrounding Cape Flats communities. If you are outside these areas, you may still submit a request and the team will advise where possible.",
			Step1Label = "About You",
			Step2Label = "Support Needed",
			Step3Label = "Confirm & Submit",
			FirstName = "First Name",
			Surname = "Surname",
			Phone = "Phone Number",
			Email = "Email Address (optional)",
			Area = "Your Area / Suburb",
			AreaPlaceholder = "e.g. Lavender Hill, Steenberg, Mitchells Plain…",
			ContactMethod = "How would you prefer us to contact you?",
			ContactPhone = "Phone call",
			ContactWhatsApp = "WhatsApp",
			ContactEmail = "Email",
			ContactVisit = "In-person visit",
			SupportType = "What type of support do you need?",
			SupportTypePlaceholder = "Select a type of support",
			Situation = "Please briefly describe your situation",
			SituationPlaceholder = "Tell us a little about what you are going through and what kind of help you need. There is no right or wrong way to explain: just use your own words.",
			Urgent = "Is this urgent?",
			UrgentYes = "Yes: I need help as soon as possible",
			UrgentNo = "No: I can wait a few days",
			ExtraInfo = "Anything else you would like us to know? (optional)",
			ExtraInfoPlaceholder = "Any additional details that might help us assist you better.",
			Declaration = "I confirm that the information I have provided is accurate. I consent to Philisa Abafazi Bethu Women Centre SA using this information to process my request, in accordance with their privacy policy.",
			SubmitRequest = "Submit Request",
			Back = "Back",
			Next = "Continue",
			Required = "Required"
		},
		Confirmation = new ConfirmationStrings
		{
			Title = "Thank You: Your Request Has Been Received",
			Sub = "Philisa Abafazi Bethu will contact you using the contact details you provided. You do not need to do anything else right now.",
			RefLabel = "Reference Number",
			DateLabel = "Date Submitted",
			StatusLabel = "Status",
			Status = "Received",
			WhatNext = "What happens next?",
			Steps = new[]
			{
				"Your request has been received by the Philisa Abafazi Bethu team.",
				"A team member will review your request and reach out to you using your preferred contact method.",
				"If your request is urgent, the team will prioritise contacting you as soon as possible.",
				"Please keep your reference number in case you need to follow up."
			},
			BackHome = "Back to Home",
			AnotherRequest = "Submit Another Request"
		},
		SupportTypes = new[]
		{
			"Food Assistance",
			"Women's Empowerment",
			"Youth Support",
			"After-school Programme",
			"Senior Care",
			"Baby Saver Programme",
			"Emergency Shelter",
			"Search & Rescue",
			"Social Work Services",
			"General Help / Not Sure"
		}
	};

	public static readonly SiteStrings Xh = new()
	{
		Lang = "xh",
		Nav = new NavStrings
		{
			Home = "Ikhaya",
			Programmes = "Iinkqubo Zethu",
			Resources = "Izibonelelo",
			Contact = "Qhagamshelana",
			RequestSupport = "Cela Uncedo"
		},
		Home = new HomeStrings
		{
			Badge = "Philisa Abafazi Bethu: Inkxaso Yoluntu",
			HeroHeading = "Ingaba ufuna uncedo?",
			HeroSub = "Silapha ukukunceda.",
			HeroParagraph = "I-Philisa Abafazi Bethu ixhasa abafazi, abantwana nezintsapho kwiindawo ezahlukeneyo zase-Kapa. Ukuba ufuna uncedo, le platfom ikunika indlela elula neyimfihlo yokufikelela usizo.",
			RequestSupport = "Cela Uncedo",
			ExplorePrograms = "Jonga Iinkqubo",
			ProgrammesSectionTitle = "Iinkqubo Zethu",
			ProgrammesSectionSub = "Sinikeza iinkqubo ezahlukeneyo zamahhala kumalungu oluntu afuna inkxaso.",
			ViewAll = "Jonga Iinkqubo Zonke",
			ResourcesTitle = "Izibonelelo Nolwazi",
			ResourcesSub = "Iinombolo zomsitho, iinkonzo zoluntu, kunye nolwazi olwexesha lokunceda wena nosapho lwakho.",
			ViewResources = "Jonga Izibonelelo",
			ContactTitle = "Awuqinisekanga ukuqala phi?",
			ContactSub = "Sifonele okanye usithumelele i-imeyili kwaye siza kukunceda ukufumana inkxaso efanelekileyo.",
			ContactUs = "Qhagamshelana Nathi",
			ServiceNote = "I-Philisa ixhasa ngokukodwa iindawo zaseLavender Hill, Steenberg nezingqongileyo ze-Cape Flats eKapa."
		},
		Form = new FormStrings
		{
			Title = "Cela Uncedo",
			Subtitle = "Awufuneki ukwenza i-akhawunti. Gcwalisa ifomu kwaye siza kuqhagamshelana nawe.",
			ServiceArea = "I-Philisa ixhasa ngokukodwa iindawo zaseLavender Hill, Steenberg nezingqongileyo ze-Cape Flats. Ukuba ukhona ngaphandle kwezo ndawo, unokwanela ukuthumela isicelo kwaye iqela liza kunika ingcebiso apho kunokwenzeka.",
			Step1Label = "Ngawe",
			Step2Label = "Uncedo Olufunekayo",
			Step3Label = "Qinisekisa Uthumele",
			FirstName = "Igama",
			Surname = "Ifani",
			Phone = "Inombolo yoMxhentsi",
			Email = "I-imeyili (ayifunekanga)",
			Area = "Indawo yakho",
			AreaPlaceholder = "umz. Lavender Hill, Steenberg, Mitchells Plain…",
			ContactMethod = "Ungathanda ukuba siqhagamshelane nawe njani?",
			ContactPhone = "Umnxeba",
			ContactWhatsApp = "WhatsApp",
			ContactEmail = "I-imeyili",
			ContactVisit = "Ukutyelelwa emntwini",
			SupportType = "Uhlobo luncedo ofuna lona?",
			SupportTypePlaceholder = "Khetha uhlobo loncedo",
			Situation = "Nceda uchaze ngamafutshane imeko yakho",
			SituationPlaceholder = "Sixelele kancinci ngento oyidlayo nokuhlobo kwoncedo ofuna. Akukho ndlela efanelekileyo okanye engafanelekanga: sebenzisa amagama akho.",
			Urgent = "Ngxama na le?",
			UrgentYes = "Ewe: ndifuna uncedo ngokukhawuleza",
			UrgentNo = "Hayi: ndinokumela iintsuku ezimbalwa",
			ExtraInfo = "Kukhona enye into ofuna ukusixelela yona? (ayifunekanga)",
			ExtraInfoPlaceholder = "Nayiphi na iinkcukacha ezithe chatha enokusinceda ukukunceda ngcono.",
			Declaration = "Ndiqinisekisa ukuba ulwazi endilunike lona luyinyaniso. Ndiyavuma ukuba i-Philisa Abafazi Bethu Women Centre SA isebenzise olu lwazi ukuze iqhube isicelo sam, ngokuvumelana nenqubela yabo yobumfihlo.",
			SubmitRequest = "Thumela Isicelo",
			Back = "Emva",
			Next = "Qhubeka",
			Required = "Ifunekayo"
		},
		Confirmation = new ConfirmationStrings
		{
			Title = "Enkosi: Isicelo Sakho Sifunyenwe",
			Sub = "I-Philisa Abafazi Bethu iza kuqhagamshelana nawe iisebenzisa iinkcukacha zoqhagamshelwano ozinike. Akufuneki ukwenza nantoni na ngoku.",
			RefLabel = "Inombolo yeSalathiso",
			DateLabel = "Umhla Wokuthumela",
			StatusLabel = "Imeko",
			Status = "Ifunyenwe",
			WhatNext = "Kwenzeka ntoni ngokulandelayo?",
			Steps = new[]
			{
				"Isicelo sakho sifunyenwe liqela le-Philisa Abafazi Bethu.",
				"Ilungu leqela liza kujonga isicelo sakho kwaye lisondelele nawe ngendlela yokuqhagamshelana oyikhethileyo.",
				"Ukuba isicelo sakho siphuthumayo, iqela liza kuprioritayiza ukukuqhagamshelana nawe ngokukhawuleza.",
				"Gcina inombolo yakho yesalathiso ukuba ufuna ukulandela."
			},
			BackHome = "Buyela Ekhaya",
			AnotherRequest = "Thumela Esinye Isicelo"
		},
		SupportTypes = new[]
		{
			"Inkxaso Yokutya",
			"Inkxaso Yabafazi",
			"Inkxaso Yentsha",
			"Uhlelo Lwemva-kwesikolo",
			"Inkxaso Yabantu Abadala",
			"Uhlelo Lwesisulu Somntwana",
			"Indawo Ephephileyo Yongxamiseko",
			"Uphando Nokuhlangulwa",
			"Iinkonzo Zomsebenzi Womphakathi",
			"Uncedo Ngokubanzi / Andiqinisekanga"
		}
	};

	public static readonly SiteStrings Af = new()
	{
		Lang = "af",
		Nav = new NavStrings
		{
			Home = "Tuis",
			Programmes = "Ons Programme",
			Resources = "Hulpbronne",
			Contact = "Kontak",
			RequestSupport = "Versoek Ondersteuning"
		},
		Home = new HomeStrings
		{
			Badge = "Philisa Abafazi Bethu: Gemeenskapsondersteuning",
			HeroHeading = "Het jy ondersteuning nodig?",
			HeroSub = "Ons is hier om te help.",
			HeroParagraph = "Philisa Abafazi Bethu ondersteun vroue, kinders en gesinne regoor Kaapstad se gemeenskappe. As jy ondersteuning nodig het, bied hierdie platform jou 'n eenvoudige en private manier om uit te reik en hulp te vra.",
			RequestSupport = "Versoek Ondersteuning",
			ExplorePrograms = "Verken Programme",
			ProgrammesSectionTitle = "Ons Programme",
			ProgrammesSectionSub = "Ons bied 'n reeks gratis programme vir gemeenskapslede wat ondersteuning nodig het.",
			ViewAll = "Bekyk Alle Programme",
			ResourcesTitle = "Hulpbronne & Inligting",
			ResourcesSub = "Noodkontakte, gemeenskapsdienste, en nuttige inligting vir jou en jou gesin.",
			ViewResources = "Bekyk Hulpbronne",
			ContactTitle = "Onseker waar om te begin?",
			ContactSub = "Skakel of e-pos ons en ons sal jou help om die regte ondersteuning te vind.",
			ContactUs = "Kontak Ons",
			ServiceNote = "Philisa bedien hoofsaaklik Lavender Hill, Steenberg en die omliggende Cape Flats-gemeenskappe in Kaapstad."
		},
		Form = new FormStrings
		{
			Title = "Versoek Ondersteuning",
			Subtitle = "Jy hoef nie 'n rekening te skep nie. Vul eenvoudig die vorm in en ons sal met jou in verbinding tree.",
			ServiceArea = "Philisa bedien hoofsaaklik Lavender Hill, Steenberg en die omliggende Cape Flats-gemeenskappe. As jy buite hierdie areas is, kan jy steeds 'n versoek indien en die span sal jou waar moontlik adviseer.",
			Step1Label = "Oor Jou",
			Step2Label = "Ondersteuning Benodig",
			Step3Label = "Bevestig & Dien In",
			FirstName = "Voornaam",
			Surname = "Van",
			Phone = "Foonnommer",
			Email = "E-posadres (opsioneel)",
			Area = "Jou Area / Voorstad",
			AreaPlaceholder = "bv. Lavender Hill, Steenberg, Mitchells Plain…",
			ContactMethod = "Hoe verkies jy dat ons jou kontak?",
			ContactPhone = "Telefoonoproep",
			ContactWhatsApp = "WhatsApp",
			ContactEmail = "E-pos",
			ContactVisit = "Persoonlike besoek",
			SupportType = "Watter tipe ondersteuning het jy nodig?",
			SupportTypePlaceholder = "Kies 'n tipe ondersteuning",
			Situation = "Beskryf asseblief kortliks jou situasie",
			SituationPlaceholder = "Vertel ons 'n bietjie oor wat jy tans deurmaak en watter soort hulp jy nodig het. Daar is nie 'n regte of verkeerde manier om dit te verduidelik nie: gebruik gerus jou eie woorde.",
			Urgent = "Is dit dringend?",
			UrgentYes = "Ja: ek het so gou moontlik hulp nodig",
			UrgentNo = "Nee: ek kan 'n paar dae wag",
			ExtraInfo = "Enigiets anders wat jy vir ons wil laat weet? (opsioneel)",
			ExtraInfoPlaceholder = "Enige bykomende besonderhede wat ons kan help om jou beter by te staan.",
			Declaration = "Ek bevestig dat die inligting wat ek verskaf het akkuraat is. Ek gee toestemming dat Philisa Abafazi Bethu Women Centre SA hierdie inligting gebruik om my versoek te verwerk, in ooreenstemming met hul privaatheidsbeleid.",
			SubmitRequest = "Dien Versoek In",
			Back = "Terug",
			Next = "Gaan Voort",
			Required = "Verpligtend"
		},
		Confirmation = new ConfirmationStrings
		{
			Title = "Dankie: Jou Versoek Is Ontvang",
			Sub = "Philisa Abafazi Bethu sal jou kontak deur die kontakbesonderhede wat jy verskaf het te gebruik. Jy hoef op die oomblik niks anders te doen nie.",
			RefLabel = "Verwysingsnommer",
			DateLabel = "Datum Ingedien",
			StatusLabel = "Status",
			Status = "Ontvang",
			WhatNext = "Wat gebeur volgende?",
			Steps = new[]
			{
				"Jou versoek is deur die Philisa Abafazi Bethu-span ontvang.",
				"'n Spanlid sal jou versoek hersien en met jou in verbinding tree deur jou verkose kontakmetode.",
				"As jou versoek dringend is, sal die span prioriteit gee om so gou moontlik met jou in verbinding te tree.",
				"Hou asseblief jou verwysingsnommer byderhand indien jy moet opvolg."
			},
			BackHome = "Terug na Tuisblad",
			AnotherRequest = "Dien Nog 'n Versoek In"
		},
		SupportTypes = new[]
		{
			"Voedselbystand",
			"Bemagtiging van Vroue",
			"Jeugondersteuning",
			"Naskoolse Program",
			"Sorg vir Bejaardes",
			"Baba-redder-program",
			"Nood-skuiling",
			"Soek en Redding",
			"Maatskaplike Werk Dienste",
			"Algemene Hulp / Onseker"
		}
	};
}
