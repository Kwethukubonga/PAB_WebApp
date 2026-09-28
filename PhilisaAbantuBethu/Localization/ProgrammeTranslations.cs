using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Localization;

public record ProgrammeText(
    string Title,
    string Tagline,
    string Description,
    string Overview,
    string[] Objectives,
    string[] Activities);

// isiXhosa and Afrikaans text for the programmes in Data/SiteData.cs, keyed by Programme.Id.
// A programme missing from a language's dictionary falls back to its English text.
public static class ProgrammeTranslations
{
    private static readonly Dictionary<string, ProgrammeText> Xh = new()
    {
        ["womens-empowerment"] = new(
            "Ukuxhobisa Abafazi",
            "Ukwakha amandla ngaphakathi",
            "Sixhasa abafazi ngokuphuhlisa izakhono, uncedo lwezomthetho, nokuzimela ngezoqoqosho, kuba abafazi abaxhobisiweyo bakha uluntu olomeleleyo.",
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
            "Iindawo zokufunda ezikhuselekileyo, ezilungelelanisiweyo zabantwana emva kwexesha lesikolo, kunye noncedo lwezifundo, imisebenzi yokwandisa, nesidlo sasemva kwemini esishushu.",
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
            "Sikhathalela amalungu oluntu asele ekhulile ngokunxibelelana noluntu, imisebenzi yempilo, noncedo olusebenzayo, kuba wonke umntu osele ekhulile ufanelwe sisidima.",
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
            "Sixhasa oomama abancinci nabasengozini ngokukhathalela, izixhobo ezibalulekileyo, noncedo lwentsapho, siqinisekisa ukuba yonke intsana ifumana ukuqala kokulungileyo.",
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
            "Iqela labavolontiya boluntu eliqeqeshiweyo elisabela kumatyala abantu abangekho nakwiimeko eziphuthumayo zoluntu, esebenza namapolisa e-SAPS neenkonzo zentlalontle.",
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

    private static readonly Dictionary<string, ProgrammeText> Af = new()
    {
        ["womens-empowerment"] = new(
            "Bemagtiging van Vroue",
            "Krag van binne bou",
            "Ons ondersteun vroue deur vaardigheidsontwikkeling, regsbystand, en ekonomiese onafhanklikheid, want bemagtigde vroue bou sterker gemeenskappe.",
            "Ons Bemagtiging van Vroue-program is die hart van alles wat ons doen. Ons glo dat wanneer vroue bemagtig word, hele gemeenskappe floreer. Deur 'n omvattende reeks dienste help ons vroue om hul waardigheid te herwin, finansiële onafhanklikheid op te bou, en leiers in hul gemeenskappe te word.",
            new[]
            {
                "Verskaf vaardigheidsopleiding en beroepsonderrig",
                "Bied regsbystand en bewusmakingswerkswinkels oor regte",
                "Ondersteun oorlewendes van gender-gebaseerde geweld",
                "Bou leierskap- en entrepreneurskapvermoë"
            },
            new[]
            {
                "Weeklikse naald-, handwerk- en kleremaakwerkswinkels",
                "Finansiële geletterdheid- en spaargroepsessies",
                "Regskliniek met pro-bono prokureurs",
                "Steungroepbyeenkomste en traumaberading",
                "Jaarlikse Vrouedag-viering en toekennings"
            }),

        ["youth-programme"] = new(
            "Jeugprogram",
            "Ontketen potensiaal in jong mense",
            "Ons rus jong mense tussen 15–35 jaar toe met vaardighede, mentorskap, en geleenthede om die siklus van armoede te deurbreek en doelgerigte toekomste te bou.",
            "Ons Jeugprogram skep veilige ruimtes waar jong mense hul potensiaal kan ontwikkel. Deur mentorskap, vaardigheidsopleiding, en lewensafrigting help ons die jeug om uitdagings te oorkom en toekomste te bou waarop hulle trots kan wees.",
            new[]
            {
                "Verminder jeugwerkloosheid deur geakkrediteerde vaardigheidsontwikkeling",
                "Verskaf mentorskap en lewensafrigting",
                "Spreek dwelmmisbruik en riskante gedrag aan",
                "Bou burgerlike verantwoordelikheid en gemeenskapstrots"
            },
            new[]
            {
                "Weeklikse mentorskapkringe en groepsessies",
                "CV-skryf, onderhoudvoorbereiding, en werkplasing",
                "Sport- en ontspanningsgeleenthede",
                "Kuns-, kultuur- en kreatiewe werkswinkels",
                "Jaarlikse jeugleierskapkamp"
            }),

        ["after-school"] = new(
            "Naskoolse Programme",
            "Leer buite die klaskamer",
            "Veilige, gestruktureerde leeromgewings vir kinders na skoolure, met opvoedkundige bystand, verrykingsaktiwiteite, en 'n warm middagete.",
            "Ons Naskoolse Program bied 'n gestruktureerde, veilige en stimulerende omgewing vir kinders van graad 1–12. Opgeleide onderrigters en vrywilligers help kinders met huiswerk, lees, en wiskunde, terwyl hulle ook verrykingsaktiwiteite aanbied wat kreatiwiteit koester.",
            new[]
            {
                "Verbeter akademiese prestasie in kernvakke",
                "Verskaf 'n veilige, toesig-omgewing na skoolure",
                "Koester kreatiwiteit deur kuns en kultuur",
                "Bou gesonde gewoontes en noodsaaklike lewensvaardighede"
            },
            new[]
            {
                "Daaglikse huiswerkbystand en klein-groep-onderrig",
                "Leesklubs en gemeenskapsbiblioteekbesoeke",
                "Wetenskap-eksperimente en STEM-aktiwiteite",
                "Kuns-, drama- en musieksessies",
                "Voedsame middagete-program"
            }),

        ["senior-programme"] = new(
            "Seniorprogram",
            "Ons eer ons bejaardes",
            "Ons versorg ouer gemeenskapslede deur sosiale verbintenis, welsynsaktiwiteite, en praktiese ondersteuning, want elke bejaarde verdien waardigheid.",
            "Ons Seniorprogram eer die wysheid en waardigheid van ons ouer gemeenskapslede. Ons bied geselskap, welsynsondersteuning, en praktiese bystand om te verseker dat ons bejaardes met waardigheid, verbintenis, en sorg verouder.",
            new[]
            {
                "Bekamp isolasie en eensaamheid onder bejaardes",
                "Ondersteun toegang tot gesondheidsorg en maatskaplike toelaes",
                "Verskaf praktiese bystand met daaglikse behoeftes",
                "Vier en bewaar kulturele erfenis"
            },
            new[]
            {
                "Weeklikse sosiale byeenkomste en teemiddae vir bejaardes",
                "Gesondheidsiftings en welsynsklinieke",
                "Bystand met SASSA-toelaagaansoeke",
                "Tussen-generasie storievertel-geleenthede",
                "Tuisbesoeke vir bejaardes met mobiliteitsgestremdhede"
            }),

        ["community-feeding"] = new(
            "Gemeenskapsvoeding",
            "Niemand gaan honger hier nie",
            "Ons verskaf warm, voedsame maaltye en maandelikse voedselpakkette aan kwesbare gesinne en individue wat voedselonsekerheid in die gesig staar.",
            "Die Gemeenskapsvoeding-program is ons antwoord op honger. Ons bedryf daaglikse voedingstasies en versprei voedselpakkette aan gesinne in nood, om te verseker dat geen lid van ons gemeenskap honger gaan slaap nie.",
            new[]
            {
                "Skakel honger onder kwesbare gemeenskapslede uit",
                "Verskaf voedsame, gebalanseerde maaltye elke dag",
                "Versprei voedselpakkette aan tuisgebonde individue",
                "Bied opvoeding oor voeding en volhoubare voedselpraktyke"
            },
            new[]
            {
                "Daaglikse gemeenskapskombuis wat 200+ warm maaltye bedien",
                "Maandelikse voedselpakket-verspreidingsveldtogte",
                "Gemeenskapsgroentetuin en voedselproduksie",
                "Voedingsopvoeding en kookwerkswinkels",
                "Vakansie- en feesseisoen-voedingsveldtogte"
            }),

        ["baby-saver"] = new(
            "Baba-redder",
            "Elke lewe is kosbaar",
            "Ons ondersteun jong en kwesbare moeders met sorg, noodsaaklike hulpbronne, en gesinsondersteuning, om te verseker dat elke baba die beste moontlike begin het.",
            "Die Baba-redder-program bied 'n lewenslyn vir jong en kwesbare moeders. Ons bied 'n veilige plek vir moeders in krisis en verseker dat elke baba toegang het tot die noodsaaklikhede wat hulle nodig het om te floreer.",
            new[]
            {
                "Voorkom babaverlating en -mishandeling",
                "Ondersteun jong en eerstekeer-moeders",
                "Verskaf noodsaaklike bababenodigdhede en voedingsondersteuning",
                "Skakel gesinne met maatskaplike dienste"
            },
            new[]
            {
                "Ouerskapklasse en portuur-steungroepe",
                "Verspreiding van bababenodigdhede (doeke, formule, klere)",
                "Borsvoedingsondersteuning en laktasieberading",
                "Krisisintervensie vir moeders in nood",
                "Aanneming- en pleegsorgfasilitering"
            }),

        ["safe-houses"] = new(
            "Nood-veiligehuise",
            "Veiligheid, altyd",
            "Ons verskaf onmiddellike, vertroulike skuiling en ondersteuning aan vroue en kinders wat van huishoudelike geweld en lewensgevaarlike situasies vlug.",
            "Ons Nood-veiligehuise bied onmiddellike toevlug vir vroue en kinders wat van huishoudelike geweld, mishandeling, en gevaarlike situasies vlug. Ons bied 'n veilige omgewing, berading, en paaie na langtermyn-veiligheid en onafhanklikheid.",
            new[]
            {
                "Verskaf onmiddellike veilige skuiling vir oorlewendes",
                "Bied traumaberading en psigososiale ondersteuning",
                "Help met regsbeskermingsbevele",
                "Bou paaie na onafhanklike, veilige lewens"
            },
            new[]
            {
                "24/7-krisislyn en noodreaksie",
                "Vertroulike veiligehuis-akkommodasie",
                "Individuele en groep-traumaberading",
                "Regsadvokaatskap en hofondersteuning",
                "Herintegrasie en lewensvaardigheid-herbou"
            }),

        ["search-rescue"] = new(
            "Soek en Redding",
            "Ons vind jou. Ons bring jou huis toe.",
            "'n Opgeleide gemeenskapsvrywilligereenheid wat reageer op vermistepersoon-sake en gemeenskapsnoodgevalle, in samewerking met SAPD en maatskaplike dienste.",
            "Ons Soek-en-Redding-span is 'n opgeleide vrywilligereenheid wat reageer op vermistepersoon-sake en gemeenskapsnoodgevalle. Ons werk saam met wetstoepassing en maatskaplike dienste om te verseker dat kwesbare individue gevind en ondersteun word.",
            new[]
            {
                "Vind vermiste persone in die gemeenskap",
                "Ondersteun gesinne van vermiste persone emosioneel",
                "Koördineer met SAPD en maatskaplike dienste",
                "Lei gemeenskapsvrywilligers op in soektegnieke"
            },
            new[]
            {
                "24/7-nooddiensontplooiing",
                "Vrywilligeropleiding en veld-simulasie-oefeninge",
                "Gemeenskapsbewusmaking- en voorkomingsveldtogte",
                "Steungroepe vir gesinne van vermiste persone",
                "Vermistepersoon-registrasie en digitale databasis"
            }),

        ["social-work"] = new(
            "Maatskaplike Werk Dienste",
            "Professionele sorg, menslike hart",
            "Gekwalifiseerde maatskaplike werkers wat gesinne verbind met hulpbronne, toelaes, berading, en die noodsaaklike dienste wat hulle nodig het om te floreer.",
            "Ons gekwalifiseerde maatskaplike werkers bied professionele, deernisvolle ondersteuning aan individue en gesinne wat komplekse uitdagings navigeer. Van toelaagaansoeke tot gesinsberading, ons span skakel mense met die hulpbronne wat hulle nodig het.",
            new[]
            {
                "Bied professionele maatskaplike-werk-assesserings",
                "Skakel gesinne met regeringstoelaes en -dienste",
                "Bied gesinsterapie en konflikbemiddeling",
                "Ondersteun kinderbeskerming en -welsyn"
            },
            new[]
            {
                "Een-tot-een maatskaplike-werk-konsultasies",
                "SASSA-toelaagaansoek- en appèlbystand",
                "Gesinshofondersteuning en verteenwoordiging",
                "Kinderwelsyn-ondersoeke en steunplanne",
                "Tuisbesoek-assesserings en gevallebestuur"
            }),

        ["mens-cafe"] = new(
            "Manne-kafee",
            "Manne betrek om gender-gebaseerde geweld te beëindig",
            "'n Vennootskap met die NPO Inside Out wat gereelde werkswinkels, lewensvaardigheidsopleiding, en 'n veilige ruimte bied waar mans kan besin oor manlikheid en help om gender-gebaseerde geweld te voorkom.",
            "Philisa Abafazi Bethu glo dat die voorkoming van gender-gebaseerde geweld nie moontlik is sonder om nou saam met mans te werk nie. Manne-kafee is in 2022 geloods in vennootskap met die NPO Inside Out, wat gereelde Manwees-opleidingswerkswinkels vanaf die PAB-sentrum fasiliteer. Hierdie sessies gaan verder as net gesprek, hulle besin oor patriargale strukture in ons samelewing, bou lewensvaardighede, en skep 'n veilige ruimte waar mans hul gevoelens en perspektiewe kan deel. Die program bedryf ook 'n Nood-veiligehuis vir mans in nood, en word trots befonds deur die World Childhood Foundation.",
            new[]
            {
                "Betrek mans as vennote in die voorkoming van gender-gebaseerde geweld",
                "Bevraagteken skadelike patriargale norme deur begeleide besinning",
                "Bou lewensvaardighede en emosionele geletterdheid onder mans",
                "Verskaf noodskuiling vir mans in krisis"
            },
            new[]
            {
                "Gereelde Manwees-opleidingswerkswinkels gefasiliteer deur Inside Out",
                "Groepbesprekings oor manlikheid en patriargale strukture",
                "Lewensvaardigheidopleidingsessies",
                "Veilige-ruimte-kringe waar mans gevoelens en perspektiewe kan deel",
                "Nood-veiligehuis-akkommodasie vir mans in nood"
            }),
    };

    private static ProgrammeText? Text(Programme p, SiteStrings s) => s.Lang switch
    {
        "xh" => Xh.GetValueOrDefault(p.Id),
        "af" => Af.GetValueOrDefault(p.Id),
        _ => null
    };

    public static string TitleFor(this Programme p, SiteStrings s) => Text(p, s)?.Title ?? p.Title;
    public static string TaglineFor(this Programme p, SiteStrings s) => Text(p, s)?.Tagline ?? p.Tagline;
    public static string DescriptionFor(this Programme p, SiteStrings s) => Text(p, s)?.Description ?? p.Description;
    public static string OverviewFor(this Programme p, SiteStrings s) => Text(p, s)?.Overview ?? p.Overview;
    public static IReadOnlyList<string> ObjectivesFor(this Programme p, SiteStrings s) => (IReadOnlyList<string>?)Text(p, s)?.Objectives ?? p.Objectives;
    public static IReadOnlyList<string> ActivitiesFor(this Programme p, SiteStrings s) => (IReadOnlyList<string>?)Text(p, s)?.Activities ?? p.Activities;
}
