using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Numerics;
using System.Text.RegularExpressions;
using static System.Formats.Asn1.AsnWriter;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace SubnettingCalculator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        //VARIABLEN
        
        //Message Variable zur fehler rückmeldung an den nutzer
        public string Message { get; set; }
        //Objekt hinterlegung der subnetze in einer Liste
        //um diese leicht in die HTML Tabelle übernehmen zu können
        public static List<SubnetzObject> subnetzliste = new List<SubnetzObject>();

        //Eingabe variablen
        //bindproperty ermöglicht die einbidung von html form daten an c# variablen
        [BindProperty]
        public string NetIdEingabe { get; set; }
        [BindProperty]
        public string NetzAnteilEingabe { get; set; }

        //FUNKTIONEN
        public void OnGet()
        {
            
        }
        
        public List<SubnetzObject> getList()
        {
            return subnetzliste;
        }

        public IActionResult OnPostTeilenButtons(int splitOnIndex)
        {
            //altes netz was geteilt wird wird aus der liste entfernt
            //neue teilnetze werden in die liste eingefügt, an der stelle wo vorher das entfernte netz lag
            if (subnetzliste[splitOnIndex].NetzAnteil == "32")
            {
                Message = "subnetz kann nicht weiter geteilt werden.";
            }
            else
            {
                List<SubnetzObject> splitList = subnetzliste[splitOnIndex].splitSubnet();

                foreach (SubnetzObject teilnetz in splitList)
                {
                    foreach(SubnetzObject supernetz in subnetzliste[splitOnIndex].netzChronik)
                    {
                        teilnetz.netzChronik.Add(supernetz);
                    }
                    teilnetz.netzChronik.Add(subnetzliste[splitOnIndex]);
                }
                
                subnetzliste.RemoveAt(splitOnIndex);
                
                for (int i = 0; i < splitList.Count; i++)
                {
                    subnetzliste.Insert(splitOnIndex + i, splitList[i]);
                }
            }            
            return Page();
        }

        public IActionResult OnPostJoinButtons(int joinOnIndex)
        {
            //Alle teilnetze die auf dem gleichen übergeordneten netz basieren werden entfernt
            //das übergeordnete netz wird wieder hinzugefügt
            SubnetzObject superNetz = subnetzliste[joinOnIndex].netzChronik.Last();
            subnetzliste.Insert(joinOnIndex, superNetz);

            for(int i = subnetzliste.Count-1; i >= 0; i--)
            {
                if (subnetzliste[i].netzChronik.Contains(superNetz))
                {
                    subnetzliste.RemoveAt(i);
                }
            }
            
            return Page();
        }

        public IActionResult OnPost()
        {
            //Ausführung des "Eingabe" buttons
            subnetzliste.Clear();
            if (isValidInput(NetIdEingabe, NetzAnteilEingabe))
            {
                //prüfung ob es sich bei der eingabe um valide werte handelt andererseits erfolgt keine ausführung
                //fehler rückmeldung erfolgt über die isValidInput Funktion
                SubnetzObject erstesSubnetz = new();
                erstesSubnetz.initializeSubnet(NetIdEingabe, NetzAnteilEingabe);
                subnetzliste.Add(erstesSubnetz);
            }
            return Page();
        }

        public Boolean isValidInput(string NetIdEingabe, string NetzAnteilEingabe)
        {
            //erstprüfung ob die werte tatsächlich inhalt haben
            //dies muss zuerst stattfinden da eine handhabung von leeren werten eine mögliche exception verursachen kann
            if(NetIdEingabe == null || NetzAnteilEingabe == null)
            {
                Message = "Ein feld ist leer";
                return false;
            }

            //Variablen dekleration
            //netID wird am "." in ein array gesplittet und später dafür genutzt zu schauen ob die richtige anzahl von blöcken existiert
            string[] netIdBloecke = NetIdEingabe.Split('.');
            int[] netIdBloackeInteger = new int[netIdBloecke.Length];
            int netzAnteilInt = 0;
            Regex regex = new Regex(@"[\d]");

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //!!!   NETID PRÜFUNGEN        !!!
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //Prüfung ob NetID aus vier blöcken besteht, da eine abweichung davon nicht zulässig ist
            if (netIdBloecke.Length != 4)
            {
                Message = "Falsche anzahl an Blöcken";
                return false;
            }

            //Prüfung das die NetID blöcke nur zahlen beinhalten
            foreach (string block in netIdBloecke)
            {
                if (!regex.IsMatch(block)){
                    Message = "In einem Block sind steht etwas anderes wie zahlen";
                    return false;
                }
            }

            //Prüft den zulässigen wertebereich der einzelnen NetID blöcke
            for (int i = 0; i < netIdBloecke.Length; i++)
            {
                netIdBloackeInteger[i] = Convert.ToInt32(netIdBloecke[i]);
                if(!(netIdBloackeInteger[i] >= 0 && netIdBloackeInteger[i] <= 255))
                {
                    Message = "Ein Block ist nicht in der richtigen range";
                    return false;
                }
            }

            //abfangen von ungraden NetIDs
            if (!(netIdBloackeInteger[netIdBloackeInteger.Length-1] == 255 && NetzAnteilEingabe == "32"))
            {
                if (netIdBloackeInteger[netIdBloackeInteger.Length-1]%2 != 0)
                {
                    Message = "NetID darf nicht ungerade enden";
                    return false;
                }
            }

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //!!!   Slash-notations PRÜFUNGEN   !!!
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //Prüfung ob der netzanteil unerlaubte zeichen beinhaltet
            if (!(int.TryParse(NetzAnteilEingabe, out netzAnteilInt)))
            {
                Message = "netzAnteil beinhaltet etwas anderes als zahlen";
                return false;
            }

            //Prüfung das der netzanteil sich in einem erlaubten wertebereich befindet
            if (!(netzAnteilInt > 0 && netzAnteilInt <= 32))
            {
                Message = "netzanteil ist nicht in der richtigen range (1-32)";
                return false;
            }

            //Compatibilitätsprüfung der NetID mit der slash-notation
            //prüft für wieviele IP's die NetID platzt hat
            //Prüft über wieviele hosts die slash-notation verfügt.
            //vergleicht diese ob genug platzt für die anzahl der hosts überhaupt existiert.
            double hosts = Math.Pow(2, 32 - Convert.ToInt32(NetzAnteilEingabe));
            double hostPlatzImSubnetz = 1;
            for(int i = 0; i < netIdBloackeInteger.Length; i++)
            {
                hostPlatzImSubnetz = hostPlatzImSubnetz
                    + (255-netIdBloackeInteger[i])*(Math.Pow(255, 3-i));
            }
            if(hosts > hostPlatzImSubnetz)
            {
                Message = "netID hat nicht genügend hosts für dieses subnetz";
                return false;
            }
            //Abfangen von netID's die sich außerhalb des subnetzbereiches befinden
            int anzahlAchterBits = 0;
            if(netzAnteilInt >= 8)
            {
                anzahlAchterBits = netzAnteilInt / 8;
            }

            string netIDBinaryString = "";
            if(anzahlAchterBits < netIdBloackeInteger.Length)
            {
                netIDBinaryString = Convert.ToString(netIdBloackeInteger[anzahlAchterBits], 2).PadLeft(8, '0');

                List<int> indexes = new List<int>();
                for (int i = 0; i < 8; i++)
                {
                    if (i > netzAnteilInt)
                    {
                        indexes.Add(i);
                    }
                }

                foreach (int index in indexes)
                {
                    if (netIDBinaryString[index].Equals('1'))
                    {
                        Message = "NetID befindet sic außerhalb des subnetzbereiches dieser slash-notation";
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
}