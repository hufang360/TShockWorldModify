using System;
using System.IO;
using System.Threading;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    public class BackupHelper
    {
        public static string BackupPath { get; set; }


        public static void Backup(TSPlayer op, string notes = "")
        {
            Thread t = new Thread(() =>
            {
                DoBackup(op, notes);
            })
            {
                Name = "[wm]Backup Thread"
            };
            t.Start();
        }

        private static void DoBackup(TSPlayer op, string notes)
        {
            try
            {
                string worldname = Main.worldPathName;
                string name = Path.GetFileName(worldname);

                if (string.IsNullOrEmpty(notes))
                    Main.ActiveWorldFileData._path = Path.Combine(BackupPath, string.Format("{0}.{1:yyyyMMddHHmmss}.bak", name, DateTime.Now));
                else
                    Main.ActiveWorldFileData._path = Path.Combine(BackupPath, string.Format("{0}.{1:yyyyMMddHHmmss}_{2}.bak", name, DateTime.Now, notes));

                string worldpath = Path.GetDirectoryName(Main.worldPathName);
                if (worldpath != null && !Directory.Exists(worldpath))
                    Directory.CreateDirectory(worldpath);

                TShock.Log.Info("[wm]正在备份地图...");
                op.SendInfoMessage("正在备份地图...");

                TShock.Utils.SaveWorld();

                TShock.Log.Info($"[wm]世界已备份 ({Main.worldPathName})");
                op.SendInfoMessage($"世界已备份");

                Main.ActiveWorldFileData._path = worldname;
            }
            catch (Exception ex)
            {
                TShock.Log.Error("[wm]备份失败!");
                op.SendInfoMessage("备份地图失败！请手动查看日志文件");
                TShock.Log.Error(ex.ToString());
            }
        }

    }
}
