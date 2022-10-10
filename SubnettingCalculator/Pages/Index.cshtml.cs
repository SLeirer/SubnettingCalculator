using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Numerics;
using System.Text.RegularExpressions;
using static System.Formats.Asn1.AsnWriter;
using System.Linq;

namespace SubnettingCalculator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public string ErsterHost { get; set; }
        public string SubnetzMaske { get; set; }
        public string LetzterHost { get; set; }
        public string Broadcast { get; set; }
        public string AnzahlHosts { get; set; }
        public string NetzAnteil { get; set; }
        public string NetID { get; set; }
        public string Message { get; set; }



        [BindProperty]
        public string NetIdEingabe { get; set; }
        [BindProperty]
        public string NetzAnteilEingabe { get; set; }


        public void OnGet()
        {
            
        }

        public void OnPost()
        {            
            if(!isValidInput(NetIdEingabe, NetzAnteilEingabe))
            {
                return;
            }
            NetID = NetIdEingabe;
            NetzAnteil = NetzAnteilEingabe;
            SubnetzMaske = evaluateSubnetmask(NetzAnteil);
            ErsterHost = evaluateFirstHost(NetID, NetzAnteil);
            AnzahlHosts = (Math.Pow(2, 32 - Convert.ToInt32(NetzAnteil))-2).ToString();
            if(Convert.ToInt32(AnzahlHosts) < 2)
            {
                AnzahlHosts = (Convert.ToInt32(AnzahlHosts) + 2).ToString();
            }

            Broadcast = evaluateBroadcast(NetzAnteil, NetIdEingabe);

        }

        public string evaluateBroadcast(string NetzAnteil, string NetID)
        {
            string broadcastStr = "";
            if (NetzAnteil == "32")
            {
                return NetID;
            }
            if(NetzAnteil == "31")
            {
                broadcastStr = NetID.Substring(NetID.LastIndexOf('.')+1);
                broadcastStr = (Convert.ToInt32(broadcastStr) + 1).ToString();
                broadcastStr = NetID.Substring(0, NetID.LastIndexOf('.')+1) + broadcastStr;
                return broadcastStr;
            }
            
            int hostanteile = 32 - Convert.ToInt32(NetzAnteil);
            int ganzeAchterblocks = hostanteile / 8;
            int temp = 1;
            int erg = 0;
            string[] netIDblocks = NetID.Split('.');
            int[] netIDblocksInt = new int[4];

          

            for (int i=0; i<netIDblocks.Length; i++)
            {
                netIDblocksInt[i] = Convert.ToInt32(netIDblocks[i]);
            }

            hostanteile = hostanteile - ganzeAchterblocks * 8;
            for(int i = 0; i < hostanteile; i++)
            {
                erg += temp;
                temp = temp * 2;
            }

            for(int i = 0; i < ganzeAchterblocks; i++)
            {
                netIDblocksInt[3 - i] = 255;
            }
            
            netIDblocksInt[3 - ganzeAchterblocks] = netIDblocksInt[3 - ganzeAchterblocks] + erg;

            foreach(int block in netIDblocksInt)
            {
                broadcastStr += block.ToString() + ".";
            }
            broadcastStr = broadcastStr.Substring(0, broadcastStr.Length-1);

            return broadcastStr;
        }
        public string evaluateFirstHost(string NetID, string NetzAnteil)
        {
            if(NetzAnteil == "31" || NetzAnteil == "32")
            {
                return NetID;
            }
            string firstHost = "";
            string NetIDlastBlock = NetID.Substring(NetID.LastIndexOf('.')+1);
            int temp = Convert.ToInt32(NetIDlastBlock);
            temp += 1;
            firstHost = NetID.Substring(0, NetID.Length-NetIDlastBlock.Length);
            firstHost += temp.ToString();

            return firstHost;
        }
        public string evaluateSubnetmask(string NetzAnteil)
        {
            string subnetzmaske = "";
            int anzahlNetzbits = Convert.ToInt32(NetzAnteil);
            int volleAchterBlocks = anzahlNetzbits / 8;
            int uebrigeBits = anzahlNetzbits - volleAchterBlocks*8;
            int temp = 128;
            int erg = 0;

            for(int i = 0; i < volleAchterBlocks; i++)
            {
                subnetzmaske += "255.";
            }
            
            for(int i = 0; i < uebrigeBits; i++)
            {
                erg += temp;
                temp = temp / 2;
            }
            subnetzmaske += erg + ".";

            while(subnetzmaske.Count(f => f == '.') < 4)
            {
                subnetzmaske += "0.";
            }

            subnetzmaske = subnetzmaske.Substring(0, subnetzmaske.Length-1);
            if(subnetzmaske.Length > 15)
            {
                subnetzmaske = subnetzmaske.Substring(0, 15);
            }


            return subnetzmaske;
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
            int localHostBits = 32 - Convert.ToInt32(NetzAnteilEingabe);
            int anzahlAchterBloecke = localHostBits / 8;
            if(anzahlAchterBloecke > 0)
            {
                localHostBits = localHostBits - anzahlAchterBloecke * 8;
            }
            if (netIdBloackeInteger[3-anzahlAchterBloecke] > Math.Pow(2, localHostBits))
            {
                Message = "Nicht genügend IP's für diesese subnetzt";
                return false;
            }
            for(int i = 0; i < anzahlAchterBloecke; i++)
            {
                if (netIdBloackeInteger[3 - anzahlAchterBloecke] == Math.Pow(2, localHostBits) && netIdBloackeInteger[3-i] > 0)
                {
                    Message = "Nicht genügend IP's für diesese subnetzt";
                    return false;
                }
            }


            return true;
        }
    }
}