using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Models;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void Initialize(ApiDbContext dbContext)
    {
        if (dbContext.Flags.Any())
        {
            return;
        }

        dbContext.Flags.Add(new CtfFlag
        {
            Flag = "#TEST_FLAG",
            Id = 0
        });

        dbContext.SaveChanges();
    }

    public static void Clear(ApiDbContext dbContext)
    {
        dbContext.Flags.ExecuteDelete();
    }
}