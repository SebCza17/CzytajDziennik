using System;
using System.Collections.Generic;
using System.Linq;
using RecoveryFromLog;

public class ArgumentyKolumn
{
    public String zawartoscHeksKolumny;

    public ArgumentyKolumn(MetadaneKolumn metadane, int przesuniecieKolumny, int typKolumny)
    {
        int wzglednyKoniecKolumny = wezWzglednyKoniecKolumny(metadane.tablicaWymiarow, przesuniecieKolumny);
        int dlugoscKolumny = wezDlugoscKolumny(metadane.tablicaWymiarow, przesuniecieKolumny, typKolumny, metadane.poczatekBlokuDanych);
        zawartoscHeksKolumny = wezZawartoscHeksKolumny(metadane.kontent, wzglednyKoniecKolumny, dlugoscKolumny);
    }
    
    private static int wezWartoscZTablicyWymiarow(String tablicaWymiarow, int przesuniecieKolumny)
    {
        przesuniecieKolumny += 1;

        return Konwersja.heksNaLiczbe(String.Concat(tablicaWymiarow.Skip((2 * (Math.Abs(przesuniecieKolumny)) - 1) * 2).Take(2 * 2)), 2);
    }


    private static int wezWzglednyKoniecKolumny(String tablicaWymiarow, int przesuniecieKolumny)
    {
        int wzglednyKoniecKolumny = wezWartoscZTablicyWymiarow(tablicaWymiarow, przesuniecieKolumny);
        return wzglednyKoniecKolumny > 30000 ? (wzglednyKoniecKolumny - (int)Math.Pow(2, 15)) : wzglednyKoniecKolumny;
    }

    private static int wezDlugoscKolumny(String tablicaWymiarow, int przesuniecieKolumny, int typKolumny, int poczatekBlokuDanych)
    {
        List<int> typeList = new List<int> { 34, 35, 99 };
        int aktualnyWzglednyKoniecKolumny = wezWartoscZTablicyWymiarow(tablicaWymiarow, przesuniecieKolumny);

        int poprzedniWzglednyKoniecKolumny = przesuniecieKolumny == -1 ? poczatekBlokuDanych : wezWartoscZTablicyWymiarow(tablicaWymiarow, przesuniecieKolumny + 1);

        if (aktualnyWzglednyKoniecKolumny > 30000)
        {
            return typeList.Contains(typKolumny) ? 16 : 24;
        }
        else if (poprzedniWzglednyKoniecKolumny < 30000)
        {
            return aktualnyWzglednyKoniecKolumny - poprzedniWzglednyKoniecKolumny;
        }
        else
        {
            return (int)Math.Pow(2, 15) + aktualnyWzglednyKoniecKolumny - poprzedniWzglednyKoniecKolumny;
        }
    }
    public static String wezZawartoscHeksKolumny(String kontent, int wzglednyKoniecKolumny, int dlugoscKolumny)
    {
        return String.Concat(kontent
        .Skip((wzglednyKoniecKolumny - dlugoscKolumny + 1) * 2)
        .Take(Math.Abs(dlugoscKolumny) * 2));

    }

}

