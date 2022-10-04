using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Terraria;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 像素画打印工具
    /// </summary>
    class PaintTool
    {
        public static void Manage(TSPlayer op)
        {
            string filename = Path.Combine(utils.SaveDir, "1.png");
            if (!File.Exists(filename))
            {
                op.SendErrorMessage($"{filename} 文件不存在！");
                return;
            }

            Bitmap bmp = new Bitmap(filename);
            if (bmp.Width > Main.maxTilesX)
            {
                op.SendErrorMessage($"图片宽度太大了，不应超过{Main.maxTilesX}px");
                bmp.Dispose();
                return;
            }
            if (bmp.Height > Main.maxTilesY)
            {
                op.SendErrorMessage($"图片的高度太大了，不应超过{Main.maxTilesY}px");
                bmp.Dispose();
                return;
            }


            int startX = op.TileX - bmp.Width / 2;
            int startY = op.TileY - bmp.Height;

            if (startX < 0)
            {
                op.SendErrorMessage($"位置太靠左了，请往右移动一些");
                bmp.Dispose();
                return;
            }
            if (startY < 0)
            {
                op.SendErrorMessage($"位置太靠上了，请往下移动一些");
                bmp.Dispose();
                return;
            }

            if (startX + bmp.Width / 2 > Main.maxTilesX)
            {
                op.SendErrorMessage($"位置太靠右了，请往左移动一些");
                bmp.Dispose();
                return;
            }

            if (startY + bmp.Height > Main.maxTilesY)
            {
                op.SendErrorMessage($"位置太靠下了，请往上移动一些");
                bmp.Dispose();
                return;
            }

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    int tileX = startX + x;
                    int tileY = startY + y;
                    Main.tile[tileX, tileY] = TileFromColor(bmp.GetPixel(x, y));
                    NetMessage.SendTileSquare(-1, tileX, tileY);
                }
            }
            bmp.Dispose();
            op.SendSuccessMessage("像素打印完成！");
        }

        private static Tile TileFromColor(Color color)
        {
            var tile = new Tile();
            int type = GetBrickFromColor(color);
            if (type != -1)
            {
                tile.type = (ushort)type;
                tile.active(active: true);
            }

            return tile;
        }


        // 假彩色图像（Real TileColor）
        public static int GetBrickFromColor(Color color)
        {
            foreach (int k in ColorDict.Keys)
            {
                Color c = ColorDict[k];
                if (color.R == c.R && color.G == c.G && color.B == c.B)
                    return k;
            }
            return -1;
        }
        private static Dictionary<int, Color> _tcDict = new Dictionary<int, Color>();
        private static bool _tcInit = false;
        public static Dictionary<int, Color> ColorDict
        {
            get
            {
                if (_tcInit) return _tcDict;

                foreach (string line in utils.FromEmbeddedPath("WorldModify.res.TileColor.csv").Split('\n'))
                {
                    var arr = line.Split(',');
                    if (arr.Length != 2) continue;

                    if (int.TryParse(arr[0], out int id))
                    {
                        if (_tcDict.ContainsKey(id)) continue;
                        _tcDict.Add(id, ColorTranslator.FromHtml($"#{arr[1]}"));
                    }
                }
                _tcInit = true;
                return _tcDict;
            }
        }

    }
}