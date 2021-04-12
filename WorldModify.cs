﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Plugin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Author => "hufang360";

        public override string Description => "简易的世界修改器";

        public override string Name => "WorldModify";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public Plugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(new List<string>() {"worldmodify"}, WorldModify, "worldmodify", "wm") { HelpText = "简易的世界修改器"});
            Commands.ChatCommands.Add(new Command(new List<string>() {"moonphase"}, ChangeMoonPhase, "moonphase", "mp", "moon") { HelpText = "修改月相"});
            Commands.ChatCommands.Add(new Command(new List<string>() {"moonstyle"}, ChangeMoonStyle, "moonstyle", "ms") { HelpText = "修改月亮样式"});
        }


        private void WorldModify(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 0)
            {
                ShowHelpText(args);
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                // 帮助
                default:
                case "help":
                    ShowHelpText(args);
                    return;

                // 世界信息
                case "info":
                    args.Player.SendInfoMessage("当前世界信息");
                    args.Player.SendInfoMessage("名字: {0}", Main.worldName);
                    args.Player.SendInfoMessage("大小: {0}", Main.ActiveWorldFileData.WorldSizeName);
                    args.Player.SendInfoMessage("难度: {0}", _worldModes.Keys.ElementAt(Main.GameMode));
                    args.Player.SendInfoMessage("种子: {0}", WorldGen.currentWorldSeed);
                    if(this.GetSecretWorldDescription()!="")
                    {
                        args.Player.SendInfoMessage(this.GetSecretWorldDescription());
                    }
                    args.Player.SendInfoMessage(this.GetCorruptionDescription());
                    args.Player.SendInfoMessage("困难模式: {0}", (Main.ActiveWorldFileData.IsHardMode ? "是" : "否"));
                    args.Player.SendInfoMessage("月相: {0}", _moonPhases.Keys.ElementAt(Main.moonPhase));
                    args.Player.SendInfoMessage("月亮样式: {0}", _moonTypes.Keys.ElementAt(Main.moonType));
                    break;


                // 名字
                case "name":
                    Main.worldName = args.Parameters[1];
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    TSPlayer.All.SendSuccessMessage("世界的名字已改成 {0}", args.Parameters[1]);
                    break;


                // 种子
                case "seed":
                    Main.ActiveWorldFileData.SetSeed(args.Parameters[1]);
                    TSPlayer.All.SendData(PacketTypes.WorldInfo);
                    TSPlayer.All.SendSuccessMessage("世界的种子已改成 {0}", args.Parameters[1]);
                    break;


                // 醉酒世界
                case "0516":
                case "05162020":
                case "516":
                case "5162020":
                case "drunk":
                    if (Main.drunkWorld) {
                        Main.drunkWorld = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        args.Player.SendSuccessMessage("已关闭 05162020 秘密世界（醉酒世界 / DrunkWorld）");
                    } else {
                        Main.drunkWorld = true;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        args.Player.SendSuccessMessage("已开启 05162020 秘密世界（醉酒世界 / DrunkWorld）");
                    }
                    break;


                // ftw
                case "ftw":
                case "for the worthy":
                    if (Main.getGoodWorld) {
                        Main.getGoodWorld = false;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        args.Player.SendSuccessMessage("已关闭 for the worthy 秘密世界");
                    } else  {
                        Main.getGoodWorld = true;
                        TSPlayer.All.SendData(PacketTypes.WorldInfo);
                        args.Player.SendSuccessMessage("已开启 for the worthy 秘密世界");
                    }
                    break;

            }
        }


        // 修改月相
        private void ChangeMoonPhase(CommandArgs args)
        {
            if(args.Parameters.Count<string>()==0 || args.Parameters[0].ToLowerInvariant()=="help")
            {
                args.Player.SendInfoMessage("当前月相: {0}", _moonPhases.Keys.ElementAt(Main.moonPhase));
                args.Player.SendInfoMessage("用法：/moon <月相>");
                args.Player.SendInfoMessage("月相：{0} （可用数字 1~8 代替）", String.Join(", ", _moonPhases.Keys));
                return;
            }

            int moon;
            if (int.TryParse(args.Parameters[0], out moon))
            {
                if (moon < 1 || moon > 8)
                {
                    args.Player.SendErrorMessage("语法错误！用法：/moon <月相>");
                    args.Player.SendErrorMessage("月相：{0} （可用数字 1~8 代替）", String.Join(", ", _moonPhases.Keys));
                    return;
                }
            }
            else if (_moonPhases.ContainsKey(args.Parameters[0]))
            {
                moon = _moonPhases[args.Parameters[0]];
            }
            else
            {
                args.Player.SendErrorMessage("语法错误！用法：/moon <月相>");
                args.Player.SendErrorMessage("月相：{0} （可用数字 1~8 代替）", String.Join(", ", _moonPhases.Keys));
                return;
            }

            Main.dayTime = false;
            Main.moonPhase = moon-1;
            Main.time = 0.0;
            TSPlayer.All.SendData(PacketTypes.WorldInfo);
            args.Player.SendSuccessMessage("月相已改为 {0}", _moonPhases.Keys.ElementAt(moon-1));
        }

        // 修改月亮样式
        private void ChangeMoonStyle(CommandArgs args)
        {
            if(args.Parameters.Count<string>()==0){
                args.Player.SendInfoMessage("当前月亮样式: {0}", _moonTypes.Keys.ElementAt(Main.moonType));
                args.Player.SendInfoMessage("用法：/moonstyle <月亮样式>");
                args.Player.SendInfoMessage("月亮样式：{0} （可用数字 1~9 代替）", String.Join(", ", _moonTypes.Keys));
                return;
            }

            int moontype;
            if (int.TryParse(args.Parameters[0], out moontype))
            {
                if (moontype < 1 || moontype > 9)
                {
                    args.Player.SendErrorMessage("语法错误！用法：/moonstyle <月亮样式>");
                    args.Player.SendErrorMessage("月亮样式：{0} （可用数字 1~9 代替）", String.Join(", ", _moonTypes.Keys));
                    return;
                }
            }
            else if (_moonTypes.ContainsKey(args.Parameters[0]))
            {
                moontype = _moonTypes[args.Parameters[0]];
            }
            else
            {
                args.Player.SendErrorMessage("语法错误！用法：/moonstyle <月亮样式>");
                args.Player.SendErrorMessage("月亮样式：{0} （可用数字 1~9 代替）", String.Join(", ", _moonTypes.Keys));
                return;
            }
            Main.dayTime = false;
            Main.moonType = moontype-1;
            Main.time = 0.0;
            TSPlayer.All.SendData(PacketTypes.WorldInfo);
            args.Player.SendSuccessMessage("月亮样式已改为 {0}", _moonTypes.Keys.ElementAt(moontype-1));
        }

        /// <summary>
        /// 帮助
        /// </summary>
        private void ShowHelpText(CommandArgs args)
        {
            args.Player.SendInfoMessage("/wm info，查看世界信息");
            args.Player.SendInfoMessage("/wm name <世界名>，修改世界名字");
            args.Player.SendInfoMessage("/wm seed <种子>，修改世界种子");
            args.Player.SendInfoMessage("/wm 0516，开启/关闭 05162020 秘密世界");
            args.Player.SendInfoMessage("/wm ftw，开启/关闭 for the worthy 秘密世界");

        }

        // 获取秘密世界种子状态描述
        private string GetSecretWorldDescription()
        {
            if(Main.drunkWorld && Main.getGoodWorld){
                return "秘密世界：for the worthy 和 05162020";
            }
            if(Main.getGoodWorld){
                return "秘密世界：for the worthy";
            }
            if(Main.drunkWorld){
                return "秘密世界：05162020";
            }
            return "";
        }


        // 获取腐化类型描述
        private string GetCorruptionDescription()
        {
            if(Main.ActiveWorldFileData.HasCrimson && Main.ActiveWorldFileData.HasCorruption){
                return "腐化类型: 腐化和猩红共存";
            }
            if(Main.ActiveWorldFileData.HasCrimson){
                return "腐化类型: 猩红";
            }
            if(Main.ActiveWorldFileData.HasCorruption){
                return "腐化类型: 腐化";
            }
            return "";
        }

        static Dictionary<string, int> _worldModes = new Dictionary<string, int>
        {
            { "经典", 1 },
            { "专家", 2 },
            { "大师", 3 },
            { "旅行", 4 }
        };

        static Dictionary<string, int> _moonPhases = new Dictionary<string, int>
        {
            { "满月", 1 },
            { "亏凸月", 2 },
            { "下弦月", 3 },
            { "残月", 4 },
            { "新月", 5 },
            { "娥眉月", 6 },
            { "上弦月", 7 },
            { "盈凸月", 8 }
        };

        // https://terraria.fandom.com/wiki/Moon_phase
        static Dictionary<string, int> _moonTypes = new Dictionary<string, int>
        {
            { "正常", 1 },
            { "火星样式", 2 },
            { "土星样式", 3 },
            { "秘银风格", 4 },
            { "明亮的偏蓝白色", 5 },
            { "绿色", 6 },
            { "糖果", 7 },
            { "金星样式", 8 },
            { "紫色的三重月亮", 9 }
        };


        protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			base.Dispose(disposing);
		}
	}
}
