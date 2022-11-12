using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    class Utils
    {
        /// <summary>
        /// 保存目录
        /// </summary>
        public static string SaveDir;

        public static string CFlag(bool foo, string fstr) { return foo ? $"[c/96FF96:✔]{fstr}" : $"-{fstr}"; }
        public static string CFlag(string fstr, bool foo) { return foo ? $"{fstr}✓" : $"{fstr}-"; }
        public static string BFlag(bool _vaule) { return _vaule ? "已开启" : "已关闭"; }

        /// <summary>
        /// 是否需要在游戏内执行，如果需要本方法会给出提示，返回值为false
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static bool NeedInGame(TSPlayer op)
        {
            if (!op.RealPlayer) op.SendErrorMessage($"请进入游戏后再操作！");
            return !op.RealPlayer;
        }

        /// <summary>
        /// 是否在区域内
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool InArea(Rectangle rect, Rectangle point)
        {
            return InArea(rect, point.X, point.Y);
        }
        /// <summary>
        /// 是否在区域内
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool InArea(Rectangle rect, int x, int y) //overloaded with x,y
        {
            /*
			DO NOT CHANGE TO Area.Contains(x, y)!
			Area.Contains does not account for the right and bottom 'border' of the rectangle,
			which results in regions being trimmed.
			*/
            return x >= rect.X && x <= rect.X + rect.Width && y >= rect.Y && y <= rect.Y + rect.Height;
        }

        /// <summary>
        /// 区域保护
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsProtected(int x, int y)
        {
            var regions = TShock.Regions.InAreaRegion(x, y);
            return regions.Any();
        }

        /// <summary>
        /// 基地范围
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static bool IsBase(int x, int y)
        {
            int sw = 122;
            int sh = 68;
            Rectangle area = new(Main.spawnTileX - sw / 2, Main.spawnTileY - sh / 2, sw, sh);
            return area.Contains(x, y);
        }

        /// <summary>
        /// 玩家所在一屏区域
        /// </summary>
        /// <param name="op"></param>
        public static Rectangle GetScreen(TSPlayer op) { return GetScreen(op.TileX, op.TileY); }

        /// <summary>
        /// 玩家所在一屏区域
        /// </summary>
        /// <param name="playerX"></param>
        /// <param name="playerY"></param>
        public static Rectangle GetScreen(int playerX, int playerY) { return new Rectangle(playerX - 61, playerY - 34 + 3, 122, 68); }

        /// <summary>
        /// 整个地图区域
        /// </summary>
        public static Rectangle GetWorldArea() { return new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY); }

        /// <summary>
        /// 基地所在一屏区域
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetBaseArea() { return new Rectangle(Main.spawnTileX - 61, Main.spawnTileY - 34, 122, 68); }

        /// <summary>
        /// 克隆 Rectangle 对象
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rectangle CloneRect(Rectangle rect) { return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height); }

        /// <summary>
        /// 地图坐标点转换成位置信息
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <returns></returns>
        public static string PointToLocationDesc(int tileX, int tileY)
        {
            int num1 = tileX * 2 - Main.maxTilesX + 2;
            string textHor = (num1 > 0) ? $"{num1}以东" : ((num1 >= 0) ? "中心" : $"{-num1}以西");

            int num2 = (int)((double)(tileY * 2f) - Main.worldSurface * 2.0 + 2);
            float num3 = Main.maxTilesX / 4200f;
            num3 *= num3;
            int num4 = 1200;
            float num5 = (float)((double)(tileY - (65f + 10f * num3)) / (Main.worldSurface / 5.0));
            string text3 = (tileY > (Main.maxTilesY - 204)) ? "地狱" : ((tileY > Main.rockLayer + num4 / 32 + 1) ? "洞穴" : ((num2 > 0) ? "地下" : ((!(num5 >= 1f)) ? "太空" : "地表")));
            num2 = Math.Abs(num2);
            string text4 = (num2 != 0) ? $"{num2}的" : "级别";
            string textVer = text4 + text3;

            return $"{textHor} {textVer}";
        }

        /// <summary>
        /// 地图坐标点转换成位置信息
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static string PointToLocationDesc(NPC npc)
        {
            int num1 = (int)((npc.position.X + (npc.width / 2)) * 2f / 16f - Main.maxTilesX);
            string textHor = (num1 > 0) ? $"{num1}以东" : ((num1 >= 0) ? "中心" : $"{-num1}以西");

            int num2 = (int)((double)((npc.position.Y + npc.height) * 2f / 16f) - Main.worldSurface * 2.0);
            float num3 = Main.maxTilesX / 4200f;
            num3 *= num3;
            int num4 = 1200;
            float num5 = (float)((double)(npc.Center.Y / 16f - (65f + 10f * num3)) / (Main.worldSurface / 5.0));
            string text3 = (npc.position.Y > (Main.maxTilesY - 204) * 16) ? "地狱" : ((npc.position.Y > Main.rockLayer * 16.0 + (num4 / 2) + 16.0) ? "洞穴" : ((num2 > 0) ? "地下" : ((!(num5 >= 1f)) ? "太空" : "地表")));
            num2 = Math.Abs(num2);
            string text4 = (num2 != 0) ? $"{num2}的" : "级别";
            string textVer = text4 + text3;

            return $"{textHor} {textVer}";
        }

        /// <summary>
        /// 字符串转Guid
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StringToGuid(string str)
        {
            Guid gv = new();
            try
            {
                gv = new Guid(str);
            }
            catch (Exception)
            {

            }
            return gv != Guid.Empty;
        }

        /// <summary>
        /// 尝试将参数清单的第x个参数转成int类型
        /// </summary>
        /// <param name="args"></param>
        /// <param name="index"></param>
        /// <param name="num"></param>
        public static bool TryParseInt(List<string> args, int index, out int num)
        {
            if (index >= args.Count)
            {
                num = 0;
                return false;
            }
            return int.TryParse(args[index], out num);
        }

        /// <summary>
        /// 尝试将参数清单的第x个参数转成string类型
        /// </summary>
        /// <param name="args"></param>
        /// <param name="index"></param>
        /// <param name="num"></param>
        public static string TryParseString(List<string> args, int index)
        {
            return index >= args.Count ? "" : args[index];
        }

        /// <summary>
        /// 获取当前时间的 unix时间戳
        /// </summary>
        public static int GetUnixTimestamp
        {
            get
            {
                return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }


        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">要保存的内容</param>
        public static void Save(string path, string content)
        {
            CreateSaveDir();
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// 保存并备份
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void SafeSave(string path, string content)
        {
            CreateSaveDir();
            if (File.Exists(path))
            {
                string ext = path.Substring(path.Length - 3);
                string newPath = path.Substring(0, path.Length - 4);
                File.Move(path, string.Format("{0}-{1:yyyyMMddHHmmss}.{2}", newPath, DateTime.Now, ext));
            }
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// 创建保存目录
        /// </summary>
        public static void CreateSaveDir()
        {
            if (!Directory.Exists(SaveDir))
            {
                Directory.CreateDirectory(SaveDir);
            }
        }

        /// <summary>
        /// 获取一个文件的路径
        /// 例如：retile.json 将得到 tshock/WorldModify/retile.json
        /// </summary>
        /// <param name="fileName">文件名（含扩展名）</param>
        public static string CombinePath(string fileName)
        {
            return Path.Combine(SaveDir, fileName);
        }

        /// <summary>
        /// 从工作目录下读取文件
        /// </summary>
        /// <param name="fileName">文件名（含扩展名）</param>
        /// <returns>若文件不存在则返回空（""）</returns>
        public static string FromCombinePath(string fileName)
        {
            string path = CombinePath(fileName);
            if (File.Exists(path))
                return File.ReadAllText(path);
            else
                return "";
        }

        /// <summary>
        /// 读取内嵌资源
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>若文件不存在则返回空（""）</returns>
        public static string FromEmbeddedPath(string fileName)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"WorldModify.res.{fileName}");
            if (stream == null)
            {
                Log($"内嵌资源 {fileName} 加载失败");
                return "";
            }
            return new StreamReader(stream).ReadToEnd();
        }

        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            TShock.Log.ConsoleInfo("[wm]" + msg);
        }
    }



    #region RectangleConverter
    public class RectangleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Rectangle rect = (Rectangle)value;
            JObject.FromObject(new { rect.X, rect.Y, rect.Width, rect.Height }).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JObject.Load(reader);

            var x = GetTokenValue(o, "x") ?? 0;
            var y = GetTokenValue(o, "y") ?? 0;
            var width = GetTokenValue(o, "width") ?? 0;
            var height = GetTokenValue(o, "height") ?? 0;

            return new Rectangle(x, y, width, height);
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        private static int? GetTokenValue(JObject o, string tokenName)
        {
            JToken t;
            return o.TryGetValue(tokenName, StringComparison.InvariantCultureIgnoreCase, out t) ? (int)t : null;
        }
    }
    #endregion
}