using System;
using System.Collections.Generic;
using System.Linq;
using RecoveryFromLog;

public class DaneKolumn 
{
    public String id;
    public String nazwaKolumny;
    public int typKolumny;
    public String zawartoscHeksKolumny;
    public DaneKolumn(String Id, String NazwaKolumny, int TypKolumny, String ZawartoscHeksKolumny)
    {
        id = Id;
        nazwaKolumny = NazwaKolumny;
        typKolumny = TypKolumny;
        zawartoscHeksKolumny = ZawartoscHeksKolumny;
    }
}
