

# 更新日志

---
## 20230107 （v1.4.0.7未发布)
* 增加了图格id查询功能，输入 `/igen <id/名称/关键词/id+>` 进行查询，关键词的匹配模式为首尾匹配，id+为某个id后面的20个数据，例如100+会显示id为101~120的图格。
* 增加了墙id查询功能，输入 `/igen wall <id/名称/关键词/id+>` 进行查询。

## 20230106 （v1.4.0.7未发布)
* 优化世界备份文件名，用`/wm backup [备注]` 生成的文件名为 `world_20230106-1524-55_备注.wld`。

## 20230105 （v1.4.0.7未发布)
* 为确保放置成功，`/igen place` 内定的放置指令，会自动生成垫脚方块。
* 图格名称录入完成。

## 20230104 （v1.4.0.7未发布)
* 新增 `/wm gps <x> <y>` 指令，用于将图格坐标转成GPS信息。
* 显示图格和墙的名称时，会自动显示物品图标以及图格id。

## 20230103 （v1.4.0.7未发布)
* 引入 `Tile.csv` 配置文件，记录有图格的名称以及图格的宽高信息，目前还无法做到区分多style的图格。
* 图格名称以游戏内的为主，也有不少参考了wiki，所以会有很多出入，使用 `/wm docs` 指令可以导出图格名称。
* 相关指令支持用名称代替图格id，例如：`/igen clear`、`/igen replace`、`/igen fill` 和 `/igen stats`。

* `/wm find <id/名称>`，支持全图查找指定图格。
* `/wm find`，支持查找 `巨型生命水晶`、`弹力巨石`、`发光郁金香`、`明胶水晶` 和 `水矢`，详见`/wm find help`。
* `/wm find` 能正确区分 `恶魔祭坛`和`猩红祭坛`，`暗影珠`和`猩红之心`。
* `/wm find` 查找`附魔剑`时，数量比实际多，主要是错误地匹配到`碎石堆`，现已修复。
* 
* `/wm clear <id/名称>`，支持全图清除指定图格。
* `/wm clear`，支持全图清除 `幼虫`、`花苞`、`仙人球`、`尖刺`、`木尖刺`、`马蜂窝`、`TNT枪管`、`巨型生命水晶`、`弹力巨石`等，详见`/wm clear help`。
* 
* `/igen place <id/名称>`，支持放置指定图格。
* `/igen place`，支持放置 `幼虫`、`花苞`、`生命果`、`发光郁金香`等，详见`/igen place help`。

* `/igen count`改名为`/igen stats`，以避免因误操作输入`/igen c`，错误地清除了图格。


---

## 20221112 （v1.4未发布)
* 钻井机的指令名改为`/igen drill`。

## 20221105 （v1.4未发布)
* 修复 `/igen dirt` 操作高度为2屏区域错误。
* `/igen fill` 填充墙体时，不以是否有图格为依据，而是以有没有墙体为准。
  * 支持填充臭臭


## 20221013 （v1.4未发布)
* 新增 `/npc find <ID/名称>` 指令，用于查询NPC详情。
* 新增 `/npc mq` 指令，用于召唤美杜莎boss。
* 新增解锁8种城镇史莱姆（1.4.4新增NPC）。
* 移除 `/npc info` 指令的查询NPC详情功能。
* `/npc clear` 支持使用 NPC ID：
  * `/npc clear enemy` 将清除所有敌怪。
* `/npc tphere` 支持使用 NPC ID。
* `/npc list` 现在会显示 NPC ID。
* `/npc <NPC ID>` 支持使用NPC ID来标记NPC解救进度。


## 20221009 （v1.4未发布)
* 怪物图鉴
  * 支持解锁截止到1.4.4.5的540条图鉴条目。
  * 导出和备份到本地的记录，现在会记录npcid和npc的名称。
  * `/wm bestiary` 指令不再直接解锁全怪物图鉴，而是显示指令用法。
  * `/wm bestiary unlock` 指令解锁全怪物图鉴。

* 物品研究
  * `/wm research` 指令不再直接解锁全物品研究，而是显示指令用法。
  * `/wm research unlock` 指令解锁全物品研究。


## 20221009 （v1.4未发布)
* `/wm find` 指令，支持查找 墓碑、梳妆台 和 最脏的块。

* 新增 `/wm remix` 指令，开关 Remix 彩蛋种子。
* 新增 `/wm "no traps"` 指令，开关 No traps 彩蛋种子。
* 新增 `/wm zenith` 指令，开关 天顶剑 彩蛋种子。
* 新增 `/wm clear` 指令，全图清除图格，清除时不产生物品:
  * `/wm clear help` 指令，查看帮助。
  * `/wm clear tomb` 指令，全图清除墓碑。
  * `/wm clear dress` 指令，全图清除梳妆台。
  * `/cleartomb` 指令，给普通用户使用的全图清除墓碑指令。
