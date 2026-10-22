namespace PhilisaAbantuBethu.Data;

/// <summary>One line in a resource category, in all three languages.</summary>
public record ResourceEntry(string En, string Xh, string Af)
{
    public string For(string lang) => lang switch { "xh" => Xh, "af" => Af, _ => En };
}

/// <summary>
/// A group of contacts on the Resources page. <see cref="Icon"/> is a Material Symbols name,
/// not an emoji, so it is drawn in the same icon family as the rest of the site (E5).
/// </summary>
public record ResourceCategory(string Id, ResourceEntry Name, string Icon, string Colour, List<ResourceEntry> Items);

public record ResourceQuestion(ResourceEntry Question, ResourceEntry Answer);

/// <summary>
/// The emergency numbers, services and questions on the Resources page. They used to be written
/// into Resources.cshtml, which meant the page was the only thing that could show them. Keeping
/// them here lets the page, /api/resources and the offline cache all read the same list, and a
/// number can be corrected without touching any markup (A8).
/// </summary>
public static class ResourceData
{

    /// <summary>The numbers someone in danger needs first. Shown before everything else and cached offline.</summary>
    public static readonly ResourceCategory Emergency = new(
        "emergency",
        new ResourceEntry("Emergency Contacts", "Amanombolo Ongxamiseko", "Noodkontakte"),
        "emergency_home",
        "#DC2626",
        new()
        {
            new("SAPS Emergency: 10111", "ISAPS Yongxamiseko: 10111", "SAPD Noodgeval: 10111"),
            new("Ambulance / Fire: 107 (landline) or 021 535 1100",
                "I-Ambulensi / Umlilo: 107 (umnxeba womhlaba) okanye 021 535 1100",
                "Ambulans / Brand: 107 (vaste lyn) of 021 535 1100"),
            new("Gender-Based Violence Helpline: 0800 428 428 (24/7)",
                "Icandelo loncedo loNdlwane ngokoBunini: 0800 428 428 (24/7)",
                "Hulplyn vir Gender-gebaseerde Geweld: 0800 428 428 (24/7)"),
            new("Childline South Africa: 116 (free, 24/7)",
                "Childline eMzantsi Afrika: 116 (mahhala, 24/7)",
                "Childline Suid-Afrika: 116 (gratis, 24/7)"),
            new("Suicide Crisis Line: 0800 567 567",
                "Umgca wokufa ngobomi: 0800 567 567",
                "Selfmoord-krisislyn: 0800 567 567")
        });

