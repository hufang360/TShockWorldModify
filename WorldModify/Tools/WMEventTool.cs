using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 世界事件工具
    /// </summary>
    class WMEventTool
    {
        /// <summary>
        /// 巨石雨开关（原版机制：临时开启 Constant 种子 + 暴风雨）
        /// </summary>
        public static bool BoulderRain { get; private set; } = false;

        // 巨石雨开启前的世界状态备份
        static bool _bakDrunk;
        static bool _bakGetGood;
        static bool _bakRemix;
        static float _bakWind;

        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);

            void Help()
            {
                List<string> lines =
                [
                    "[c/FFE55C:雨/沙尘暴/血月/日食/灯笼夜/流星雨/陨石/入侵 等功能，请使用 /worldevent 指令]",
                    "/wm e stone [on/off]，查看/开关 巨石雨（临时开启 Constant 种子 + 暴风雨）",
                    "/wm e slimerain [on/off]，查看/开关 史莱姆雨",
                    "/wm e windy [on/off]，查看/开关 大风天（白天 7:30~16:30 生效）",
                    "/wm e storm [on/off]，查看/开关 暴风雨",
                    "/wm e party [on/off]，查看/开关 派对",
                    "/wm e clear，风和日丽（停雨/无风/清云/时间调到08:15/跳过入侵/停止事件）",
                    "/wm e skip，跳过 入侵",
                ];

                Utils.Pagination(args, ref lines, "/wm event");
            }

            TSPlayer op = args.Player;

            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            string kw = args.Parameters[0].ToLowerInvariant();
            switch (kw)
            {
                case "help":
                case "h":
                    Help();
                    break;

                case "stone":
                case "boulderrain":
                case "boulder":
                case "br":
                    ToggleBoulderRain(args);
                    break;

                case "slimerain":
                case "slime":
                    ToggleState(args, "史莱姆雨", () => Main.slimeRain, v => Main.slimeRain = v);
                    break;

                case "windy":
                    ToggleWindy(args);
                    break;

                case "storm":
                    ToggleStorm(args);
                    break;

                case "party":
                    ToggleState(args, "派对", () => BirthdayParty.ManualParty, v => BirthdayParty.ManualParty = v);
                    break;

                case "clear":
                case "c":
                case "clearweather":
                    ClearWeather(args);
                    break;

                case "skip":
                case "skipinvasion":
                    SkipInvasion(args);
                    break;

                default:
                    op.SendErrorMessage("语法错误！输入 /wm event help 查询用法");
                    break;
            }
        }

        /// <summary>
        /// 同步世界信息
        /// </summary>
        static void SyncWorld()
        {
            TSPlayer.All.SendData(PacketTypes.WorldInfo);
        }

        /// <summary>
        /// 时间调到白天（0=4:30）
        /// </summary>
        static void SetDay(double time = 0)
        {
            Main.dayTime = true;
            Main.time = time;
        }

        /// <summary>
        /// 解析可选开关参数
        /// </summary>
        static bool TryParseState(List<string> parameters, TSPlayer op, out bool? state)
        {
            state = null;
            if (parameters.Count < 2) return true;
            string s = parameters[1].ToLowerInvariant();
            switch (s)
            {
                case "on":
                case "true":
                case "1":
                    state = true;
                    return true;
                case "off":
                case "false":
                case "0":
                    state = false;
                    return true;
                default:
                    op.SendErrorMessage("无效的状态参数，请使用 on/off 或 true/false");
                    return false;
            }
        }

        /// <summary>
        /// 通用开关
        /// </summary>
        static void ToggleState(CommandArgs args, string name, Func<bool> get, Action<bool> set)
        {
            TSPlayer op = args.Player;
            if (!TryParseState(args.Parameters, op, out bool? state)) return;

            if (!state.HasValue)
            {
                op.SendInfoMessage($"{name}：{Utils.BFlag(get())}");
                return;
            }

            if (get() == state.Value)
            {
                op.SendSuccessMessage($"{name} 已是{Utils.BFlag(state.Value)}状态");
                return;
            }

            set(state.Value);
            SyncWorld();
            op.SendSuccessMessage($"{name} {Utils.BFlag(state.Value)}");
        }

        /// <summary>
        /// 巨石雨（原版机制：Constant 种子 + 暴风雨）
        /// </summary>
        static void ToggleBoulderRain(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (!TryParseState(args.Parameters, op, out bool? state)) return;

            if (!state.HasValue)
            {
                op.SendInfoMessage($"巨石雨：{Utils.BFlag(BoulderRain)}");
                return;
            }

            if (BoulderRain == state.Value)
            {
                op.SendSuccessMessage($"巨石雨 已是{Utils.BFlag(state.Value)}状态");
                return;
            }

            if (state.Value)
            {
                _bakDrunk = Main.drunkWorld;
                _bakGetGood = Main.getGoodWorld;
                _bakRemix = Main.remixWorld;
                _bakWind = Main.windSpeedTarget;

                Main.drunkWorld = true;
                Main.getGoodWorld = true;
                Main.remixWorld = false;
                Main.StartRain(instant: true, strengthOverride: 0.9f);
                Main.windSpeedTarget = 0.5f;
                SyncWorld();

                BoulderRain = true;
                op.SendSuccessMessage("巨石雨 已开启（临时开启 Constant 种子 + 暴风雨，关闭请执行 /wm event boulderrain off）");
            }
            else
            {
                Main.drunkWorld = _bakDrunk;
                Main.getGoodWorld = _bakGetGood;
                Main.remixWorld = _bakRemix;
                Main.StopRain(instant: true);
                Main.windSpeedTarget = _bakWind;
                SyncWorld();

                BoulderRain = false;
                op.SendSuccessMessage("巨石雨 已关闭");
            }
        }

        /// <summary>
        /// 大风天
        /// </summary>
        static void ToggleWindy(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (!TryParseState(args.Parameters, op, out bool? state)) return;

            bool isWindy = Math.Abs(Main.windSpeedTarget) >= 0.4f && !Main.raining;

            if (!state.HasValue)
            {
                op.SendInfoMessage($"大风天：{Utils.BFlag(isWindy)}");
                return;
            }

            if (state.Value)
            {
                if (isWindy)
                {
                    op.SendSuccessMessage("大风天 已是开启状态");
                    return;
                }
                Main.StopRain(instant: true);
                Main.windSpeedTarget = 0.5f;
                SetDay(10 * 3600);
                SyncWorld();
                op.SendSuccessMessage("大风天 已开启（白天 7:30~16:30 生效）");
            }
            else
            {
                if (!isWindy)
                {
                    op.SendSuccessMessage("大风天 已是关闭状态");
                    return;
                }
                Main.windSpeedTarget = 0f;
                SyncWorld();
                op.SendSuccessMessage("大风天 已关闭");
            }
        }

        /// <summary>
        /// 暴风雨
        /// </summary>
        static void ToggleStorm(CommandArgs args)
        {
            TSPlayer op = args.Player;
            if (!TryParseState(args.Parameters, op, out bool? state)) return;

            bool isStorm = Main.IsItStorming;

            if (!state.HasValue)
            {
                op.SendInfoMessage($"暴风雨：{Utils.BFlag(isStorm)}");
                return;
            }

            if (isStorm == state.Value)
            {
                op.SendSuccessMessage($"暴风雨 已是{Utils.BFlag(state.Value)}状态");
                return;
            }

            if (state.Value)
            {
                Main.StartRain(instant: true, strengthOverride: 0.9f);
                Main.windSpeedTarget = 0.5f;
                SyncWorld();
                op.SendSuccessMessage("暴风雨 已开启");
            }
            else
            {
                Main.StopRain(instant: true);
                Main.windSpeedTarget = 0f;
                SyncWorld();
                op.SendSuccessMessage("暴风雨 已关闭");
            }
        }

        /// <summary>
        /// 风和日丽
        /// </summary>
        static void ClearWeather(CommandArgs args)
        {
            TSPlayer op = args.Player;

            Main.StopRain(instant: true);
            Main.windSpeedTarget = 0f;
            Main.numClouds = 0;
            SetDay(8 * 3600 + 15 * 60);

            Main.bloodMoon = false;
            Main.eclipse = false;
            Main.slimeRain = false;
            Sandstorm.Happening = false;
            Sandstorm.IntendedSeverity = 0f;
            BirthdayParty.ManualParty = false;
            LanternNight.ManualLanterns = false;
            Main.pumpkinMoon = false;
            Main.snowMoon = false;

            SkipInvasionState();

            SyncWorld();
            op.SendSuccessMessage("风和日丽 已开启（停雨/无风/清云/时间08:15/跳过入侵/停止事件）");
        }

        static void SkipInvasionState()
        {
            Main.invasionType = 0;
            Main.invasionSize = 0;
            Main.invasionSizeStart = 0;
            Main.invasionProgress = 0;
        }

        /// <summary>
        /// 跳过入侵
        /// </summary>
        static void SkipInvasion(CommandArgs args)
        {
            TSPlayer op = args.Player;
            SkipInvasionState();
            SyncWorld();
            op.SendSuccessMessage("入侵 已跳过");
        }
    }
}
