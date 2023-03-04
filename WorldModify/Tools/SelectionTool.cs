using Microsoft.Xna.Framework;
using System;
using Terraria;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 选区工具
    /// </summary>
    public class SelectionTool
    {
        static bool hasEvent = false;
        static bool[] flags = new bool[Main.maxPlayers];
        static Rectangle[] rects = new Rectangle[Main.maxPlayers];
        public static void Mange(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            Rectangle rect = GetSelection(op.Index);
            if (args.Parameters.Count == 0)
            {
                if (rect.Width == 0 && rect.Height == 0)
                    op.SendInfoMessage("选区：以玩家为中心的一屏区域，区域将实时计算");
                else if (rect.Width == Main.maxTilesX && rect.Height == Main.maxTilesY)
                    op.SendErrorMessage("选区：整个世界！整个世界！整个世界！");
                else
                    op.SendInfoMessage($"选区：x={rect.X} y={rect.Y} 宽={rect.Width} 高={rect.Height}");
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help":
                    op.SendInfoMessage("/igen s, 查看 选区");
                    op.SendInfoMessage("/igen s all，选中整个世界");
                    op.SendInfoMessage("/igen s 0，以玩家为中心的一屏区域");
                    op.SendInfoMessage("/igen s 1，自定义选区（[i:3611]）");
                    break;

                case "all":
                    rects[op.Index] = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
                    op.SendSuccessMessage("已将选区设置为整个世界");
                    break;

                case "screen":
                case "0":
                    rects[op.Index] = Rectangle.Empty;
                    op.SendSuccessMessage("已将选区设置为以玩家为中心的一屏区域，区域将实时计算。");
                    break;

                case "1":
                    flags[args.Player.Index] = true;
                    RegisterEvent();
                    op.SendSuccessMessage("使用[i:3611]精密线控仪拉动放置红电线，以设置选区");
                    break;
            }
        }

        public static Rectangle GetSelection(int index)
        {
            if (index == -1)
                return Utils.GetBaseArea();

            if (rects[index] != Rectangle.Empty)
                return rects[index];
            else
                return Utils.GetScreen(TShock.Players[index]);
        }

        public static Rectangle GetSelection2(int index)
        {
            return rects[index];
        }

        static void RegisterEvent()
        {
            if (!hasEvent)
            {
                hasEvent = true;
                GetDataHandlers.MassWireOperation += OnMassWire;
            }
        }

        static void OnMassWire(object sender, GetDataHandlers.MassWireOperationEventArgs e)
        {
            var index = e.Player.Index;
            // ToolMode BitFlags: 1 = Red, 2 = Green, 4 = Blue, 8 = Yellow, 16 = Actuator, 32 = Cutter 33移除红电线 34移绿
            if (flags[index] && e.ToolMode == 1)
            {
                Rectangle rect = new(e.StartX, e.StartY, e.EndX - e.StartX, e.EndY - e.StartY);
                if (rect.Width < 0)
                {
                    rect.X += rect.Width;
                    rect.Width = Math.Abs(rect.Width);
                }
                if (rect.Height < 0)
                {
                    rect.Y += rect.Height;
                    rect.Height = Math.Abs(rect.Height);
                }

                // 边界
                rect.Width++;
                rect.Height++;

                rects[index] = rect;
                flags[index] = false;
                e.Handled = true;
                e.Player.SendSuccessMessage($"已选取区域：x={rect.X} y={rect.Y} 宽={rect.Width} 高={rect.Height}（仅本次开服有效）");
            }
        }

        public static void Dispose()
        {
            if (hasEvent)
            {
                hasEvent = false;
                GetDataHandlers.MassWireOperation -= OnMassWire;
            }

        }
    }

}