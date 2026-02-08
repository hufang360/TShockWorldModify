using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 信息查询
    /// </summary>
    class InfoTool
    {
        /// <summary>
        /// 脚下一格的信息
        /// </summary>
        /// <param name="op"></param>
        public static void UnderInfo(TSPlayer op)
        {
            var rect = SelectionTool.GetSelection2(op.Index);
            int cx = rect.X;
            int cy = rect.Y;
            string title = "选区起始位置：";
            if (rect == Rectangle.Empty)
            {
                cx = op.TileX;
                cy = op.TileY + 3;
                title = "脚下1格：";
            }
            ITile tile = Main.tile[cx, cy];

            string tileDesc = "空";
            if (tile.active() && tile.type >= 0)
            {
                var tileProp = TileHelper.GetTileByID(tile.type);
                if (tileProp != null)
                    tileDesc = tileProp.Desc;
            }
            List<string> li = new() {
                $"{title}",
                $"坐标：{cx},{cy} | {Utils.GetLocationDesc(cx, cy)} | {op.TPlayer.position.X},{op.TPlayer.position.Y})",
                $"图格：{tileDesc}",
                $"frameXY：{tile.frameX},{tile.frameY}",
                $"blockType：{tile.blockType()}",
                $"斜面：{tile.slope()}",
                $"颜色：{tile.color()}",
            };

            List<string> li2 = new();

            // 墙
            if (tile.wall > 0)
            {
                var wp = TileHelper.GetWallByID(tile.wall);
                li2.Add($"{(wp != null ? wp.Desc : tile.wall)}");
            }
            // 液体
            if (tile.liquid > 0)
            {
                if (tile.lava()) li2.Add("岩浆");
                else if (tile.honey()) li2.Add("蜂蜜");
                else if (tile.shimmer()) li2.Add("微光");
                else if (tile.liquidType() == 0) li2.Add("水");
            }

            // 电线
            if (tile.wire()) li2.Add("红电线");
            if (tile.wire2()) li2.Add("蓝电线");
            if (tile.wire3()) li2.Add("绿电线");
            if (tile.wire4()) li2.Add("黄电线");

            if (li2.Any()) li.Add($"其它：{string.Join(", ", li2)}");
            op.SendInfoMessage(string.Join("\n", li));
        }
    }
}