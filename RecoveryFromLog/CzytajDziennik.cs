using System;
using System.Collections;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using RecoveryFromLog;

public class CzytajDziennik
{
    [SqlFunction(SystemDataAccess = SystemDataAccessKind.Read, FillRowMethodName = "pobierzWpisyDziennika")]
    private static IEnumerable UsunieteEtap1(String nazwaTabeli)
    {
        ArrayList zbiorWyjsciowy = new ArrayList();

        SqlConnection polaczenie = new SqlConnection("context connection=true");
        polaczenie.Open();

        SqlCommand zapytanieDoLog = new SqlCommand(
            "SELECT " +
                "[Current LSN]," +
                "CONVERT(NVARCHAR(MAX), [RowLog Contents 0], 1), " +
                "AllocUnitId " +
            "FROM " +
                "fn_dblog(NULL, NULL) " +
            "WHERE " +
                "Operation IN('LOP_DELETE_ROWS') " +
                    "AND AllocUnitName like '%" + nazwaTabeli + "%'",
             polaczenie);

        SqlDataReader rezultat = zapytanieDoLog.ExecuteReader();

        while (rezultat.Read())
        {
            String id = rezultat.GetString(0);
            String kontent = rezultat.GetString(1);
            Int64 idTabeli = rezultat.GetInt64(2);

            zbiorWyjsciowy.Add(
                new MetadaneKolumn(id, kontent, idTabeli)
            );
        }

        rezultat.Close();
        polaczenie.Close();

        return zbiorWyjsciowy;
    }

    private static void pobierzWpisyDziennika(object wynikZapytania, 
        out String id,
        out String kontent,
        out Int64 idTabeli,
        out int nKolumn,
        out String tablicaWymiarow,
        out int poczatekBlokuDanych,
        out String tablicaPustychBin)
    {
        MetadaneKolumn metadaneKolumny = (MetadaneKolumn)wynikZapytania;

        id = metadaneKolumny.id;
        kontent = metadaneKolumny.kontent;
        idTabeli = metadaneKolumny.idTabeli;
        nKolumn = metadaneKolumny.nKolumn;
        tablicaWymiarow = metadaneKolumny.tablicaWymiarow;
        poczatekBlokuDanych = metadaneKolumny.poczatekBlokuDanych;
        tablicaPustychBin = metadaneKolumny.tablicaPustychBin;

    }

    [SqlFunction(SystemDataAccess = SystemDataAccessKind.Read, FillRowMethodName = "pobierzSchematTabeli")]
    private static IEnumerable UsunieteEtap2(String nazwaTabeli)
    {
        ArrayList ZbiorMetadanychKolumn = (ArrayList)UsunieteEtap1(nazwaTabeli);

        ArrayList zbiorWyjsciowy = new ArrayList();

        SqlConnection conn = new SqlConnection("context connection=true");
        conn.Open();
        foreach (MetadaneKolumn MetadaneKolumn in ZbiorMetadanychKolumn)
        {

            SqlCommand zap = new SqlCommand(
                "SELECT " +
                    "ISNULL(NAME, 'USUNIETA'), " +
                    "CAST(s.leaf_null_bit AS INT), " +
                    "CAST(leaf_offset AS INT), " +
                    "CAST(s.system_type_id AS INT), " +
                    "ISNULL(CAST(c.length AS INT), max_length), " +
                    "leaf_offset + 1" +
                "FROM " +
                    "sys.allocation_units a " +
                        "JOIN sys.partitions p ON(a.type IN(1, 3) " +
                            "AND p.hobt_id = a.container_id) " +
                            "OR(a.type = 2 AND p.partition_id = a.container_id) " +
                        "JOIN sys.system_internals_partition_columns s ON s.partition_id = p.partition_id " +
                        "LEFT OUTER JOIN syscolumns c ON c.id = p.object_id AND c.colid = s.partition_column_id " +
                "WHERE " +
                    "a.[Allocation_Unit_Id] = " + MetadaneKolumn.idTabeli, conn);

            SqlDataReader zapReader = zap.ExecuteReader();

            while (zapReader.Read())
            {
                String zawartoscHeksKolumny = "";

                String columnName = zapReader.GetString(0);
                int idKolumny = zapReader.GetInt32(1);
                int przesuniecieKolumny = zapReader.GetInt32(2);
                int typKolumny = zapReader.GetInt32(3);
                int dlugosc = zapReader.GetInt32(4);
                int pozycja = zapReader.GetInt32(5);
                
                int columnIsNull = MetadaneKolumn.wezBitTablicyPustychBin(idKolumny);
                if (columnIsNull == 0 && idKolumny <= MetadaneKolumn.nKolumn)
                {
                    if (przesuniecieKolumny < 1)
                    {
                        ArgumentyKolumn ArgumentyKolumn = new ArgumentyKolumn(MetadaneKolumn, przesuniecieKolumny, typKolumny);
                        zawartoscHeksKolumny = ArgumentyKolumn.zawartoscHeksKolumny;
                    }
                    else
                    {
                        zawartoscHeksKolumny = String.Concat(((String)MetadaneKolumn.kontent).Skip(pozycja * 2).Take(dlugosc * 2));
                    }
                }

                zbiorWyjsciowy.Add(new DaneKolumn(
                    MetadaneKolumn.id,
                    columnName,
                    typKolumny,
                    zawartoscHeksKolumny
                    ));
            }
            zapReader.Close();
        }
        conn.Close();

        return zbiorWyjsciowy;
    }

    private static void pobierzSchematTabeli(object zapResultObj,
        out String nazwaKolumny,
        out int typKolumny,
        out String zawartoscHeksKolumny
    )
    {
        DaneKolumn zapResult = (DaneKolumn)zapResultObj;

        nazwaKolumny = zapResult.nazwaKolumny;
        typKolumny = zapResult.typKolumny;
        zawartoscHeksKolumny = zapResult.zawartoscHeksKolumny;
    }

    [SqlFunction(SystemDataAccess = SystemDataAccessKind.Read, FillRowMethodName = "dekoduj")]
    public static IEnumerable Usuniete (String nazwaTabeli)
    {
        ArrayList ZbiorDanychKolumn = (ArrayList)UsunieteEtap2(nazwaTabeli);

        ArrayList zbiorWyjsciowy = new ArrayList();

        foreach (DaneKolumn DaneKolumn in ZbiorDanychKolumn)
        {
            String zawartoscHeksKolumny = (String)DaneKolumn.zawartoscHeksKolumny;

            String zawartoscKolumny = Konwersja.heksNaTyp(zawartoscHeksKolumny, DaneKolumn.typKolumny);

            zbiorWyjsciowy.Add(new ZawartoscKolumn(
                DaneKolumn.id,
                DaneKolumn.nazwaKolumny,
                zawartoscKolumny,
                zawartoscHeksKolumny
                ));
        }
        return zbiorWyjsciowy;
    }
  
    private static void dekoduj(object wynikZapytania,
        out String id,
        out String nazwa,
        out String wartosc,
        out String debuge
    )
    {
        ZawartoscKolumn zawartoscKolumny = (ZawartoscKolumn)wynikZapytania;
        id = zawartoscKolumny.id;
        nazwa = zawartoscKolumny.nazwa;
        wartosc = zawartoscKolumny.wartosc;
        debuge = zawartoscKolumny.debuge;
    }
}