using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

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

        public string evaluatelastHost(string broadcast, string NetzAnteil)
        {
            if(NetzAnteil == "32")
            {
                return this.NetID;
            }
            if(NetzAnteil == "31")
            {
                return this.Broadcast;
            }
            string lastHost = this.Broadcast;
            string[] explodedBroadcast = lastHost.Split('.');
            int[] explodedBroadcastInt = new int[explodedBroadcast.Length];

            for(int i = 0; i < explodedBroadcast.Length; i++)
            {
                explodedBroadcastInt[i] = int.Parse(explodedBroadcast[i]);
            }

            for (int i = 0; i < explodedBroadcastInt.Length; i++)
            {
                if (explodedBroadcastInt[explodedBroadcastInt.Length - 1 - i] == 0)
                {
                    explodedBroadcastInt[explodedBroadcastInt.Length - 1 - i] = 255;
                }
                else
                {
                    explodedBroadcastInt[explodedBroadcastInt.Length - 1 - i] -= 1;
                    break;
                }
            }

            lastHost = "";
            foreach(int block in explodedBroadcastInt)
            {
                lastHost += block + ".";
            }
            lastHost = lastHost.Substring(0, lastHost.Length - 1);

            return lastHost;
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
                //Subnet 31 und 32 sind ausnahmen in denen der erste host identisch mit der NetID sind.
                return NetID;
            }
            string firstHost = "";
            string[] explodedNetID = NetID.Split('.');
            int[] explodedNetIDInt = new int[explodedNetID.Length];
            for(int i= 0; i < explodedNetID.Length; i++)
            {
                explodedNetIDInt[i] = Convert.ToInt32(explodedNetID[i]);
            }

            for(int i = 0; i < explodedNetIDInt.Length; i++)
            {
                if (explodedNetIDInt[explodedNetIDInt.Length-1-i] == 255)
                {
                    explodedNetIDInt[explodedNetIDInt.Length - 1 - i] = 0;
                }
                else
                {
                    explodedNetIDInt[explodedNetIDInt.Length - 1 - i] += 1;
                    break;
                }
            }

            foreach(int block in explodedNetIDInt)
            {
                firstHost += block.ToString() + ".";
            }
            firstHost = firstHost.Substring(0, firstHost.Length-1);

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
            this.SubnetzMaske = evaluateSubnetmask(this.NetzAnteil);
            this.ErsterHost = evaluateFirstHost(this.NetID, this.NetzAnteil);
            this.Broadcast = evaluateBroadcast(this.NetzAnteil, this.NetID);
            this.LetzterHost = evaluatelastHost(this.Broadcast, this.NetzAnteil);
        }
    }
}
