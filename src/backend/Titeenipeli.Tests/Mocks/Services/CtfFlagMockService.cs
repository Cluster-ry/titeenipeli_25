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

    public CtfFlag GetById(int id)
    {
        return _ctfFlags.FirstOrDefault(flag => flag.Id == id);
    }

    public List<CtfFlag> GetAll()
    {
        return _ctfFlags;
    }

    public void Add(CtfFlag ctfFlag)
    {
        _ctfFlags.Add(ctfFlag);
    }

    public CtfFlag GetByToken(string token)
    {
        return _ctfFlags.FirstOrDefault(flag => flag.Token == token);
    }
}