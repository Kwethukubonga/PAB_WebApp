using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Localization;

public record ProgrammeText(
    string Title,
    string Tagline,
    string Description,
    string Overview,
    string[] Objectives,
    string[] Activities);

// isiXhosa text for the programmes in Data/SiteData.cs, keyed by Programme.Id.
// A programme missing here falls back to its English text.
public static class ProgrammeTranslations
{
    private static readonly Dictionary<string, ProgrammeText> Xh = new()
    {
        ["womens-empowerment"] = new(
            "Ukuxhobisa Abafazi",
            "Ukwakha amandla ngaphakathi",
            "Sixhasa abafazi ngokuphuhlisa izakhono, uncedo lwezomthetho, nokuzimela ngezoqoqosho — kuba abafazi abaxhobisiweyo bakha uluntu olomeleleyo.",
            "Inkqubo yethu yokuxhobisa abafazi ikumbindi wayo yonke into esiyenzayo. Sikholelwa ekubeni xa abafazi bexhobisiwe, uluntu lonke luyaphumelela. Ngoluhlu olubanzi lweenkonzo, sinceda abafazi ukuba babuyisele isidima sabo, bakhe ukuzimela ngezemali, baze babe ziinkokeli koluntu lwabo.",
            new[]
            {
                "Ukubonelela ngoqeqesho lwezakhono nemfundo yobugcisa",
                "Ukunikezela ngoncedo lwezomthetho neenkomfa zokwazisa ngamalungelo",
                "Ukuxhasa abasindileyo kubundlobongela obusekelwe kubulili",
                "Ukwakha amandla obunkokeli nobonoyilo-mashishini"
            },
            new[]
            {
                "Iindibano zeveki nganye zokuthunga, izandla, nokuthunga iimpahla",
                "Iiseshoni zokwazi ngemali nezamaqela okonga",
                "Iiklinikhi zomthetho kunye neengqwetha ezinceda simahla",
                "Iindibano zamaqela oxhaso noluleko ngentlungu yengqondo",
                "Umbhiyozo wonyaka woMhla Wabafazi neembasa"
            }),

        ["youth-programme"] = new(
            "Inkqubo Yolutsha",
            "Ukubasa ubuchule bolutsha",
            "Sixhobisa ulutsha oluneminyaka eyi-15–35 ngezakhono, ukhokelo, namathuba okophula umjikelo wobuhlwempu nokwakha ikamva elinenjongo.",
            "Inkqubo yethu Yolutsha yakha iindawo ezikhuselekileyo apho ulutsha lunokuphuhlisa amandla aluwo. Ngokhokelo, uqeqesho lwezakhono, noluleko lobomi, sinceda ulutsha lujongane nemiceli-mngeni lwakhe ikamva elinokuzingca ngalo.",
            new[]
            {
                "Ukunciphisa ukungasebenzi kolutsha ngokuphuhlisa izakhono ezivunyiweyo",
                "Ukubonelela ngokhokelo noluleko lobomi",
                "Ukujongana nokusetyenziswa gwenxa kweziyobisi nokuziphatha okunobungozi",
                "Ukwakha uxanduva loluntu nokuzingca ngoluntu"
            },
            new[]
            {
                "Iiseshoni zokhokelo zeveki nganye namaqela oxoxo",
                "Ukubhala i-CV, ukulungiselela udliwano-ndlebe, nokufumana imisebenzi",
                "Imisitho yezemidlalo nokuzonwabisa",
                "Iindibano zobugcisa, inkcubeko nokuyila",
                "Inkampu yonyaka yobunkokeli bolutsha"
            }),

        ["after-school"] = new(
            "Iinkqubo Zangasemva Kwesikolo",
            "Ukufunda ngaphaya kweklasi",
            "Iindawo zokufunda ezikhuselekileyo, ezilungelelanisiweyo zabantwana emva kwexesha lesikolo — kunye noncedo lwezifundo, imisebenzi yokwandisa, nesidlo sasemva kwemini esishushu.",
            "Inkqubo yethu Yangasemva Kwesikolo inika indawo elungelelanisiweyo, ekhuselekileyo, nekhuthaza abantwana beBanga loku-1 ukuya kweli-12. Abafundisi abaqeqeshiweyo nabavolontiya bancedisa abantwana ngomsebenzi wasekhaya, ukufunda, nemathematika, ngelixa banikezela ngemisebenzi yokwandisa ekhuthaza ukuyila.",
            new[]
            {
                "Ukuphucula intsebenzo yezifundo kwizifundo eziphambili",
                "Ukubonelela ngendawo ekhuselekileyo, ejongwayo emva kwexesha lesikolo",
                "Ukukhulisa ukuyila ngobugcisa nenkcubeko",
                "Ukwakha izenzo ezilungileyo zempilo nezakhono ezibalulekileyo zobomi"
            },
            new[]
            {
                "Uncedo lomsebenzi wasekhaya wemihla ngemihla noncedo lwamaqela amancinci",
                "Amaqela okufunda nokundwendwela ilayibrari yoluntu",
                "Ii-eksperimenti zesayensi nemisebenzi ye-STEM",
                "Iiseshoni zobugcisa, umdlalo weqonga nomculo",
                "Inkqubo yesidlo esincinci esinesondlo sasemva kwemini"
            }),

        ["senior-programme"] = new(
            "Inkqubo Yabadala",
            "Ukuhlonipha abadala bethu",
            "Sikhathalela amalungu oluntu asele ekhulile ngokunxibelelana noluntu, imisebenzi yempilo, noncedo olusebenzayo — kuba wonke umntu osele ekhulile ufanelwe sisidima.",
            "Inkqubo yethu Yabadala ihlonipha ubulumko nesidima samalungu oluntu asele ekhulile. Sibonelela ngokuhlala nabo, ngoncedo lwempilo, nangoncedo olusebenzayo ukuqinisekisa ukuba abadala bethu bayaguga benesidima, benxibelelene, bekhathalelwe.",
            new[]
            {
                "Ukulwa nokuzimela nesizungu phakathi kwabadala",
                "Ukuxhasa ukufikelela kwezempilo nakwimivuzo yentlalontle",
                "Ukubonelela ngoncedo olusebenzayo kwiimfuno zemihla ngemihla",
                "Ukubhiyozela nokugcina ilifa lenkcubeko"
            },
            new[]
            {
                "Iindibano zentlalo yabadala neentsuku zeti zeveki nganye",
                "Uhlolo lwempilo neekliniki zempilo",
                "Uncedo ekufakeni izicelo zemivuzo yeSASSA",
                "Iimeko zokuxelelana amabali phakathi kwezizukulwana",
                "Utyelelo lwasekhaya kubadala abangakwaziyo ukuhamba"
            }),

        ["community-feeding"] = new(
            "Ukondla Uluntu",
            "Akukho mntu ulala elambile apha",
            "Sibonelela ngokutya okushushu, okunesondlo neeparcel zokutya zenyanga ngenyanga kwiintsapho nabantu abasengozini abajongene nokungabikho kokutya koluntu lwethu.",
            "Inkqubo Yokondla Uluntu luphendulo lwethu kunxaki yendlala. Siqhuba iindawo zokondla zemihla ngemihla size sabelane ngeeparcel zokutya kwiintsapho ezikwimfuno, siqinisekisa ukuba akukho lungu loluntu lethu elilala lilambile.",
            new[]
            {
                "Ukuphelisa indlala phakathi kwamalungu oluntu asengozini",
                "Ukubonelela ngokutya okunesondlo, okulinganayo yonke imihla",
                "Ukwabela iiparcel zokutya kubantu abangakwaziyo ukuphuma ekhaya",
                "Ukufundisa ngesondlo nemikhwa yokuvelisa ukutya ezinzileyo"
            },
            new[]
            {
                "Ikhitshi loluntu lemihla ngemihla elondla ukutya okushushu okungaphezu kwe-200",
                "Ukwabelana ngeeparcel zokutya inyanga nenyanga",
                "Igadi yemifuno yoluntu nokuvelisa ukutya",
                "Iiwokishop zemfundo yesondlo nokupheka",
                "Iinkqubo zokondla ngexesha leeholide nemibhiyozo"
            }),

        ["baby-saver"] = new(
            "Umsindisi Wosana",
            "Ubomi bonke bubaluleke",
            "Sixhasa oomama abancinci nabasengozini ngokukhathalela, izixhobo ezibalulekileyo, noncedo lwentsapho — siqinisekisa ukuba yonke intsana ifumana ukuqala kokulungileyo.",
            "Inkqubo Yomsindisi Wosana ibonelela ngendlela yokuphila koomama abancinci nabasengozini. Sinika indawo ekhuselekileyo koomama abasekwexingwe kwaye siqinisekisa ukuba yonke intsana ifumana izinto ezisisiseko ezifunekayo ukuze ikhule.",
            new[]
            {
                "Ukuthintela ukulahlwa nokuxhatshazwa kwezintsana",
                "Ukuxhasa oomama abancinci nabokuqala",
                "Ukubonelela ngezinto ezisisiseko zosana noncedo lwesondlo",
                "Ukunxibelelanisa iintsapho neenkonzo zentlalontle"
            },
            new[]
            {
                "Iiklasi zokukhulisa abantwana namaqela oxhaso lwabalingane",
                "Ukwabelana ngezinto ezisisiseko zosana (iidayapha, ifomula, iimpahla)",
                "Uncedo lokuncancisa noluleko lwe-lactation",
                "Ungenelelo lwexesha lengxaki koomama abasengxakini",
                "Ukuququzelela ukwamkelwa nokukhuliswa kwabantwana ngentsapho"
            }),

        ["safe-houses"] = new(
            "Amakhaya Okhuseleko Aphuthumayo",
            "Ukhuseleko, rhoqo",
            "Sibonelela ngendawo yokuhlala ekhuselekileyo, eyimfihlo nangoncedo olukhawulezileyo koomama nabantwana abasaba ubundlobongela basekhaya neemeko ezibeka ubomi esichengeni.",
            "Amakhaya ethu Okhuseleko Aphuthumayo abonelela ngendawo yokuhlala ekhawulezileyo koomama nabantwana abasaba ubundlobongela basekhaya, ukuxhatshazwa neemeko eziyingozi. Sibonelela ngendawo ekhuselekileyo, ngoluleko, nangeendlela eziya kukhuseleko nokuzimela okuhlala kade.",
            new[]
            {
                "Ukubonelela ngendawo ekhuselekileyo ekhawulezileyo kwabasindileyo",
                "Ukunikezela ngoluleko lwentlungu yengqondo noncedo lwengqondo nentlalo",
                "Ukuncedisa ngemiyalelo yomthetho yokhuseleko",
                "Ukwakha iindlela eziya kubomi obuzimeleyo, obukhuselekileyo"
            },
            new[]
            {
                "Umgca wengxaki nokusabela okuphuthumayo iiyure ezingama-24 imihla yonke",
                "Indawo yokuhlala yekhaya lokhuseleko eyimfihlo",
                "Ululeko lomntu ngamnye nolwamaqela ngentlungu yengqondo",
                "Ukuxhasa ngomthetho nokuxhasa enkundleni",
                "Ukubuyisela nokwakha ngokutsha izakhono zobomi"
            }),

        ["search-rescue"] = new(
            "Ukukhangela Nokuhlangula",
            "Siyakufumana. Siyakubuyisela ekhaya.",
            "Iqela labavolontiya boluntu eliqeqeshiweyo elisabela kumatyala abantu abangekho nakwiimeko eziphuthumayo zoluntu — esebenza namapolisa e-SAPS neenkonzo zentlalontle.",
            "Iqela lethu lokuKhangela nokuHlangula liyiyunithi yabavolontiya eqeqeshiweyo elisabela kumatyala abantu abangekho neemeko eziphuthumayo zoluntu. Sisebenza kunye namapolisa neenkonzo zentlalontle ukuqinisekisa ukuba abantu abasengozini bafunyanwa baxhaswe.",
            new[]
            {
                "Ukufumana abantu abangekho koluntu",
                "Ukuxhasa iintsapho zabantu abangekho ngokweemvakalelo",
                "Ukuququzelelana ne-SAPS neenkonzo zentlalontle",
                "Ukuqeqesha abavolontiya boluntu kubuchule bokukhangela"
            },
            new[]
            {
                "Ukuthunyelwa kokusabela okuphuthumayo iiyure ezingama-24 imihla yonke",
                "Uqeqesho lwabavolontiya neyokuzilolonga entsimini",
                "Iinkqubo zokwazisa uluntu nokuthintela",
                "Amaqela oxhaso lweentsapho zabantu abangekho",
                "Ukubhalisa abantu abangekho nedatha yedijithali"
            }),

        ["social-work"] = new(
            "Iinkonzo Zezentlalontle",
            "Ukhathalelo lobuchule, intliziyo yomntu",
            "Abasebenzi bezentlalontle abaqeqeshiweyo abanxibelelanisa iintsapho nezixhobo, imivuzo, ululeko, neenkonzo ezibalulekileyo ekufuneka bekhule.",
            "Abasebenzi bethu bezentlalontle abaqeqeshiweyo babonelela ngoncedo lobuchule, olunemfesane kubantu nakwiintsapho ezijongene neengxaki ezinzima. Ukusuka kwizicelo zemivuzo ukuya kululeko lwentsapho, iqela lethu linxibelelanisa abantu nezixhobo abazidingayo.",
            new[]
            {
                "Ukunikezela ngovavanyo lobuchule lwezentlalontle",
                "Ukunxibelelanisa iintsapho nemivuzo yorhulumente neenkonzo",
                "Ukubonelela ngonyango lwentsapho nokuxazulula ungquzulwano",
                "Ukuxhasa ukukhuselwa nentlalontle yabantwana"
            },
            new[]
            {
                "Iingxoxo zomntu ngamnye nomsebenzi wezentlalontle",
                "Uncedo lokufaka izicelo nezibheno zemivuzo yeSASSA",
                "Uxhaso nokumelwa enkundleni yentsapho",
                "Uphando lwentlalontle yabantwana neenkqubo zoxhaso",
                "Uvavanyo lotyelelo lwasekhaya nolawulo lwamatyala"
            }),
    };

    private static ProgrammeText? Text(Programme p, SiteStrings s) =>
        s.IsEnglish ? null : Xh.GetValueOrDefault(p.Id);

    public static string TitleFor(this Programme p, SiteStrings s) => Text(p, s)?.Title ?? p.Title;
    public static string TaglineFor(this Programme p, SiteStrings s) => Text(p, s)?.Tagline ?? p.Tagline;
    public static string DescriptionFor(this Programme p, SiteStrings s) => Text(p, s)?.Description ?? p.Description;
    public static string OverviewFor(this Programme p, SiteStrings s) => Text(p, s)?.Overview ?? p.Overview;
    public static IReadOnlyList<string> ObjectivesFor(this Programme p, SiteStrings s) => (IReadOnlyList<string>?)Text(p, s)?.Objectives ?? p.Objectives;
    public static IReadOnlyList<string> ActivitiesFor(this Programme p, SiteStrings s) => (IReadOnlyList<string>?)Text(p, s)?.Activities ?? p.Activities;
}
