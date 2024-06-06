using Titeenipeli.Connectors;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void CreateAndFillCtfTable()
    {
        DbConnector connector = new DbConnector();
        string sql = """
                     CREATE TABLE IF NOT EXISTS "CtfFlags" (
                         "Id"     SERIAL PRIMARY KEY,
                         "Flag"   VARCHAR NOT NULL
                     );
                     """;
        
        connector.ExecuteCommand(sql);

        sql = """
              INSERT INTO "CtfFlags" ("Flag")
              VALUES ('#TEST_FLAG');
              """;
        
        connector.ExecuteCommand(sql);
    }

    public static void ClearDatabase()
    {
        DbConnector connector = new DbConnector();
        string sql = """
                     DROP TABLE IF EXISTS "CtfFlags";
                     """;

        connector.ExecuteCommand(sql);

    }
}