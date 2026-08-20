# 简易的世界修改器

## 一级指令

| 指令 | 简写 | 权限 | 说明 |
|---|---|---|---|
| /worldmodify | /wm | worldmodify | 简易的世界修改器 |
| /moonphase | /moon | moonphase | 月相管理 |
| /moonstyle | /ms | moonstyle | 月亮样式管理 |
| /bossmanage | /boss | bossmanage | boss管理 |
| /npcmanage | /npc | npcmanage | npc管理 |
| /igen | | igen | 建造世界 |
| /worldinfo | /wi | worldinfo | 世界信息 |
| /bossinfo | /bi | bossinfo | boss进度信息 |
| /relive | | relive | 复活NPC |
| /cleartomb | /ct | cleartomb | 清理墓碑 |

## 指令用法

```
/wm info，查看世界信息；
/wm version，查看 本插件版本；

/wm name [世界名]，查看/修改 世界名字；
/wm id [id]，查看/修改 世界ID；
/wm uuid [uuid]，查看/修改 世界uuid；
/wm mode [1~4/经典/专家/大师/旅行]，查看/修改 世界难度；
/wm seed [种子]，查看/修改 世界种子；

# 秘密世界/特性开关
/wm 2020，开启/关闭 05162020 秘密世界；
/wm 2021，开启/关闭 05162021 秘密世界；
/wm ftw，开启/关闭 for the worthy 秘密世界；
/wm ntb，开启/关闭 not the bees 秘密世界；
/wm dst，开启/关闭 the constant 秘密世界；

/wm secret help
/wm s <remix/nt/zenith/sky/vampire/infected/team/dual/rain/full> [on/off]，秘密世界/特性开关；
/wm s random，随机开启一个秘密世界特性（2020/2021/rain/full，随机到已开启的特性会自动跳过，最多随机3次）；

/wm research，查看 物品研究用法；
/wm re unlock，解锁 全物品研究；
/wm re <id/名称>，研究单个物品；
/wm re import，导入 物品研究；
/wm re reset，重置 当前世界 物品研究；
/wm re clear，清空 物品研究（所有地图）；
/wm re backup，备份 物品研究 到csv文件；

/wm bestiary，查看 怪物图鉴用法；
/wm be unlock，解锁 全怪物图鉴；
/wm be <id/名称>，解锁 单条图鉴记录；
/wm be reset，重置 怪物图鉴；
/wm be import，导入 怪物图鉴；
/wm be backup，备份 怪物图鉴 到csv文件；

/wm backup [备注]，备份地图（别名 /wm save）；

/wm spawn，查看 出生点；
/wm dungeon，查看 地牢点；
/wm surface [深度]，查看/修改 地表深度；
/wm cave [深度]，查看/修改 洞穴深度；
/wm wind，查看 风速；

/wm find，全图查找图格；
/wm f <id/名称>，查找 指定图格（未能区分多样式图格，不计算家具、制作站等的体积，实际数值要除于体积）；
/wm f tomb，查找 墓碑（全类型）；
/wm f dress，查找 梳妆台（全类型）；
/wm f sword，查找 附魔剑；
/wm f life，查找 生命水晶；
/wm f lf，查找 生命果；
/wm f lcb，查找 巨型生命水晶；
/wm f bb，查找 弹力巨石；
/wm f wb，查找 水矢；
/wm f gc，查找 明胶水晶；
/wm f orb，查找 暗影珠；
/wm f heart，查找 猩红之心；
/wm f demon，查找 恶魔祭坛；
/wm f crimson，查找 猩红祭坛；
/wm f la，查找 丛林蜥蜴祭坛；
/wm f hf，查找 地狱熔炉；
/wm f larva，查找 幼虫；
/wm f bulb，查找 花苞；
/wm f ex，查找 提炼机；
/wm f loom，查找 织布机；
/wm f dirtiest，查找 最脏的块；
/wm f tulip，查找 发光郁金香；
/wm f digtoise，查找 碎岩龟；
/wm f egg，查找 巨型龙蛋（疾旋鼬）；
/wm f boulder，查找 友好巨石；
/wm f rainbow，查找 彩虹巨石；
/wm f <森林/丛林/神圣/洞穴/海洋/沙漠/雪原/蘑菇/万能/地狱/以太 晶塔>，查找 晶塔；

/wm clear，全图清理图格；
/wm c <id/名称>，全图清除 指定图格（未能区分多样式图格）；
/wm c tomb，全图清除 墓碑（全样式）；
/wm c dress，全图清除 梳妆台（全样式）；
/wm c larva，全图清除 幼虫；
/wm c bulb，全图清除 花苞；
/wm c rolling，全图清除 仙人球；
/wm c hive，全图清除 马蜂窝；
/wm c tnt，全图清除 TNT枪管；
/wm c lcb，全图清除 巨型生命水晶；
/wm c bb，全图清除 弹力巨石；
/wm c spike，全图清除 尖刺；
/wm c ws，全图清除 木尖刺；


/wm e stone [on/off]，查看/开关 巨石雨（临时开启 Constant 种子 + 暴风雨）；
/wm e slime [on/off]，查看/开关 史莱姆雨；
/wm e windy [on/off]，查看/开关 大风天（白天 7:30~16:30 生效）；
/wm e storm [on/off]，查看/开关 暴风雨；
/wm e party [on/off]，查看/开关 派对；
/wm e clear，风和日丽（停雨/无风/清云/时间调到08:15/跳过入侵/停止事件）；
/wm e skip，跳过 入侵；
（雨/沙尘暴/血月/日食/灯笼夜/流星雨/陨石/入侵 等功能，请使用 /worldevent 指令）


/wm gps <x> <y>，将图格坐标转成GPS信息（有少许误差）；
/wm sundial 查看 附魔日晷；
/wm moondial 查看 附魔月晷；
/wm sd [on/off/天数]，开关 附魔日晷 / 修改 冷却天数；
/wm md [on/off/天数]，开关 附魔月晷 / 修改 冷却天数；

/moon <月相>，修改月相；
/moonstyle <月亮样式>，修改月亮样式；


/boss，boss管理；
/boss info，查看boss进度；
/boss <boss名>，切换boss击败状态；
/boss list，查看支持切换击败状态的boss名；
/boss sb，sb 召唤指令备注；


/npc，npc管理；
/npc info，查看npc解救情况；
/npc <解救npc名 或 猫/狗/兔 >，切换NPC解救状态；
/npc list，查看支持切换击败状态的boss名；
/npc find <id/名称>，查询指定NPC的信息；
/npc clear <NPC名>，移除一个NPC；
/npc clear enemy，清除所有敌怪（保留友善NPC）；
/npc unique，移除重复NPC；
/npc tphere <NPC名|town>，将NPC传送到你身边（town 为所有城镇NPC，简写 /npc th）；
/npc relive，复活NPC（根据怪物图鉴记录）；
/npc gohome，让NPC回家（简写 /npc gh）；
/npc mq true，召唤美杜莎boss（三合一机械boss，需输入 true 确认）；
/npc demo，召唤几个NPC（向导、商人、护士、军火商。服主测试用）；
/npc sm，sm召唤指令备注（SpawnMob npc召唤指令）；


/igen world [种子] [腐化] [大小] [彩蛋特性], 重建地图（无需进入游戏）；（泰拉1.4.5后本插件有问题，暂时不知道原因，不建议使用）
/igen random true，全图图格和背景墙随机（无需进入游戏）；
/igen stats help，统计工具（选区内，无需进入游戏时统计整个世界的图格）；

# 下面的 /igen 指令均需进入游戏（在游戏内使用）：
/igen room <数量>，生成玻璃小房间（默认生成3个）；
/igen hotel，NPC小旅馆；
/igen pond [water/lava/honey/shimmer/main/full]，生成玻璃鱼池；
/igen hell，地狱直通车；
/igen we，水电梯（WaterElevator）；
/igen sm <w> <h>，盾构机（默认清空前方宽61高34区域）；
/igen drill <w> <h>，钻井机（默认清空脚下宽3高34区域）；
/igen dirt，填土（一个屏幕范围内，脚下部分填充土块，上面的部分会被清空）；
/igen hole，打洞（清出一块站立区域）；

/igen selection help，选区工具；
/igen s <all/0/1>，选择 全图/ 一屏 / 红电线自定义
/igen replace help，替换工具（选区内）；
/igen fill help，填充工具（选区内）；
/igen clear help，清除工具（选区内）；
/igen copy <名称>，复制脚下图格到剪贴板；
/igen paste <名称>，粘贴剪贴板图格；

/igen place，放置图格；
/igen p <id/名称> [样式]，放置 指定图格（不一定成功）；
/igen p lc，放置 生命水晶；
/igen p lf，放置 生命果；
/igen p sword，放置 附魔剑；
/igen p orb，放置 暗影珠；
/igen p heart，放置 猩红之心；
/igen p demon，放置 恶魔祭坛；
/igen p crimson，放置 猩红祭坛；
/igen p altar，放置 祭坛（自动判断腐化类型）；
/igen p bulb，放置 花苞；
/igen p larva，放置 幼虫；
/igen p tulip，放置 发光郁金香；
/igen p digtoise，放置 碎岩龟；
/igen p egg，放置 巨型龙蛋（疾旋鼬）；
/igen p pot，放置 罐子；
/igen p stone，放置 友好巨石；
/igen p rainbow，放置 彩虹巨石；


/worldinfo，查看世界信息（分配 worldinfo 权限后可用，简写 /wi）；
/bossinfo，查看boss进度（分配 bossinfo 权限后可用，简写 /bi）；
/relive，复活入住过的NPC（分配 relive 权限后可用）；
/cleartomb，清理全部墓碑（分配 cleartomb 权限后可用，简写 /ct）；
```


<br/>

## 权限

普通用户使用需分配权限

```bash
/group addperm default bossinfo
/group addperm default worldinfo
/group addperm default relive
/group addperm default cleartomb

/group addperm default worldmodify
/group addperm default igen
/group addperm default moonphase
/group addperm default moonstyle
/group addperm default bossmanage
/group addperm default npcmanage
```

<br/>

## /moonphase，/moonstyle，切换 月相和月亮样式 指令

- 8种月相：满月、亏凸月、下弦、残月、新月、娥眉月、上弦月、盈凸月；

- 9种月亮样式：正常的、火星样式、土星样式、秘银风格、明亮的偏蓝白色、绿色、糖果、金星样式 和 紫色的三重月亮；

```bash
# 切换至满月，moonphase指令可以缩写成 moon
/moon 1

# 切换至 秘银风格，moonstyle指令可以缩写成 ms
/moonstyle 4
```
