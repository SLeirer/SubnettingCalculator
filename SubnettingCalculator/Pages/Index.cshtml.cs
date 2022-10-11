using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Numerics;
using System.Text.RegularExpressions;
using static System.Formats.Asn1.AsnWriter;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SubnettingCalculator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public string Message { get; set; }

        public List<SubnetzObject> subnetzliste = new List<SubnetzObject>();

        [BindProperty]
        public string NetIdEingabe { get; set; }
        [BindProperty]
        public string NetzAnteilEingabe { get; set; }


        public void OnGet()
        {
            
        }

        public void OnPost()
        {
            if (!isValidInput(NetIdEingabe, NetzAnteilEingabe))
            {
                return;
            }
            SubnetzObject erstesSubnetz = new();
            erstesSubnetz.initializeSubnet(NetIdEingabe, NetzAnteilEingabe);
            subnetzliste.Add(erstesSubnetz);
            subnetzliste.Add(erstesSubnetz);
            subnetzliste.Add(erstesSubnetz);
            subnetzliste.Add(erstesSubnetz);
        }
        public Boolean isValidInput(string NetIdEingabe, string NetzAnteilEingabe)
        {
            if(NetIdEingabe == null)
            {
                Message = "NetID feld ist leer";
                return false;
            }
            
            if(NetzAnteilEingabe == "0")
            {
                Message = "Subnet 0 existiert nicht";
                return false;
            }

            string[] netIdBloecke = NetIdEingabe.Split('.');
            int[] netIdBloackeInteger = new int[netIdBloecke.Length];
            int netzAnteilInt = 0;
            Regex regex = new Regex(@"[\d]");

            
            //EVALUATION OF NETID
            if (netIdBloecke.Length != 4)
            {
                //catches if number of blocks in the netID are not correct
                Message = "Falsche anzahl an Blöcken";
                return false;
            }
            foreach(string block in netIdBloecke)
            {
                //checks if netID blocks only contain numbers
                if (!regex.IsMatch(block)){
                    Message = "In einem Block sind steht etwas anderes wie zahlen";
                    return false;
                }
            }
            for (int i = 0; i < netIdBloecke.Length; i++)
            {
                //Checks if the range of netID blocks is correct
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

            //EVALUATION OF MASKINGBITS
            if (!(int.TryParse(NetzAnteilEingabe, out netzAnteilInt)))
            {
                //catches if mask bits cannot be parsed into int aka not having a valid input
                Message = "netzAnteil beinhaltet etwas anderes als zahlen";
                return false;
            }
            if (!(netzAnteilInt >= 0 && netzAnteilInt <= 32))
            {
                //checks if the range of maks bits is correct
                Message = "netzanteil ist nicht in der richtigen range";
                return false;
            }

            //CHECK IF NETID AND SUBNET FIT TOGETHER DEPENDING OF NUMBER OF HOSTS
            //irgendwas ist hier noch nicht richtig
            double hosts = Math.Pow(2, 32 - Convert.ToInt32(NetzAnteilEingabe));
            double hostPlatzImSubnetz = 1;
            for(int i = 0; i < netIdBloackeInteger.Length; i++)
            {
                hostPlatzImSubnetz = hostPlatzImSubnetz + (255-netIdBloackeInteger[i])*(Math.Pow(255, 3-i));
            }
            if(hosts > hostPlatzImSubnetz)
            {
                Message = "netID hat nicht genügend hosts für dieses subnetz";
                return false;
            }


            return true;
        }
    }
}