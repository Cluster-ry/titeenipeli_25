using System.Collections.Generic;
using System.Linq;
using Titeenipeli.Schema;
using Titeenipeli.Services.Interfaces;

namespace Titeenipeli.Tests.Mocks.Services;

public class CtfFlagMockService : ICtfFlagService
{
    private readonly List<CtfFlag> _ctfFlags;

    public CtfFlagMockService(List<CtfFlag> ctfFlags)
    {
        _ctfFlags = ctfFlags;
    }

    public CtfFlag GetCtfFlag(int id)
    {
        return _ctfFlags.FirstOrDefault(flag => flag.Id == id);
    }
    
    public CtfFlag GetCtfFlagByToken(string token)
    {
        return _ctfFlags.FirstOrDefault(flag => flag.Token == token);
    }
}