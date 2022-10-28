using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        private ArrayList abgeleitetVon = new ArrayList();

        public SubnetzObject()
        {
            this.ErsterHost = "";
            this.SubnetzMaske = "";
            this.LetzterHost = "";
            this.Broadcast = "";
            this.AnzahlHosts = "";
            this.NetzAnteil = "";
            this.NetID = "";
        }

        public List<SubnetzObject> splitSubnet()
        {
            //Erstellung der nötigen variablen
            List<SubnetzObject> splitList = new List<SubnetzObject>();
            SubnetzObject tempSubnetobject1 = new();
            SubnetzObject tempSubnetobject2 = new();

            //Neu Initialisierung des ersten Teilnetzes
            string netzanteilNew = (int.Parse(this.NetzAnteil) + 1).ToString();
            tempSubnetobject1.initializeSubnet(this.NetID, netzanteilNew);
            tempSubnetobject1.abgeleitetVon = this.abgeleitetVon;
            tempSubnetobject1.abgeleitetVon.Add(this);
            Debug.WriteLine("count für supernetz:" + this.abgeleitetVon.Count);
            splitList.Add(tempSubnetobject1);

            //Erstellung nötiger variablen zur bearbeitung des 2ten Teilnetzes
            string[] broadcastSplit = splitList[0].Broadcast.Split('.');
            int[] broadcastSplitInt = new int[broadcastSplit.Length];

            //umwandlung in Integer um das subnetz leichter aufzählen zu können
            for(int i = 0; i < broadcastSplit.Length; i++)
            {
                broadcastSplitInt[i] = Convert.ToInt32(broadcastSplit[i]);
            }

            //aufzählung des subnetzes
            for(int i = 0; i < broadcastSplitInt.Length; i++)
            {
                if (broadcastSplitInt[3-i] < 255)
                {
                    broadcastSplitInt[3 - i] += 1;
                    break;
                }
                else
                {
                    broadcastSplitInt[3 - i] = 0;
                }
            }

            //umwandlung der integer in string um die neue NetID darzustellen
            string netIdNew = "";
            foreach(int block in broadcastSplitInt)
            {
                netIdNew += block.ToString() + ".";
            }

            //initialisierung des zweiten teilnetzes
            netIdNew = netIdNew.Substring(0, netIdNew.Length - 1);
            tempSubnetobject2.initializeSubnet(netIdNew, netzanteilNew);
            tempSubnetobject2.abgeleitetVon = this.abgeleitetVon;
            tempSubnetobject2.abgeleitetVon.Add(this);
            Debug.WriteLine("count für supernetz:" + this.abgeleitetVon.Count);
            splitList.Add(tempSubnetobject2);

            //rückgabe der liste mit den 2 teilnetzen
            return splitList;
        }
        public string evaluatelastHost(string broadcast, string NetzAnteil)
        {
            //Ausnahmefälle für slash-notation 31 u. 32
            if(NetzAnteil == "32")
            {
                return this.NetID;
            }
            if(NetzAnteil == "31")
            {
                return this.Broadcast;
            }

            //VARIABLEN DEKLERATION
            string lastHost = this.Broadcast;
            string[] explodedBroadcast = lastHost.Split('.');
            int[] explodedBroadcastInt = new int[explodedBroadcast.Length];

            //Convertierung zu int um damit rechnen zu können
            for(int i = 0; i < explodedBroadcast.Length; i++)
            {
                explodedBroadcastInt[i] = int.Parse(explodedBroadcast[i]);
            }

            //runterzählen vom broadcast
            for (int i = 0; i < explodedBroadcastInt.Length; i++)
            {
                if (explodedBroadcastInt[explodedBroadcastInt.Length - 1 - i] == 0)
                {
                    //wenn der momentane block bereits das minimum erreicht hat
                    //block wird auf maximum gesetzt und es wird im nächsten block wieder runtergezählt.
                    explodedBroadcastInt[explodedBroadcastInt.Length - 1 - i] = 255;
                }
                else
                {
                    explodedBroadcastInt[explodedBroadcastInt.Length - 1 - i] -= 1;
                    break;
                }
            }

            //Array wird in string zusammengefügt für die ausgabe
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

            //Ausnahmefälle des Broadcast für slash 31 u. 32er netze
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

            //VARIABLEN DEKLERATION
            int hostanteile = 32 - Convert.ToInt32(NetzAnteil); //32 maximale anzahl von bits, hostanteil + netzanteil = gesamtes netz
            int ganzeAchterblocks = hostanteile / 8;    //hier wird wieder das arbeiten mit vollen 8bit blöcken vereinfacht
            int temp = 1;
            int erg = 0;
            string[] netIDblocks = NetID.Split('.');
            int[] netIDblocksInt = new int[4];

            //Convertierung in Integer für tatsächliche berechnungen
            for (int i = 0; i < netIDblocks.Length; i++)
            {
                netIDblocksInt[i] = Convert.ToInt32(netIDblocks[i]);
            }

            //achterblöcke werden von den hostanteilen abgezogen
            hostanteile = hostanteile - ganzeAchterblocks * 8;

            //berechnung des unvollständigen host-blocks
            for (int i = 0; i < hostanteile; i++)
            {
                erg += temp;
                temp = temp * 2;
            }

            //aufstocken der blöcke die volle 8bit blöcke beinhalten
            for (int i = 0; i < ganzeAchterblocks; i++)
            {
                netIDblocksInt[3 - i] = 255;
            }

            //aufzählen des ergebnisses zu den zugehörigen unvollständigen 8bit block
            netIDblocksInt[3 - ganzeAchterblocks] = netIDblocksInt[3 - ganzeAchterblocks] + erg;

            //umwandlung des arrays zu einem string für die ausgabe
            foreach (int block in netIDblocksInt)
            {
                broadcastStr += block.ToString() + ".";
            }
            broadcastStr = broadcastStr.Substring(0, broadcastStr.Length - 1);

            return broadcastStr;
        }
        public string evaluateFirstHost(string NetID, string NetzAnteil)
        {
            //Subnet 31 und 32 sind ausnahmen in denen der erste host identisch mit der NetID sind.
            if (NetzAnteil == "31" || NetzAnteil == "32")
            {
                return NetID;
            }

            //VARIABLEN DEKLERATION
            string firstHost = "";
            string[] explodedNetID = NetID.Split('.');  //string wird in einem array aufgelöst um leichter damit arbeiten zu können
            int[] explodedNetIDInt = new int[explodedNetID.Length];

            //Convertierung zu tatsächlichen integerwerten zur berechnung
            for(int i= 0; i < explodedNetID.Length; i++)
            {
                explodedNetIDInt[i] = Convert.ToInt32(explodedNetID[i]);
            }

            //Aufzählung von der NetID um auf den ersten host zu kommen
            for(int i = 0; i < explodedNetIDInt.Length; i++)
            {
                if (explodedNetIDInt[explodedNetIDInt.Length-1-i] == 255)
                {
                    //in dem fall das die maximale grenze im momentanten block bereits erreicht wurde
                    //block wird auf 0 gesetzt und aufzählung wird im nächsten block weitergeführt
                    explodedNetIDInt[explodedNetIDInt.Length - 1 - i] = 0;
                }
                else
                {
                    explodedNetIDInt[explodedNetIDInt.Length - 1 - i] += 1;
                    break;
                }
            }

            //Kombiniert das array zu einem string und seperiert die blöcke mit einem "."
            foreach(int block in explodedNetIDInt)
            {
                firstHost += block.ToString() + ".";
            }
            firstHost = firstHost.Substring(0, firstHost.Length-1);

            return firstHost;
        }
        public string evaluateSubnetmask(string NetzAnteil)
        {
            //VARIABLEN DEKLERATION
            string subnetzmaske = "";
            int anzahlNetzbits = Convert.ToInt32(NetzAnteil);
            int volleAchterBlocks = anzahlNetzbits / 8;     //ein block der subnetzmaske beinhaltet immer 8 bit
            int uebrigeBits = anzahlNetzbits - volleAchterBlocks * 8;   //was übrig bleibt nach abzug der vollen 8bit blöcke
            int temp = 128;     //ein bit am anfang eines 8bit blockes = 128, dies wird hier festgelegt um später damit rechnen zu können
            int erg = 0;

            //ein voller achterblock = 255
            //für jeden achterblock wird dies in der ausgabevariable übernommen
            for (int i = 0; i < volleAchterBlocks; i++)
            {
                subnetzmaske += "255.";
            }

            //mit den übrigen bits muss tatsächlich gerechnet werden
            //wobei jedes folgende bit die hälfte des vorrigen ist weshalb dies in jedem schritt durch 2 geteilt wird.
            for (int i = 0; i < uebrigeBits; i++)
            {
                erg += temp;
                temp = temp / 2;
            }
            subnetzmaske += erg + ".";

            //da eine subnetzmaske über 4 blöcke verfügen muss wird dies solange weiter aufgefüllt bis dies der fall ist
            while (subnetzmaske.Count(f => f == '.') < 4)
            {
                subnetzmaske += "0.";
            }

            //entfernung des hinteren überflüssigen punktes
            subnetzmaske = subnetzmaske.Substring(0, subnetzmaske.Length - 1);

            //verkleinerung sollte die Subnetzmaske die maximalgröße überschreiten
            if (subnetzmaske.Length > 15)
            {
                subnetzmaske = subnetzmaske.Substring(0, 15);
            }


            return subnetzmaske;
        }

        public void evaluateNumHosts()
        {
            //anzahl hosts = 2^hostbits
            //hostbits = 32 - NetzAnteilDerBits (32 in diesem fall ist die volle länge einer IP)
            this.AnzahlHosts = (Math.Pow(2, 32 - Convert.ToInt32(this.NetzAnteil)) - 2).ToString();
            if (Convert.ToInt32(this.AnzahlHosts) < 2)
            {
                //in dem fall das weniger wie 2 rauskommt handelt es sich um spezielle fälle
                //im 31 u. 32 netzt werden jeweils der Broadcast und/oder die netID mitgezählt
                //weshalb man das ergebnis in diesen fällen einfach um 2 erhöhen kann
                this.AnzahlHosts = (Convert.ToInt32(this.AnzahlHosts) + 2).ToString();
            }
        }
        public void initializeSubnet(string netIDEingabe, string netzAnteilEingabe)
        {
            //das object ist bei der erstellung leer
            //um das object zu befüllen muss es über diese methode initialisiert werden.

            //die eingabewerte werden übernommen
            this.NetID = netIDEingabe;
            this.NetzAnteil = netzAnteilEingabe;

            //der rest der werte wird anhand der eingabe berechnet
            evaluateNumHosts();
            this.SubnetzMaske = evaluateSubnetmask(this.NetzAnteil);
            this.ErsterHost = evaluateFirstHost(this.NetID, this.NetzAnteil);
            this.Broadcast = evaluateBroadcast(this.NetzAnteil, this.NetID);
            this.LetzterHost = evaluatelastHost(this.Broadcast, this.NetzAnteil);
        }
    }
}
