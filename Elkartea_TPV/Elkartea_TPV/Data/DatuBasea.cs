using System.Collections.Generic;
using System.IO;
using Elkartea_TPV.Models;
using Microsoft.Data.Sqlite;
namespace Elkartea_TPV;

public class DatuBasea
{
    // Bide absolutua erabiltzen dugu, fitxategia aplikazioaren karpetan gordetzeko.
    public string FitxategiBidea { get; } = Path.Combine(
        AppContext.BaseDirectory,
        "elkartea_tpv.db");

    private string KonexioKatea => new SqliteConnectionStringBuilder
    {
        DataSource = FitxategiBidea
    }.ToString();

    public void Hasieratu()
    {
        // Karpeta sortzen dugu, oraindik existitzen ez bada.
        Directory.CreateDirectory(Path.GetDirectoryName(FitxategiBidea)!);

        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();
     
        komandoa.CommandText = """
			-- 1. Erabiltzaileen taula (Administratzaileak eta Erabiltzaile komunak)
			CREATE TABLE IF NOT EXISTS Erabiltzaileak (
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				ErabiltzaileIzena TEXT NOT NULL UNIQUE,
				Pasahitza TEXT NOT NULL,
				Mota TEXT NOT NULL CHECK(Mota IN ('Administratzailea', 'Erabiltzailea'))
			);

			-- 2. Biltegiko stockaren taula (Produktuak eta prezioak)
			CREATE TABLE IF NOT EXISTS Stocka (
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				Produktua TEXT NOT NULL UNIQUE,
				Kantitatea INTEGER NOT NULL DEFAULT 0,
				Prezioa REAL NOT NULL DEFAULT 0.0
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

        komandoa.ExecuteNonQuery();

        // Hasierako administratzaile bat sortu adibide gisa (existitzen ez bada)
        SortuHasierakoErabiltzaileak(konexioa);
    }

    private void SortuHasierakoErabiltzaileak(SqliteConnection konexioa)
    {
        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
        INSERT OR IGNORE INTO Erabiltzaileak
        (ErabiltzaileIzena, Pasahitza, Mota)
        VALUES
        ('admin', 'admin123', 'Administratzailea'),
        ('proba', '123456', 'Erabiltzailea');
        """;

        komandoa.ExecuteNonQuery();
    }

    // Adibidez, erabiltzaile berri bat gehitzeko metodoa
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