    public static readonly List<ResourceCategory> Categories = new()
    {
        Emergency,

        new ResourceCategory(
            "community-services",
            new ResourceEntry("Community Services", "Iinkonzo Zoluntu", "Gemeenskapsdienste"),
            "diversity_3",
            "#6B21A8",
            new()
            {
                new("SASSA Grant Offices: 0800 601 011",
                    "Iihhovisi ze-SASSA Grant: 0800 601 011",
                    "SASSA-toelaagkantore: 0800 601 011"),
                new("City of Cape Town Social Services: 021 400 4685",
                    "Iinkonzo Zentlalontle zeDolophu yaseKapa: 021 400 4685",
                    "Stad Kaapstad Maatskaplike Dienste: 021 400 4685"),
                new("Department of Social Development: 0800 220 250",
                    "ISebe loPhuhliso lweNtlalo: 0800 220 250",
                    "Departement van Maatskaplike Ontwikkeling: 0800 220 250"),
                new("Community Development Workers (CDW): Contact your ward councillor",
                    "Abasebenzi boPhuhliso lweNdawo (CDW): Qhagamshelana nekhonselarha wendawo yakho",
                    "Gemeenskapsontwikkelingswerkers (CDW): Kontak jou wardraadslid")
            }),

        new ResourceCategory(
            "womens-rights",
            new ResourceEntry("Women's Rights", "Amalungelo Abafazi", "Vroueregte"),
            "balance",
            "#7C3AED",
            new()
            {
                new("Legal Aid South Africa: 0800 110 110",
                    "Inkxaso Yezomthetho eMzantsi Afrika: 0800 110 110",
                    "Regshulp Suid-Afrika: 0800 110 110"),
                new("Commission for Gender Equality: 0800 007 709",
                    "IKomishini yoLinganiso lweZini: 0800 007 709",
                    "Kommissie vir Geslagsgelykheid: 0800 007 709"),
                new("Rape Crisis Cape Town: 021 447 9762",
                    "Ulwaphulo lwesondo eKapa: 021 447 9762",
                    "Verkragtingskrisis Kaapstad: 021 447 9762"),
                new("Women Legal Centre: 021 421 5505",
                    "Iziko lwabaFazi Zemthetho: 021 421 5505",
                    "Vroue Regsentrum: 021 421 5505"),
                new("TEARS Foundation: *134*7355# (free SMS)",
                    "I-TEARS Foundation: *134*7355# (i-SMS mahhala)",
                    "TEARS-stigting: *134*7355# (gratis SMS)")
            }),

        new ResourceCategory(
            "parenting",
            new ResourceEntry("Parenting Resources", "Izibonelelo Zabazali", "Ouerskaphulpbronne"),
            "child_care",
            "#DB2777",
            new()
            {
                new("Parent Advice Line: 0800 567 567",
                    "Umgca Weengcebiso Zabazali: 0800 567 567",
                    "Ouer-adieslyn: 0800 567 567"),
                new("South African Breastfeeding Association: 021 671 3907",
                    "Umanyano Wokunyiswa eMzantsi Afrika: 021 671 3907",
                    "Suid-Afrikaanse Borsvoedingsvereniging: 021 671 3907"),
                new("Early Childhood Development (ECD) Registration: DSD offices",
                    "Ubhaliso lwePhuhliso lwaBasana Abancinane (ECD): Iihhovisi ze-DSD",
                    "Vroeë Kinderontwikkeling (ECD) Registrasie: DSD-kantore"),
                new("Children Court: 021 469 0800",
                    "Inkundla Yabantwana: 021 469 0800",
                    "Kinderhof: 021 469 0800")
            }),

        new ResourceCategory(
            "mental-health",
            new ResourceEntry("Mental Health", "Impilo Yengqondo", "Geestesgesondheid"),
            "psychology",
            "#0F766E",
            new()
            {
                new("SADAG Mental Health Line: 011 234 4837",
                    "Umgca weNtlalontle yengqondo we-SADAG: 011 234 4837",
                    "SADAG Geestesgesondheidslyn: 011 234 4837"),
                new("Lifeline South Africa: 0861 322 322",
                    "I-Lifeline eMzantsi Afrika: 0861 322 322",
                    "Lifeline Suid-Afrika: 0861 322 322"),
                new("South African Depression & Anxiety Group: 0800 456 789",
                    "Iqela laMafa neNkathalo eMzantsi Afrika: 0800 456 789",
                    "Suid-Afrikaanse Depressie- en Angsgroep: 0800 456 789"),
                new("Narcotics Anonymous: 083 900 6962",
                    "I-Narcotics Anonymous: 083 900 6962",
                    "Narcotics Anonymous: 083 900 6962"),
                new("Alcoholics Anonymous: 0861 435 722",
                    "I-Alcoholics Anonymous: 0861 435 722",
                    "Alcoholics Anonymous: 0861 435 722")
            })
    };

