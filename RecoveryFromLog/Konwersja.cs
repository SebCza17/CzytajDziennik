using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecoveryFromLog
{
    public class Konwersja
    {
        public static int heksNaLiczbe(String zawartoscHeksKolumny, int n = 0)
        {
            int zlicz = 0; //#1
            double wynik = 0; //#2

            for (int i = 1; i <= zawartoscHeksKolumny.Count(); i++) //#3
            {
                if (i % 2 == 0) //#4
                {
                    string heksCzescOdwrocony = String.Concat(zawartoscHeksKolumny.Substring(i - 2, 2).Reverse()); //#5
                    for (int j = 0; j < 2; j++) //#6
                    {
                        wynik += int.Parse(String.Concat(Convert.ToByte(
                            heksCzescOdwrocony.Substring(j, 1), 16)
                        )) * Math.Pow(16, zlicz); //#7
                        zlicz++; //#8
                    }
                }

                if (zlicz >= n) //#9
                    break;
            }
            return (int)wynik; //#10
        }

        public static String heksNaString(string zawartoscHeksKolumny)
        {
            int j = 0;
            var zbiorWyjsciowy = new byte[zawartoscHeksKolumny.Length / 2];
            for (int i = 0; i < zbiorWyjsciowy.Length; i++)
            {
                string fragment = zawartoscHeksKolumny.Substring(i * 2, 2);
                if (!fragment.Equals("00"))
                {
                    zbiorWyjsciowy[j] = Convert.ToByte(fragment, 16);
                    j++;
                }
            }
            return Encoding.GetEncoding("windows-1250").GetString(zbiorWyjsciowy);
        }



        public static String heksNaTyp(String zawartoscHeksKolumny, int typKolumny)
        {
            String zawartoscKolumny = "";
            switch (typKolumny)
            {
                case 48:
                    zawartoscKolumny = heksNaLiczbe(zawartoscHeksKolumny, 1).ToString();
                    break;
                case 52:
                    zawartoscKolumny = heksNaLiczbe(zawartoscHeksKolumny, 2).ToString();
                    break;
                case 56:
                    zawartoscKolumny = heksNaLiczbe(zawartoscHeksKolumny, 4).ToString();
                    break;
                case 127:
                    zawartoscKolumny = heksNaLiczbe(zawartoscHeksKolumny, 8).ToString();
                    break;
                case 35:
                case 99:
                case 167:
                case 175:
                case 231:
                case 239:
                    zawartoscKolumny = heksNaString(zawartoscHeksKolumny);
                    break;
                default:
                    zawartoscKolumny = "NieRozpoznano";
                    break;
            }

            return zawartoscKolumny;
        }
    }
}
