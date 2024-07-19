using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface ICtfFlagService
{
    public CtfFlag? GetCtfFlag(int id);
    public CtfFlag? GetCtfFlagByToken(string token);
}