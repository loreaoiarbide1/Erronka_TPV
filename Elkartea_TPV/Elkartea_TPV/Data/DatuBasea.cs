using System.IO;
using Elkartea_TPV.Models;

// SQLite-rekin lan egiteko behar diren klaseak inportatzen ditu
using Microsoft.Data.Sqlite;
// Fitxategi honetako kodea Elkartea_TPVri dagokion espazioan dago.
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

        // Eskaeran eskatutako taulak sortzen ditugu: Erabiltzaileak, Stocka, Erreserbak eta Kontsumoak/Ticketak.
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
        GeyiAdministratzaileLehenetsia(konexioa);
    }

    private void GeyiAdministratzaileLehenetsia(SqliteConnection konexioa)
    {
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = """
			INSERT OR IGNORE INTO Erabiltzaileak (ErabiltzaileIzena, Pasahitza, Mota)
			VALUES ('admin', 'admin123', 'Administratzailea');
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

    // Behar dituzun gainerako metodoak (stocka aldatu, erreserbak kudeatu, etab.) 
    // modu berean gehitu ditzakezu hemen.

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

    public void GordeErreserba(int mahaiZenbakia, string data, string bazkariaAfaria, string erabiltzaileIzena)
    {
        using var konexioa = new SqliteConnection(KonexioKatea);
        konexioa.Open();
        using var komandoa = konexioa.CreateCommand();
        komandoa.CommandText = """
            INSERT INTO Erreserbak (MahaiZenbakia, Data, BazkariaAfaria, ErabiltzaileIzena)
            VALUES ($mahaiZenbakia, $data, $bazkariaAfaria, $erabiltzaileIzena);
            """;
        komandoa.Parameters.AddWithValue("$mahaiZenbakia", mahaiZenbakia);
        komandoa.Parameters.AddWithValue("$data", data);
        komandoa.Parameters.AddWithValue("$bazkariaAfaria", bazkariaAfaria);
        komandoa.Parameters.AddWithValue("$erabiltzaileIzena", erabiltzaileIzena);
        komandoa.ExecuteNonQuery();
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


}