using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify;

public class TileScaner
{
    public static void CopyUnder(TSPlayer op)
    {
        Rectangle rect = SelectionTool.GetSelection(op.Index);

        TileSaver tc = new()
        {
            width = rect.Width,
            height = rect.Height
        };

        for (int rx = rect.Left; rx < rect.Right; rx++)
        {
            for (int ry = rect.Top; ry < rect.Bottom; ry++)
            {
                ITile tile = Main.tile[rx, ry];
                var tsd = new TileSaverData();
                tsd.Create(tile);
                tc.tiles.Add(tsd);
            }
        }

        string tileName = string.Format("{0:MMddHHmm}", DateTime.Now);
        tc.Save(tileName);
        op.SendSuccessMessage($"已复制区域 {tileName}！{Utils.RectangleToString(rect)},");
    }

    public static void Paste(TSPlayer op, string name)
    {
        Rectangle selection = SelectionTool.GetSelection2(op.Index);
        int posX = selection.X;
        int posY = selection.Y;
        if (selection == Rectangle.Empty)
        {
            posX = op.TileX;
            posY = op.TileY + 3;
        }

        TileSaver saver = TileSaver.Load(name);
        if (saver.tiles.Count == 0)
        {
            op.SendErrorMessage($"找不到 {name}");
            return;
        }

        Rectangle rect = new(posX - saver.width / 2 - 1, posY, saver.width, saver.height);

        int i = 0;
        int ry;
        Point tempPos = new();
        for (int rx = rect.Left; rx < rect.Right; rx++)
        {
            for (ry = rect.Top; ry < rect.Bottom; ry++)
            {
                ITile tile = Main.tile[rx, ry];
                if (i >= saver.tiles.Count)
                {
                    break;
                }
                tile.ClearEverything();

                TileSaverData rawTile = saver.tiles[i];
                i++;


                if (rawTile.type == TileID.Toilets)
                {
                    if (tempPos == Point.Zero)
                    {
                        tempPos.X = rx;
                        tempPos.Y = ry;
                    }
                }

                tile.type = rawTile.type;
                tile.wall = rawTile.wall;
                tile.liquid = rawTile.liquid;
                tile.sTileHeader = rawTile.sTileHeader;
                tile.bTileHeader = rawTile.bTileHeader;
                tile.bTileHeader2 = rawTile.bTileHeader2;
                tile.bTileHeader3 = rawTile.bTileHeader3;
                tile.frameX = rawTile.frameX;
                tile.frameY = rawTile.frameY;

                //NetMessage.SendTileSquare(-1, rx, ry, 2);
            }
        }
        Netplay.ResetSections();
        //TileHelper.InformPlayers();
        op.SendSuccessMessage($"已粘贴到选区的起始位置！宽x高={rect.Width}x{rect.Height}");
    }


    public static void Dispose()
    {
    }

}
