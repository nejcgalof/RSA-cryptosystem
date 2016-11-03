using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.IO;

namespace RSA
{

    public partial class Form1 : Form
    {
        private uint LCG(uint m, uint a, uint b, uint RN)
        {
            return a * RN + b % m;
        }
        private bool miller_rabin(uint stevilo, int s)
        {
            if (stevilo < 3)
            {
                return true;
            }
            if (stevilo % 2 == 0) //stevilo je sodo
            {
                return false;
            }
            var parametra_k_in_d = k_in_d(stevilo);
            uint k = parametra_k_in_d.Item1;
            uint d = parametra_k_in_d.Item2;

            for (int i = 1; i <= s; i++)
            {
                uint a_int = give_random(2, stevilo - 2); //priča
                ulong a = (ulong)a_int; //ulong zaradi modular funkcije - veliko mnozenje unici zadevo
                ulong x = modular_exponentiation(a, d, stevilo); //a^d mod stevilo
                if (x != 1)
                {
                    for (int j = 0; j < k - 1; j++)
                    {
                        if (x == stevilo - 1)
                        {
                            break;
                        }
                        x = modular_exponentiation(x, 2, stevilo);
                    }
                    if (x != stevilo - 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private ulong modular_exponentiation(ulong a, uint b, uint n)
        {
            ulong d = 1;
            string biti = Convert.ToString(b, 2); //dvojiska j mestna predstavitev stevila b
            for (int i = 0; i < biti.Length; i++)
            {
                d = (d * d) % n;
                if (biti[i] == '1')
                {
                    d = (d * a) % n;
                }
            }
            return d;
        }

        private Tuple<uint, uint> k_in_d(uint stevilo) //d2^k = r-1 (to more veljati)
        {
            uint k = 0;
            uint d = stevilo - 1;
            while (d % 2 == 0) //dokler je d sod
            {
                d = d / 2;
                k = k + 1;
            }
            return new Tuple<uint, uint>(k, d);
        }

        private uint give_random(uint a, uint b) //za generiranje prič
        {
            Random random = new Random();
            int seme = random.Next();
            LCG(uint.MaxValue, 69069, 0, Convert.ToUInt32(seme));
            return a + LCG(uint.MaxValue, 69069, 0, Convert.ToUInt32(seme)) % (b - a + 1);
        }

        public Form1()
        {
            InitializeComponent();
        }

        void generiranje_2_prastevil()
        {
            int seme = 0;
            Random random = new Random();
            bool ni_najdel = true;
            uint prastevilo = 0;
            for (int g = 0; g < 2; g++)
            {
                while (ni_najdel)
                {
                    seme = random.Next();

                    uint RN1 = LCG(uint.MaxValue, 69069, 0, Convert.ToUInt32(seme));

                    uint min = 1;
                    uint max = 1;
                    prastevilo = 0;
                    if (textBox1.Text != string.Empty)
                    {
                        int st_znakov = Convert.ToInt32(textBox1.Text);
                        for (int i = 1; i < st_znakov; i++)
                        {
                            min *= 10;
                        }
                        max = min * 10 - 1;
                    }
                    prastevilo = min + RN1 % (max - min + 1);
                    if (prastevilo % 2 == 0) //stevilo je sodo
                    {
                        prastevilo += 1; //damo na liho stevilo
                    }
                    if (prastevilo_p == prastevilo)//ne smeta biti enaka
                    {
                        prastevilo += 2;
                    }
                    if (miller_rabin(prastevilo, 20))//20 je že dokaz da je število praštevilo
                    {
                        ni_najdel = false;
                    }
                }//sem najdel
                if (g == 0)
                {
                    prastevilo_p = prastevilo;
                }
                else
                {
                    prastevilo_q = prastevilo;
                }
                ni_najdel = true;
            }
            //konec generiranja 2 prastevil
        }

        long eulerjeva(long p, long q)
        {
            return ((p - 1) * (q - 1));
        }
        long nakljucno_prastevilo_e(long euler)
        {
            int seme = 0;
            Random random = new Random();
            bool ni_najdel = true;
            bool deli = true;
            uint prastevilo = 0;
            while (ni_najdel || deli)
            {
                seme = random.Next();

                uint RN1 = LCG(uint.MaxValue, 69069, 0, Convert.ToUInt32(seme));

                uint min = 2;
                uint max = (uint)euler;
                prastevilo = 0;
                prastevilo = min + RN1 % (max - min + 1);
                if (prastevilo % 2 == 0) //stevilo je sodo
                {
                    prastevilo += 1; //damo na liho stevilo
                }
                if (prastevilo_p == prastevilo)//ne smeta biti enaka
                {
                    prastevilo += 2;
                }
                if (miller_rabin(prastevilo, 20))//20 je že dokaz da je število praštevilo
                {
                    ni_najdel = false; 
                    //Ok je prastevilo, ampak ne sme delit eulerja
                    if(euler % prastevilo != 0)
                    {
                        deli = false;
                    }
                    else //ce deli, spet od zacetka
                    {
                        MessageBox.Show(prastevilo.ToString());
                        ni_najdel = true;
                        deli = true;
                    }
                }
            }//sem najdel
            return prastevilo;
        }


        long n_d = 1;
        long n_x = 0;
        long n_y = 0;
        void extended_euclid(long a, long b, long d, long x, long y)
        {
            if (b == 0)
            {
                d = a;
                x = 1;
                y = 0;
                n_d = d;
                n_y = y;
                n_x = x;
            }
            else
            {
                extended_euclid(b, a % b, n_d, n_x, n_y);
                d = n_d;
                x = n_y;
                y = n_x - ((long)(a / b)) * n_y;
                n_d = d;
                n_y = y;
                n_x = x;
            }
        }

        long modExp(long a,long b, long n)
        {
            long d = 1;
            String k = Convert.ToString(b, 2);
            for (int i = 1; i <= k.Length; i++)
            {
                if (k[k.Length - i] == '1')
                {
                    d = (d * a) % n;
                }
                a = (a * a) % n;
            }
            return d;
        }

        BigInteger modExpBig(BigInteger a, long b, BigInteger n)
        {
            BigInteger d = 1;
            String k = Convert.ToString(b, 2);
            for (int i = 1; i <= k.Length; i++)
            {
                if (k[k.Length - i] == '1')
                {
                    d = (d * a) % n;
                }
                a = (a * a) % n;
            }
            return d;
        }

        long modular_linear_equation_solver(long a, long b, long n)
        {
            long x=0;
            long y = 0;
            long d = 1;
            extended_euclid(a,n,d,x,y); //poisce najvecji skupni delitelj stevil a in b, to je d in x ter y. (resujemo d=ax+by)

            if (n_x < 0) //ce je x negativen
            {
                n_x=n_x + n;
            }
            
            //imamo d, e in n
            if (n_d % b==0)
            {
                //long x0 = n_x * (b / d) % n;
                long x0 = modExp(n_x, (b / n_d), n);
                return x0;
            }
            else
            {
                MessageBox.Show("resitev ne obstja");
                return 11; //primer neke napake
            }
            return 11; //primer neke napake
        }

        void zakodiraj_sporocilo(long parameter_e, long n,long d)
        {
            using (FileStream fs = new FileStream("sporocilo.txt", FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                byte[] asciiBytes = Encoding.ASCII.GetBytes(textBox3.Text);
                BigInteger st_znakov = modExpBig(asciiBytes.Length, parameter_e, n);
                sw.Write(st_znakov.ToString());
                for (int i = 0; i < asciiBytes.Length; i++) {
                    BigInteger c2 = modExpBig(asciiBytes[i], parameter_e, n);
                    sw.Write(" ");
                    sw.Write(c2);
                }
            }
        }

        long prastevilo_p;
        long prastevilo_q;
        private void button1_Click(object sender, EventArgs e)
        {
            //GENERIRANJE PRAŠTEVIL
            generiranje_2_prastevil();
            //imamo p in q ki sta si različna
            //izracunamo produkt n=p*q
            long n = prastevilo_p * prastevilo_q;
            long euler = eulerjeva(prastevilo_p, prastevilo_q);
            //izberemo nakjlucno liho število e (2 < e<eulerja) -> je pratevilo in je tuje z Eulerjem
            long parameter_e = nakljucno_prastevilo_e(euler);
            //izracunamo skriti eksponent d - multiplikativni inverz stevila e po modulu eulerja (e d =1 (mod euler))
            long d=modular_linear_equation_solver(parameter_e, 1, euler); //izracunamo skriti eksponent d 
            //zapisi javni in skriti kljuc
            // izhod:
            //[e,n]
            //[d,n]
            string keys_file = parameter_e.ToString()+" "+n.ToString() + Environment.NewLine + d.ToString() + " " + n.ToString();
            File.WriteAllText("kljuci.txt", keys_file);
            //BigInteger c2 = modExpBig(459, parameter_e, n);
            //BigInteger s = modExpBig(c2, d, n);
            zakodiraj_sporocilo(parameter_e, n, d);

            MessageBox.Show("zakodirano");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //pridobim kljuce
            textBox3.Text = "";
            var kluci = File.ReadAllText("kljuci.txt");
            var array_kluci = kluci.Split((string[])null, StringSplitOptions.RemoveEmptyEntries); //na presledke locim
            long parameter_e = Convert.ToInt64(array_kluci[0]);
            long n = Convert.ToInt64(array_kluci[1]);
            long d = Convert.ToInt64(array_kluci[2]);
            //pridobim sporocilo
            var fileContent = File.ReadAllText("sporocilo.txt");
            var array_sifriranih = fileContent.Split((string[])null, StringSplitOptions.RemoveEmptyEntries); //na presledke locim
            //pridobim prvo cifro za dolzino
            long zt_znak_sifr = Convert.ToInt64(array_sifriranih[0]);
            BigInteger st_znak = modExpBig(zt_znak_sifr, d, n);
            if ((long)st_znak == array_sifriranih.Length-1) //ce se ujema, smo prav naredili
            {
                byte[] znaki_ascii = new byte[(int)st_znak];
                for (int i = 1; i < array_sifriranih.Length; i++)            //dekodiram znak po znak
                {
                    long znak_sifr = Convert.ToInt64(array_sifriranih[i]);
                    BigInteger s = modExpBig(znak_sifr, d, n);
                    textBox3.Text += Encoding.ASCII.GetString(new byte[] { (byte)s });
                }
            }
            else
            {
                MessageBox.Show("neujemanje");
            }
        }

        private void button3_Click(object sender, EventArgs e) //kodiramo stevilsko sporocilo
        {
            //GENERIRANJE PRAŠTEVIL
            generiranje_2_prastevil();
            //imamo p in q ki sta si različna
            //izracunamo produkt n=p*q
            long n = prastevilo_p * prastevilo_q;
            if (Convert.ToInt64(textBox4.Text) >= n)
            {
                MessageBox.Show("sporocilo ni manjse od produkta p in q");
                return;
            }
            long euler = eulerjeva(prastevilo_p, prastevilo_q);
            //izberemo nakjlucno liho število e (2 < e<eulerja) -> je pratevilo in je tuje z Eulerjem
            long parameter_e = nakljucno_prastevilo_e(euler);
            //izracunamo skriti eksponent d - multiplikativni inverz stevila e po modulu eulerja (e d =1 (mod euler))
            long d = modular_linear_equation_solver(parameter_e, 1, euler); //izracunamo skriti eksponent d 
            //zapisi javni in skriti kljuc
            // izhod:
            //[e,n]
            //[d,n]
            string keys_file = parameter_e.ToString() + " " + n.ToString() + Environment.NewLine + d.ToString() + " " + n.ToString();
            File.WriteAllText("kljuci.txt", keys_file);
            BigInteger c2 = modExpBig(Convert.ToInt64(textBox4.Text), parameter_e, n);
            //BigInteger s = modExpBig(c2, d, n);
            //zakodiraj_sporocilo(parameter_e, n, d);
            string number_file = c2.ToString();
            File.WriteAllText("sporocilo.txt",number_file);
            MessageBox.Show("zakodirano");
        }

        private void button4_Click(object sender, EventArgs e) //dekodiram stevilko
        {
            //pridobim kljuce
            textBox4.Text = "";
            var kluci = File.ReadAllText("kljuci.txt");
            var array_kluci = kluci.Split((string[])null, StringSplitOptions.RemoveEmptyEntries); //na presledke locim
            long parameter_e = Convert.ToInt64(array_kluci[0]);
            long n = Convert.ToInt64(array_kluci[1]);
            long d = Convert.ToInt64(array_kluci[2]);
            //pridobim sporocilo
            var fileContent = File.ReadAllText("sporocilo.txt");
            long znak = Convert.ToInt64(fileContent);
            BigInteger st = modExpBig(znak, d, n);
            textBox4.Text += st.ToString();
        }
    }
}
