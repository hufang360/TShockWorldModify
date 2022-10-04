using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace WorldModify
{
    class RetileTool
    {
        private static RetileConfig _config;
        public static string SaveFile;

        public static void Init() { Reload(); }
        public static void Reload() { _config = RetileConfig.Load(SaveFile); }
        public static void Save() { File.WriteAllText(SaveFile, JsonConvert.SerializeObject(_config, Formatting.Indented)); }
        public static RetileConfig Con { get { return _config; } }
    }

    class RetileConfig
    {
        public List<TileReInfo> replace = new List<TileReInfo>();

        public static RetileConfig Load(string path)
        {
            string text = "";
            if (File.Exists(path))
            {
                text = File.ReadAllText(path);
            }
            else
            {
                // 读取内嵌配置文件
                text = utils.FromEmbeddedPath("WorldModify.res.retile_desert.json");
                // 将内嵌配置文件拷出
                File.WriteAllText(path, text);
            }


            return JsonConvert.DeserializeObject<RetileConfig>(text, new JsonSerializerSettings()
            {
                Error = (sender, error) => error.ErrorContext.Handled = true
            });
        }

    }


    #region TileReInfo
    class TileReInfo
    {
        public TileInfo before = new TileInfo();
        public TileInfo after = new TileInfo();
        public string comment = "";

        public TileReInfo(int beforeID, int afterID, int bType = 0, int aType = 0, string _comment = "")
        {
            before.id = beforeID;
            after.id = afterID;

            before.type = bType;
            after.type = aType;

            if (!string.IsNullOrEmpty(_comment)) comment = _comment;
        }
    }
    #endregion


    #region TileInfo
    public class TileInfo
    {
        // 0 图格 1 墙 3 液体
        public int type = 0;

        public int id = 0;
        public int style = 0;

        public TileInfo() { }

        public TileInfo(int _id, int _style)
        {
            id = _id;
            style = _style;
        }
    }
    #endregion

}
