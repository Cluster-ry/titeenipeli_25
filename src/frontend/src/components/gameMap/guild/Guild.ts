export enum Guild {
  Tietokilta = 0,
  Algo = 1,
  Cluster = 2,
  OulunTietoteekkarit = 3,
  TietoTeekkarikilta = 4,
  Digit = 5,
  Datateknologerna = 6,
  Sosa = 7,
}

export function guildColor(guild: Guild | undefined): number {
  switch (guild) {
    case Guild.Tietokilta:
      return 0xd50000;
    case Guild.Algo:
      return 0xc51162;
    case Guild.Cluster:
      return 0xaa00ff;
    case Guild.OulunTietoteekkarit:
      return 0x6200ea;
    case Guild.TietoTeekkarikilta:
      return 0x304ffe;
    case Guild.Digit:
      return 0x2962ff;
    case Guild.Datateknologerna:
      return 0x0091ea;
    case Guild.Sosa:
      return 0x00b8d4;
    default:
      return 0x000000;
  }
}
