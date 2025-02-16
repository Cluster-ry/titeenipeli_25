enum Guild {
    Nobody,
    Tietokilta,
    Algo,
    Cluster,
    OulunTietoteekkarit,
    TietoTeekkarikilta,
    Digit,
    Datateknologerna,
    Sosa,
    Date,
    Tutti,
}

export const shortGuildName = (guild: Guild) => {
    switch (guild) {
        case Guild.Tietokilta:
            return "TiK";
        case Guild.Algo:
            return "Algo";
        case Guild.Cluster:
            return "Cluster";
        case Guild.OulunTietoteekkarit:
            return "OTiT";
        case Guild.TietoTeekkarikilta:
            return "TiTe";
        case Guild.Digit:
            return "Digit";
        case Guild.Datateknologerna:
            return "Datatekno";
        case Guild.Sosa:
            return "Sosa";
        case Guild.Date:
            return "Date";
        case Guild.Tutti:
            return "Tutti";
        default:
            return "Nobody";
    }
};

export default Guild;
