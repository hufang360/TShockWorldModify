using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Map;

namespace WorldModify
{
    /// <summary>
    /// 数据资源辅助
    /// </summary>
    public class ResHelper
    {
        public static void Init()
        {
            BackupHelper.BackupPath = Utils.CombinePath("backups");
            RetileTool.SaveFile = Utils.CombinePath("retile.json"); // 导出
            //ReportTool.SaveFile = Utils.CombinePath("report.json");
            ResearchHelper.SaveFile = Utils.CombinePath("research.csv"); // 导出
            BestiaryHelper.SaveFile = Utils.CombinePath("bestiary.csv"); // 导出
        }


        #region TileFrame
        //private static List<TileProp> Tiles = new List<TileProp>();
        //private static bool TilesIsLoad = false;

        //public static void LoadTileFrame()
        //{
        //    if (TilesIsLoad) return;
        //    else TilesIsLoad = true;

        //    foreach (string line in Utils.FromEmbeddedPath("TileFrame.csv").Split('\n'))
        //    {
        //        var arr = line.Split(';');

        //        var arrSize = arr[1].Split(',');
        //        var arrFrame = arr[2].Split(':');

        //        TileProp tile = new TileProp
        //        {
        //            id = int.Parse(arr[0])
        //        };
        //        if (arrSize.Length == 2)
        //        {
        //            tile.w = int.Parse(arrSize[0]);
        //            tile.h = int.Parse(arrSize[1]);
        //        }
        //        foreach (string s in arrFrame)
        //        {
        //            var arrUV = s.Split(',');
        //            if (arrUV.Length == 2)
        //            {
        //                //tile.frames.Add(new Point16(int.Parse(arrUV[0]), int.Parse(arrUV[1])));
        //            }
        //        }
        //        Tiles.Add(tile);
        //    }
        //}

        //public static FindData GetFindData(int id, int style = -1)
        //{
        //    FindData fd = new FindData(id, style);
        //    fd.style = style;

        //    TileProp tprop = GetTileProp(id);
        //    fd.w = tprop.w;
        //    fd.h = tprop.h;
        //    if (style != -1 && style < tprop.frames.Count)
        //    {
        //        fd.frameX = tprop.frames[style].frameX;
        //        fd.frameY = tprop.frames[style].frameY;
        //    }
        //    return fd;
        //}

        //private static TileProp GetTileProp(int id)
        //{
        //    foreach (var tile in Tiles)
        //    {
        //        if (tile.id == id) return tile;
        //    }
        //    return new TileProp();
        //}
        #endregion

        



        #region 转储XML文件（开发者自用）
        /// <summary>
        /// 解析 tshock/WorldModify/settings.xml 文件，并导出至对应的csv文件。
        /// settings.xml 来自：https://raw.github.com/BinaryConstruct/Terraria-Map-Editor/master/TEdit/settings.xml
        /// </summary>
        public static void DumpXML()
        {
            var xmlStr = Utils.FromCombinePath("settings.xml");
            if (string.IsNullOrEmpty(xmlStr))
            {
                Utils.Log($"{Utils.WorkDir} 目录下没有 settings.xml 文件");
                return;
            }

            var xmlSettings = XElement.Parse(xmlStr);

            List<TileProp> tiles = new();
            List<int> solids = new();
            List<int> frameds = new();
            foreach (var xElement in xmlSettings.Elements("Tiles").Elements("Tile"))
            {
                TileProp tile = new()
                {
                    id = (int?)xElement.Attribute("Id") ?? 0,
                    isFrame = (bool?)xElement.Attribute("Framed") ?? false,
                    name = (string)xElement.Attribute("Name"),
                    color = (string)xElement.Attribute("Color")
                };

                if ((bool?)xElement.Attribute("Solid") ?? false) solids.Add(tile.id);
                if (tile.isFrame) frameds.Add(tile.id);

                Point16 p = StringToPoint16((string)xElement.Attribute("Size"));
                if (p.X != 0 && p.Y != 0)
                {
                    tile.w = p.X;
                    tile.h = p.Y;
                }
                foreach (var elementFrame in xElement.Elements("Frames").Elements("Frame"))
                {
                    p = StringToPoint16((string)elementFrame.Attribute("UV"));

                    string name = (string)elementFrame.Attribute("Name");
                    string variety = (string)elementFrame.Attribute("Variety");
                    tile.Add(p.X, p.Y, name, variety);
                }
                tiles.Add(tile);
            }


            Utils.Log($"Solids:{string.Join(",", solids)}");
            //Utils.Log($"Frameds:{string.Join(",", frameds)}");

            // 图格
            StringBuilder buffer = new();
            buffer.AppendLine("id,style,name,w,h,frameX,frameY,color");
            foreach (var data in tiles)
            {
                buffer.AppendLine($"{data.id},,{data.name},{data.w},{data.h},,,{data.color}");
                foreach (var frame in data.frames)
                {
                    buffer.AppendLine($"{data.id},{frame.style},{frame.name},,,{frame.frameX},{frame.frameY}");
                }
            }
            File.WriteAllText(Utils.CombinePath("Tile.csv"), buffer.ToString());
            Utils.Log($"已生成 {Utils.CombinePath("Tile.csv")}");


            // 图格颜色
            buffer.Clear();
            buffer.AppendLine("id,name,color");
            foreach (var data in tiles)
            {
                buffer.AppendLine($"{data.id},{data.name},{data.color}");
            }
            File.WriteAllText(Utils.CombinePath("TileColor.csv"), buffer.ToString());
            Utils.Log($"已生成 {Utils.CombinePath("TileColor.csv")}");
        }

        static Point16 StringToPoint16(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var split = text.Split(',');
                if (split.Length == 2)
                {
                    if (short.TryParse(split[0], out short x) && short.TryParse(split[1], out short y))
                    {
                        return new Point16(x, y);
                    }
                }
            }
            return new Point16();
        }
        #endregion


        #region 名称
        /// <summary>
        /// 获得小地图图格名称（系统只包含一小部分的图格名称）
        /// </summary>
        /// <param name="projID"></param>
        /// <param name="damage"></param>
        //public static void BuildTileName()
        //{
        //    if (MapHelper.tileLookup == null)
        //    {
        //        bool status = Main.dedServ;
        //        Main.dedServ = false;
        //        MapHelper.Initialize();
        //        Main.dedServ = status;

        //        // dedServ为假时，不执行 Main 会执行 MapHelper.Initialize();
        //        // 执行 MapHelper.Initialize(); 时会执行 Lang.BuildMapAtlas();
        //        // 但是执行 Lang.BuildMapAtlas(); 遇到dedServ为真时，会不执行
        //    }

        //    string s;
        //    for (int i = 0; i < MapHelper.tileLookup.Length; i++)
        //    {
        //        int len = MapHelper.tileOptionCounts[i];
        //        if (len == 0)
        //        {
        //            s = Lang._mapLegendCache[MapHelper.tileLookup[i]].Value;
        //            Console.WriteLine($"-{i},{s}");
        //        }
        //        else
        //        {
        //            for (int j = 0; j < len; j++)
        //            {
        //                s = Lang._mapLegendCache[MapHelper.TileToLookup(i, j)].Value;
        //                Console.WriteLine($"{i},{s}");
        //            }
        //        }
        //    }
        //}
        #endregion
    }

}
