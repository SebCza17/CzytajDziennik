using System;
using System.Linq;
using System.Text;
using RecoveryFromLog;

public class MetadaneKolumn
{
    public String id;                   //#1
    public String kontent;              //#2
    public Int64 idTabeli;              //#4
    public int nKolumn;                 //#5
    public String tablicaWymiarow;      //#6
    public int poczatekBlokuDanych;     //#7
    public String tablicaPustychBin;    //#8
    public MetadaneKolumn(
        String Id, String Kontent, Int64 IdTabeli)
    {
        id = Id;
        kontent = Kontent;
        idTabeli = IdTabeli;

        int przesuniecie = wezPrzesuniecie(kontent);
        nKolumn = wezNKolumn(kontent, przesuniecie);

        int dlugoscTablicyPustych = wezDlugoscTablicyPustych(nKolumn);
        int nKolumnZnakowych = wezNKolumnZnakowych(kontent, przesuniecie, dlugoscTablicyPustych);
        tablicaWymiarow = wezTabliceWymiarow(kontent, przesuniecie, dlugoscTablicyPustych, nKolumnZnakowych);
        poczatekBlokuDanych = wezPoczatekBlokuDanych(przesuniecie, dlugoscTablicyPustych, nKolumnZnakowych);

        String tablicaPustych = wezTablicePustych(kontent, przesuniecie, dlugoscTablicyPustych);
        tablicaPustychBin = wezTablicePustychBIN(tablicaPustych);
    }

    public int wezPrzesuniecie(String kontent)
    {
        return Konwersja.heksNaLiczbe(String.Concat(kontent.Skip(3 * 2).Take(2 * 2)), 2);
    }

    public int wezNKolumn(String kontent, int przesuniecie)
    {
        return Konwersja.heksNaLiczbe(String.Concat(kontent.Skip((przesuniecie + 1) * 2).Take(2 * 2)), 2);
    }

    public int wezDlugoscTablicyPustych(int nKolumn)
    {
        return (int)Math.Ceiling(nKolumn / 8.0);
    }

    public String wezTablicePustych(String kontent, int przesuniecie, int dlugoscTablicyPustych)
    {
        return String.Concat(kontent.Skip((przesuniecie + 3) * 2).Take(dlugoscTablicyPustych * 2));
    }

    public int wezNKolumnZnakowych(String kontent, int przesuniecie, int dlugoscTablicyPustych)
    {
        return Konwersja.heksNaLiczbe(String.Concat(kontent.Skip((przesuniecie + 3) * 2 + dlugoscTablicyPustych * 2).Take(2 * 2)), 2);
    }

    public String wezTabliceWymiarow(String kontent, int przesuniecie, int dlugoscTablicyPustych, int nKolumnZnakowych)
    {
        return String.Concat(kontent.Skip((przesuniecie + 3 + dlugoscTablicyPustych + 2) * 2).Take(nKolumnZnakowych * 2 * 2));
    }

    public int wezPoczatekBlokuDanych(int przesuniecie, int dlugoscTablicyPustych, int nKolumnZnakowych)
    {
        return przesuniecie + dlugoscTablicyPustych + nKolumnZnakowych * 2 + 4;
    }

    public String wezTablicePustychBIN(String tablicaPustych)
    {
        return String.Concat(tablicaPustych.Aggregate(new StringBuilder(), (builder, c) =>
                builder.Append(Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0'))).ToString().Reverse());
    }

    public int wezBitTablicyPustychBin(int nBit)
    {
        return Int32.Parse(String.Concat(tablicaPustychBin.Skip(nBit - 1).Take(1)));
    } 
}
