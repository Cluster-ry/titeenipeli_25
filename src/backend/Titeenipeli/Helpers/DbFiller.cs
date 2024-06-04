using Titeenipeli.Connectors;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void CreateAndFillCtfTable()
    {
        DbConnector connector = new DbConnector();
        string sql = """
                     CREATE TABLE IF NOT EXISTS "CtfFlags" (
                         id     SERIAL PRIMARY KEY,
                         flag   VARCHAR NOT NULL
                     );
                     """;
        
        connector.ExecuteCommand(sql);

        sql = """
              INSERT INTO "CtfFlags" (flag)
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