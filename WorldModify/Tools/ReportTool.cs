using System.IO;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 导出工具
    /// </summary>
    class ReportTool
    {
        public static string SaveFile;
        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            StringBuilder Alllists = new StringBuilder();
            StringBuilder lists;
            if (args.Parameters.Count == 0)
            {
                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    lists = new StringBuilder();
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        ITile tile = Main.tile[x, y];
                        if (tile.active())
                        {
                            lists.Append("1");
                        }
                        else
                        {
                            lists.Append("0");
                        }
                    }
                    Alllists.AppendLine(string.Join(",", lists));
                }

                Utils.Save(SaveFile, Alllists.ToString());
                op.SendInfoMessage($"已保存{SaveFile}");
            }
            else
            {
                string[] Alllists2 = File.ReadAllLines(SaveFile);
                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        ITile tile = Main.tile[x, y];
                        if (Alllists2[x].Substring(y, 1) == "0")
                        {
                            tile.ClearEverything();
                        }
                    }
                }
                TileHelper.InformPlayers();
                op.SendInfoMessage($"镂空完成！");
            }
        }

    }

}