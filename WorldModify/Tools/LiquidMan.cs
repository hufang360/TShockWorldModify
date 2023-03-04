using Terraria;
using Terraria.ID;

namespace WorldModify.Tools;

public class LiquidMan
{
    /// <summary>
    /// 获得液体id
    /// </summary>
    /// <returns>匹配失败则返回-1</returns>
    public static short GetLiquidID(string numOrName)
    {
        //LiquidID.Water
        return numOrName switch
        {
            "水" or "water" => 0,
            "岩浆" or "lava" => 1,
            "蜂蜜" or "honey" => 2,
            "微光" or "shimmer" => 3,
            _ => -1,
        };
    }

    /// <summary>
    /// 获得液体名称
    /// </summary>
    /// <returns>匹配失败则""</returns>
    public static string GetName(short id)
    {
        return id switch
        {
            0 => "水",
            1 => "岩浆",
            2 => "蜂蜜",
            3 => "微光",
            _ => "",
        };
    }


    /// <summary>
    /// 替换液体
    /// </summary>
    public static int ReplaceLiquid(int x, int y, int before, int after, bool needUpdate = false)
    {
        ITile tile = Main.tile[x, y];
        if (tile != null && tile.liquidType() == (byte)before)
        {
            switch (after)
            {
                default:
                    tile.lava(false);
                    tile.honey(false);
                    tile.shimmer(false);
                    break;

                case 1: tile.lava(true); break;
                case 2: tile.honey(true); break;
                case 3: tile.shimmer(true); break;
            }
            if (needUpdate)
            {
                WorldGen.SquareTileFrame(x, y);
                NetMessage.SendTileSquare(-1, x, y);
            }
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// 将水换成薄冰
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>0表示未替换，1表示已替换</returns>
    public static int IceAgeTile(int x, int y, bool needUpdate = false)
    {
        ITile tile = Main.tile[x, y];

        if (tile.liquid > 0)
        {
            switch (tile.liquidType())
            {
                // 水
                // Tile.Liquid_Water
                case 0:
                    tile.liquid = 0;
                    if (needUpdate) WorldGen.SquareTileFrame(x, y);

                    if (!tile.active())
                    {
                        tile.type = TileID.BreakableIce;
                        tile.active(true);
                        tile.slope(0);
                        tile.halfBrick(false);
                    }

                    if (needUpdate) NetMessage.SendTileSquare(-1, x, y);
                    return 1;


                // 岩浆
                case 1: break;
                // 蜂蜜
                case 2: break;
            }
        }
        return 0;
    }

    /// <summary>
    /// 将薄冰替换成水
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>0表示未替换，1表示已替换</returns>
    public static int IceMeltTile(int x, int y, bool needUpdate = false)
    {
        ITile tile = Main.tile[x, y];
        if (tile.type != TileID.BreakableIce) return 0;

        tile.active(active: false);
        tile.liquid = byte.MaxValue;
        if (needUpdate)
        {
            WorldGen.SquareTileFrame(x, y);
            NetMessage.SendTileSquare(-1, x, y);
        }
        return 1;
    }

}
