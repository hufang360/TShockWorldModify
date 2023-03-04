using System;
using System.IO;
using System.Threading;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    /// <summary>
    /// 世界备份
    /// </summary>
    public class BackupHelper
    {
        public static string BackupPath { get; set; }


        public static void Backup(TSPlayer op, string notes = "")
        {
            Thread t = new(() =>
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
            string worldname = Main.worldPathName;
            if (string.IsNullOrEmpty(notes))
                Main.ActiveWorldFileData._path = Path.Combine(BackupPath, string.Format("world_{0:yyyyMMdd-HHmm-ss}.wld", DateTime.Now));
            else
                Main.ActiveWorldFileData._path = Path.Combine(BackupPath, string.Format("world_{0:yyyyMMdd-HHmm-ss}_{1}.wld", DateTime.Now, notes));

            try
            {
                if (!Directory.Exists(BackupPath))
                    Directory.CreateDirectory(BackupPath);

                op.SendInfoMessage("正在备份世界...");
                TShock.Utils.SaveWorld();
                op.SendSuccessMessage($"世界已备份至：{Main.worldPathName}");
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
                op.SendErrorMessage("备份世界失败！请手动查看日志文件");
            }

            Main.ActiveWorldFileData._path = worldname;
        }

    }
}
