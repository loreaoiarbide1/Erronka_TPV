
using System;
using System.Collections.Generic;
using System.IO;
using Elkartea_TPV.Models;
using Microsoft.Data.Sqlite;

namespace Elkartea_TPV;

public class DatuBasea
{
    // Aplikazioak erabiliko duen SQLite datu-basearen fitxategiaren bidea gordetzen du.
    public string FitxategiBidea { get; } = Path.Combine(
        AppContext.BaseDirectory,
        "elkartea_tpv.db");

    // Datu-basearekin konektatzeko behar den konexio-katea sortzen du.
    private string KonexioKatea => new SqliteConnectionStringBuilder
    {
        DataSource = FitxategiBidea
    }.ToString();


    // Aplikazioa abiaraztean datu-basea eta bere taulak prestatzen ditu.
    public void Hasieratu()
    {
        // Datu-basearen fitxategia gordeko den karpeta sortzen du.
        Directory.CreateDirectory(Path.GetDirectoryName(FitxategiBidea)!);

        // SQLite datu-basearekin konexio berria sortzen du.
        using var konexioa = new SqliteConnection(KonexioKatea);

        // Datu-baserako konexioa irekitzen du.
        konexioa.Open();

        // SQL komandoak exekutatzeko komando-objektua sortzen du.
        using var komandoa = konexioa.CreateCommand();

        // Aplikazioak behar dituen taulak sortzen ditu.
        komandoa.CommandText = """
            -- 1. Erabiltzaileen taula (Administratzaileak eta Erabiltzaile komunak)
            CREATE TABLE IF NOT EXISTS Erabiltzaileak (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ErabiltzaileIzena TEXT NOT NULL UNIQUE,
                Pasahitza TEXT NOT NULL,
                Mota TEXT NOT NULL CHECK(Mota IN ('Administratzailea', 'Erabiltzailea'))
            );

            -- 2. Biltegiko stockaren taula (Produktuak, prezioak eta kategoriak)
            CREATE TABLE IF NOT EXISTS Stocka (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Produktua TEXT NOT NULL UNIQUE,
                Kantitatea INTEGER NOT NULL DEFAULT 0,
                Prezioa REAL NOT NULL DEFAULT 0.0,
                Kategoria TEXT NOT NULL DEFAULT ''
            );

            -- 3. Mahaien erreserben taula (Bazkaria edo afaria bereiziz)
            CREATE TABLE IF NOT EXISTS Erreserbak (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MahaiZenbakia INTEGER NOT NULL,
                Data TEXT NOT NULL,
                BazkariaAfaria TEXT NOT NULL CHECK(BazkariaAfaria IN ('Bazkaria', 'Afaria')),
                ErabiltzaileIzena TEXT NOT NULL
            );

            -- 4. Kontsumoen/Ticketen taula
            CREATE TABLE IF NOT EXISTS Kontsumoak (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ErabiltzaileIzena TEXT NOT NULL,
                Produktua TEXT NOT NULL,
                Kantitatea INTEGER NOT NULL,
                PrezioTotala REAL NOT NULL,
                DataUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now'))
            );
            """;

        // SQL kode osoa exekutatzen du eta taulak sortzen ditu.
        komandoa.ExecuteNonQuery();

        // Stocka taulan Kategoria zutabea dagoen egiaztatzen du.
        using var egiaztatuKomandoa = konexioa.CreateCommand();
        egiaztatuKomandoa.CommandText = "PRAGMA table_info(Stocka);";

        bool kategoriaDago = false;

        using (var irakurlea = egiaztatuKomandoa.ExecuteReader())
        {
            while (irakurlea.Read())
            {
                if (irakurlea.GetString(1) == "Kategoria")
                {
                    kategoriaDago = true;
                    break;
                }
            }
        }

        // Kategoria zutabea ez badago, gehitu egiten du.
        if (!kategoriaDago)
        {
            using var gehituKomandoa = konexioa.CreateCommand();

            gehituKomandoa.CommandText = """
                ALTER TABLE Stocka
                ADD COLUMN Kategoria TEXT NOT NULL DEFAULT '';
                """;

            gehituKomandoa.ExecuteNonQuery();
        }

        // Lehenengo administratzailea eta probako erabiltzaileak sortzen ditu.
        SortuHasierakoErabiltzaileak(konexioa);

        //Produktuak sortzen ditu hasierako stockarekin.
        SortuHasierakoProduktuak(konexioa);
    }

    
    // Aplikazioaren lehen erabilerarako hasierako erabiltzaileak sortzen ditu.
    private void SortuHasierakoErabiltzaileak(SqliteConnection konexioa)
    {
        // SQL komandoa sortzen du.
        using var komandoa = konexioa.CreateCommand();

        // Erabiltzaileak bakarrik gehitzen dira lehendik existitzen ez badira.
        komandoa.CommandText = """
            INSERT OR IGNORE INTO Erabiltzaileak
            (ErabiltzaileIzena, Pasahitza, Mota)
            VALUES
            ('admin', 'admin123', 'Administratzailea'),
            ('proba', '123456', 'Erabiltzailea'),
            ('haizea', '123456', 'Erabiltzailea');
            """;

        // Hasierako erabiltzaileak datu-basean sartzen ditu.
        komandoa.ExecuteNonQuery();
    }

