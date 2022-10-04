using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using TShockAPI;


namespace WorldModify
{
    class BestiaryHelper
    {
        public static string SaveFile;

        public static void Manage(CommandArgs args)
        {
            TSPlayer op = args.Player;

            if (args.Parameters.Count == 0)
            {
                Unlock(op);
                return;
            }


            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "reset":
                    Reset(op);
                    break;

                case "import":
                    Import(op);
                    break;

                case "backup":
                    Backup();
                    op.SendSuccessMessage($"备份完成（{SaveFile}）");
                    break;

                case "help":
                    op.SendInfoMessage("/wm be，解锁 全怪物图鉴");
                    op.SendInfoMessage("/wm be reset，清空 怪物图鉴");
                    op.SendInfoMessage("/wm be import，导入 怪物图鉴");
                    op.SendInfoMessage("/wm be backup，备份 怪物图鉴 到 csv文件，解锁和清空前会自动备份");
                    break;

                default:
                    op.SendSuccessMessage("语法错误，输入 /wm be help 查看用法！");
                    break;
            }

        }

        // 参考：https://github.com/TEdit/Terraria-Map-Editor/blob/d0cd544c2f08ca96b8723257b98d4ba120db81f8/src/TEdit/Terraria/World.FileV2.cs#L111
        static string killedData = "BigHornetStingy,LittleHornetStingy,BigHornetSpikey,LittleHornetSpikey,BigHornetLeafy,LittleHornetLeafy,BigHornetHoney,LittleHornetHoney,BigHornetFatty,LittleHornetFatty,BigRainZombie,SmallRainZombie,BigPantlessSkeleton,SmallPantlessSkeleton,BigMisassembledSkeleton,SmallMisassembledSkeleton,BigHeadacheSkeleton,SmallHeadacheSkeleton,BigSkeleton,SmallSkeleton,BigFemaleZombie,SmallFemaleZombie,DemonEye2,PurpleEye2,GreenEye2,DialatedEye2,SleepyEye2,CataractEye2,BigTwiggyZombie,SmallTwiggyZombie,BigSwampZombie,SmallSwampZombie,BigSlimedZombie,SmallSlimedZombie,BigPincushionZombie,SmallPincushionZombie,BigBaldZombie,SmallBaldZombie,BigZombie,SmallZombie,BigCrimslime,LittleCrimslime,BigCrimera,LittleCrimera,GiantMossHornet,BigMossHornet,LittleMossHornet,TinyMossHornet,BigStinger,LittleStinger,HeavySkeleton,BigBoned,ShortBones,BigEater,LittleEater,JungleSlime,YellowSlime,RedSlime,PurpleSlime,BlackSlime,BabySlime,Pinky,GreenSlime,Slimer2,Slimeling,None,BlueSlime,DemonEye,Zombie,EyeofCthulhu,ServantofCthulhu,EaterofSouls,DevourerHead,DevourerBody,DevourerTail,GiantWormHead,GiantWormBody,GiantWormTail,EaterofWorldsHead,EaterofWorldsBody,EaterofWorldsTail,MotherSlime,Merchant,Nurse,ArmsDealer,Dryad,Skeleton,Guide,MeteorHead,FireImp,BurningSphere,GoblinPeon,GoblinThief,GoblinWarrior,GoblinSorcerer,ChaosBall,AngryBones,DarkCaster,WaterSphere,CursedSkull,SkeletronHead,SkeletronHand,OldMan,Demolitionist,BoneSerpentHead,BoneSerpentBody,BoneSerpentTail,Hornet,ManEater,UndeadMiner,Tim,Bunny,CorruptBunny,Harpy,CaveBat,KingSlime,JungleBat,DoctorBones,TheGroom,Clothier,Goldfish,Snatcher,CorruptGoldfish,Piranha,LavaSlime,Hellbat,Vulture,Demon,BlueJellyfish,PinkJellyfish,Shark,VoodooDemon,Crab,DungeonGuardian,Antlion,SpikeBall,DungeonSlime,BlazingWheel,GoblinScout,Bird,Pixie,None2,ArmoredSkeleton,Mummy,DarkMummy,LightMummy,CorruptSlime,Wraith,CursedHammer,EnchantedSword,Mimic,Unicorn,WyvernHead,WyvernLegs,WyvernBody,WyvernBody2,WyvernBody3,WyvernTail,GiantBat,Corruptor,DiggerHead,DiggerBody,DiggerTail,SeekerHead,SeekerBody,SeekerTail,Clinger,AnglerFish,GreenJellyfish,Werewolf,BoundGoblin,BoundWizard,GoblinTinkerer,Wizard,Clown,SkeletonArcher,GoblinArcher,VileSpit,WallofFlesh,WallofFleshEye,TheHungry,TheHungryII,LeechHead,LeechBody,LeechTail,ChaosElemental,Slimer,Gastropod,BoundMechanic,Mechanic,Retinazer,Spazmatism,SkeletronPrime,PrimeCannon,PrimeSaw,PrimeVice,PrimeLaser,BaldZombie,WanderingEye,TheDestroyer,TheDestroyerBody,TheDestroyerTail,IlluminantBat,IlluminantSlime,Probe,PossessedArmor,ToxicSludge,SantaClaus,SnowmanGangsta,MisterStabby,SnowBalla,None3,IceSlime,Penguin,PenguinBlack,IceBat,Lavabat,GiantFlyingFox,GiantTortoise,IceTortoise,Wolf,RedDevil,Arapaima,VampireBat,Vampire,Truffle,ZombieEskimo,Frankenstein,BlackRecluse,WallCreeper,WallCreeperWall,SwampThing,UndeadViking,CorruptPenguin,IceElemental,PigronCorruption,PigronHallow,RuneWizard,Crimera,Herpling,AngryTrapper,MossHornet,Derpling,Steampunker,CrimsonAxe,PigronCrimson,FaceMonster,FloatyGross,Crimslime,SpikedIceSlime,SnowFlinx,PincushionZombie,SlimedZombie,SwampZombie,TwiggyZombie,CataractEye,SleepyEye,DialatedEye,GreenEye,PurpleEye,LostGirl,Nymph,ArmoredViking,Lihzahrd,LihzahrdCrawler,FemaleZombie,HeadacheSkeleton,MisassembledSkeleton,PantlessSkeleton,SpikedJungleSlime,Moth,IcyMerman,DyeTrader,PartyGirl,Cyborg,Bee,BeeSmall,PirateDeckhand,PirateCorsair,PirateDeadeye,PirateCrossbower,PirateCaptain,CochinealBeetle,CyanBeetle,LacBeetle,SeaSnail,Squid,QueenBee,ZombieRaincoat,FlyingFish,UmbrellaSlime,FlyingSnake,Painter,WitchDoctor,Pirate,GoldfishWalker,HornetFatty,HornetHoney,HornetLeafy,HornetSpikey,HornetStingy,JungleCreeper,JungleCreeperWall,BlackRecluseWall,BloodCrawler,BloodCrawlerWall,BloodFeeder,BloodJelly,IceGolem,RainbowSlime,Golem,GolemHead,GolemFistLeft,GolemFistRight,GolemHeadFree,AngryNimbus,Eyezor,Parrot,Reaper,ZombieMushroom,ZombieMushroomHat,FungoFish,AnomuraFungus,MushiLadybug,FungiBulb,GiantFungiBulb,FungiSpore,Plantera,PlanterasHook,PlanterasTentacle,Spore,BrainofCthulhu,Creeper,IchorSticker,RustyArmoredBonesAxe,RustyArmoredBonesFlail,RustyArmoredBonesSword,RustyArmoredBonesSwordNoArmor,BlueArmoredBones,BlueArmoredBonesMace,BlueArmoredBonesNoPants,BlueArmoredBonesSword,HellArmoredBones,HellArmoredBonesSpikeShield,HellArmoredBonesMace,HellArmoredBonesSword,RaggedCaster,RaggedCasterOpenCoat,Necromancer,NecromancerArmored,DiabolistRed,DiabolistWhite,BoneLee,DungeonSpirit,GiantCursedSkull,Paladin,SkeletonSniper,TacticalSkeleton,SkeletonCommando,AngryBonesBig,AngryBonesBigMuscle,AngryBonesBigHelmet,BirdBlue,BirdRed,Squirrel,Mouse,Raven,SlimeMasked,BunnySlimed,HoppinJack,Scarecrow1,Scarecrow2,Scarecrow3,Scarecrow4,Scarecrow5,Scarecrow6,Scarecrow7,Scarecrow8,Scarecrow9,Scarecrow10,HeadlessHorseman,Ghost,DemonEyeOwl,DemonEyeSpaceship,ZombieDoctor,ZombieSuperman,ZombiePixie,SkeletonTopHat,SkeletonAstonaut,SkeletonAlien,MourningWood,Splinterling,Pumpking,PumpkingBlade,Hellhound,Poltergeist,ZombieXmas,ZombieSweater,SlimeRibbonWhite,SlimeRibbonYellow,SlimeRibbonGreen,SlimeRibbonRed,BunnyXmas,ZombieElf,ZombieElfBeard,ZombieElfGirl,PresentMimic,GingerbreadMan,Yeti,Everscream,IceQueen,SantaNK1,ElfCopter,Nutcracker,NutcrackerSpinning,ElfArcher,Krampus,Flocko,Stylist,WebbedStylist,Firefly,Butterfly,Worm,LightningBug,Snail,GlowingSnail,Frog,Duck,Duck2,DuckWhite,DuckWhite2,ScorpionBlack,Scorpion,TravellingMerchant,Angler,DukeFishron,DetonatingBubble,Sharkron,Sharkron2,TruffleWorm,TruffleWormDigger,SleepingAngler,Grasshopper,ChatteringTeethBomb,CultistArcherBlue,CultistArcherWhite,BrainScrambler,RayGunner,MartianOfficer,ForceBubble,GrayGrunt,MartianEngineer,MartianTurret,MartianDrone,GigaZapper,ScutlixRider,Scutlix,MartianSaucer,MartianSaucerTurret,MartianSaucerCannon,MartianSaucerCore,MoonLordHead,MoonLordHand,MoonLordCore,MartianProbe,MoonLordFreeEye,MoonLordLeechBlob,StardustWormHead,StardustWormBody,StardustWormTail,StardustCellBig,StardustCellSmall,StardustJellyfishBig,StardustJellyfishSmall,StardustSpiderBig,StardustSpiderSmall,StardustSoldier,SolarCrawltipedeHead,SolarCrawltipedeBody,SolarCrawltipedeTail,SolarDrakomire,SolarDrakomireRider,SolarSroller,SolarCorite,SolarSolenian,NebulaBrain,NebulaHeadcrab,NebulaBeast,NebulaSoldier,VortexRifleman,VortexHornetQueen,VortexHornet,VortexLarva,VortexSoldier,ArmedZombie,ArmedZombieEskimo,ArmedZombiePincussion,ArmedZombieSlimed,ArmedZombieSwamp,ArmedZombieTwiggy,ArmedZombieCenx,CultistTablet,CultistDevote,CultistBoss,CultistBossClone,GoldBird,GoldBunny,GoldButterfly,GoldFrog,GoldGrasshopper,GoldMouse,GoldWorm,BoneThrowingSkeleton,BoneThrowingSkeleton2,BoneThrowingSkeleton3,BoneThrowingSkeleton4,SkeletonMerchant,CultistDragonHead,CultistDragonBody1,CultistDragonBody2,CultistDragonBody3,CultistDragonBody4,CultistDragonTail,Butcher,CreatureFromTheDeep,Fritz,Nailhead,CrimsonBunny,CrimsonGoldfish,Psycho,DeadlySphere,DrManFly,ThePossessed,CrimsonPenguin,GoblinSummoner,ShadowFlameApparition,BigMimicCorruption,BigMimicCrimson,BigMimicHallow,BigMimicJungle,Mothron,MothronEgg,MothronSpawn,Medusa,GreekSkeleton,GraniteGolem,GraniteFlyer,EnchantedNightcrawler,Grubby,Sluggy,Buggy,TargetDummy,BloodZombie,Drippler,PirateShip,PirateShipCannon,LunarTowerStardust,Crawdad,Crawdad2,GiantShelly,GiantShelly2,Salamander,Salamander2,Salamander3,Salamander4,Salamander5,Salamander6,Salamander7,Salamander8,Salamander9,LunarTowerNebula,LunarTowerVortex,TaxCollector,GiantWalkingAntlion,GiantFlyingAntlion,DuneSplicerHead,DuneSplicerBody,DuneSplicerTail,TombCrawlerHead,TombCrawlerBody,TombCrawlerTail,SolarFlare,LunarTowerSolar,SolarSpearman,SolarGoop,MartianWalker,AncientCultistSquidhead,AncientLight,AncientDoom,DesertGhoul,DesertGhoulCorruption,DesertGhoulCrimson,DesertGhoulHallow,DesertLamiaLight,DesertLamiaDark,DesertScorpionWalk,DesertScorpionWall,DesertBeast,DesertDjinn,DemonTaxCollector,SlimeSpiked,TheBride,SandSlime,SquirrelRed,SquirrelGold,PartyBunny,SandElemental,SandShark,SandsharkCorrupt,SandsharkCrimson,SandsharkHallow,Tumbleweed,DD2AttackerTest,DD2EterniaCrystal,DD2LanePortal,DD2Bartender,DD2Betsy,DD2GoblinT1,DD2GoblinT2,DD2GoblinT3,DD2GoblinBomberT1,DD2GoblinBomberT2,DD2GoblinBomberT3,DD2WyvernT1,DD2WyvernT2,DD2WyvernT3,DD2JavelinstT1,DD2JavelinstT2,DD2JavelinstT3,DD2DarkMageT1,DD2DarkMageT3,DD2SkeletonT1,DD2SkeletonT3,DD2WitherBeastT2,DD2WitherBeastT3,DD2DrakinT2,DD2DrakinT3,DD2KoboldWalkerT2,DD2KoboldWalkerT3,DD2KoboldFlyerT2,DD2KoboldFlyerT3,DD2OgreT2,DD2OgreT3,DD2LightningBugT3,BartenderUnconscious,WalkingAntlion,FlyingAntlion,LarvaeAntlion,FairyCritterPink,FairyCritterGreen,FairyCritterBlue,ZombieMerman,EyeballFlyingFish,Golfer,GolferRescue,TorchZombie,ArmedTorchZombie,GoldGoldfish,GoldGoldfishWalker,WindyBalloon,BlackDragonfly,BlueDragonfly,GreenDragonfly,OrangeDragonfly,RedDragonfly,YellowDragonfly,GoldDragonfly,Seagull,Seagull2,LadyBug,GoldLadyBug,Maggot,Pupfish,Grebe,Grebe2,Rat,Owl,WaterStrider,GoldWaterStrider,ExplosiveBunny,Dolphin,Turtle,TurtleJungle,BloodNautilus,BloodSquid,GoblinShark,BloodEelHead,BloodEelBody,BloodEelTail,Gnome,SeaTurtle,Seahorse,GoldSeahorse,Dandelion,IceMimic,BloodMummy,RockGolem,MaggotZombie,BestiaryGirl,SporeBat,SporeSkeleton,HallowBoss,TownCat,TownDog,GemSquirrelAmethyst,GemSquirrelTopaz,GemSquirrelSapphire,GemSquirrelEmerald,GemSquirrelRuby,GemSquirrelDiamond,GemSquirrelAmber,GemBunnyAmethyst,GemBunnyTopaz,GemBunnySapphire,GemBunnyEmerald,GemBunnyRuby,GemBunnyDiamond,GemBunnyAmber,HellButterfly,Lavafly,MagmaSnail,TownBunny,QueenSlimeBoss,QueenSlimeMinionBlue,QueenSlimeMinionPink,QueenSlimeMinionPurple,EmpressButterfly,PirateGhost,Princess,TorchGod,ChaosBallTim,VileSpitEaterOfWorlds,GoldenSlime,Deerclops,DeerclopsLeg";
        static List<string> killedKeys = killedData.Split(',').Reverse().ToList();

