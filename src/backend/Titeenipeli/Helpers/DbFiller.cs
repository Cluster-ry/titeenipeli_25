using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Models;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void Initialize(ApiDbContext context, bool clearDatabase = false)
    {
        if (context.Flags.Any() && !clearDatabase)
        {
            return;
        }

        if (clearDatabase)
        {
            context.Flags.ExecuteDelete();
        }

        context.Flags.Add(new CtfFlag
        {
            Flag = "#TEST_FLAG",
            Id = 0
        });

        context.SaveChanges();
    }
}