    public static readonly List<ResourceQuestion> Questions = new()
    {
        new(new("How do I apply for a SASSA grant?",
                "Ndifake njani isicelo se-SASSA grant?",
                "Hoe doen ek aansoek om 'n SASSA-toelaag?"),
            new("Visit your nearest SASSA office or speak to our social work team who can assist you with the application process.",
                "Tyelela ihhovisi le-SASSA ekufutshane nawe okanye uthethe neqela lethu lomsebenzi womphakathi elinokukunceda nenkqubo yesicelo.",
                "Besoek jou naaste SASSA-kantoor of praat met ons maatskaplike werk-span wat jou met die aansoekproses kan help.")),

        new(new("Can men access support from Philisa?",
                "Amadoda anokufumana inkxaso kwa-Philisa?",
                "Kan mans ondersteuning van Philisa kry?"),
            new("Our services are open to all community members who need help, regardless of gender.",
                "Iphothali yethu yenkxaso ivulekele onke amalungu oluntu afuna uncedo, nokuba ngumfazi okanye indoda.",
                "Ons dienste is oop vir alle gemeenskapslede wat hulp nodig het, ongeag geslag.")),

        new(new("Is my request kept private?",
                "Ingaba isicelo sam sigcinwa siyimfihlo?",
                "Word my versoek privaat gehou?"),
            new("Yes: all submissions are treated with strict confidentiality. Your information is only shared within the Philisa team for the purpose of assisting you.",
                "Ewe: zonke iintlelo ziqhatshelwa ngemfihlo epheleleyo. Ulwazi lwakho luhlanganyela iqela le-Philisa kuphela ukuze likuncede.",
                "Ja: alle indienings word met streng vertroulikheid hanteer. Jou inligting word slegs binne die Philisa-span gedeel met die doel om jou by te staan.")),

        new(new("How long does it take to hear back?",
                "Kuthatha ixesha elingakanani ukuva impendulo?",
                "Hoe lank neem dit om terugvoer te kry?"),
            new("The team typically responds within 2–5 business days. Urgent requests are prioritised.",
                "Iqela ngokuqhelekileyo liphendula phakathi kweentsuku ezi-2–5 zomsebenzi. Izicelo eziphuthumayo ziprioritishiwe.",
                "Die span reageer gewoonlik binne 2–5 besigheidsdae. Dringende versoeke word geprioritiseer.")),

        new(new("Do I need to live in Lavender Hill or Steenberg to get help?",
                "Ngaba ndifanele ukuhlala eLavender Hill okanye eeSteenberg ukuze ndifumane uncedo?",
                "Moet ek in Lavender Hill of Steenberg woon om hulp te kry?"),
            new("No. While Philisa primarily serves the Cape Flats area, you may still submit a request from outside these areas and the team will advise where possible.",
                "Hayi. Nangona i-Philisa ixhasa ngokukodwa indawo ye-Cape Flats, unokwanela ukuthumela isicelo ngaphandle kwezo ndawo kwaye iqela liza kunika ingcebiso apho kunokwenzeka.",
                "Nee. Alhoewel Philisa hoofsaaklik die Cape Flats-area bedien, kan jy steeds 'n versoek van buite hierdie areas indien en die span sal jou waar moontlik adviseer.")),

        // Part of the quick exit work (G4): leaving the page is not enough if the visit is still
        // in the browser's history.
        new(new("How do I remove this site from my browser history?",
                "Ndiyisusa njani le sayithi kwimbali yesikhangeli sam?",
                "Hoe verwyder ek hierdie werf uit my blaaigeskiedenis?"),
            new("Press Ctrl+Shift+Delete (or on a phone, open your browser menu, then History) and clear your browsing history. Browsing in a private or incognito window keeps no history at all. If the phone is shared or watched, a friend's phone or a public computer at a library is safer.",
                "Cofa u-Ctrl+Shift+Delete (okanye kwifowuni, vula imenyu yesikhangeli, emva koko uye kwiHistory) uze ususe imbali yokhangelo lwakho. Ukukhangela kwifestile eyimfihlo okanye ye-incognito akugcini nto konke konke. Ukuba ifowuni isetyenziswa ngabanye okanye ijongwe, ifowuni yomhlobo okanye ikhompyutha yasethala leencwadi ikhuselekile ngakumbi.",
                "Druk Ctrl+Shift+Delete (of op 'n foon, maak jou blaaier se kieslys oop en kies Geskiedenis) en maak jou blaaigeskiedenis skoon. 'n Privaat- of incognito-venster hou glad nie geskiedenis nie. As die foon gedeel of dopgehou word, is 'n vriend se foon of 'n publieke rekenaar by 'n biblioteek veiliger."))
    };

}
