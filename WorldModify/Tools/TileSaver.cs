using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace WorldModify;

public class TileSaver
{
    public int width = 1;
    public int height = 1;
    public List<TileSaverData> tiles = new();

    static string ConDir = Path.Combine(Utils.WorkDir, "clips");

    public static TileSaver Load(string fileName)
    {
        string ConFile = Path.Combine(ConDir, $"{fileName}.json");
        if (!File.Exists(ConFile))
        {
            return new TileSaver();
        }
        return JsonConvert.DeserializeObject<TileSaver>(File.ReadAllText(ConFile));
        //return JsonConvert.DeserializeObject<TileSaver>(File.ReadAllText(path), new JsonSerializerSettings()
        //{
        //    Error = (sender, error) => error.ErrorContext.Handled = true
        //});
    }

    public void Save(string fileName)
    {
        // 创建文件夹
        if (!Directory.Exists(ConDir))
        {
            Directory.CreateDirectory(ConDir);
        }
        string ConFile = Path.Combine(ConDir, $"{fileName}.json");
        File.WriteAllText(ConFile, JsonConvert.SerializeObject(this, Formatting.Indented));
    }


}