        static string sightData = "Bird,BirdBlue,BirdRed,Buggy,PartyBunny,ExplosiveBunny,GemBunnyAmethyst,GemBunnyTopaz,GemBunnySapphire,GemBunnyEmerald,GemBunnyRuby,GemBunnyDiamond,GemBunnyAmber,TownBunny,Bunny,CorruptBunny,BunnySlimed,BunnyXmas,GoldBunny,CrimsonBunny,Dolphin,Duck,Duck2,DuckWhite,DuckWhite2,EnchantedNightcrawler,FairyCritterPink,FairyCritterGreen,FairyCritterBlue,Firefly,Frog,GoldFrog,GlowingSnail,CrimsonGoldfish,GoldGoldfish,GoldGoldfishWalker,Goldfish,CorruptGoldfish,GoldfishWalker,Grasshopper,GoldGrasshopper,Grebe,Grebe2,Grubby,LadyBug,GoldLadyBug,MushiLadybug,Lavafly,LightningBug,Maggot,MagmaSnail,Mouse,GoldMouse,Owl,Penguin,PenguinBlack,CorruptPenguin,CrimsonPenguin,Pupfish,Rat,ScorpionBlack,Scorpion,Seagull,Seagull2,Seahorse,GoldSeahorse,SeaTurtle,Sluggy,Snail,GlowingSnail,SeaSnail,SquirrelRed,SquirrelGold,GemSquirrelAmethyst,GemSquirrelTopaz,GemSquirrelSapphire,GemSquirrelEmerald,GemSquirrelRuby,GemSquirrelDiamond,GemSquirrelAmber,Squirrel,Turtle,TurtleJungle,WaterStrider,GoldWaterStrider,Worm,GoldWorm,HellButterfly,EmpressButterfly,Butterfly,GoldButterfly,BlackDragonfly,BlueDragonfly,GreenDragonfly,OrangeDragonfly,RedDragonfly,YellowDragonfly,GoldDragonfly,TruffleWorm";
        static List<string> sightKeys = sightData.Split(',').Reverse().ToList();

