using Microsoft.Xna.Framework;
using System;
using Terraria;
using TShockAPI;


namespace WorldModify
{
    class GenHelper
    {
        public static bool isTaskRunning { get; set; }
        public static int realcount { get; set; }

        public async static void GenManage(CommandArgs args)
        {
            TSPlayer op = args.Player;
            void helpText()
            {
                op.SendInfoMessage("/igen world [种子] [腐化] [大小] [彩蛋特性], 重建地图");
                op.SendInfoMessage("/igen fixtp，将自己对齐到网格");
                op.SendInfoMessage("/igen room <数量>，生成玻璃小房间（默认生成3个）");
                op.SendInfoMessage("/igen pond，生成玻璃鱼池");
                op.SendInfoMessage("/igen hell，地狱直通车");
                op.SendInfoMessage("/igen sm <w> <h>，盾构机");
                op.SendInfoMessage("/igen dig <w> <h>，钻井机");
                op.SendInfoMessage("/igen dirt，填土");
                op.SendInfoMessage("/igen clear [all]，清空 区域/全图");
                op.SendInfoMessage("/igen random [all]，随机 区域/全图");
                op.SendInfoMessage("/igen replace [all]，图格替换");
                op.SendInfoMessage("/igen ice [all]，冰河化");
                op.SendInfoMessage("/igen melt [all]，冰融化");
            }
            if (args.Parameters.Count == 0)
            {
                helpText();
                return;
            }

            bool isRight;
            bool needAll = utils.TryString(args.Parameters, 1).ToLowerInvariant() == "all";
            int w;
            int h;
            int num;
            int x;
            int y;
            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "help": helpText(); break;
                default: op.SendErrorMessage("语法错误，请输入 /igen help 查询帮助"); break;
                #region info
                case "info":
                    if (NeedInGame(op)) return;
                    //OTAPI.Tile.ITileCollection 
                    int cx = op.TileX;
                    int cy = op.TileY + 3;
                    op.SendInfoMessage($"pos:{op.TileX},{op.TileY} || {op.TPlayer.position.X},{op.TPlayer.position.Y}");
                    op.SendInfoMessage($"type:{Main.tile[cx, cy].type}");
                    op.SendInfoMessage($"wall:{Main.tile[cx, cy].wall}");
                    op.SendInfoMessage($"frameX:{Main.tile[cx, cy].frameX}");
                    op.SendInfoMessage($"frameY:{Main.tile[cx, cy].frameY}");
                    op.SendInfoMessage($"blockType:{Main.tile[cx, cy].blockType()}");
                    op.SendInfoMessage($"slope:{Main.tile[cx, cy].slope()}");
                    break;
                #endregion

                // 将自己对齐网格
                case "fixtp":
                    if (NeedInGame(op)) return;
                    x = (int)(op.TPlayer.position.X / 16) * 16;
                    y = (int)(op.TPlayer.position.Y / 16) * 16;
                    op.SendInfoMessage($"{op.TPlayer.position.X},{op.TPlayer.position.Y} {x},{y}");
                    op.Teleport(x, y);
                    break;

                #region 重建世界
                case "world":
                case "w":
                    if (NeedWaitTask(op)) return;
                    if (args.Parameters.Count == 1)
                    {
                        op.SendErrorMessage("参数不够，用法如下");
                        op.SendErrorMessage("/igen world <种子> [腐化] [大小] [彩蛋特性], 重建地图");
                        op.SendErrorMessage("种子：输入任意种子名，0表示随机");
                        op.SendErrorMessage("腐化：腐化/猩红 或 1/2, 0表示随机");
                        op.SendErrorMessage("大小：小/中/大 或 1/2/3, 0表示忽略");
                        op.SendErrorMessage("彩蛋特性：种子名中间输入英文逗号，例如 2020,ftw");
                        return;
                    }
                    string seedStr = args.Parameters.Count > 1 ? args.Parameters[1] : "";
                    string evilStr = args.Parameters.Count > 2 ? args.Parameters[2] : "";
                    string sizeStr = args.Parameters.Count > 3 ? args.Parameters[3] : "";

                    string eggStr = "";
                    if (args.Parameters.Count > 4)
                    {
                        args.Parameters.RemoveAt(0);
                        args.Parameters.RemoveAt(0);
                        args.Parameters.RemoveAt(0);
                        args.Parameters.RemoveAt(0);
                        eggStr = string.Join(" ", args.Parameters);
                    }

                    int size = 0;
                    if (sizeStr == "小" || sizeStr == "1")
                        size = 1;
                    else if (sizeStr == "中" || sizeStr == "2")
                        size = 2;
                    else if (sizeStr == "大" || sizeStr == "3")
                        size = 3;

                    int evil = -1;
                    if (evilStr == "腐化" || evilStr == "1")
                        evil = 0;
                    else if (evilStr == "猩红" || evilStr == "2")
                        evil = 1;

                    Regen.GenWorld(op, seedStr, size, evil, eggStr);
                    return;
                #endregion


                #region 玻璃小房间
                case "room":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    int total = 3;
                    if (args.Parameters.Count > 1)
                    {
                        if (!int.TryParse(args.Parameters[1], out total))
                        {
                            op.SendErrorMessage("输入的房间数量不对");
                            return;
                        }
                        if (total < 1 || total > 1000)
                        {
                            total = 3;
                        }
                    }
                    isRight = op.TPlayer.direction != -1;
                    await Regen.AsyncGenRoom(op.TileX, op.TileY + 4, total, isRight, true);
                    return;
                #endregion


                #region 鱼池
                case "pond":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    await Regen.AsyncGenPond(op.TileX, op.TileY + 3);
                    return;
                #endregion


                #region 盾构机
                case "shieldmachine":
                case "sm":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    isRight = op.TPlayer.direction != -1;
                    w = 61;
                    h = 34;
                    if (args.Parameters.Count > 1)
                    {
                        if (int.TryParse(args.Parameters[1], out num))
                            w = Math.Max(3, num);
                    }
                    if (args.Parameters.Count > 2)
                    {
                        if (int.TryParse(args.Parameters[2], out num))
                            h = Math.Max(3, num);
                    }
                    await Regen.AsyncGenShieldMachine(op.TileX, op.TileY + 3, w, h, isRight);
                    return;
                #endregion


                #region 挖掘机
                case "dig":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    isRight = op.TPlayer.direction != -1;
                    w = 3;
                    h = 34;
                    if (utils.TryParseInt(args.Parameters, 1, out num)) w = Math.Max(3, num);
                    if (utils.TryParseInt(args.Parameters, 2, out num)) h = Math.Max(34, num);
                    await Regen.AsyncDigArea(op.TileX, op.TileY + 3, w, h, isRight);
                    return;
                #endregion


                // 地狱直通车
                case "hell":
                    await (Regen.AsyncGenHellevator(op.TileX, op.TileY + 3));
                    Regen.FinishGen();
                    Regen.InformPlayers();
                    op.SendSuccessMessage("创建地狱直通车结束");
                    return;

                #region 填土
                case "dirt":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    await Regen.AsyncPlaceDirt(op.TileX, op.TileY + 3);
                    return;
                #endregion


                #region 清空区域
                case "clear":
                case "c":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    bool clearAll = false;
                    Rectangle rect = new Rectangle(op.TileX - 61, op.TileY - 34 + 2, 122, 68);
                    if (args.Parameters.Count > 1)
                    {
                        if (args.Parameters[1].ToLowerInvariant() == "all") clearAll = true;
                        else if (int.TryParse(args.Parameters[1], out num)) rect.X = op.TileX - Math.Abs(num);
                    }
                    if (utils.TryParseInt(args.Parameters, 2, out num)) rect.Y = op.TileY - Math.Abs(num);
                    if (utils.TryParseInt(args.Parameters, 3, out num)) rect.Width = op.TileX - rect.X + Math.Abs(num);
                    if (utils.TryParseInt(args.Parameters, 4, out num)) rect.Height = op.TileY - rect.Y + Math.Abs(num);
                    await Regen.AsyncClearArea(rect, new Point(op.TileX, op.TileY + 3), clearAll);
                    if (clearAll)
                        op.SendSuccessMessage("已清空全图");
                    else
                        op.SendSuccessMessage("已清空指定区域");
                    Regen.InformPlayers();
                    break;
                #endregion



                #region 物块替换
                case "replace":
                case "re":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    RetileHelper.Init();
                    Regen.AsyncUniReTile(op, 1, needAll);
                    break;
                #endregion


                #region 冰河化
                case "ice":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    Regen.AsyncUniReTile(op, 2, needAll);
                    break;
                #endregion


                #region 冰融化
                case "melt":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    Regen.AsyncUniReTile(op, 3, needAll);
                    break;
                #endregion


                #region 全图沙漠化
                case "egypt":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    //Regen.AsyncDesertWorld(op);
                    op.SendInfoMessage("这个功能还没写好");
                    break;

                case "char":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    //Regen.CharPaint(op);
                    op.SendInfoMessage("这个功能还没写好");
                    break;
                #endregion


                #region 随机
                case "random":
                    if (NeedInGame(op) || NeedWaitTask(op)) return;
                    if (needAll)
                        Regen.AsyncRandomAll(op);
                    else
                        Regen.AsyncRandomArea(3, op);
                    break;
                    #endregion

            }

        }

        public static bool NeedWaitTask(TSPlayer op)
        {
            if (isTaskRunning)
            {
                if (op != null) op.SendErrorMessage("另一个创建任务正在执行，请稍后再操作");
            }
            return isTaskRunning;
        }

        public static bool NeedInGame(TSPlayer op)
        {
            return utils.NeedInGame(op);
        }

    }
}