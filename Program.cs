using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
namespace Bachelorscriptie
{
    class Program
    {
        static void Main()
        {
            //Lees de graaf in
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Graaf graaf = Graaf.LeesGraaf();

            //Los hem op
            IList<Paar> paren = graaf.LosOp(true, true, true, TypeSorteerStructruur.Buckets, false, stopwatch);
            for (int x = 0; x < paren.Count; x++)
                Console.WriteLine(paren[x]);
            Console.ReadLine();
        }
    }
    class Knoop : ISorteerElement, ILijstElement, IHeapElement
    {
        public int LijstIndex
        {
            get
            {
                return this.lijstIndex;
            }
            set
            {
                this.lijstIndex = value;
            }
        }
        private int lijstIndex;
        public int HeapIndex
        {
            get
            {
                return this.heapIndex;
            }
            set
            {
                this.heapIndex = value;
            }
        }
        private int heapIndex;
        public double Waarde
        {
            get
            {
                return this.Graad;
            }
        }
        public int Nummer;//Het nummer van deze knoop
        public ModuloLijst<Zijde> Zijden;//De zijden vanuit deze knoop (wordt later misschien 
        public Dictionary<Knoop, Paar> Paren;//Dit woordenboek bevat alle kandidaatparen waar hij mee om kan gaan
        public int ModuloIndex;//De modulo index van deze knoop
        public int Graad//De graad van deze knoop
        {
            get
            {
                return this.Zijden.Aantal;
            }
        }
        public Knoop(int nummer)
        {
            this.Nummer = nummer;
            this.Zijden = new ModuloLijst<Zijde>(1);
            this.Paren = new Dictionary<Knoop, Paar>();
            this.lijstIndex = -1;
        }
        public override string ToString()
        {
            return this.Nummer.ToString();
        }
        public override int GetHashCode()
        {
            return this.Nummer;
        }
        public void NieuweModulus(int m)//Pas de modulus aan
        {
            this.Zijden.AndereModulus(m);
        }
    }//Een algemene knoop
    class Zijde : IModuloLijstElement
    {
        public int LijstIndex
        {
            get
            {
                return this.lijstIndex;
            }
            set
            {
                this.lijstIndex = value;
            }
        }
        private int lijstIndex;
        public int ModuloWaarde
        {
            get
            {
                return this.Eind.ModuloIndex;
            }
        }
        public Knoop Begin, Eind;//Het beginpunt en eindpunt
        public int Stroom;//De stroom door deze zijde
        public Zijde Andersom;//De zijde die andersom loopt
        public bool Rood;//Is deze zijde rood?
        public Zijde(Knoop begin, Knoop eind)
        {
            this.Begin = begin;
            this.Eind = eind;
            this.Stroom = 0;
            this.Andersom = null;
            this.Rood = false;
            this.lijstIndex = -1;
        }
        public override string ToString()
        {
            return this.Begin + ";" + this.Eind;
        }
    }//Een zijde tussen 2 knopen
    class Graaf
    {
        public Lijst<Knoop> Knopen;//Alle knopen van de graaf
        public ISorteerStructuur<Paar> Contracties;//De mogelijke contracties
        public bool UpdateAlles;//Moet alles geupdated worden?
        public int Modulus, ModuloSom;//De modulus en de som waar paren aan moeten voldoen modulo
        public bool Aeerst;//Moet A eerst komen bij een nieuw paar?
        public List<Zijde> Leeg, Leeg2;//De lege lijsten voor sorteren
        public TypeSorteerStructruur Type;//De type sorteerstructuur
        public const double CompPerSec = 5000000;//Hoeveel complexiteit per seconde kunnen we aan?
        public double Waarde;//De huidige waarde
        private List<int> indices;//De indices die meedoen bij samenvoegen
        private bool[] gehad;//Hebben we ze gehad
        public Graaf()//Maak een lege graaf
        {
            this.Knopen = new Lijst<Knoop>();
            this.Contracties = new Heap<Paar>();
            this.Modulus = 1;
            this.ModuloSom = 0;
            this.Aeerst = true;
            this.Leeg = new List<Zijde>();
            this.Leeg2 = new List<Zijde>();
            this.indices = new List<int>();
            this.gehad = new bool[1];
        }
        public static Graaf LeesGraaf(string filenaam)//Lees de graaf van die filenaam in
        {
            //Lees de graaf in
            string naam = filenaam;
            if (naam.Length < 4 || naam.Substring(naam.Length - 4) != ".txt")
                naam = naam + ".txt";
            FileStream file = new FileStream(naam, FileMode.Open);
            TextReader lezer = new StreamReader(file);
            Graaf graaf = leesGraaf(lezer);
            lezer.Close();
            file.Close();
            return graaf;
        }
        public static Graaf LeesGraaf()//Lees de graaf van de console in
        {
            return leesGraaf(Console.In);
        }
        private static Graaf leesGraaf(TextReader lezer)//Lees de graaf van deze lezer in
        {
            //Zoek voor de eerste regel
            Graaf graaf = new Graaf();
            string regel;
            string[] delen;
            while (true)
            {
                regel = lezer.ReadLine();
                if (regel.Trim().Length == 0)
                    continue;
                if (regel.Trim()[0] == 'p')
                    break;
            }
            delen = regel.Split(' ');
            int n = int.Parse(delen[2]), m = int.Parse(delen[3]);
            for (int x = 0; x < n; x++)
                graaf.Knopen.VoegToe(new Knoop(x + 1));

            //Lees de zijden in
            int b, e;
            Zijde z1, z2;
            int zijdengehad = 0;
            while (zijdengehad < m)
            {
                regel = lezer.ReadLine();
                if (regel.Trim().Length == 0)
                    continue;
                if (regel[0] == 'c')
                    continue;
                delen = regel.Trim().Split(' ');
                b = int.Parse(delen[0]) - 1;
                e = int.Parse(delen[1]) - 1;
                z1 = new Zijde(graaf.Knopen[b], graaf.Knopen[e]);
                z2 = new Zijde(graaf.Knopen[e], graaf.Knopen[b]);
                z1.Andersom = z2;
                z2.Andersom = z1;
                graaf.Knopen[b].Zijden.VoegToe(z1);
                graaf.Knopen[e].Zijden.VoegToe(z2);
                zijdengehad++;
            }
            return graaf;
        }
        public Paar MaakPaar(Knoop A, Knoop B)//Maak een paar tussen deze knopen
        {
            //Maak hem
            return new Paar(A, B);
        }
        public void BerekenOvereenkomst(Paar paar)//Bereken de overeenkomst in dit paar
        {
            //Haal de gesorteerde zijden op
            Knoop A = paar.Begin, B = paar.Eind;
            List<Zijde> zijdenA = A.Zijden.List, zijdenB = B.Zijden.List;
            Algoritmen.Sorteer<Zijde>(zijdenA, this.Leeg, (Zijde z1, Zijde z2) => z1.Eind.Nummer < z2.Eind.Nummer);
            Algoritmen.Sorteer<Zijde>(zijdenB, this.Leeg, (Zijde z1, Zijde z2) => z1.Eind.Nummer < z2.Eind.Nummer);

            //Loop ze langs
            paar.Overeenkomst = 0;
            int ia = 0, ib = 0;
            while (ia < zijdenA.Count && ib < zijdenB.Count)
            {
                //Controleer of een buur overeenkomt
                if (zijdenA[ia].Eind == zijdenB[ib].Eind)
                {
                    if (zijdenA[ia].Rood || zijdenB[ib].Rood)
                        paar.Overeenkomst++;
                    else
                        paar.Overeenkomst += 2;
                    ia++;
                    ib++;
                }

                //Tel door
                if (zijdenA[ia].Eind.Nummer < zijdenB[ib].Eind.Nummer)
                    ia++;
                else
                    ib++;
            }
        }
        public void BerekenOvereenkomst(Knoop A)//Bereken de overeenkomsten van alle paren vanuit A
        {
            //Zet ze eerst allemaal op 0
            Paar paar;
            foreach (KeyValuePair<Knoop, Paar> kandidaat in A.Paren)
            {
                paar = kandidaat.Value;
                paar.Overeenkomst = 0;
            }

            //Loop vervolgens alle buren op afstand 2 langs
            Knoop tussen, B;
            Zijde zijde1, zijde2;
            int aantal;
            int welk = A.ModuloIndex;
            Dictionary<int, Lijst<Zijde>> boek;
            Lijst<Zijde> zijden, zijden2;
            for (int c = 0; c < 2; c++)
            {
                foreach (KeyValuePair<int, Lijst<Zijde>> kandidaat in A.Zijden.Lijsten)
                {
                    zijden = kandidaat.Value;
                    for (int x = 0; x < zijden.Aantal; x++)
                    {
                        zijde1 = zijden[x];
                        tussen = zijde1.Eind;
                        boek = tussen.Zijden.Lijsten;
                        if (!boek.ContainsKey(welk))
                            continue;
                        zijden2 = boek[welk];
                        aantal = zijden2.Aantal;
                        for (int y = 0; y < aantal; y++)
                        {
                            zijde2 = zijden2[y];
                            B = zijde2.Eind;
                            if (A == B)
                                continue;
                            paar = A.Paren[B];
                            if (zijde1.Rood || zijde2.Rood)
                                paar.Overeenkomst++;
                            else
                                paar.Overeenkomst += 2;
                        }
                    }
                }

                //Corrigeer voor de gemeenschappelijke zijde
                boek = A.Zijden.Lijsten;
                if (boek.ContainsKey(welk))
                {
                    zijden = boek[welk];
                    for (int x = 0; x < zijden.Aantal; x++)
                    {
                        B = zijden[x].Eind;
                        A.Paren[B].Overeenkomst += 2;
                    }
                }

                //Pas welk aan
                if ((this.ModuloSom - 2 * welk) % this.Modulus == 0)
                    break;
                welk = this.ModuloSom - welk;
                if (welk < 0)
                    welk += this.Modulus;
            }
        }
        public void BerekenInvloedBuren(Knoop A, bool positief, bool voegtoe, bool contracties)//Bereken de invloed op de buren
        {
            //Pas de paren aan
            Paar paar;
            int aantal, welkm;
            Zijde z1, z2;
            Knoop B, C;
            double verschil;
            Dictionary<int, Lijst<Zijde>> boek = A.Zijden.Lijsten;
            Lijst<Zijde> zijden, zijden2;
            foreach (KeyValuePair<int, Lijst<Zijde>> kandidaat in boek)
            {
                zijden = kandidaat.Value;
                welkm = this.ModuloSom - (kandidaat.Key % this.Modulus);
                if (welkm < 0)
                    welkm += this.Modulus;
                aantal = zijden.Aantal;
                for (int i = 0; i < aantal; i++)
                {
                    z1 = zijden[i];
                    B = z1.Eind;
                    for (int j = i + 1; j < aantal; j++)
                    {
                        z2 = zijden[j];
                        C = z2.Eind;
                        if (voegtoe && !B.Paren.ContainsKey(C))
                        {
                            paar = this.MaakPaar(B, C);
                            B.Paren.Add(C, paar);
                            C.Paren.Add(B, paar);
                            if (z1.Rood || z2.Rood)
                                verschil = 1;
                            else
                                verschil = 2;
                            if (positief)
                                paar.Overeenkomst = verschil;
                            else
                                paar.Overeenkomst = -verschil;
                            this.Contracties.VoegToe(paar);
                        }
                        else
                        {
                            paar = B.Paren[C];
                            if (z1.Rood || z2.Rood)
                                verschil = 1;
                            else
                                verschil = 2;
                            if (positief)
                                paar.Overeenkomst += verschil;
                            else
                                paar.Overeenkomst -= verschil;
                            if (contracties)
                                this.Contracties.AndereWaarde(paar);
                        }
                    }
                }
                if (kandidaat.Key != welkm)
                {
                    if (!boek.ContainsKey(welkm))
                        continue;
                    zijden2 = boek[welkm];
                    aantal = zijden2.Aantal;
                    for (int i = 0; i < zijden.Aantal; i++)
                    {
                        z1 = zijden[i];
                        B = z1.Eind;
                        for (int j = 0; j < aantal; j++)
                        {
                            z2 = zijden2[j];
                            C = z2.Eind;
                            if (voegtoe && !B.Paren.ContainsKey(C))
                            {
                                paar = this.MaakPaar(B, C);
                                B.Paren.Add(C, paar);
                                C.Paren.Add(B, paar);
                                if (z1.Rood || z2.Rood)
                                    verschil = 1;
                                else
                                    verschil = 2;
                                if (positief)
                                    paar.Overeenkomst = verschil;
                                else
                                    paar.Overeenkomst = -verschil;
                                this.Contracties.VoegToe(paar);
                            }
                            else
                            {
                                paar = B.Paren[C];
                                if (z1.Rood || z2.Rood)
                                    verschil = 1;
                                else
                                    verschil = 2;
                                if (positief)
                                    paar.Overeenkomst += verschil;
                                else
                                    paar.Overeenkomst -= verschil;
                                if (contracties)
                                    this.Contracties.AndereWaarde(paar);
                            }
                        }
                    }
                }
            }
        }
        public double BerekenMoeilijkheid()//Bereken de moeilijkheid van de graaf
        {
            //Bereken het
            double som = 0;
            double g;
            for (int x = 0; x < this.Knopen.Aantal; x++)
            {
                g = this.Knopen[x].Graad;
                som += g * (g - 1);
            }
            som *= Math.Log10(this.Knopen.Aantal);
            return som;
        }
        public double BerekenGraadVerhouding(int deel)//Bereken hoe groot de verhouding van de graad in het kwadraat is van de eerste deel knopen
        {
            //Controleer de input
            if (deel <= 0)
                return 0;
            if (deel >= this.Knopen.Aantal)
                return 1;

            //Loop alle knopen langs en bereken hun kwadraat
            List<double> alles = new List<double>();
            int aantal = this.Knopen.Aantal, graad;
            for (int x = 0; x < aantal; x++)
            {
                graad = this.Knopen[x].Graad;
                alles.Add(graad * (graad - 1));
            }

            //Sorteer het en vergelijk de gemiddelden
            Algoritmen.Sorteer<double>(alles, new List<double>(), (double a, double b) => a < b);
            double som = 0;
            for (int x = 0; x < deel; x++)
                som += alles[x];
            double gklein = som / deel;
            for (int x = deel; x < aantal; x++)
                som += alles[x];
            double galles = som / aantal;
            return gklein / galles;
        }
        public int AantalParen()//Tel het aantal paren dat nu ontstaat
        {
            //Loop alles langs
            bool[] gehad = new bool[this.Knopen.Aantal];
            Knoop A, tussen, B;
            Lijst<Zijde> zijden, zijden2;
            int welk, aantal;
            Dictionary<int, Lijst<Zijde>> boek;
            int paren = 0;
            for (int x = 0; x < this.Knopen.Aantal; x++)
            {
                for (int y = 0; y < this.Knopen.Aantal; y++)
                    gehad[y] = false;
                A = this.Knopen[x];
                welk = this.ModuloSom - A.ModuloIndex;
                if (welk < 0)
                    welk += this.Modulus;
                foreach (KeyValuePair<int, Lijst<Zijde>> paar in A.Zijden.Lijsten)
                {
                    zijden = paar.Value;
                    for (int y = 0; y < zijden.Aantal; y++)
                    {
                        tussen = zijden[y].Eind;
                        boek = tussen.Zijden.Lijsten;
                        if (!boek.ContainsKey(welk))
                            continue;
                        zijden2 = boek[welk];
                        aantal = zijden2.Aantal;
                        for (int z = 0; z < aantal; z++)
                        {
                            B = zijden2[z].Eind;
                            if (!gehad[B.LijstIndex])
                            {
                                gehad[B.LijstIndex] = true;
                                paren++;
                            }
                        }
                    }
                }
            }
            return paren;
        }
        public bool MaakBeginParen(Stopwatch stopwatch, double eindtijd)//Maak de begintoestand klaar en geef terug of het binnen de tijd gelukt is
        {
            //Verwijder alvast alle oude paren
            for (int x = 0; x < this.Knopen.Aantal; x++)
                this.Knopen[x].Paren.Clear();

            //Bepaal eerst alle paren met 1 gemeenschappelijke buur
            IList<Paar> paren = new List<Paar>();
            Knoop tussen, A, B;
            Paar paar;
            int welkm;
            int aantal;
            bool[] gehad = new bool[this.Modulus];
            Aeerst = true;
            Lijst<Zijde> zijden, zijden2;
            Dictionary<int, Lijst<Zijde>> boek;
            for (int x = 0; x < this.Knopen.Aantal && stopwatch.ElapsedMilliseconds / 1000.0 < eindtijd; x++)
            {
                for (int m = 0; m < this.Modulus; m++)
                    gehad[m] = false;
                tussen = this.Knopen[x];
                boek = tussen.Zijden.Lijsten;
                foreach (KeyValuePair<int, Lijst<Zijde>> kandidaat in boek)
                {
                    welkm = this.ModuloSom - kandidaat.Key;
                    if (welkm < 0)
                        welkm += this.Modulus;
                    zijden = kandidaat.Value;
                    aantal = zijden.Aantal;
                    for (int i = 0; i < aantal; i++)
                    {
                        A = zijden[i].Eind;
                        for (int j = i + 1; j < aantal; j++)
                        {
                            B = zijden[j].Eind;
                            if (A.Paren.ContainsKey(B))
                                continue;
                            paar = this.MaakPaar(A, B);
                            A.Paren.Add(B, paar);
                            B.Paren.Add(A, paar);
                            paren.Add(paar);
                        }
                    }
                    if (kandidaat.Key != welkm)
                    {
                        if (!boek.ContainsKey(welkm))
                            continue;
                        zijden2 = boek[welkm];
                        aantal = zijden2.Aantal;
                        for (int i = 0; i < zijden.Aantal; i++)
                        {
                            A = zijden[i].Eind;
                            for (int j = 0; j < aantal; j++)
                            {
                                B = zijden2[j].Eind;
                                if (A.Paren.ContainsKey(B))
                                    continue;
                                if (Aeerst)
                                    paar = this.MaakPaar(A, B);
                                else
                                    paar = this.MaakPaar(B, A);
                                Aeerst = !Aeerst;
                                A.Paren.Add(B, paar);
                                B.Paren.Add(A, paar);
                                paren.Add(paar);
                            }
                        }
                    }
                }
            }

            //Bepaal alle paren die verbonden zijn door een zijde
            for (int x = 0; x < this.Knopen.Aantal && stopwatch.ElapsedMilliseconds / 1000.0 < eindtijd; x++)
            {
                A = this.Knopen[x];
                welkm = A.ModuloIndex;
                boek = A.Zijden.Lijsten;
                for (int c = 0; c < 2; c++)
                {
                    if (!boek.ContainsKey(welkm))
                    {
                        welkm = this.ModuloSom - welkm;
                        if (welkm < 0)
                            welkm += this.Modulus;
                        continue;
                    }
                    zijden = boek[welkm];
                    aantal = zijden.Aantal;
                    for (int i = 0; i < aantal; i++)
                    {
                        B = zijden[i].Eind;
                        if (A == B)
                            continue;
                        if (A.Paren.ContainsKey(B))
                            continue;
                        paar = this.MaakPaar(A, B);
                        A.Paren.Add(B, paar);
                        B.Paren.Add(A, paar);
                        paren.Add(paar);
                    }
                    if ((this.ModuloSom - 2 * welkm) % this.Modulus == 0)
                        break;
                    welkm = this.ModuloSom - welkm;
                    if (welkm < 0)
                        welkm += this.Modulus;
                }
            }

            //Zet vervolgens de paren goed in de heap
            for (int x = 0; x < this.Knopen.Aantal && stopwatch.ElapsedMilliseconds / 1000.0 < eindtijd; x++)
                this.BerekenOvereenkomst(this.Knopen[x]);
            if (stopwatch.ElapsedMilliseconds / 1000.0 >= eindtijd)
                return false;
            switch (this.Type)
            {
                case TypeSorteerStructruur.Heap:
                    this.Contracties = new Heap<Paar>(paren);
                    break;
                case TypeSorteerStructruur.Buckets:
                    this.Contracties = new Buckets<Paar>(0, 1, paren);
                    break;
            }
            return true;
        }
        public void VoegSamen(Knoop A, Knoop B, double waarde)//Voeg A en B samen
        {
            //Verwijder dit paar
            A.Paren.Remove(B);
            B.Paren.Remove(A);

            //Pas eerst de oude paren aan
            this.BerekenInvloedBuren(A, false, false, false);
            this.BerekenInvloedBuren(B, false, false, false);

            //Haal de zijden op en sorteer ze
            IList<Knoop> dubbel = new List<Knoop>(), extra = new List<Knoop>();
            Dictionary<int, Lijst<Zijde>> boekA = A.Zijden.Lijsten, boekB = B.Zijden.Lijsten;
            this.indices.Clear();
            int welk;
            foreach (KeyValuePair<int, Lijst<Zijde>> kandidaat in boekA)
            {
                welk = kandidaat.Key;
                this.indices.Add(welk);
                gehad[welk] = true;
            }
            foreach (KeyValuePair<int, Lijst<Zijde>> kandidaat in boekB)
            {
                welk = kandidaat.Key;
                if (!gehad[welk])
                {
                    this.indices.Add(welk);
                    gehad[welk] = true;
                }
            }
            for (int x = 0; x < this.indices.Count; x++)
                gehad[this.indices[x]] = false;
            int m;
            for (int x = 0; x < this.indices.Count; x++) 
            {
                m = this.indices[x];
                List<Zijde> zijdenA;
                if (!boekA.ContainsKey(m))
                    zijdenA = this.Leeg2;
                else
                    zijdenA = boekA[m].List;
                List<Zijde> zijdenB;
                if (!boekB.ContainsKey(m))
                    zijdenB = this.Leeg2;
                else
                    zijdenB = boekB[m].List;
                if (zijdenA.Count == 0 && zijdenB.Count == 0)
                    continue;
                Algoritmen.Sorteer(zijdenA, this.Leeg, (Zijde z1, Zijde z2) => z1.Eind.Nummer < z2.Eind.Nummer);
                Algoritmen.Sorteer(zijdenB, this.Leeg, (Zijde z1, Zijde z2) => z1.Eind.Nummer < z2.Eind.Nummer);

                //Loop de zijden langs
                int ia = 0, ib = 0;
                bool nua, nub;
                Zijde ander;
                while (ia < zijdenA.Count || ib < zijdenB.Count)
                {
                    nua = false;
                    nub = false;
                    if (ia == zijdenA.Count)
                        nub = true;
                    if (ib == zijdenB.Count)
                        nua = true;
                    if (!nua && (nub || zijdenB[ib].Eind.Nummer < zijdenA[ia].Eind.Nummer))//Nu gaan we een unieke zijde van B toevoegen aan A
                    {
                        //Controleer op de eventuele zijde van A naar B
                        if (zijdenB[ib].Eind == A)
                        {
                            ib++;
                            continue;
                        }

                        //Voeg de zijde toe
                        zijdenB[ib].Begin = A;
                        zijdenB[ib].Rood = true;
                        ander = zijdenB[ib].Andersom;
                        if (A.ModuloIndex == B.ModuloIndex)
                            ander.Eind = A;
                        else
                        {
                            ander.Begin.Zijden.Verwijder(ander);
                            ander.Eind = A;
                            ander.Begin.Zijden.VoegToe(ander);
                        }
                        ander.Rood = true;
                        A.Zijden.VoegToe(zijdenB[ib]);
                        extra.Add(zijdenB[ib].Eind);
                        ib++;
                        continue;
                    }
                    if (!nub && (nua || zijdenA[ia].Eind.Nummer < zijdenB[ib].Eind.Nummer))//Nu hebben we een unieke zijde van A
                    {
                        //Controleer op de eventuele zijde van A naar B
                        if (zijdenA[ia].Eind == B)
                        {
                            A.Zijden.Verwijder(zijdenA[ia]);
                            ia++;
                            continue;
                        }

                        //Maak de zijde rood
                        if (!zijdenA[ia].Rood)
                        {
                            zijdenA[ia].Rood = true;
                            ander = zijdenA[ia].Andersom;
                            ander.Rood = true;
                        }
                        ia++;
                        continue;
                    }

                    //Nu hebben we een zijde die bij beiden voorkomt
                    if (zijdenB[ib].Rood && !zijdenA[ia].Rood)//Nu moeten we hem rood maken
                    {
                        zijdenA[ia].Rood = true;
                        ander = zijdenA[ia].Andersom;
                        ander.Rood = true;
                    }

                    //Verwijder de zijde naar B
                    ander = zijdenB[ib].Andersom;
                    ander.Begin.Zijden.Verwijder(ander);
                    dubbel.Add(zijdenA[ia].Eind);

                    //Ga door met tellen
                    ia++;
                    ib++;
                }
            }

            //Pas de nieuwe paren aan
            this.BerekenInvloedBuren(A, true, true, true);

            //Voeg hele nieuwe paren van B naar D toe aan A
            Knoop anderknoop;
            Paar paar;
            foreach (KeyValuePair<Knoop, Paar> nieuw in B.Paren)
            {
                anderknoop = nieuw.Key;
                paar = nieuw.Value;
                if (!A.Paren.ContainsKey(anderknoop))
                {
                    A.Paren.Add(anderknoop, paar);
                    anderknoop.Paren.Add(A, paar);
                    anderknoop.Paren.Remove(B);
                    if (paar.Begin == B)
                        paar.Begin = A;
                    else
                        paar.Eind = A;
                }
                else
                {
                    this.Contracties.Verwijder(paar);
                    anderknoop.Paren.Remove(B);
                }
            }

            //Bereken de nieuwe overeenkomsten van alle paren
            this.BerekenOvereenkomst(A);
            foreach (KeyValuePair<Knoop, Paar> kandidaat in A.Paren)
            {
                paar = kandidaat.Value;
                this.Contracties.AndereWaarde(paar);
            }
            if (this.UpdateAlles)
                for (int x = 0; x < dubbel.Count; x++)
                    foreach (KeyValuePair<Knoop, Paar> kandidaat in dubbel[x].Paren)
                    {
                        paar = kandidaat.Value;
                        this.Contracties.AndereWaarde(paar);
                    }

            //Verwijder knoop B
            this.Knopen.Verwijder(B);
        }
        public IList<int> KiesBesteSom(Stopwatch stopwatch, double eindtijd)//Kies de beste som uit
        {
            //Bereken de waarden
            int[] waarden = new int[this.Modulus];
            Knoop knoop;
            IList<int> aantal = new List<int>(), index = new List<int>();
            for (int x = 0; x < this.Knopen.Aantal && stopwatch.ElapsedMilliseconds / 1000.0 < eindtijd; x++)
            {
                knoop = this.Knopen[x];
                aantal.Clear();
                index.Clear();
                foreach (KeyValuePair<int, Lijst<Zijde>> paar in knoop.Zijden.Lijsten)
                {
                    aantal.Add(paar.Value.Aantal);
                    index.Add(paar.Key);
                }
                for (int i1 = 0; i1 < index.Count; i1++)
                    for (int i2 = i1 + 1; i2 < index.Count; i2++)
                        waarden[(index[i1] + index[i2]) % this.Modulus] += aantal[i1] * aantal[i2];
            }
            if (stopwatch.ElapsedMilliseconds / 1000.0 >= eindtijd)
                return new List<int>();

            //Bepaal het beste
            IList<int> best = new List<int>();
            int max = 10, plek;
            for (int x = 0; x < this.Modulus; x++)
            {
                for (plek = 0; plek < best.Count; plek++)
                    if (waarden[x] > waarden[best[plek]])
                        break;
                best.Insert(plek, x);
                if (best.Count > max)
                    best.RemoveAt(max);
            }
            return best;
        }
        public void BepaalModuloIndices()//Bepaal voor elke knoop een moduloindex
        {
            for (int x = 0; x < this.Knopen.Aantal; x++)
                this.Knopen[x].ModuloIndex = x % this.Modulus;
        }
        public bool VeranderModulus(int nieuw, Stopwatch stopwatch, double eindtijd)//Verander de modulus
        {
            //Controleer of het nodig is
            if (nieuw == this.Modulus)
                return true;

            //Pas hem aan
            this.Modulus = nieuw;
            this.BepaalModuloIndices();
            if (stopwatch.ElapsedMilliseconds / 1000.0 >= eindtijd)
            {
                this.Modulus = -1;
                return false;
            }
            for (int x = 0; x < this.Knopen.Aantal && stopwatch.ElapsedMilliseconds / 1000.0 < eindtijd; x++)
                this.Knopen[x].NieuweModulus(this.Modulus);
            if (stopwatch.ElapsedMilliseconds / 1000.0 >= eindtijd)
            {
                this.Modulus = -1;
                return false;
            }
            this.gehad = new bool[nieuw];
            return true;
        }
        public void VerwijderGraad1(IList<Paar> paren)//Verwijder de knopen van graad 1 die toch worden samengevoegd
        {
            //Loop de knopen langs
            IList<Knoop> knopen = this.Knopen.List;
            Knoop welk, A, B;
            IList<Zijde> zijden;
            for (int x = 0; x < knopen.Count; x++)
            {
                A = knopen[x];
                zijden = A.Zijden.List;
                welk = null;
                for (int y = 0; y < zijden.Count; y++)
                {
                    B = zijden[y].Eind;
                    if (B.Graad == 1)
                    {
                        if (welk == null)
                            welk = B;
                        else
                        {
                            paren.Add(new Paar(welk, B));
                            this.Knopen.Verwijder(B);
                            A.Zijden.Verwijder(zijden[y]);
                        }
                    }
                }
            }
        }
        public bool CheckKliek(IList<Paar> paren)//Check voor klieken en verwijder ze
        {
            //Bepaal eerst de klieken
            bool[] gehad = new bool[this.Knopen.Aantal];
            for (int x = 0; x < this.Knopen.Aantal; x++)
                gehad[x] = false;
            IList<IList<Knoop>> klieken = new List<IList<Knoop>>();
            Lijst<Zijde> buren;
            Knoop A, B;
            IList<Knoop> kandidaat, kliek;
            IList<bool> doetmee;
            Dictionary<Knoop, bool> buurgehad = new Dictionary<Knoop, bool>();
            int niet;
            for (int x = 0; x < this.Knopen.Aantal; x++)
                if (!gehad[x])
                {
                    A = this.Knopen[x];
                    buren = A.Zijden[0];
                    kandidaat = new List<Knoop>();
                    doetmee = new List<bool>();
                    kandidaat.Add(A);
                    doetmee.Add(true);
                    niet = 0;
                    for (int y = 0; y < buren.Aantal; y++)
                    {
                        kandidaat.Add(buren[y].Eind);
                        if (buren[y].Eind.Graad != A.Graad)
                        {
                            niet++;
                            doetmee.Add(false);
                        }
                        else
                            doetmee.Add(true);
                    }
                    buurgehad.Clear();
                    for (int z = 0; z < kandidaat.Count; z++)
                        buurgehad.Add(kandidaat[z], false);
                    for (int y = 1; y < kandidaat.Count; y++)
                        if (kandidaat[y].Graad == A.Graad)
                        {
                            buren = kandidaat[y].Zijden[0];
                            for (int z = 0; z < buren.Aantal; z++)
                            {
                                B = buren[z].Eind;
                                if (!buurgehad.ContainsKey(B))
                                {
                                    niet++;
                                    doetmee[y] = false;
                                    break;
                                }
                            }
                        }
                    if (kandidaat.Count - niet >= 2)
                    {
                        kliek = new List<Knoop>();
                        for (int y = 0; y < kandidaat.Count; y++)
                            if (doetmee[y])
                            {
                                kliek.Add(kandidaat[y]);
                                gehad[kandidaat[y].LijstIndex] = true;
                            }
                        klieken.Add(kliek);
                    }
                }

            //Voeg ze samen
            Zijde ander;
            for (int x = 0; x < klieken.Count; x++)
            {
                kliek = klieken[x];
                for (int y = 1; y < kliek.Count; y++)
                {
                    //Verwijder al zijn zijden
                    buren = kliek[y].Zijden[0];
                    for (int z = 0; z < buren.Aantal; z++)
                    {
                        ander = buren[z].Andersom;
                        ander.Begin.Zijden.Verwijder(ander);
                    }

                    //Verwijder de knoop zelf
                    paren.Add(new Paar(kliek[0], kliek[y]));
                    this.Knopen.Verwijder(kliek[y]);
                }
            }
            return (klieken.Count > 0);
        }
        public bool CheckKliekAlles(IList<Paar> paren, bool positief, IList<IList<Knoop>> oud)//Check voor klieken die dezelfde buren hebben en afhankelijk van positief ook zelf een kliek vormen of helemaal niet
        {
            //Groepeer de knopen eerst op basis van graad
            IList<IList<Knoop>> groepen = new List<IList<Knoop>>();
            IDictionary<Knoop, int> welkegroep = new Dictionary<Knoop, int>(), groepindex = new Dictionary<Knoop, int>();
            Knoop A;
            int aantal = this.Knopen.Aantal;
            for (int x = 0; x < aantal; x++)
            {
                A = this.Knopen[x];
                while (groepen.Count <= A.Graad)
                {
                    if (oud.Count == 0)
                        groepen.Add(new List<Knoop>());
                    else
                    {
                        groepen.Add(oud[oud.Count - 1]);
                        oud.RemoveAt(oud.Count - 1);
                    }
                }
                groepen[A.Graad].Add(A);
                welkegroep.Add(A, A.Graad);
                groepindex.Add(A, groepen[A.Graad].Count - 1);
            }

            //Verwijder de lege groepen
            for (int x = 0; x < groepen.Count; x++)
                if (groepen[x].Count <= 1)
                {
                    for (int y = 0; y < groepen[x].Count; y++)
                        welkegroep[groepen[x][y]] = -1;
                    groepen[x].Clear();
                    oud.Add(groepen[x]);
                    groepen[x] = groepen[groepen.Count - 1];
                    groepen.RemoveAt(groepen.Count - 1);
                    if (x == groepen.Count)
                        break;
                    for (int y = 0; y < groepen[x].Count; y++)
                        welkegroep[groepen[x][y]] = x;
                    x--;
                }

            //Gebruik de knopen
            IDictionary<int, int> nieuwegroepen = new Dictionary<int, int>();
            Lijst<Zijde> zijden;
            IList<Knoop> invloed = new List<Knoop>();
            int aantal2, oudgroep, oudindex, nieuwgroep, nieuwindex;
            Knoop B, C;
            IList<int> leeg = new List<int>();
            for (int x = 0; x < aantal; x++)
            {
                //Verdeel de groepen
                A = this.Knopen[x];
                nieuwegroepen.Clear();
                zijden = A.Zijden[0];
                invloed.Clear();
                aantal2 = zijden.Aantal;
                leeg.Clear();
                for (int y = 0; y < aantal2; y++)
                    invloed.Add(zijden[y].Eind);
                if (positief)
                {
                    invloed.Add(A);
                    aantal2++;
                }
                for (int y = 0; y < aantal2; y++)
                {
                    B = invloed[y];
                    oudgroep = welkegroep[B];
                    if (oudgroep == -1)
                        continue;
                    if ((!nieuwegroepen.ContainsKey(oudgroep)) && groepen[oudgroep].Count == 1)//Nu is het niet de moeite waard
                    {
                        leeg.Add(oudgroep);
                        continue;
                    }
                    oudindex = groepindex[B];
                    groepen[oudgroep][oudindex] = groepen[oudgroep][groepen[oudgroep].Count - 1];
                    C = groepen[oudgroep][oudindex];
                    groepen[oudgroep].RemoveAt(groepen[oudgroep].Count - 1);
                    if (groepen[oudgroep].Count == 1)
                        leeg.Add(oudgroep);
                    groepindex[C] = oudindex;
                    if (nieuwegroepen.ContainsKey(oudgroep))
                        nieuwgroep = nieuwegroepen[oudgroep];
                    else
                    {
                        nieuwgroep = groepen.Count;
                        if (oud.Count == 0)
                            groepen.Add(new List<Knoop>());
                        else
                        {
                            groepen.Add(oud[oud.Count - 1]);
                            oud.RemoveAt(oud.Count - 1);
                        }
                        nieuwegroepen.Add(oudgroep, nieuwgroep);
                    }
                    nieuwindex = groepen[nieuwgroep].Count;
                    groepen[nieuwgroep].Add(B);
                    welkegroep[B] = nieuwgroep;
                    groepindex[B] = nieuwindex;   
                }

                //Verwijder lege groepen
                nieuwegroepen.Clear();
                for (int y = 0; y < leeg.Count; y++)
                {
                    oudgroep = leeg[y];
                    while (nieuwegroepen.ContainsKey(oudgroep))
                        oudgroep = nieuwegroepen[oudgroep];
                    for (int z = 0; z < groepen[oudgroep].Count; z++)
                        welkegroep[groepen[oudgroep][z]] = -1;
                    groepen[oudgroep].Clear();
                    oud.Add(groepen[oudgroep]);
                    groepen[oudgroep] = groepen[groepen.Count - 1];
                    groepen.RemoveAt(groepen.Count - 1);
                    nieuwegroepen.Add(groepen.Count, oudgroep);
                    if (oudgroep != groepen.Count)
                        for (int z = 0; z < groepen[oudgroep].Count; z++)
                            welkegroep[groepen[oudgroep][z]] = oudgroep;
                }
            }

            //Verwijder de groepen die niet de moeite waard zijn
            for (int x = 0; x < groepen.Count; x++)
                if (groepen[x].Count == 1)
                {
                    groepen[x].Clear();
                    oud.Add(groepen[x]);
                    groepen[x] = groepen[groepen.Count - 1];
                    groepen.RemoveAt(groepen.Count - 1);
                    x--;
                }

            //Voeg ze vervolgens samen
            IList<Knoop> groep;
            Zijde ander;
            for (int x = 0; x < groepen.Count; x++)
            {
                groep = groepen[x];
                for (int y = 1; y < groep.Count; y++)
                {
                    //Verwijder al zijn zijden
                    zijden = groep[y].Zijden[0];
                    for (int z = 0; z < zijden.Aantal; z++)
                    {
                        ander = zijden[z].Andersom;
                        ander.Begin.Zijden.Verwijder(ander);
                    }

                    //Verwijder de knoop zelf
                    paren.Add(new Paar(groep[0], groep[y]));
                    this.Knopen.Verwijder(groep[y]);
                }
                groep.Clear();
                oud.Add(groep);
            }
            return groepen.Count > 0;
        }
        public void CheckWaarde0(IList<Paar> paren)//Voeg alles samen met waarde 0
        {
            //Probeer klieken zo lang het kan
            bool netdoor = false;
            IList<IList<Knoop>> oud = new List<IList<Knoop>>();
            for (int x = 0; x < 2 || netdoor; x++)
                netdoor = CheckKliekAlles(paren, (x % 2 == 0), oud);
        }
        public void CheckSter(IList<Paar> paren)//Check of de graaf stervormig is en verminder de sterren
        {
            //Bepaal alle sterren
            const int minster = 10000, doelster = 1000;
            List<Knoop> sterren = new List<Knoop>();
            for (int x = 0; x < this.Knopen.Aantal; x++)
                if (this.Knopen[x].Graad > minster)
                    sterren.Add(this.Knopen[x]);
            if (sterren.Count == 0)
                return;

            //Voeg de sterren vervolgens samen
            Knoop ster;
            List<Knoop> lijstburen = new List<Knoop>();
            Heap<Knoop> buren;
            Knoop A, B;
            Lijst<Zijde> zijden;
            List<Zijde> zijdenA, zijdenB;
            for (int x = 0; x < sterren.Count; x++)
            {
                //Sorteer eerst de buren
                lijstburen.Clear();
                ster = sterren[x];
                if (ster.Graad < minster)
                    continue;
                zijden = ster.Zijden[0];
                for (int y = 0; y < zijden.Aantal; y++)
                    lijstburen.Add(zijden[y].Eind);
                buren = new Heap<Knoop>(lijstburen);

                //Ga ze vervolgens samen voegen
                while (buren.Aantal > doelster)
                {
                    //Sorteer de zijden
                    B = buren.VerwijderMin();
                    A = buren.Min;
                    zijdenA = A.Zijden[0].List;
                    zijdenB = B.Zijden[0].List;
                    Algoritmen.Sorteer<Zijde>(zijdenA, this.Leeg, (Zijde z1, Zijde z2) => z1.Eind.Nummer < z2.Eind.Nummer);
                    Algoritmen.Sorteer<Zijde>(zijdenB, this.Leeg, (Zijde z1, Zijde z2) => z1.Eind.Nummer < z2.Eind.Nummer);
                    paren.Add(new Paar(A, B));

                    //Loop vervolgens de zijden langs
                    int ia = 0, ib = 0;
                    bool nua, nub;
                    Zijde ander;
                    while (ia < zijdenA.Count || ib < zijdenB.Count)
                    {
                        nua = false;
                        nub = false;
                        if (ia == zijdenA.Count)
                            nub = true;
                        if (ib == zijdenB.Count)
                            nua = true;
                        if (!nua && (nub || zijdenB[ib].Eind.Nummer < zijdenA[ia].Eind.Nummer))//Nu gaan we een unieke zijde van B toevoegen aan A
                        {
                            //Controleer op de eventuele zijde van A naar B
                            if (zijdenB[ib].Eind == A)
                            {
                                ib++;
                                continue;
                            }

                            //Voeg de zijde toe
                            zijdenB[ib].Begin = A;
                            zijdenB[ib].Rood = true;
                            ander = zijdenB[ib].Andersom;
                            if (A.ModuloIndex == B.ModuloIndex)
                                ander.Eind = A;
                            else
                            {
                                ander.Begin.Zijden.Verwijder(ander);
                                ander.Eind = A;
                                ander.Begin.Zijden.VoegToe(ander);
                            }
                            ander.Rood = true;
                            A.Zijden.VoegToe(zijdenB[ib]);
                            ib++;
                            continue;
                        }
                        if (!nub && (nua || zijdenA[ia].Eind.Nummer < zijdenB[ib].Eind.Nummer))//Nu hebben we een unieke zijde van A
                        {
                            //Controleer op de eventuele zijde van A naar B
                            if (zijdenA[ia].Eind == B)
                            {
                                A.Zijden.Verwijder(zijdenA[ia]);
                                ia++;
                                continue;
                            }

                            //Maak de zijde rood
                            if (!zijdenA[ia].Rood)
                            {
                                zijdenA[ia].Rood = true;
                                ander = zijdenA[ia].Andersom;
                                ander.Rood = true;
                            }
                            ia++;
                            continue;
                        }

                        //Nu hebben we een zijde die bij beiden voorkomt
                        if (zijdenB[ib].Rood && !zijdenA[ia].Rood)//Nu moeten we hem rood maken
                        {
                            zijdenA[ia].Rood = true;
                            ander = zijdenA[ia].Andersom;
                            ander.Rood = true;
                        }

                        //Verwijder de zijde naar B
                        ander = zijdenB[ib].Andersom;
                        ander.Begin.Zijden.Verwijder(ander);

                        //Ga door met tellen
                        ia++;
                        ib++;
                    }

                    //Plaats A ergens anders en verwijder B
                    buren.AndereWaarde(A);
                    this.Knopen.Verwijder(B);
                }
            }
        }
        public void GretigAlles(IList<Paar> paren, Stopwatch stopwatch, double eindtijd, bool maakparen)//Doe het gretige algoritme op alles tot de eindtijd
        {
            //Zet alles klaar
            if (!this.VeranderModulus(1, stopwatch, eindtijd)) 
                return;
            if (maakparen)
                if (!this.MaakBeginParen(stopwatch, eindtijd))
                    return;

            //Doe het algoritme
            Paar nu;
            while (this.Contracties.Aantal > 0 && stopwatch.ElapsedMilliseconds < eindtijd * 1000)
            {
                nu = this.Contracties.VerwijderMin();
                paren.Add(nu);
                if (nu.Waarde > this.Waarde)
                {
                    this.Waarde = nu.Waarde;
                }
                this.VoegSamen(nu.Begin, nu.Eind, nu.Waarde);
            }
        }
        public void GretigDeels(IList<Paar> paren, Stopwatch stopwatch, double eindtijd, int doel, double macht)//Doe het gretige algoritme met de delen en laat hem naar dit doel gaan qua aantal knopen
        {
            //Bepaal eerst hoe lang het verwacht duurt zonder beginparen
            double complexiteit = this.BerekenMoeilijkheid();
            double graaddeel = this.BerekenGraadVerhouding(this.Knopen.Aantal - doel);
            double comppersec = CompPerSec * 25;
            int max = this.Knopen.Aantal, min = doel;
            double factor = complexiteit / Math.Pow(max, macht), tijd;
            if (Math.Abs(macht) > 0.001)
            {
                factor /= macht;
                tijd = graaddeel * factor * (Math.Pow(max, macht) - Math.Pow(min, macht)) / comppersec;
            }
            else
                tijd = graaddeel * factor * (Math.Log(max) - Math.Log(min)) / comppersec;
            double paarbegin = 3 * complexiteit / (comppersec);
            if (tijd < (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) / 2 && paarbegin < (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0)) 
            {
                int m = (int)Math.Ceiling(100 * (2 * tijd / (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0)));
                int mpaar = (int)Math.Ceiling(100 * (3 * complexiteit / (comppersec * (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0))));
                if (mpaar > m)
                    m = mpaar;
                if (m > 100)
                    m = 100;
                if (!this.VeranderModulus(m, stopwatch, eindtijd))
                    return;
                comppersec /= m;
                tijd *= m;
            }
            else
            {
                if (tijd < (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) * 2 / 3.0 && paarbegin < (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) * 2 / 3.0)
                {
                    if (!this.VeranderModulus(100, stopwatch, eindtijd))
                        return;
                }
                else
                {
                    if ((tijd < (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) * 5 && paarbegin < (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) * 5) || this.Knopen.Aantal <= 20000)
                    {
                        tijd /= 4;
                        if (!this.VeranderModulus(1000, stopwatch, eindtijd))
                            return;
                        comppersec *= 4;
                        if (tijd > (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) * 9 / 10.0)
                            tijd = (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) * 9 / 10.0;
                    }
                    else
                    {
                        tijd /= 16;
                        if (!this.VeranderModulus(10000, stopwatch, eindtijd))
                            return;
                        comppersec *= 16;
                        if (tijd > (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) * 9 / 10.0)
                            tijd = (eindtijd - stopwatch.ElapsedMilliseconds / 1000.0) * 9 / 10.0;
                    }
                }
            }

            //Bepaal vervolgens de gegevens
            IList<int> sommen = this.KiesBesteSom(stopwatch, eindtijd);
            if (sommen.Count == 0)
                return;
            this.ModuloSom = sommen[0];
            sommen.RemoveAt(0);
            if (!this.MaakBeginParen(stopwatch, eindtijd))
                return;
            double paartijd = eindtijd - stopwatch.ElapsedMilliseconds / 1000.0 - tijd;
            int aantalparen = this.Contracties.Aantal;
            int rondes;
            double stap;
            if (macht < 0.1)//Nu hebben we een gekke situatie
            {
                rondes = (int)(paartijd * comppersec / (complexiteit + 10 * aantalparen * comppersec / CompPerSec));
                if (rondes <= 0)
                    rondes = 1;
                stap = Math.Pow(doel * 1.0 / max, 1.0 / rondes);
            }
            else
            {
                factor = paartijd * comppersec / (complexiteit + 10 * aantalparen * comppersec / CompPerSec);
                if (factor < 2)
                    factor = 2;
                factor /= (1 - Math.Pow(doel * 1.0 / this.Knopen.Aantal, macht));
                stap = 1 - 1 / factor;
                stap = Math.Pow(stap, 1 / macht);
                rondes = (int)Math.Ceiling(Math.Log(doel * 1.0 / max) / Math.Log(stap));
                stap = Math.Pow(doel * 1.0 / this.Knopen.Aantal, 1.0 / rondes);
            }

            //Voer het algoritme uit
            double nieuwstap;
            double vorigwaarde;
            double waardefactor = 1 + 1.0 / this.Modulus;
            double waardestap = 1 + 1.0 / this.Modulus, maxomlaag = 2, maxomhoog = 2;
            Paar nu;
            double tussendoel = max;
            int nieuwrondes;
            bool netgestart = true;
            while (stopwatch.ElapsedMilliseconds / 1000 < eindtijd && this.Knopen.Aantal > doel)
            {
                //Bepaal de groepen
                if (!netgestart)
                {
                    if (sommen.Count == 0)
                        sommen = this.KiesBesteSom(stopwatch, eindtijd);
                    if (sommen.Count == 0)
                        return;
                    this.ModuloSom = sommen[0];
                    sommen.RemoveAt(0);

                    //Zet de paren klaar voor de volgende ronde
                    if (!this.MaakBeginParen(stopwatch, eindtijd))
                        return;
                    aantalparen = this.Contracties.Aantal;
                }
                netgestart = false;

                //Voer vervolgens de contracties uit
                vorigwaarde = this.Waarde;
                tussendoel *= stap;
                while (this.Contracties.Aantal > 0 && stopwatch.ElapsedMilliseconds / 1000.0 < eindtijd)
                {
                    nu = this.Contracties.Min;
                    if (nu.Waarde > vorigwaarde * waardestap && this.Knopen.Aantal <= tussendoel)
                        break;
                    this.Contracties.VerwijderMin();
                    paren.Add(nu);
                    if (nu.Waarde > this.Waarde)
                    {
                        this.Waarde = nu.Waarde;
                    }
                    this.VoegSamen(nu.Begin, nu.Eind, nu.Waarde);
                }


                //Doe een reflectie
                complexiteit = this.BerekenMoeilijkheid();
                graaddeel = this.BerekenGraadVerhouding(this.Knopen.Aantal - doel);
                factor = complexiteit / Math.Pow(this.Knopen.Aantal, macht);
                if (Math.Abs(macht) > 0.001)
                {
                    factor /= macht;
                    tijd = graaddeel * factor * (Math.Pow(this.Knopen.Aantal, macht) - Math.Pow(min, macht)) / comppersec;
                }
                else
                    tijd = graaddeel * factor * (Math.Log(this.Knopen.Aantal) - Math.Log(min)) / comppersec;
                paartijd = eindtijd - tijd - stopwatch.ElapsedMilliseconds / 1000.0;
                if (paartijd < 0)
                    paartijd = 0;
                if (macht < 0.1)
                {
                    nieuwrondes = (int)(paartijd * comppersec / (complexiteit + 10 * aantalparen * comppersec / CompPerSec));
                    if (nieuwrondes <= 0)
                        nieuwrondes = 1;
                    nieuwstap = Math.Pow(doel * 1.0 / this.Knopen.Aantal, 1.0 / nieuwrondes);
                }
                else
                {
                    factor = paartijd * comppersec / (complexiteit + 10 * aantalparen * comppersec / CompPerSec);
                    if (factor < 2)
                        factor = 2;
                    factor /= (1 - Math.Pow(doel * 1.0 / this.Knopen.Aantal, macht));
                    nieuwstap = 1 - 1 / factor;
                    nieuwstap = Math.Pow(nieuwstap, 1 / macht);
                    nieuwrondes = (int)Math.Ceiling(Math.Log(doel * 1.0 / max) / Math.Log(nieuwstap));
                    nieuwstap = Math.Pow(doel * 1.0 / this.Knopen.Aantal, 1.0 / nieuwrondes);
                }
                if (nieuwstap > 1)
                    nieuwstap = 1;
                if (nieuwstap >= stap / maxomlaag && nieuwstap <= stap * maxomhoog)
                    stap = nieuwstap;
                else
                {
                    if (nieuwstap < stap)
                        stap /= maxomlaag;
                    else
                        stap *= maxomhoog;
                }
            }
        }
        public void MaakAf(IList<Paar> paren)//Maak het af
        {
            //Voeg de knopen samen op volgorde van graad
            List<Knoop> knopen = this.Knopen.List, leeg = new List<Knoop>();
            Algoritmen.Sorteer<Knoop>(knopen, leeg, (Knoop k1, Knoop k2) => k1.Graad < k2.Graad);
            Knoop A, B;
            for (int x = 1; x < knopen.Count; x++) 
            {
                A = knopen[x];
                B = knopen[x - 1];
                paren.Add(this.MaakPaar(A, B));
            }
        }
        public IList<Paar> LosOp(bool graad1weg, bool checkkliek, bool checkster, TypeSorteerStructruur type, bool updatealles, Stopwatch stopwatch)//Los de graaf op
        {
            //Maak eerst de begintoestand
            this.Type = type;
            this.Modulus = 1;
            this.ModuloSom = 0;
            this.UpdateAlles = updatealles;
            IList<Paar> paren = new List<Paar>();
            if (graad1weg)
                this.VerwijderGraad1(paren);
            if (checkkliek)
                this.CheckWaarde0(paren);
            if (checkster)
                this.CheckSter(paren);

            //Handel de ijle grafen af
            double complexiteit = this.BerekenMoeilijkheid();
            if (complexiteit < this.Knopen.Aantal * 200) //Dit is een dusdanig ijle graaf dat we toch gretig gaan doen
            {
                this.GretigAlles(paren, stopwatch, 290, true);
                this.MaakAf(paren);
                return paren;
            }

            //Probeer het algoritme in 4 fasen
            double macht = 3, oudmacht = 3;
            double vorig, nieuwcomplexiteit;
            int doel;
            int aantal = this.Knopen.Aantal;
            bool netgretig = false;
            this.Waarde = 0;
            double totaal;
            const int einddoel = 500;
            double[] eindtijden = new double[] { 60, 160, 220, 280 };
            double tijd;
            for (int m = 1; m <= 4 && stopwatch.ElapsedMilliseconds / 1000.0 < eindtijden[3]; m++)
            {
                tijd = complexiteit / Math.Pow(this.Knopen.Aantal, macht);
                if (Math.Abs(macht) > 0.1)
                {
                    tijd /= macht;
                    tijd *= (Math.Pow(this.Knopen.Aantal, macht) - Math.Pow(einddoel, macht)) / CompPerSec;
                }
                else
                    tijd = (Math.Log(this.Knopen.Aantal) - Math.Log(einddoel)) / CompPerSec;
                if ((complexiteit < 200000000 && tijd < eindtijden[3] - stopwatch.ElapsedMilliseconds / 1000.0) || this.Knopen.Aantal <= einddoel || (this.Knopen.Aantal <= 10000 && complexiteit < 1000000000))
                {
                    if (m < 4)
                        tijd = 60 * m + 30;
                    else
                        tijd = eindtijden[m - 1];
                    if (!this.VeranderModulus(1, stopwatch, tijd))
                        continue;
                    if (!netgretig)
                        if (!this.MaakBeginParen(stopwatch, tijd))
                            continue;
                    vorig = stopwatch.ElapsedMilliseconds / 1000.0;
                    this.GretigAlles(paren, stopwatch, tijd, false);
                    netgretig = true;
                }
                else
                {
                    totaal = Math.Pow(this.Knopen.Aantal, macht) - Math.Pow(einddoel, macht);
                    if (m < 3)
                        totaal *= (eindtijden[2] - eindtijden[m - 1]) / (eindtijden[2] - stopwatch.ElapsedMilliseconds / 1000.0);
                    else
                        totaal = 0;
                    totaal += Math.Pow(einddoel, macht);
                    totaal = Math.Pow(totaal, 1.0 / macht);
                    doel = (int)totaal;
                    if (doel < einddoel)
                        doel = einddoel;
                    this.GretigDeels(paren, stopwatch, eindtijden[m - 1], doel, macht);
                    netgretig = false;
                }
                nieuwcomplexiteit = this.BerekenMoeilijkheid();
                if (this.Knopen.Aantal < aantal)
                    macht = Math.Log(nieuwcomplexiteit / complexiteit) / Math.Log(this.Knopen.Aantal * 1.0 / aantal);
                else
                    macht = 0;
                if (macht > 3)
                    macht = 3;
                if (oudmacht - macht > 1)
                    macht--;
                else
                {
                    if (macht < oudmacht)
                        macht = 2 * macht - oudmacht;
                }
                if (macht < -1)
                    macht = -1;
                oudmacht = macht;
                complexiteit = nieuwcomplexiteit;
                aantal = this.Knopen.Aantal;
            }

            //Maak het af
            this.MaakAf(paren);
            return paren;
        }
        public void Print()//Print de graaf
        {
            //Print eerst de knopen
            Console.WriteLine("Knopen");
            for (int x = 0; x < this.Knopen.Aantal; x++)
                Console.WriteLine(this.Knopen[x]);

            //Print vervolgens de zijden
            Console.WriteLine("Zijden");
            Lijst<Zijde> zijden;
            Zijde zijde;
            for (int x = 0; x < this.Knopen.Aantal; x++)
                foreach (KeyValuePair<int, Lijst<Zijde>> paar in this.Knopen[x].Zijden.Lijsten)
                {
                    zijden = paar.Value;
                    for (int y = 0; y < zijden.Aantal; y++)
                    {
                        zijde = zijden[y];
                        Console.Write(zijde.Begin + " " + zijde.Eind + " ");
                        if (zijde.Rood)
                            Console.WriteLine("Rood");
                        else
                            Console.WriteLine("Zwart");
                    }
                }
            Console.ReadLine();
        }
    }//De hele graaf met alle extra informatie
    interface ILijstElement
    {
        int LijstIndex
        {
            get;
            set;
        }
    }//Objecten van dit type kunnen goed worden opgeslagen in 1 lijst (niet in meerdere)
    class Lijst<T> where T : ILijstElement
    {
        private IList<T> elementen;//De elementen uit de lijst
        public T this[int x]
        {
            get
            {
                return this.elementen[x];
            }
        }
        public int Aantal
        {
            get
            {
                return this.elementen.Count;
            }
        }
        public List<T> List
        {
            get
            {
                //Maak de lijst
                List<T> lijst = new List<T>();
                for (int x = 0; x < this.elementen.Count; x++)
                    lijst.Add(elementen[x]);
                return lijst;
            }
        }
        public Lijst()//Maak een lege lijst
        {
            this.elementen = new List<T>();
        }
        public void VoegToe(T nieuw)//Voeg dit element toe aan de lijst
        {
            nieuw.LijstIndex = this.elementen.Count;
            this.elementen.Add(nieuw);
        }
        public void Verwijder(T weg)//Verwijder dit element uit de lijst
        {
            //Haal het laatste element naar voren
            T laatst = this.elementen[this.elementen.Count - 1];
            laatst.LijstIndex = weg.LijstIndex;
            this.elementen[weg.LijstIndex] = laatst;
            this.elementen.RemoveAt(this.elementen.Count - 1);
            weg.LijstIndex = -1;
        }
        public override string ToString()
        {
            string resultaat = "";
            for (int x = 0; x < elementen.Count; x++)
                resultaat += elementen[x] + "\n";
            return resultaat;
        }
    }//Een algemene lijst van elementen waarin je snel toe kan voegen en verwijderen
    class Paar : IHeapElement, IBucketElement, ISorteerElement
    {
        public double Waarde//De waarde van dit paar
        {
            get
            {
                return this.Begin.Graad + this.Eind.Graad - this.Overeenkomst;
            }
        }
        public double Overeenkomst;//De overeenkomst tussen de knopen
        public int HeapIndex//De index in de heap
        {
            get
            {
                return this.heapIndex;
            }
            set
            {
                this.heapIndex = value;
            }
        }
        private int heapIndex;//De heapindex
        public int LijstIndex
        {
            get
            {
                return this.lijstIndex;
            }
            set
            {
                this.lijstIndex = value;
            }
        }//De index in een lijst
        private int lijstIndex;
        public int BucketIndex//In welke bucket zit hij?
        {
            get
            {
                return this.bucketIndex;
            }
            set
            {
                this.bucketIndex = value;
            }
        }
        private int bucketIndex;
        public Knoop Begin, Eind;//De knopen die samengevoegd gaan worden
        public Paar(Knoop begin, Knoop eind)
        {
            this.Begin = begin;
            this.Eind = eind;
            this.Overeenkomst = 0;
            this.HeapIndex = -1;
            this.LijstIndex = -1;
            this.BucketIndex = -1;
        }
        public override string ToString()
        {
            return this.Begin + " " + this.Eind;
        }
    }//Een paar van knopen dat samengevoegd kan worden
    interface ISorteerElement
    {
        double Waarde
        {
            get;
        }
    }
    interface IHeapElement
    {
        int HeapIndex
        {
            get;
            set;
        }
    }
    interface IBucketElement : ILijstElement
    {
        int BucketIndex
        {
            get;
            set;
        }
    }
    interface ISorteerStructuur<T> where T : ISorteerElement
    {
        int Aantal
        {
            get;
        }
        T Min
        {
            get;
        }
        List<T> List
        {
            get;
        }
        T VerwijderMin();//Verwijder het kleinste element en geef hem terug
        void VoegToe(T extra);//Voeg dit element toe aan de structuur
        void AndereWaarde(T element);//Pas de waarde van dit element aan
        void Verwijder(T weg);//Verwijder dit element
    }//Dit stelt een algemene structuur voor waarin snel dingen kunnen worden gesorteerd
    class Heap<T> : ISorteerStructuur<T> where T:ISorteerElement, IHeapElement
    {
        private IList<T> elementen;//De elementen van de heap
        public List<T> List
        {
            get
            {
                return (List<T>)this.elementen;
            }
        }
        public int Aantal
        {
            get
            {
                return this.elementen.Count;
            }
        }//Het aantal elementen in de heap
        public T Min
        {
            get
            {
                return this.elementen[0];
            }
        }//Het kleinste element
        public Heap()//Maak een lege heap
        {
            this.elementen = new List<T>();
        }
        public Heap(IList<T> elementen)//Maak een heap van deze elementen aan
        {
            //Voeg de elementen toe
            this.elementen = new List<T>();
            for (int x = 0; x < elementen.Count; x++)
            {
                this.elementen.Add(elementen[x]);
                elementen[x].HeapIndex = x;
            }

            //Maak de heap-structuur aan
            for (int x = elementen.Count - 1; x >= 0; x--)
                this.naarBlad(x);
        }
        private bool naarWortel(int index)//Haal het element op deze plek omhoog naar de wortel en geef terug of het nodig was
        {
            //Kijk of het ￃﾃￂﾼberhaupt kan
            if (index == 0)
                return false;

            //Vergelijk hem met zijn ouder
            int boven = index / 2;
            if (this.elementen[boven].Waarde <= this.elementen[index].Waarde)
                return false;

            //Wissel hem naar boven
            this.wissel(index, boven);
            this.naarWortel(boven);
            return true;
        }
        private bool naarBlad(int index)//Haal het element op deze plek omlaag naar de bladeren en geef terug of het nodig was
        {
            //Kijk of het mogelijk is
            if (2 * index + 1 >= this.Aantal)
                return false;

            //Bepaal met welk kind moet worden gewisseld
            int kind = 2 * index + 1;
            if (2 * index + 2 < this.Aantal && this.elementen[2 * index + 2].Waarde < this.elementen[2 * index + 1].Waarde)
                kind = 2 * index + 2;
            if (this.elementen[kind].Waarde >= this.elementen[index].Waarde)
                return false;

            //Wissel hem
            this.wissel(index, kind);
            this.naarBlad(kind);
            return true;
        }
        private void wissel(int a, int b)//Wissel de elementen op plek a en b om
        {
            //Controleer of het moet
            if (a == b)
                return;

            //Voer het uit
            T e1 = this.elementen[a], e2 = this.elementen[b];
            e1.HeapIndex = b;
            e2.HeapIndex = a;
            this.elementen[a] = e2;
            this.elementen[b] = e1;
        }
        public T VerwijderMin()//Verwijder het minimum
        {
            //Verwijder hem
            T min = this.Min, laatst = this.elementen[this.Aantal - 1];
            this.elementen[0] = laatst;
            laatst.HeapIndex = 0;
            this.elementen.RemoveAt(this.Aantal - 1);
            min.HeapIndex = -1;

            //Herstel de heap
            this.naarBlad(0);
            return min;
        }
        public void VoegToe(T extra)//Voeg een element toe aan de heap
        {
            //Voeg het element toe
            extra.HeapIndex = this.Aantal;
            this.elementen.Add(extra);

            //Herstel de heap
            this.naarWortel(this.Aantal - 1);
        }
        public void AndereWaarde(T element)//Reageer als dit element een andere waarde krijgt
        {
            //Herstel de heap
            if (!this.naarWortel(element.HeapIndex))
                this.naarBlad(element.HeapIndex);
        }
        public void Verwijder(T weg)//Verwijder dit element uit de heap
        {
            //Controleer of we moeilijk moeten doen
            int index = weg.HeapIndex;
            if (index == this.Aantal - 1)
            {
                this.elementen.RemoveAt(this.Aantal - 1);
                return;
            }

            //Haal het laatste element terug
            T ander = this.elementen[this.Aantal - 1];
            this.elementen[index] = ander;
            this.elementen.RemoveAt(this.Aantal - 1);
            ander.HeapIndex = index;
            weg.HeapIndex = -1;

            //Herstel de heap
            if (!this.naarWortel(index))
                this.naarBlad(index);
        }
    }//Een klasse om de heap van paren mee weer te geven
    class Buckets<T> : ISorteerStructuur<T> where T : ISorteerElement, IBucketElement
    {
        public int Aantal
        {
            get
            {
                return this.aantal;
            }
        }//Het aantal elementen in deze lijst van buckets
        private int aantal;
        private int minBucket;//De minimale bucket met inhoud
        public T Min
        {
            get
            {
                return this.buckets[this.minBucket][0];
            }
        }
        private IList<Lijst<T>> buckets;//De buckets
        private double minWaarde, stapWaarde;//De minimale waarde en de stap die wordt gemaakt per bucket
        public List<T> List
        {
            get
            {
                //Maak hem
                List<T> lijst = new List<T>();
                int aantal;
                for (int x = 0; x < this.buckets.Count; x++)
                {
                    aantal = this.buckets[x].Aantal;
                    for (int y = 0; y < aantal; y++)
                        lijst.Add(this.buckets[x][y]);
                }
                return lijst;
            }
        }
        public Buckets(double minwaarde, double stapwaarde)
        {
            this.buckets = new List<Lijst<T>>();
            this.aantal = 0;
            this.minBucket = int.MaxValue;
            this.minWaarde = minwaarde;
            this.stapWaarde = stapwaarde;
        }
        public Buckets(double minwaarde, double stapwaarde, IList<T> elementen)
        {
            //Maak een lege structuur klaar
            this.minWaarde = minwaarde;
            this.stapWaarde = stapwaarde;
            this.buckets = new List<Lijst<T>>();
            this.minBucket = int.MaxValue;
            this.aantal = 0;

            //Voeg ze toe
            for (int x = 0; x < elementen.Count; x++)
                this.VoegToe(elementen[x]);
        }
        public T VerwijderMin()//Verwijder het minimum
        {
            //Haal hem op en verwijder hem
            T min = this.Min;
            this.buckets[this.minBucket].Verwijder(min);
            this.aantal--;
            if (this.aantal == 0)
            {
                this.minBucket = int.MaxValue;
                return min;
            }

            //Bepaal de volgende bucket die niet leeg is
            while (this.buckets[this.minBucket].Aantal == 0)
                this.minBucket++;
            return min;
        }
        public void VoegToe(T extra)//Voeg dit element toe
        {
            //Bepaal waar hij heen moet
            int index = (int)((extra.Waarde - this.minWaarde) / this.stapWaarde);
            while (index >= this.buckets.Count)
                this.buckets.Add(new Lijst<T>());

            //Voeg hem toe
            this.buckets[index].VoegToe(extra);
            extra.BucketIndex = index;
            this.aantal++;
            if (index < this.minBucket)
                this.minBucket = index;
        }
        public void AndereWaarde(T element)//Pas de waarde aan van dit element
        {
            //Bepaal of er iets moet gebeuren
            int index = (int)((element.Waarde - this.minWaarde) / this.stapWaarde);
            if (index == element.BucketIndex)
                return;

            //Pas hem aan
            this.buckets[element.BucketIndex].Verwijder(element);
            while (index >= this.buckets.Count)
                this.buckets.Add(new Lijst<T>());
            this.buckets[index].VoegToe(element);
            element.BucketIndex = index;
            if (index < this.minBucket)
                this.minBucket = index;
            while (this.buckets[this.minBucket].Aantal == 0)
                this.minBucket++;
        }
        public void Verwijder(T weg)//Verwijder dit element
        {
            //Verwijder hem
            this.buckets[weg.BucketIndex].Verwijder(weg);
            this.aantal--;

            //Herstel de minbucket
            if (this.aantal == 0)
            {
                this.minBucket = int.MaxValue;
                return;
            }
            while (this.buckets[this.minBucket].Aantal == 0)
                this.minBucket++;
        }
        public void Print()
        {
            Console.WriteLine("Buckets");
            for (int x = 0; x < this.buckets.Count; x++)
            {
                Console.WriteLine((this.minWaarde + x * this.stapWaarde) + " - " + (this.minWaarde + (x + 1) * this.stapWaarde));
                for (int y = 0; y < this.buckets[x].Aantal; y++)
                    Console.WriteLine(this.buckets[x][y]);
                Console.WriteLine();
            }
        }
    }//Een lijst van buckets
    enum TypeSorteerStructruur
    {
        Heap,
        Buckets
    }
    interface IModuloLijstElement : ILijstElement
    {
        int ModuloWaarde
        {
            get;
        }
    }
    class ModuloLijst<T> where T : IModuloLijstElement
    {
        private Dictionary<int, Lijst<T>> lijsten;//De lijsten van deze modulolijst
        private int modulus;//De modulus van de lijst
        private int aantal;//Het aantal elementen
        public List<T> List
        {
            get
            {
                //Maak de lijst
                List<T> list = new List<T>();
                Lijst<T> lijst;
                int aantal;
                foreach (KeyValuePair<int, Lijst<T>> paar in this.lijsten)
                {
                    lijst = paar.Value;
                    aantal = lijst.Aantal;
                    for (int x = 0; x < aantal; x++)
                        list.Add(lijst[x]);
                }
                return list;
            }
        }
        public int Aantal
        {
            get
            {
                return this.aantal;
            }
        }
        public Lijst<T> this[int x]
        {
            get
            {
                return this.lijsten[x];
            }
        }
        public Dictionary<int, Lijst<T>> Lijsten
        {
            get
            {
                return this.lijsten;
            }
        }
        public ModuloLijst(int m)
        {
            //Maak lege lijsten aan
            this.modulus = m;
            this.aantal = 0;
            this.lijsten = new Dictionary<int, Lijst<T>>();
        }
        public void VoegToe(T extra)//Voeg dit element toe
        {
            //Voeg hem toe aan de juiste lijst
            int i = extra.ModuloWaarde % this.modulus;
            if (!this.lijsten.ContainsKey(i))
                this.lijsten.Add(i, new Lijst<T>());
            this.lijsten[i].VoegToe(extra);
            this.aantal++;
        }
        public void Verwijder(T weg)//Verwijder dit element
        {
            //Verwijder hem
            int i = weg.ModuloWaarde % this.modulus;
            this.lijsten[i].Verwijder(weg);
            this.aantal--;
        }
        public void AndereModulus(int m)//Pas de modulus aan
        {
            //Controleer of het nodig is
            if (m == this.modulus)
                return;

            //Verzamel alle items
            IList<T> items = new List<T>();
            Lijst<T> lijst;
            int aantal;
            foreach (KeyValuePair<int, Lijst<T>> paar in this.lijsten)
            {
                lijst = paar.Value;
                aantal = lijst.Aantal;
                for (int y = 0; y < aantal; y++)
                    items.Add(lijst[y]);
            }

            //Pas ze toe
            this.lijsten.Clear();
            this.aantal = 0;
            this.modulus = m;
            for (int x = 0; x < items.Count; x++)
                this.VoegToe(items[x]);
        }
    }//Een array van lijsten gebaseerd op modulorekenen
    static class Algoritmen
    {
        public static void Sorteer<T>(List<T> lijst, List<T> leeg, Func<T, T, bool> kleiner)//Sorteer deze lijst op basis van deze functie
        {
            //Sorteer ze
            leeg.Clear();
            for (int x = 0; x < lijst.Count; x++)
                leeg.Add(lijst[x]);
            sorteer(lijst, leeg, kleiner, 0, lijst.Count);
        }
        private static void sorteer<T>(List<T> lijst, List<T> ander, Func<T, T, bool> kleiner, int a, int b)//Sorteer de lijst van element a naar b
        {
            if (b - a <= 20)//Nu gaan we het met insertion sort doen
            {
                int welk;
                for (int rang = 0; rang < b - a; rang++)
                {
                    welk = a + rang;
                    for (int x = a + rang + 1; x < b; x++)
                        if (kleiner(lijst[x], lijst[welk]))
                            welk = x;
                    wissel(lijst, a + rang, welk);
                }
                return;
            }

            //We gaan hem nu eerst deels sorteren
            int m = (a + b) / 2;
            sorteer(ander, lijst, kleiner, a, m);
            sorteer(ander, lijst, kleiner, m, b);

            //Nu voegen we de 2 stukken samen
            int w1 = a, w2 = m;
            while (w1 < m || w2 < b)
            {
                if (w2 == b)
                {
                    lijst[w1 + w2 - m] = ander[w1];
                    w1++;
                    continue;
                }
                if (w1 == m)
                {
                    lijst[w1 + w2 - m] = ander[w2];
                    w2++;
                    continue;
                }
                if (kleiner(ander[w1], ander[w2]))
                {
                    lijst[w1 + w2 - m] = ander[w1];
                    w1++;
                }
                else
                {
                    lijst[w1 + w2 - m] = ander[w2];
                    w2++;
                }
            }
        }
        private static void wissel<T>(List<T> lijst, int a, int b)//Wissel deze elementen om
        {
            //Wissel ze
            T ander = lijst[a];
            lijst[a] = lijst[b];
            lijst[b] = ander;
        }
    }//Deze klasse bevat algemeen nuttige algoritmen zoals sorteren
}
