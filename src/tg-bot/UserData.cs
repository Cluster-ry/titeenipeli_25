namespace Titeenipeli_bot;

public class UserData(long id)
{
    public readonly long Id = id;
    public bool TosAccepted = false; // I know its more of a privacy notice than tos but tos is easier to write :D
    public bool UserCreated = false;
    public bool ChoosingGuild = false;
    public guildEnum? GuildChosen;
    public bool GuildSelected = false;
}