    // Hasierako produktu guztiak datu-basean sartzen ditu.
    private void SortuHasierakoProduktuak(SqliteConnection konexioa)
    {
        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
        INSERT OR IGNORE INTO Stocka
        (Produktua, Kantitatea, Prezioa, Kategoria)
        VALUES
        ('Coca-Cola', 20, 2.60, 'Edariak'),
        ('Garagardoa', 20, 2.00, 'Edariak'),
        ('Kas Naranja', 20, 2.50, 'Edariak'),
        ('Ura', 20, 1.50, 'Edariak'),

        ('Ebakia', 20, 1.20, 'Kafeak'),
        ('Kafe Deskafeinatua', 20, 1.30, 'Kafeak'),
        ('Kafe Hutsa', 20, 1.00, 'Kafeak'),
        ('Kafesnia', 20, 1.20, 'Kafeak'),

        ('Azeitunak', 20, 1.50, 'Janaria'),
        ('Gildak', 20, 2.00, 'Janaria'),
        ('Kroketak', 20, 2.50, 'Janaria'),
        ('Patatak', 20, 2.00, 'Janaria'),
        ('Rabak', 20, 2.50, 'Janaria');
        """;

        komandoa.ExecuteNonQuery();
    }


    // Erabiltzaile berri bat datu-basean gordetzen du.
    public void GordeErabiltzailea(string izena, string pasahitza, string mota)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
            INSERT INTO Erabiltzaileak (ErabiltzaileIzena, Pasahitza, Mota)
            VALUES ($izena, $pasahitza, $mota);
            """;

        komandoa.Parameters.AddWithValue("$izena", izena);
        komandoa.Parameters.AddWithValue("$pasahitza", pasahitza);
        komandoa.Parameters.AddWithValue("$mota", mota);

        komandoa.ExecuteNonQuery();
    }


    // Datu-basean gordetako erabiltzaile guztiak lortzen ditu.
    public List<Erabiltzaileak> LortuErabiltzaileak()
    {
        var erabiltzaileak = new List<Erabiltzaileak>();

        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText =
            "SELECT Id, ErabiltzaileIzena, Pasahitza, Mota FROM Erabiltzaileak;";

        using var irakurlea = komandoa.ExecuteReader();

        while (irakurlea.Read())
        {
            erabiltzaileak.Add(new Erabiltzaileak
            {
                Id = irakurlea.GetInt64(0),
                ErabiltzaileIzena = irakurlea.GetString(1),
                Pasahitza = irakurlea.GetString(2),
                Mota = irakurlea.GetString(3)
            });
        }

        return erabiltzaileak;
    }