        static string chatData = "SleepingAngler,Angler,OldMan,Guide,Merchant,Dryad,BestiaryGirl,TravellingMerchant,Painter,Demolitionist,ArmsDealer,DyeTrader,SkeletonMerchant,Pirate,BoundGoblin,GoblinTinkerer,Nurse,Clothier,BartenderUnconscious,DD2Bartender,BoundMechanic,Mechanic,TownCat,TownDog,WitchDoctor,WebbedStylist,Stylist,TaxCollector,BoundWizard,Wizard,Truffle,Steampunker,TownBunny,PartyGirl,GolferRescue,Golfer,Cyborg,Princess,SantaClaus";
        static List<string> chatKeys = chatData.Split(',').Reverse().ToList();

        private static async void Unlock(TSPlayer op)
        {
            await Task.Run(() =>
            {
                Backup();

                foreach (string key in killedKeys)
                {
                    if (Main.BestiaryTracker.Kills._killCountsByNpcId.ContainsKey(key))
                    {
                        if (Main.BestiaryTracker.Kills._killCountsByNpcId[key] < 50)
                            Main.BestiaryTracker.Kills._killCountsByNpcId[key] = 50;
                    }
                    else
                    {
                        Main.BestiaryTracker.Kills._killCountsByNpcId.Add(key, 50);
                    }
                }

                foreach (string key in sightKeys)
                {
                    if (!Main.BestiaryTracker.Sights._wasNearPlayer.Contains(key))
                        Main.BestiaryTracker.Sights._wasNearPlayer.Add(key);
                }

                foreach (string key in chatKeys)
                {
                    if (!Main.BestiaryTracker.Chats._chattedWithPlayer.Contains(key))
                        Main.BestiaryTracker.Chats._chattedWithPlayer.Add(key);
                }

                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                        Main.BestiaryTracker.OnPlayerJoining(i);
                }

                BestiaryUnlockProgressReport result = Main.GetBestiaryProgressReport();
                op.SendSuccessMessage($"怪物图鉴 已全部解锁 ;-) {result.CompletionAmountTotal}/{result.EntriesTotal}");
            });
        }


        private static async void Reset(TSPlayer op)
        {
            await Task.Run(() =>
            {
                Backup();
                Main.BestiaryTracker.Reset();
                TSPlayer.All.SendData(PacketTypes.WorldInfo);

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                        Main.BestiaryTracker.OnPlayerJoining(i);
                }
                op.SendSuccessMessage("怪物图鉴 已清空，重进游戏后生效");
            });
        }

        private static void Backup()
        {
            // 备份现有的记录
            StringBuilder str = new StringBuilder();
            foreach (var obj in Main.BestiaryTracker.Kills._killCountsByNpcId)
            {
                str.Append($"{obj.Key},{obj.Value}\n");
            }
            foreach (string s in Main.BestiaryTracker.Sights._wasNearPlayer)
            {
                str.Append($"{s}\n");
            }
            foreach (string s in Main.BestiaryTracker.Chats._chattedWithPlayer)
            {
                str.Append($"{s}\n");
            }
            utils.SaveAndBack(SaveFile, str.ToString());
        }


        private static async void Import(TSPlayer op)
        {
            if (!File.Exists(SaveFile))
            {
                op.SendInfoMessage($"{SaveFile}文件不存在，无法导入，解锁/清空 全怪物图鉴 会自动生成该文件。");
                return;
            }
            await Task.Run(() =>
            {
                op.SendInfoMessage("正在导入，请稍等……");
                int count = 0;

                foreach (string s in File.ReadAllLines(SaveFile))
                {
                    string[] arr = s.Split(',');
                    string key = arr[0];
                    if (arr.Length > 1)
                    {
                        if (!killedKeys.Contains(key) || !int.TryParse(arr[1], out int num))
                            continue;

                        if (Main.BestiaryTracker.Kills._killCountsByNpcId.ContainsKey(key))
                            Main.BestiaryTracker.Kills._killCountsByNpcId[key] = num;
                        else
                            Main.BestiaryTracker.Kills._killCountsByNpcId.Add(key, num);
                        count++;
                        continue;
                    }

                    if (sightKeys.Contains(key))
                    {
                        if (!Main.BestiaryTracker.Sights._wasNearPlayer.Contains(key))
                            Main.BestiaryTracker.Sights._wasNearPlayer.Add(key);
                        count++;
                        continue;
                    }
                    if (chatKeys.Contains(key))
                    {
                        if (!Main.BestiaryTracker.Chats._chattedWithPlayer.Contains(key))
                            Main.BestiaryTracker.Chats._chattedWithPlayer.Add(key);
                        count++;
                        continue;
                    }
                }

                op.SendSuccessMessage($"已处理 {count} 个怪物图鉴");
            });
        }

    }
}