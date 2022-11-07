using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Terraria.DataStructures;

namespace WorldModify
{

    public class ResHelper
    {
        #region TileFrame
        private static List<TileProp> Tiles = new List<TileProp>();
        private static bool TilesIsLoad = false;

        public static void LoadTileFrame()
        {
            if (TilesIsLoad) return;
            else TilesIsLoad = true;

            foreach (string line in Utils.FromEmbeddedPath("TileFrame.csv").Split('\n'))
            {
                var arr = line.Split(';');

                var arrSize = arr[1].Split(',');
                var arrFrame = arr[2].Split(':');

                TileProp tile = new TileProp
                {
                    id = int.Parse(arr[0])
                };
                if (arrSize.Length == 2)
                {
                    tile.w = int.Parse(arrSize[0]);
                    tile.h = int.Parse(arrSize[1]);
                }
                foreach (string s in arrFrame)
                {
                    var arrUV = s.Split(',');
                    if (arrUV.Length == 2)
                    {
                        //tile.frames.Add(new Point16(int.Parse(arrUV[0]), int.Parse(arrUV[1])));
                    }
                }
                Tiles.Add(tile);
            }
        }

        public static FindData GetFindData(int id, int style = -1)
        {
            FindData fd = new FindData(id, style);
            fd.style = style;

            TileProp tprop = GetTileProp(id);
            fd.w = tprop.w;
            fd.h = tprop.h;
            if (style != -1 && style < tprop.frames.Count)
            {
                fd.frameX = tprop.frames[style].frameX;
                fd.frameY = tprop.frames[style].frameY;
            }
            return fd;
        }

        private static TileProp GetTileProp(int id)
        {
            foreach (var tile in Tiles)
            {
                if (tile.id == id) return tile;
            }
            return new TileProp();
        }
        #endregion

        #region 墙
        public static Dictionary<int, WallProp> Walls = new();
        /// <summary>
        /// 加载内嵌的墙体配置文件（.csv）
        /// </summary>
        public static void LoadWall()
        {
            if (Walls.Any()) return;

            int id = 1;
            foreach (string line in Utils.FromEmbeddedPath("Wall.csv").Split('\n'))
            {
                var arr = line.Split(',');
                Walls.Add(id, new WallProp
                {
                    id = id,
                    name = arr[0],
                    color = arr[1]
                });
                id++;
            }
        }

        /// <summary>
        /// 获得墙属性
        /// </summary>
        /// <param name="idOrName"></param>
        /// <returns>查找失败时返回 null</returns>
        public static WallProp GetWallByIDOrName(string idOrName)
        {
            LoadWall();
            if (!int.TryParse(idOrName, out int id))
            {
                foreach (var w in Walls.Values)
                {
                    if (w.name == idOrName)
                    {
                        id = w.id;
                        break;
                    }
                }
            }
            return Walls.ContainsKey(id) ? Walls[id] : null;
        }
        #endregion

        #region DumpXML
        /// <summary>
        /// 解析 tshock/WorldModify/settings.xml 文件，并导出至对应的csv文件。
        /// settings.xml 来自：https://raw.github.com/BinaryConstruct/Terraria-Map-Editor/master/TEdit/settings.xml
        /// </summary>
        public static void DumpXML()
        {
            var xmlStr = Utils.FromCombinePath("settings.xml");
            if (string.IsNullOrEmpty(xmlStr))
            {
                Utils.Log($"{Utils.SaveDir} 目录下没有 settings.xml 文件");
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
            Utils.Log($"Frameds:{string.Join(",", frameds)}");

            // 图格
            StringBuilder text1 = new();
            StringBuilder text2 = new();
            text1.AppendLine("id,name,w,h,color,framed");
            text2.AppendLine("id,style,name,frameX,frameY");
            foreach (var data in tiles)
            {
                text1.AppendLine($"{data.id},{data.name},{data.w},{data.h},{data.color},{(data.isFrame ? 1 : 0)}");
                foreach (var frame in data.frames)
                {
                    text2.AppendLine($"{data.id},{frame.style},{frame.name},{frame.frameX},{frame.frameY}");
                }
            }
            File.WriteAllText(Utils.CombinePath("Tile.csv"), text1.ToString());
            Utils.Log($"已生成 {Utils.CombinePath("Tile.csv")}");
            File.WriteAllText(Utils.CombinePath("TileFrame.csv"), text2.ToString());
            Utils.Log($"已生成 {Utils.CombinePath("TileFrame.csv")}");

        }

        static Point16 StringToPoint16(string text)
        {
            Point16 p = new Point16();
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
            return p;
        }
        #endregion
    }

}
