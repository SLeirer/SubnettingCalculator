using System.Text.RegularExpressions;

namespace SubnettingCalculator.Pages
{
    public class SubnetzObject
    {
        public string ErsterHost { get; set; }
        public string SubnetzMaske { get; set; }
        public string LetzterHost { get; set; }
        public string Broadcast { get; set; }
        public string AnzahlHosts { get; set; }
        public string NetzAnteil { get; set; }
        public string NetID { get; set; }
        public string Message { get; set; }

        public SubnetzObject()
        {
            this.ErsterHost = "";
            this.SubnetzMaske = "";
            this.LetzterHost = "";
            this.Broadcast = "";
            this.AnzahlHosts = "";
            this.NetzAnteil = "";
            this.NetID = "";
            this.Message = "";
        }

        public string evaluateBroadcast(string NetzAnteil, string NetID)
        {
            string broadcastStr = "";
            if (NetzAnteil == "32")
            {
                return NetID;
            }
            if (NetzAnteil == "31")
            {
                broadcastStr = NetID.Substring(NetID.LastIndexOf('.') + 1);
                broadcastStr = (Convert.ToInt32(broadcastStr) + 1).ToString();
                broadcastStr = NetID.Substring(0, NetID.LastIndexOf('.') + 1) + broadcastStr;
                return broadcastStr;
            }

            int hostanteile = 32 - Convert.ToInt32(NetzAnteil);
            int ganzeAchterblocks = hostanteile / 8;
            int temp = 1;
            int erg = 0;
            string[] netIDblocks = NetID.Split('.');
            int[] netIDblocksInt = new int[4];



            for (int i = 0; i < netIDblocks.Length; i++)
            {
                netIDblocksInt[i] = Convert.ToInt32(netIDblocks[i]);
            }

            hostanteile = hostanteile - ganzeAchterblocks * 8;
            for (int i = 0; i < hostanteile; i++)
            {
                erg += temp;
                temp = temp * 2;
            }

            for (int i = 0; i < ganzeAchterblocks; i++)
            {
                netIDblocksInt[3 - i] = 255;
            }

            netIDblocksInt[3 - ganzeAchterblocks] = netIDblocksInt[3 - ganzeAchterblocks] + erg;

            foreach (int block in netIDblocksInt)
            {
                broadcastStr += block.ToString() + ".";
            }
            broadcastStr = broadcastStr.Substring(0, broadcastStr.Length - 1);

            return broadcastStr;
        }
        public string evaluateFirstHost(string NetID, string NetzAnteil)
        {
            if (NetzAnteil == "31" || NetzAnteil == "32")
            {
                return NetID;
            }
            string firstHost = "";
            string NetIDlastBlock = NetID.Substring(NetID.LastIndexOf('.') + 1);
            int temp = Convert.ToInt32(NetIDlastBlock);
            temp += 1;
            firstHost = NetID.Substring(0, NetID.Length - NetIDlastBlock.Length);
            firstHost += temp.ToString();

            return firstHost;
        }
        public string evaluateSubnetmask(string NetzAnteil)
        {
            string subnetzmaske = "";
            int anzahlNetzbits = Convert.ToInt32(NetzAnteil);
            int volleAchterBlocks = anzahlNetzbits / 8;
            int uebrigeBits = anzahlNetzbits - volleAchterBlocks * 8;
            int temp = 128;
            int erg = 0;

            for (int i = 0; i < volleAchterBlocks; i++)
            {
                subnetzmaske += "255.";
            }

            for (int i = 0; i < uebrigeBits; i++)
            {
                erg += temp;
                temp = temp / 2;
            }
            subnetzmaske += erg + ".";

            while (subnetzmaske.Count(f => f == '.') < 4)
            {
                subnetzmaske += "0.";
            }

            subnetzmaske = subnetzmaske.Substring(0, subnetzmaske.Length - 1);
            if (subnetzmaske.Length > 15)
            {
                subnetzmaske = subnetzmaske.Substring(0, 15);
            }


            return subnetzmaske;
        }
        public void initializeSubnet(string netIDEingabe, string netzAnteilEingabe)
        {
            this.NetID = netIDEingabe;
            this.NetzAnteil = netzAnteilEingabe;
            this.AnzahlHosts = (Math.Pow(2, 32 - Convert.ToInt32(this.NetzAnteil)) - 2).ToString();
            if (Convert.ToInt32(this.AnzahlHosts) < 2)
            {
                this.AnzahlHosts = (Convert.ToInt32(this.AnzahlHosts) + 2).ToString();
            }
            this.SubnetzMaske = evaluateSubnetmask(NetzAnteil);
            this.ErsterHost = evaluateFirstHost(NetID, NetzAnteil);
            this.Broadcast = evaluateBroadcast(NetzAnteil, NetID);
        }
    }
}