* 新增 "/wm moondial" 指令，查看和控制附魔月晷：
  * `/wm moondial <on/off>` 开关附魔月晷。
  * `/wm moondial <天数>` 设置附魔月晷冷却天数。



## 20220522 （v1.4未发布)
改进
`/wm research` 指令，输入 `/wm re help` 获取帮助，全解锁时会自动备份，且支持从csv文件导入，解锁时不会卡服务器；
`/wm bestiary` 指令，输入 `/wm be help` 获取帮助，全解锁时会自动备份，且支持从csv文件导入；
`/wm backup [备注]` 指令，可以给文件添加备注，例如 world.wld.20220521210053_击败巨鹿.bak；

`/igen room`，建造材料改用灰砖、灰砖墙 和 灰砖平台。
`/igen pond`，会给鱼池注满水，建造材料改用灰砖和灰砖平台，砖墙的宽度改为1格。
`/igen sm <w> <h>`，盾构机
`/igen dig <w> <h>`，钻井机
`/igen dirt`，填土


新增
`/igen hell`，创建 地狱直通车
`/wm find [图格名]`，支持查找 附魔剑、花苞、提炼机等图格

`/igen fixtp`，将自己对齐到网格

`/igen re [all]`，图格替换
/igen place 放置

/igen selection help
/igen se 查看区域
/igen se 1，设置区域的 起始点
/igen se 2，设置区域的 结束点
/igen se 0，清空区域

`/igen clear`，清空
`/igen re`，图格替换
`/igen ice`，冰河化
`/igen melt`，冰融化
`/igen water`，注水


`/igen random`，随机
`/igen egypt`，创建沙漠地形
`/igen forest`

## ==================================================

## 20220407 (v1.3)
- /igen 指令，权限 wm.igen：
    - /igen world <种子> [腐化] [大小] [彩蛋特性], 重建地图；
    - /igen room <数量>，生成玻璃小房间（默认生成3个）；
    - /igen pond，生成玻璃鱼池框架；
    - /igen sm <w> <h>，盾构机；
    - /igen dig <w> <h>，钻井机；
    - /igen dirt，填土；
    - /igen clear，清空世界；
    - /igen info，（测试）当前物块信息；
- 完善 /npc tphere <npc名>, 将npc tp到你旁边；

## 20220402 (v1.3)
- /wm backup 备份地图；

## 20220401 (v1.3)
- /npc info 会详细显示城镇NPC情况；
- /npc info <npc名> 可以查看npc数量及其所在坐标；

## 20220329 (v1.3)
- 新增 /gen2 指令，用于重建世界；
- 新增 /wm sundial 指令：
    - /wm sundial <on|off> 开关附魔日晷；
    - /wm sundial <天数> 设置附魔日晷冷却天数；
- /wm mode 指令回归；
- /wm info, 醉酒世界会显示当天的腐化类型；

## 20220325 (v1.3)
- /wm mode 指令回归
  

## 20220323 (v1.3)

新增
- /wm uuid [uuid]，查看/修改 世界uuid；
- /wm ntb，开启/关闭 not the bees 秘密世界；
- /wm spawn，查看 出生点；
- /wm dungeon，查看 地牢点；
- /wm surface [深度]，查看/修改 地表深度；
- /wm cave [深度]，查看/修改 洞穴深度；
- /wm wind，查看 风速；
- /wm bestiary, 解锁 怪物图鉴全收集；
- /wm bestiary reset,  重置 怪物图鉴；


增强
- /wm help 显示内容支持分页显示；
- /wm info 会额外显示更多信息：
  - 时间；
  - 附魔日晷（有状态才显示）；
  - 物品研究进度；
  - 怪物图鉴进度；
  - 出生点；
  - 地牢点；
  - 表层深度；
  - 洞穴深度；
  - 撒旦军队通关难度（有状态才显示）；
  - 入侵（哥布林入侵、海盗入侵、南瓜月、雪人军团、霜月、火星暴乱、撒旦军团）（有状态才显示）；
  - 事件（派对、灯笼夜、流星雨、血月、日食、雨、雷雨、大风天、沙尘暴、史莱姆雨）（有状态才显示）；
  - 杂项2（陨石、圣诞节、万圣节）（有状态才显示）；

