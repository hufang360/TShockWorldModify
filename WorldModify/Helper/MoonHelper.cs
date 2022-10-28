using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    public class MoonHelper
    {
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

        // 修改月相
        public static void ChangeMoonPhase(CommandArgs args)
        {
            if (args.Parameters.Count<string>() == 0 || args.Parameters[0].ToLowerInvariant() == "help")
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
            Main.moonPhase = moon - 1;
            Main.time = 0.0;
            TSPlayer.All.SendData(PacketTypes.WorldInfo);
            args.Player.SendSuccessMessage("月相已改为 {0}", _moonPhases.Keys.ElementAt(moon - 1));
        }

        // 修改月亮样式
        public static void ChangeMoonStyle(CommandArgs args)
        {
            void helpText()
            {
                args.Player.SendInfoMessage("用法：/moonstyle <月亮样式>");
                args.Player.SendInfoMessage("月亮样式：{0} （可用数字 1~9 代替）", String.Join(", ", _moonTypes.Keys));
            }

            if (args.Parameters.Count<string>() == 0)
            {
                args.Player.SendInfoMessage("当前月亮样式: {0}", _moonTypes.Keys.ElementAt(Main.moonType));
                helpText();
                return;
            }

            if (args.Parameters[0].ToLowerInvariant() == "help")
            {
                helpText();
                return;
            }

            int moontype;
            if (int.TryParse(args.Parameters[0], out moontype))
            {
                if (moontype < 1 || moontype > 9)
                {
                    helpText();
                    return;
                }
            }
            else if (_moonTypes.ContainsKey(args.Parameters[0]))
            {
                moontype = _moonTypes[args.Parameters[0]];
            }
            else
            {
                helpText();
                return;
            }
            Main.dayTime = false;
            Main.moonType = moontype - 1;
            Main.time = 0.0;
            TSPlayer.All.SendData(PacketTypes.WorldInfo);
            args.Player.SendSuccessMessage("月亮样式已改为 {0}", _moonTypes.Keys.ElementAt(moontype - 1));
        }

        public static string MoonPhaseDesc { get { return _moonPhases.Keys.ElementAt(Main.moonPhase); } }
        public static string MoonTypeDesc { get { return _moonTypes.Keys.ElementAt(Main.moonType); } }

        public static void Clear()
        {
            _moonPhases.Clear();
            _moonTypes.Clear();
        }
    }
}