    public List<Erabiltzaileak> LortuErabiltzaileak()
    {
        var erabiltzaileak = new List<Erabiltzaileak>();
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "SELECT Id, ErabiltzaileIzena, Pasahitza, Mota FROM Erabiltzaileak;";
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


    public List<Stocka> LortuStocka()
    {
        var stocka = new List<Stocka>();
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "SELECT Id, Produktua, Kantitatea, Prezioa FROM Stocka;";
        using var irakurlea = komandoa.ExecuteReader();
        while (irakurlea.Read())
        {
            stocka.Add(new Stocka
            {
                Id = irakurlea.GetInt64(0),
                Produktua = irakurlea.GetString(1),
                Kantitatea = irakurlea.GetInt32(2),
                Prezioa = irakurlea.GetDouble(3)
            });
        }
        return stocka;
    }

    public List<Erreserbak> LortuErreserbak()
    {
        var erreserbak = new List<Erreserbak>();
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "SELECT Id, MahaiZenbakia, Data, BazkariaAfaria, ErabiltzaileIzena FROM Erreserbak;";
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

    public List<Kontsumoak> LortuKontsumoak()
    {
        var kontsumoak = new List<Kontsumoak>();
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "SELECT Id, ErabiltzaileIzena, Produktua, Kantitatea, PrezioTotala, DataUtc FROM Kontsumoak;";
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

    public void GordeStocka(string produktua, int kantitatea, double prezioa)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = """
            INSERT INTO Stocka (Produktua, Kantitatea, Prezioa)
            VALUES ($produktua, $kantitatea, $prezioa);
            """;
        komandoa.Parameters.AddWithValue("$produktua", produktua);
        komandoa.Parameters.AddWithValue("$kantitatea", kantitatea);
        komandoa.Parameters.AddWithValue("$prezioa", prezioa);
        komandoa.ExecuteNonQuery();

    }


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
            // Mahaia data eta zerbitzu horretan libre dagoen egiaztatzen du.
            using var egiaztatuKomandoa = konexioa.CreateCommand();

            egiaztatuKomandoa.Transaction = transakzioa;

            egiaztatuKomandoa.CommandText = """
            SELECT COUNT(*)
            FROM Erreserbak
            WHERE MahaiZenbakia = $mahaia
            AND Data = $data
            AND BazkariaAfaria = $zerbitzua;
            """;

            egiaztatuKomandoa.Parameters.AddWithValue(
                "$mahaia", mahaiZenbakia);

            egiaztatuKomandoa.Parameters.AddWithValue(
                "$data", data);

            egiaztatuKomandoa.Parameters.AddWithValue(
                "$zerbitzua", bazkariaAfaria);

            long erreserbak = (long)egiaztatuKomandoa.ExecuteScalar()!;

            if (erreserbak > 0)
            {
                throw new InvalidOperationException(
                    "Mahai hau dagoeneko erreserbatuta dago data eta zerbitzu horretarako.");
            }

            // Erreserba datu-basean gordetzen du.
            using var komandoa = konexioa.CreateCommand();

            komandoa.Transaction = transakzioa;

            komandoa.CommandText = """
            INSERT INTO Erreserbak
            (MahaiZenbakia, Data, BazkariaAfaria, ErabiltzaileIzena)
            VALUES
            ($mahaia, $data, $zerbitzua, $erabiltzailea);
            """;

            komandoa.Parameters.AddWithValue(
                "$mahaia", mahaiZenbakia);

            komandoa.Parameters.AddWithValue(
                "$data", data);

            komandoa.Parameters.AddWithValue(
                "$zerbitzua", bazkariaAfaria);

            komandoa.Parameters.AddWithValue(
                "$erabiltzailea", erabiltzaileIzena);

            komandoa.ExecuteNonQuery();

            transakzioa.Commit();
        }
        catch
        {
            transakzioa.Rollback();
            throw;
        }
    }


    public void GordeKontsumoa(string erabiltzaileIzena, string produktua, int kantitatea, double prezioa)
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

    public void EzabatuErabiltzailea(long id)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "DELETE FROM Erabiltzaileak WHERE Id = $id;";
        komandoa.Parameters.AddWithValue("$id", id);
        komandoa.ExecuteNonQuery();
    }

    public void EzabatuStocka(long id)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "DELETE FROM Stocka WHERE Id = $id;";
        komandoa.Parameters.AddWithValue("$id", id);
        komandoa.ExecuteNonQuery();
    }

    public void EzabatuErreserba(long id)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = "DELETE FROM Erreserbak WHERE Id = $id;";
        komandoa.Parameters.AddWithValue("$id", id);
        komandoa.ExecuteNonQuery();
    }


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

    public void EguneratuErabiltzailea(long id, string izena, string pasahitza, string mota)
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


    public void EguneratuStocka(long id, string produktua, int kantitatea, double prezioa)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        using var komandoa = konexioa.CreateCommand();

        komandoa.CommandText = """
        UPDATE Stocka
        SET Produktua = $produktua,
            Kantitatea = $kantitatea,
            Prezioa = $prezioa
        WHERE Id = $id;
        """;

        komandoa.Parameters.AddWithValue("$id", id);
        komandoa.Parameters.AddWithValue("$produktua", produktua);
        komandoa.Parameters.AddWithValue("$kantitatea", kantitatea);
        komandoa.Parameters.AddWithValue("$prezioa", prezioa);

        komandoa.ExecuteNonQuery();
    }


    // Kontsumoa gorde eta stockeko kantitateak eguneratzen ditu.
    public void GordeKontsumoaEtaEguneratuStocka(
        string erabiltzaileIzena,
        IEnumerable<KontsumoLerroa> kontsumoak)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();

        // Eragiketa guztiak batera egiten dira.
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

                // Stockean nahikoa produktu dagoen egiaztatzen du.
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

                // Kontsumoa datu-basean gordetzen du.
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

            // Dena ondo badago, aldaketak gordetzen dira.
            transakzioa.Commit();
        }
        catch
        {
            // Errore bat badago, aldaketa guztiak desegiten dira.
            transakzioa.Rollback();

            throw;
        }
    }
}