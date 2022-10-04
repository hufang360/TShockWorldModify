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
    class utils
    {
        public static string SaveDir;

        public static string CFlag(bool foo, string fstr) { return foo ? $"[c/96FF96:✔{fstr}]" : $"-{fstr}"; }
        public static string BFlag(bool _vaule) { return _vaule ? "已开启" : "已关闭"; }

        public static bool NeedInGame(TSPlayer op)
        {
            if (!op.RealPlayer) op.SendErrorMessage($"请进入游戏后再操作！");
            return !op.RealPlayer;
        }

        public static bool InArea(Rectangle rect, Rectangle point)
        {
            return InArea(rect, point.X, point.Y);
        }
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
        /// <returns></returns>
        public static bool IsBase(int x, int y)
        {
            int sw = 122;
            int sh = 68;
            Rectangle area = new Rectangle(Main.spawnTileX - sw / 2, Main.spawnTileY - sh / 2, sw, sh);
            return area.Contains(x, y);
        }

        /// <summary>
        /// 玩家所在一屏区域
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static Rectangle GetScreen(TSPlayer op) { return GetScreen(op.TileX, op.TileY); }
        public static Rectangle GetScreen(int playerX, int playerY) { return new Rectangle(playerX - 61, playerY - 34 + 3, 122, 68); }
        public static Rectangle GetWorldArea() { return new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY); }
        public static Rectangle CloneRect(Rectangle rect) { return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height); }

        public static bool ToGuid(string str)
        {
            Guid gv = new Guid();
            try
            {
                gv = new Guid(str);
            }
            catch (Exception)
            {

            }
            return gv != Guid.Empty;
        }

        public static bool TryParseInt(List<string> args, int index, out int num)
        {
            if (index >= args.Count)
            {
                num = 0;
                return false;
            }
            return int.TryParse(args[index], out num);
        }
        public static string TryString(List<string> args, int index)
        {
            return index >= args.Count ? "" : args[index];
        }

        public static int GetUnixTimestamp { get { return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; } }


        // 保存前先备份文件
        public static void SaveAndBack(string SaveFile, string SaveStr)
        {
            if (File.Exists(SaveFile))
            {
                string ext = SaveFile.Substring(SaveFile.Length - 3);
                string newSaveFile = SaveFile.Substring(0, SaveFile.Length - 4);
                File.Move(SaveFile, string.Format("{0}-{1:yyyyMMddHHmmss}.{2}", newSaveFile, DateTime.Now, ext));
            }
            File.WriteAllText(SaveFile, SaveStr);
        }

        public static string FromEmbeddedPath(string path)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        public static void Log(string msg) { TShock.Log.ConsoleInfo("[wm]" + msg); }

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
            return o.TryGetValue(tokenName, StringComparison.InvariantCultureIgnoreCase, out t) ? (int)t : (int?)null;
        }
    }
    #endregion
}