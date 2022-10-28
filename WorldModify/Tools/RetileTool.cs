using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace WorldModify
{
    class RetileTool
    {
        private static RetileConfig _config;
        public static string SaveFile;

        public static bool FirstCreate()
        {
            if (File.Exists(SaveFile))
                return false;

            // 读取内嵌配置文件
            string text = utils.FromEmbeddedPath("WorldModify.res.retile.json");
            utils.CreateSaveDir();
            // 将内嵌配置文件拷出
            File.WriteAllText(SaveFile, text);
            return true;
        }
        public static void Init() { Reload(); }
        public static void Reload() { _config = RetileConfig.Load(SaveFile); }
        public static void Save() { File.WriteAllText(SaveFile, JsonConvert.SerializeObject(_config, Formatting.Indented)); }
        public static RetileConfig Con { get { return _config; } }
    }

    class RetileConfig
    {
        public List<ReTileInfo> replace = new List<ReTileInfo>();

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
                text = utils.FromEmbeddedPath("WorldModify.res.retile.json");
                utils.CreateSaveDir();
                // 将内嵌配置文件拷出
                File.WriteAllText(path, text);
            }

            return JsonConvert.DeserializeObject<RetileConfig>(text, new JsonSerializerSettings()
            {
                Error = (sender, error) => error.ErrorContext.Handled = true
            });
        }

    }



}