    // Biltegiko produktu guztiak eta haien datuak lortzen ditu.
    public List<Stocka> LortuStocka()
    {
        var stocka = new List<Stocka>();

        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText =
            "SELECT Id, Produktua, Kantitatea, Prezioa, Kategoria FROM Stocka;";

        using var irakurlea = komandoa.ExecuteReader();

        while (irakurlea.Read())
        {
            stocka.Add(new Stocka
            {
                Id = irakurlea.GetInt64(0),
                Produktua = irakurlea.GetString(1),
                Kantitatea = irakurlea.GetInt32(2),
                Prezioa = irakurlea.GetDouble(3),
                Kategoria = irakurlea.GetString(4)
            });
        }

        return stocka;
    }


    // Datu-basean gordetako erreserba guztiak lortzen ditu.
    public List<Erreserbak> LortuErreserbak()
    {
        var erreserbak = new List<Erreserbak>();

        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText =
            "SELECT Id, MahaiZenbakia, Data, BazkariaAfaria, ErabiltzaileIzena FROM Erreserbak;";

        using var irakurlea = komandoa.ExecuteReader();

        while (irakurlea.Read())
        {
            erreserbak.Add(new Erreserbak
            {
                Id = irakurlea.GetInt64(0),
                MahaiZenbakia = irakurlea.GetInt32(1),
                Data = irakurlea.GetString(2),
                BazkariaAfaria = irakurlea.GetString(3),
                ErabiltzaileIzena = irakurlea.GetString(4)
            });
        }

        return erreserbak;
    }


    // Datu-basean gordetako kontsumo guztiak lortzen ditu.
    public List<Kontsumoak> LortuKontsumoak()
    {
        var kontsumoak = new List<Kontsumoak>();

        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText =
            "SELECT Id, ErabiltzaileIzena, Produktua, Kantitatea, PrezioTotala, DataUtc FROM Kontsumoak;";

        using var irakurlea = komandoa.ExecuteReader();

        while (irakurlea.Read())
        {
            kontsumoak.Add(new Kontsumoak
            {
                Id = irakurlea.GetInt64(0),
                ErabiltzaileIzena = irakurlea.GetString(1),
                Produktua = irakurlea.GetString(2),
                Kantitatea = irakurlea.GetInt32(3),
                Prezioa = irakurlea.GetDouble(4)
            });
        }

        return kontsumoak;
    }


    // Biltegian produktu berri bat gordetzen du.
    public void GordeStocka(
        string produktua,
        int kantitatea,
        double prezioa,
        string kategoria = "")
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
            INSERT INTO Stocka (Produktua, Kantitatea, Prezioa, Kategoria)
            VALUES ($produktua, $kantitatea, $prezioa, $kategoria);
            """;

        komandoa.Parameters.AddWithValue("$produktua", produktua);
        komandoa.Parameters.AddWithValue("$kantitatea", kantitatea);
        komandoa.Parameters.AddWithValue("$prezioa", prezioa);
        komandoa.Parameters.AddWithValue("$kategoria", kategoria);

        komandoa.ExecuteNonQuery();
    }


    // Erreserba berri bat gordetzen du, aurretik mahai hori libre dagoela egiaztatuz.
    public void GordeErreserba(
        int mahaiZenbakia,
        string data,
        string bazkariaAfaria,
        string erabiltzaileIzena)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var transakzioa = konexioa.BeginTransaction();

        try
        {
            using var egiaztatuKomandoa = konexioa.CreateCommand();
            egiaztatuKomandoa.Transaction = transakzioa;

            egiaztatuKomandoa.CommandText = """
                SELECT COUNT(*)
                FROM Erreserbak
                WHERE MahaiZenbakia = $mahaia
                AND Data = $data
                AND BazkariaAfaria = $zerbitzua;
                """;

            egiaztatuKomandoa.Parameters.AddWithValue("$mahaia", mahaiZenbakia);
            egiaztatuKomandoa.Parameters.AddWithValue("$data", data);
            egiaztatuKomandoa.Parameters.AddWithValue("$zerbitzua", bazkariaAfaria);

            long erreserbak = (long)egiaztatuKomandoa.ExecuteScalar()!;

            if (erreserbak > 0)
            {
                throw new InvalidOperationException(
                    "Mahai hau dagoeneko erreserbatuta dago data eta zerbitzu horretarako.");
            }

            using var komandoa = konexioa.CreateCommand();
            komandoa.Transaction = transakzioa;

            komandoa.CommandText = """
                INSERT INTO Erreserbak
                (MahaiZenbakia, Data, BazkariaAfaria, ErabiltzaileIzena)
                VALUES
                ($mahaia, $data, $zerbitzua, $erabiltzailea);
                """;

            komandoa.Parameters.AddWithValue("$mahaia", mahaiZenbakia);
            komandoa.Parameters.AddWithValue("$data", data);
            komandoa.Parameters.AddWithValue("$zerbitzua", bazkariaAfaria);
            komandoa.Parameters.AddWithValue("$erabiltzailea", erabiltzaileIzena);

            komandoa.ExecuteNonQuery();

            transakzioa.Commit();
        }
        catch
        {
            transakzioa.Rollback();
            throw;
        }
    }


    // Kontsumo bat datu-basean gordetzen du.
    public void GordeKontsumoa(
        string erabiltzaileIzena,
        string produktua,
        int kantitatea,
        double prezioa)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
            INSERT INTO Kontsumoak (ErabiltzaileIzena, Produktua, Kantitatea, PrezioTotala)
            VALUES ($erabiltzaileIzena, $produktua, $kantitatea, $prezioa);
            """;