变更
- /wm info 和 /wi 不再显示 boss进度；
- /wm info 不再显示 世界id；
- /relivenpc 简写成 /relive；
```


## 20220313 (v1.3)
- /wm info 指令会显示 摧毁的 暗影珠/猩红之心 个数；
- /wm info 指令会显示 摧毁的 祭坛 个数；
- /wm info 指令会显示 腐化/猩红/神圣 百分比；
- /wm info 指令会显示 详细的boss进度（实验）；
- /npc relive 指令会告知 复活了哪些npc；

- 新增 /worldinfo 指令，建议分配给普通用户使用，/worldinfo 基本等同于 /wm info；

- /boss toggle <boss名> 简化为 /boss <boss名>，搭配 /boss list 可查看可用的boss名；
- /npc toggle <npc名> 简化为 /npc <npc名>，搭配 /npc list 可查看可用的npc名；

为防止游客嗅探世界信息，给普通用户使用的指令，请分配权限：
- /worldinfo 指令授权 /group addperm default worldinfo；
- /bossinfo 指令授权 /group addperm default bossinfo；
- /relivenpc 指令授权 /group addperm default relivenpc；



## v1.3
- 新增 /npc unique 指令，城镇NPC去重；
- 新增 /npc relive 指令，复活NPC（根据怪物图鉴记录）；

- /boss toggle <boss名> 简化为 /boss <boss名>;
- /npc toggle <npc名> 简化为 /npc <npc名>;


## 2021124 (v1.2)
- 新增 /bossinfo 指令，无需分配权限，普通用户就能使用；
- 新增 /npc clear 指令，用于清除指定NPC；

- 支持 1.4.3；
- 支持 开启/关闭 the constant 彩蛋世界（/wm dst）；
- 支持 显示和标记 鹿角怪 进度；


## 20210521（v1.1）
新增 /boss 指令，可查看/修改boss进度，以及boss召唤指令备忘；
新增 /npc 指令，可查看npc解救情况，以及npc召唤指令备忘；
新增 /wm 05162021 指令，可标记地图为新的彩蛋种子；
新增 /wm research 指令，可解锁当前地图的物品研究，解锁后需重启服务器；

移除 /helper 指令；
由于支持 05162021标记，插件可能不支持 1.4.0.5 的服务器；


## 20210511
新增 /wm id 指令，可修改/显示 世界id；
新增 /wm boss 指令，可查看boss进度；
新增 /helper 指令，目前可查询召唤boss 和 召唤NPC指令用法；
/wm info，显示的信息更加完整；





- 参考链接
https://terraria.fandom.com/zh/wiki/秘密世界种子
https://terraria.fandom.com/wiki/Secret_world_seeds
https://terraria.fandom.com/wiki/Moon_phase


- 世界大小
4200×1200  小
6400×1800  中
8400×2400  大
1750×900   微小（1.3移动版更新之前生成的世界）


- 月相和月亮样式
8种月相：满月,亏凸月,下弦月,残月,新月,娥眉月,上弦月,盈凸月
9种月亮样式：正常的、火星样式、土星样式、秘银风格、明亮的偏蓝白色、绿色、糖果、金星样式 和 紫色的三重月亮


- BOSS
困难模式之前：史莱姆王、克苏鲁之眼、世界吞噬怪、克苏鲁之脑、蜂王、骷髅王、血肉墙
困难模式：史莱姆皇后、双子魔眼、毁灭者、机械骷髅王、世纪之花、石巨人、光之女皇、猪龙鱼公爵、拜月教邪教徒、月亮领主

史莱姆王		King Slime
克苏鲁之眼		Eye of Cthulhu
世界吞噬怪		Eater of Worlds
克苏鲁之脑		Brain of Cthulhu
蜂王		Queen Bee
骷髅王		Skeletron
血肉墙		Wall of Flesh

史莱姆皇后		Queen Slime
双子魔眼		The Twins
毁灭者		The Destroyer
机械骷髅王		Skeletron Prime
世纪之花		Plantera
石巨人		Golem
光之女皇		Empress of Light
猪龙鱼公爵		Duke Fishron
拜月教邪教徒		Lunatic Cultist
月亮领主		Moon Lord


- NPC
困难模式之前：渔夫、军火商、服装商、爆破专家、树妖、染料商、哥布林工匠、高尔夫球手、向导、机械师、商人、护士、老人、油漆工、派对女孩、骷髅商人、发型师、酒馆老板、旅商、巫医、动物学家
困难模式：机器侠、海盗、公主、圣诞老人、蒸汽朋克人、税收官、松露人、巫师


- 测试dll
T:\TShock\1412\测试插件.bat
```bash
@echo off
set "file1=T:\\TShock\\TerrariaWorldModify\\bin\\Debug\\WorldModify.dll"
set "file2=T:\\TShock\\1412\\tshock-client\\ServerPlugins\\WorldModify.dll"
copy /y "%file1%" "%file2%"

cd /d "T:\\TShock\\1412\\"
run.bat
```

```bash
/group addperm default moonphase moonstyle
```