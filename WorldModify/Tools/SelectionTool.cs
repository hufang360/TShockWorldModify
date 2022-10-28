using Microsoft.Xna.Framework;
using System;
using Terraria;
using TShockAPI;

namespace WorldModify
{
    class SelectionTool
    {
        private static TempPointData[] TempPoints = new TempPointData[Main.maxPlayers];
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

            TempPointData tpd = GetPointData(op.Index);
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help":
                    op.SendInfoMessage("/igen s, 查看 选区");
                    op.SendInfoMessage("/igen s all，将 选区 设置为 整个世界");
                    op.SendInfoMessage("/igen s screen，将 选区 设置为 以玩家为中心的一屏区域");
                    op.SendInfoMessage("/igen s 1，设置 选区的 起始点");
                    op.SendInfoMessage("/igen s 2，设置 选区的 结束点");
                    break;

                case "all":
                    tpd.rect = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
                    op.SendSuccessMessage("已将选区设置为整个世界");
                    break;

                case "screen":
                case "0":
                    tpd.rect = new Rectangle();
                    op.SendSuccessMessage("已将 选区 设置为 以玩家为中心的一屏区域，区域将实时计算");
                    break;

                case "1":
                    tpd.AwaitingTempPoint = 1;
                    RegisterEvent();
                    op.SendSuccessMessage("用镐子敲击方块，以设置选区的起始点");
                    break;

                case "2":
                    if (tpd.TempPoints[0] == Point.Zero)
                    {
                        op.SendInfoMessage("请先执行 /igen s 1 设置选区的起始点");
                        return;
                    }
                    tpd.AwaitingTempPoint = 2;
                    op.SendSuccessMessage("用镐子敲击方块，以完成选区的设置");
                    break;
            }
        }

        public static Rectangle GetSelection(int index)
        {
            if (index == -1)
                return utils.GetBaseArea();

            if (TempPoints[index] == null)
                return utils.GetScreen(TShock.Players[index]);
            else
                return utils.CloneRect(TempPoints[index].rect);
        }

        private static TempPointData GetPointData(int index)
        {
            TempPointData tpd = TempPoints[index];
            if (tpd == null)
            {
                tpd = new TempPointData();
                TempPoints[index] = tpd;
            }
            return tpd;
        }

        static bool hasEvent = false;
        private static void RegisterEvent()
        {
            if (!hasEvent)
            {
                hasEvent = true;
                GetDataHandlers.TileEdit += OnTileEdit;
            }
        }

        // 敲击方块
        public static void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs e)
        {
            TSPlayer op = e.Player;
            TempPointData tpd = GetPointData(op.Index);

            if (tpd.AwaitingTempPoint != 0)
            {
                tpd.TempPoints[tpd.AwaitingTempPoint - 1].X = e.X;
                tpd.TempPoints[tpd.AwaitingTempPoint - 1].Y = e.Y;

                if (tpd.AwaitingTempPoint == 1)
                {
                    op.SendInfoMessage($"已设置起始点，输入 /igen s 2 以设置结束点");
                }
                else
                {
                    var x = Math.Min(tpd.TempPoints[0].X, tpd.TempPoints[1].X);
                    var y = Math.Min(tpd.TempPoints[0].Y, tpd.TempPoints[1].Y);
                    var width = Math.Abs(tpd.TempPoints[0].X - tpd.TempPoints[1].X);
                    var height = Math.Abs(tpd.TempPoints[0].Y - tpd.TempPoints[1].Y);

                    tpd.TempPoints[0] = Point.Zero;
                    tpd.TempPoints[1] = Point.Zero;

                    Rectangle rect = new Rectangle(x, y, width + 1, height + 1);
                    tpd.rect = rect;
                    op.SendSuccessMessage($"已将选区设置为 x={rect.X} y={rect.Y} 宽={rect.Width} 高={rect.Height}（仅本次开服有效）");
                }

                tpd.AwaitingTempPoint = 0;
                op.SendTileSquareCentered(e.X, e.Y, 4);
                e.Handled = true;
            }

        }

        public static void dispose()
        {
            if (hasEvent)
            {
                hasEvent = false;
                GetDataHandlers.TileEdit -= OnTileEdit;
            }
        }
    }


    #region TempPointData
    class TempPointData
    {
        public int AwaitingTempPoint { get; set; }

        public Point[] TempPoints = new Point[2];
        public Rectangle rect = new Rectangle();
    }
    #endregion
}