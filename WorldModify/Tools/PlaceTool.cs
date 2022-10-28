using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace WorldModify
{
    /// <summary>
    /// 替换工具
    /// </summary>
    class PlaceTool
    {
        public static void Manage(CommandArgs args)
        {
            args.Parameters.RemoveAt(0);
            TSPlayer op = args.Player;
            void Help()
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, op, out int pageNumber))
                    return;

                List<string> lines = new()
                {
                    "/igen p <id> [style]，放置",
                };

                PaginationTools.SendPage(op, pageNumber, lines, new PaginationTools.Settings
                {
                    HeaderFormat = "/igen place 指令用法 ({0}/{1})：",
                    FooterFormat = "输入 /igen p help {{0}} 查看更多".SFormat(Commands.Specifier)
                });
            }
            if (args.Parameters.Count == 0)
            {
                Help();
                return;
            }

            string kw = args.Parameters[0].ToLowerInvariant();
            switch (kw)
            {
                case "help": Help(); break;

                default:
                    int tileID;
                    int tileStyle = 0;
                    if (int.TryParse(kw, out tileID))
                    {
                        if (tileID >= 0 && tileID < TileID.Count)
                        {
                        }
                        else
                        {
                            op.SendErrorMessage($"图格id，有效值为 0~{TileID.Count - 1}");
                            return;
                        }

                        if (args.Parameters.Count > 1)
                        {
                            if (!int.TryParse(args.Parameters[1], out tileStyle))
                            {
                                op.SendErrorMessage("图格样式输入错误！");
                                return;
                            }
                        }
                        int x = op.TileX;
                        int y = op.TileY + 2;
                        WorldGen.PlaceTile(x, y, tileID, false, true, -1, tileStyle);
                        NetMessage.SendTileSquare(-1, x, y, 20);
                        //TileHelper.GenAfter();
                        op.SendSuccessMessage($"放置完成 id={tileID} 样式={tileStyle}");
                    }
                    else
                    {
                        Help();
                    }
                    break;
            }
        }
    }
}