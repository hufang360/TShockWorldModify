using Terraria;

namespace WorldModify;

public class TileSaverData
{
    public ushort type;
    public ushort wall;
    public byte liquid;
    public ushort sTileHeader;
    public byte bTileHeader;
    public byte bTileHeader2;
    public byte bTileHeader3;
    public short frameX;
    public short frameY;

    public void Create(ITile tile)
    {
        type = tile.type;
        wall = tile.wall;
        liquid = tile.liquid;
        sTileHeader = tile.sTileHeader;
        bTileHeader = tile.bTileHeader;
        bTileHeader2 = tile.bTileHeader2;
        bTileHeader3 = tile.bTileHeader3;
        frameX = tile.frameX;
        frameY = tile.frameY;
    }
}