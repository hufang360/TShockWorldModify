using System.Collections.Generic;

namespace WorldModify
{
    class ReTileTheme
    {
        public static List<ReTileInfo> GetDesert()
        {
            return new List<ReTileInfo>()
            {
                new ReTileInfo(0, 53), // 土块 → 沙块
                new ReTileInfo(2, 53), // 草块 → 沙块
                new ReTileInfo(199, 53), // 猩红草 → 沙块
                new ReTileInfo(23, 53), // 腐化草 → 沙块
                new ReTileInfo(40, 397), // 粘土块 → 硬化沙块
                new ReTileInfo(1, 396), // 石块 → 沙岩块
                new ReTileInfo(30, 396), // 木材 → 沙岩块
                new ReTileInfo(124, 577), // 木梁 → 沙岩柱
                new ReTileInfo(3, 529), // 高茎草 → 海燕麦
                new ReTileInfo(73, 529), // 高茎草 → 海燕麦
    
                new ReTileInfo(5, -1), // 清除树
                new ReTileInfo(596, -1), // 清除 樱花树
                new ReTileInfo(616, -1), // 清除 黄柳树

                new ReTileInfo(179, 396), // 绿苔藓（石头上） → 沙岩块"
                new ReTileInfo(180, 396), // 棕苔藓（石头上） → 沙岩块"
                new ReTileInfo(181, 396), // 红苔藓（石头上） → 沙岩块"
                new ReTileInfo(182, 396), // 蓝苔藓（石头上） → 沙岩块"
                new ReTileInfo(183, 396), // 紫苔藓（石头上） → 沙岩块"
                new ReTileInfo(381, 396), // 熔岩苔藓（在石头上） → 沙岩块"
                new ReTileInfo(512, 396), // 绿苔藓 (在灰砖上)  → 沙岩块"
                new ReTileInfo(513, 396), // 绿苔藓 (在灰砖上)  → 沙岩块"
                new ReTileInfo(514, 396), // 绿苔藓 (在灰砖上)  → 沙岩块"
                new ReTileInfo(515, 396), // 绿苔藓 (在灰砖上)  → 沙岩块"
                new ReTileInfo(516, 396), // 绿苔藓 (在灰砖上)  → 沙岩块"
                new ReTileInfo(517, 396), // 熔岩苔藓 (在灰砖上) → 沙岩块"
                new ReTileInfo(534, 396), // 氪苔藓  → 沙岩块"
                new ReTileInfo(535, 396), // 氪苔藓 → 沙岩块"
                new ReTileInfo(536, 396), // 氪苔藓 → 沙岩块"
                new ReTileInfo(537, 396), // 氪苔藓 → 沙岩块"
                new ReTileInfo(539, 396), // 氩苔藓 → 沙岩块"
                new ReTileInfo(540, 396), // 氩苔藓 → 沙岩块"
    
                new ReTileInfo(184, -1), // 清除 多种苔藓植株
                new ReTileInfo(52, -1), // 清除 常规藤蔓
                new ReTileInfo(382, -1), // 清除 花蔓藤
                new ReTileInfo(205, -1), // 清除 猩红藤蔓
                new ReTileInfo(549, -1), // 清除 海草

                new ReTileInfo(73, 529), // 高茎草 → 海燕麦

                new ReTileInfo(2, 216, 1,1), // 土墙（天然）→ 硬化沙墙（天然）
                new ReTileInfo(16, 304, 1,1), // 土墙 → 硬化沙墙
    
                new ReTileInfo(196, 304, 1,1), // 土墙（天然） → 硬化沙墙
                new ReTileInfo(197, 304, 1,1), // 土墙（天然） → 硬化沙墙
                new ReTileInfo(198, 304, 1,1), // 土墙（天然） → 硬化沙墙
                new ReTileInfo(199, 304, 1,1), // 土墙（天然） → 硬化沙墙
                new ReTileInfo(261, 235, 1,1), // 岩石土墙 → 硬化沙墙
                new ReTileInfo(270, 235, 1,1), // 洞穴土墙 → 硬化沙墙
                new ReTileInfo(271, 235, 1,1), // 粗糙土墙 → 硬化沙墙
                new ReTileInfo(284, 235, 1,1), // 分层土墙 → 硬化沙墙
                new ReTileInfo(285, 235, 1,1), // 剥落土墙 → 硬化沙墙
                new ReTileInfo(286, 235, 1,1), // 破裂土墙 → 硬化沙墙
                new ReTileInfo(287, 235, 1,1), // 皱曲土墙 → 硬化沙墙

                new ReTileInfo(63, 187, 1,1), // 草墙（天然）→沙岩墙
                new ReTileInfo(66, 187, 1,1), // 草墙→沙岩墙
                new ReTileInfo(65, 235, 1,1), // 花墙（天然）→ 光面沙岩墙
                new ReTileInfo(68, 235, 1,1), // 花墙 → 光面沙岩墙
                new ReTileInfo(1, 187, 1,1), // 石墙 → 沙岩墙
                new ReTileInfo(1, 187, 1,1), // 石墙 → 沙岩墙
                new ReTileInfo(212, 235, 1,1), // 岩石墙3（天然）→ 光面沙岩墙
                new ReTileInfo(213, 235, 1,1), // 岩石墙3（天然）→ 光面沙岩墙
                new ReTileInfo(214, 235, 1,1), // 岩石墙3（天然）→ 光面沙岩墙
                new ReTileInfo(215, 235, 1,1), // 岩石墙3（天然）→ 光面沙岩墙
                new ReTileInfo(54, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(55, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(56, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(57, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(58, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(59, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(61, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(170, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(171, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(185, 235, 1,1), // 洞壁（天然）→ 光面沙岩墙
                new ReTileInfo(27, 34, 1,1),  // 板条墙 → 沙岩砖墙


                new ReTileInfo(59, 396), // 泥块 → 沙岩块
                new ReTileInfo(60, 53), // 丛林草块 → 沙块
                new ReTileInfo(158, 396), // 红木 → 沙岩块
                new ReTileInfo(575, 577), // 红木梁 → 沙岩柱
                new ReTileInfo(61, 529), // 矮丛林植物 → 海燕麦
                new ReTileInfo(74, 529), // 高丛林植物 → 海燕麦
                new ReTileInfo(62, -1), // 清除丛林藤蔓
                new ReTileInfo(233, -1), // 清除丛林植被
                new ReTileInfo(15, 216, 1,1), // 泥墙（天然）→ 硬化沙墙（天然）
                new ReTileInfo(247, 304, 1,1), // 泥墙→ 硬化沙墙
                new ReTileInfo(64, 187, 1,1), // 丛林墙（天然）→沙岩墙
                new ReTileInfo(67, 187, 1,1), // 丛林墙 →沙岩墙
                new ReTileInfo(204, 187, 1,1), // 丛林墙（天然）→沙岩墙
                new ReTileInfo(205, 187, 1,1), // 丛林墙（天然）→沙岩墙
                new ReTileInfo(206, 187, 1,1), // 丛林墙（天然）→沙岩墙
                new ReTileInfo(207, 187, 1,1), // 丛林墙（天然）→沙岩墙
                new ReTileInfo(293, 187, 1,1), // 多叶丛林墙→沙岩墙
                new ReTileInfo(67, 187, 1,1), // 丛林墙 → 沙岩墙
                new ReTileInfo(42, 187, 1,1), // 红木墙 → 砂岩墙
    

                new ReTileInfo(147, 53), // 雪块 → 沙块
                new ReTileInfo(161, 396), // 冰雪块 → 沙岩块
                new ReTileInfo(321, 396), // 针叶木 → 沙岩块
                new ReTileInfo(574, 577), // 针叶木梁 → 沙岩柱
                new ReTileInfo(149, 187, 1,1), // 针叶木墙 → 沙岩墙
                new ReTileInfo(71, 187, 1,1), // 冰雪墙（天然）→ 沙岩墙
                new ReTileInfo(40, 216, 1,1), // 雪墙（天然） → 硬化沙墙（天然）
    
    
                new ReTileInfo(70, 397), // 蘑菇草 → 硬化沙块
                new ReTileInfo(190, 396), // 发光蘑菇 → 沙岩块
                new ReTileInfo(578, 577), // 蘑菇梁 → 沙岩柱
                new ReTileInfo(528, -1), // 清除蘑菇藤蔓
                new ReTileInfo(74, 216, 1,1), // 蘑菇墙 → 硬化沙墙（天然）
                new ReTileInfo(80, 216, 1,1), // 蘑菇墙（天然）→ 硬化沙墙（天然）

    
                new ReTileInfo(368, 397), // 花岗岩块 → 硬化沙块
                new ReTileInfo(576, 577), // 花岗岩柱 → 沙岩柱
                new ReTileInfo(369, 396), // 光面花岗岩块 → 沙岩块
                new ReTileInfo(180, 216, 1,1), // 花岗岩墙（天然） → 硬化沙墙（天然）
                new ReTileInfo(181, 216, 1,1), // 花岗岩墙（天然） → 硬化沙墙（天然）
                new ReTileInfo(184, 216, 1,1), // 花岗岩墙（天然） → 硬化沙墙（天然）
                new ReTileInfo(273, 216, 1,1), // 花岗岩墙（天然） → 硬化沙墙（天然）
    

                new ReTileInfo(367, 397), // 大理石块→ 硬化沙块
                new ReTileInfo(561, 577), // 大理石柱 → 沙岩柱
                new ReTileInfo(178, 216, 1,1), // 大理石墙（天然） → 硬化沙墙（天然）
                new ReTileInfo(179, 235, 1,1), // 光面大理石墙 → 光面沙岩墙
                new ReTileInfo(183, 216, 1,1), // 大理石墙（天然） → 硬化沙墙（天然）
                new ReTileInfo(272, 216, 1,1), // 大理石墙（天然） → 硬化沙墙（天然）
    
    
                //new ReTileInfo(25, 396), // 黑檀石块 → 沙岩块
                //new ReTileInfo(203, 396), // 猩红石块  → 沙岩块
            };
        }
    }
}