        komandoa.Parameters.AddWithValue("$erabiltzaileIzena", erabiltzaileIzena);
        komandoa.Parameters.AddWithValue("$produktua", produktua);
        komandoa.Parameters.AddWithValue("$kantitatea", kantitatea);
        komandoa.Parameters.AddWithValue("$prezioa", prezioa);

        komandoa.ExecuteNonQuery();
    }


    // Id bidez erabiltzaile bat datu-basetik ezabatzen du.
    public void EzabatuErabiltzailea(long id)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "DELETE FROM Erabiltzaileak WHERE Id = $id;";

        komandoa.Parameters.AddWithValue("$id", id);

        komandoa.ExecuteNonQuery();
    }


    // Id bidez produktu bat biltegitik ezabatzen du.
    public void EzabatuStocka(long id)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "DELETE FROM Stocka WHERE Id = $id;";

        komandoa.Parameters.AddWithValue("$id", id);

        komandoa.ExecuteNonQuery();
    }


    // Id bidez erreserba bat datu-basetik ezabatzen du.

    // Erreserba bat ezabatzen du, erabiltzailea jabea dela egiaztatuz.
    public void EzabatuErreserba(long id, string erabiltzaileIzena)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
        DELETE FROM Erreserbak
        WHERE Id = $id
        AND ErabiltzaileIzena = $erabiltzailea;
        """;

        komandoa.Parameters.AddWithValue("$id", id);
        komandoa.Parameters.AddWithValue("$erabiltzailea", erabiltzaileIzena);

        int ezabatutakoErrenkadak = komandoa.ExecuteNonQuery();

        if (ezabatutakoErrenkadak == 0)
        {
            throw new InvalidOperationException(
                "Ezin duzu beste erabiltzaile baten erreserba ezabatu.");
        }
    }



    // Erabiltzailearen izena eta pasahitza egiaztatzen ditu saioa hasteko.
    public Erabiltzaileak? EgiaztatuLogin(string izena, string pasahitza)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
            SELECT Id, ErabiltzaileIzena, Pasahitza, Mota
            FROM Erabiltzaileak
            WHERE ErabiltzaileIzena = $izena
            AND Pasahitza = $pasahitza;
            """;

        komandoa.Parameters.AddWithValue("$izena", izena);
        komandoa.Parameters.AddWithValue("$pasahitza", pasahitza);

        using var irakurlea = komandoa.ExecuteReader();

        if (irakurlea.Read())
        {
            return new Erabiltzaileak
            {
                Id = irakurlea.GetInt64(0),
                ErabiltzaileIzena = irakurlea.GetString(1),
                Pasahitza = irakurlea.GetString(2),
                Mota = irakurlea.GetString(3)
            };
        }

        return null;
    }


    // Dagoen erabiltzaile baten datuak eguneratzen ditu.
    public void EguneratuErabiltzailea(
        long id,
        string izena,
        string pasahitza,
        string mota)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
            UPDATE Erabiltzaileak
            SET ErabiltzaileIzena = $izena,
                Pasahitza = $pasahitza,
                Mota = $mota
            WHERE Id = $id;
            """;

        komandoa.Parameters.AddWithValue("$id", id);
        komandoa.Parameters.AddWithValue("$izena", izena);
        komandoa.Parameters.AddWithValue("$pasahitza", pasahitza);
        komandoa.Parameters.AddWithValue("$mota", mota);

        komandoa.ExecuteNonQuery();
    }


    // Biltegian dagoen produktu baten datuak eguneratzen ditu.
    public void EguneratuStocka(
        long id,
        string produktua,
        int kantitatea,
        double prezioa,
        string kategoria = "")
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
            UPDATE Stocka
            SET Produktua = $produktua,
                Kantitatea = $kantitatea,
                Prezioa = $prezioa,
                Kategoria = $kategoria
            WHERE Id = $id;
            """;

        komandoa.Parameters.AddWithValue("$id", id);
        komandoa.Parameters.AddWithValue("$produktua", produktua);
        komandoa.Parameters.AddWithValue("$kantitatea", kantitatea);
        komandoa.Parameters.AddWithValue("$prezioa", prezioa);
        komandoa.Parameters.AddWithValue("$kategoria", kategoria);

        komandoa.ExecuteNonQuery();
    }


    // Kontsumoa datu-basean gordetzen du eta biltegiko stocka eguneratzen du.
    public void GordeKontsumoaEtaEguneratuStocka(
        string erabiltzaileIzena,
        IEnumerable<KontsumoLerroa> kontsumoak)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var transakzioa = konexioa.BeginTransaction();

        try
        {
            foreach (KontsumoLerroa lerroa in kontsumoak)
            {
                if (lerroa.Kantitatea <= 0)
                {
                    throw new InvalidOperationException(
                        "Produktu baten kantitatea ez da zuzena.");
                }

                using var stockKomandoa = konexioa.CreateCommand();
                stockKomandoa.Transaction = transakzioa;

                stockKomandoa.CommandText = """
                    UPDATE Stocka
                    SET Kantitatea = Kantitatea - $kantitatea
                    WHERE Produktua = $produktua
                    AND Kantitatea >= $kantitatea;
                    """;

                stockKomandoa.Parameters.AddWithValue(
                    "$produktua", lerroa.Produktua);

                stockKomandoa.Parameters.AddWithValue(
                    "$kantitatea", lerroa.Kantitatea);

                int eguneratutakoErrenkadak =
                    stockKomandoa.ExecuteNonQuery();

                if (eguneratutakoErrenkadak != 1)
                {
                    throw new InvalidOperationException(
                        "Ez dago nahikoa stock produktu honentzat: " +
                        lerroa.Produktua);
                }

                using var kontsumoKomandoa = konexioa.CreateCommand();
                kontsumoKomandoa.Transaction = transakzioa;

                kontsumoKomandoa.CommandText = """
                    INSERT INTO Kontsumoak
                    (ErabiltzaileIzena, Produktua, Kantitatea, PrezioTotala)
                    VALUES
                    ($erabiltzaileIzena, $produktua, $kantitatea, $prezioa);
                    """;

                kontsumoKomandoa.Parameters.AddWithValue(
                    "$erabiltzaileIzena", erabiltzaileIzena);

                kontsumoKomandoa.Parameters.AddWithValue(
                    "$produktua", lerroa.Produktua);

                kontsumoKomandoa.Parameters.AddWithValue(
                    "$kantitatea", lerroa.Kantitatea);

                kontsumoKomandoa.Parameters.AddWithValue(
                    "$prezioa", lerroa.Guztira);

                kontsumoKomandoa.ExecuteNonQuery();
            }

            transakzioa.Commit();
        }
        catch
        {
            transakzioa.Rollback();
            throw;
        }
    }
}
