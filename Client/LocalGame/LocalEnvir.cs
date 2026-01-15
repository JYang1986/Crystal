using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Client.MirObjects;
using Client.MirScenes;

namespace Client.LocalGame
{
    /// <summary>
    /// 本地游戏环境 - 替代服务端 Envir
    /// 管理所有本地游戏数据、怪物、NPC、地图等
    /// </summary>
    public class LocalEnvir
    {
        private static LocalEnvir _instance;
        public static LocalEnvir Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LocalEnvir();
                return _instance;
            }
        }

        // 数据路径
        public static string DataPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        public static string SavePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
        public static string MapPath => Path.Combine(DataPath, "Maps");

        // 游戏数据
        public Dictionary<int, LocalMapInfo> MapInfoList { get; private set; }
        public Dictionary<int, LocalItemInfo> ItemInfoList { get; private set; }
        public Dictionary<int, LocalMonsterInfo> MonsterInfoList { get; private set; }
        public Dictionary<int, LocalNPCInfo> NPCInfoList { get; private set; }
        public Dictionary<int, LocalMagicInfo> MagicInfoList { get; private set; }

        // 运行时对象
        public Dictionary<uint, MapObject> Objects { get; private set; }
        public List<UserObject> Players { get; private set; }
        public List<MapObject> Monsters { get; private set; }

        // 本地怪物扩展映射
        private Dictionary<uint, LocalMonster> _localMonsters;

        // 当前玩家
        public UserObject LocalPlayer { get; set; }

        // 随机数生成器
        public readonly Random Random = new Random();

        // 游戏时间
        public long GameTime { get; private set; }

        private LocalEnvir()
        {
            MapInfoList = new Dictionary<int, LocalMapInfo>();
            ItemInfoList = new Dictionary<int, LocalItemInfo>();
            MonsterInfoList = new Dictionary<int, LocalMonsterInfo>();
            NPCInfoList = new Dictionary<int, LocalNPCInfo>();
            MagicInfoList = new Dictionary<int, LocalMagicInfo>();

            Objects = new Dictionary<uint, MapObject>();
            Players = new List<UserObject>();
            Monsters = new List<MapObject>();
            _localMonsters = new Dictionary<uint, LocalMonster>();
        }

        /// <summary>
        /// 初始化本地环境
        /// </summary>
        public void Initialize()
        {
            // 确保目录存在
            EnsureDirectories();

            // 加载游戏数据
            LoadGameData();

            // 初始化对象ID生成器
            _nextObjectId = 1;

            // 初始化怪物刷新系统
            LocalMonsterSpawner.Instance.Initialize();
        }

        private void EnsureDirectories()
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            if (!Directory.Exists(MapPath))
                Directory.CreateDirectory(MapPath);
        }

        /// <summary>
        /// 加载游戏数据（从服务端数据库文件）
        /// </summary>
        private void LoadGameData()
        {
            // TODO: 从服务端的 Envir 目录加载数据
            // 这里先创建一些测试数据

            // 加载物品信息
            LoadItemInfo();

            // 加载怪物信息
            LoadMonsterInfo();

            // 加载地图信息
            LoadMapInfo();

            // 加载技能信息
            LoadMagicInfo();
        }

        private void LoadItemInfo()
        {
            // TODO: 从 Server/Envir/ItemInfo.DB 加载
            // 这里先创建一些基础物品
        }

        private void LoadMonsterInfo()
        {
            // TODO: 从 Server/Envir/MonsterInfo.DB 加载
            // 这里先创建一些测试怪物

            // 鸡 (1级小怪)
            MonsterInfoList[1] = new LocalMonsterInfo
            {
                Index = 1,
                Name = "鸡",
                Image = 1, // Monster.Chicken
                AI = 1,
                Level = 1,
                HP = 50,
                ViewRange = 5,
                MinAC = 0, MaxAC = 0,
                MinMAC = 0, MaxMAC = 0,
                MinDC = 2, MaxDC = 5,
                MinMC = 0, MaxMC = 0,
                MinSC = 0, MaxSC = 0,
                AttackSpeed = 1500,
                MoveSpeed = 1200,
                Experience = 10,
                Accuracy = 5,
                Agility = 5,
                Light = 0
            };

            // 鹿 (3级怪物)
            MonsterInfoList[2] = new LocalMonsterInfo
            {
                Index = 2,
                Name = "鹿",
                Image = 2, // Monster.Deer
                AI = 2,
                Level = 3,
                HP = 100,
                ViewRange = 6,
                MinAC = 0, MaxAC = 1,
                MinMAC = 0, MaxMAC = 0,
                MinDC = 5, MaxDC = 10,
                MinMC = 0, MaxMC = 0,
                MinSC = 0, MaxSC = 0,
                AttackSpeed = 1800,
                MoveSpeed = 1500,
                Experience = 25,
                Accuracy = 6,
                Agility = 8,
                Light = 0
            };

            // 钉耙猫 (10级怪物)
            MonsterInfoList[3] = new LocalMonsterInfo
            {
                Index = 3,
                Name = "钉耙猫",
                Image = 12, // Monster.Cat1
                AI = 3,
                Level = 10,
                HP = 300,
                ViewRange = 7,
                MinAC = 2, MaxAC = 4,
                MinMAC = 0, MaxMAC = 1,
                MinDC = 15, MaxDC = 25,
                MinMC = 0, MaxMC = 0,
                MinSC = 0, MaxSC = 0,
                AttackSpeed = 2000,
                MoveSpeed = 1800,
                Experience = 100,
                Accuracy = 10,
                Agility = 10,
                Light = 0
            };

            // 钉耙猫王 (15级怪物)
            MonsterInfoList[4] = new LocalMonsterInfo
            {
                Index = 4,
                Name = "钉耙猫王",
                Image = 13, // Monster.Cat2
                AI = 5,
                Level = 15,
                HP = 500,
                ViewRange = 8,
                MinAC = 4, MaxAC = 7,
                MinMAC = 0, MaxMAC = 2,
                MinDC = 25, MaxDC = 40,
                MinMC = 0, MaxMC = 0,
                MinSC = 0, MaxSC = 0,
                AttackSpeed = 1800,
                MoveSpeed = 1600,
                Experience = 200,
                Accuracy = 12,
                Agility = 12,
                Light = 0
            };

            Console.WriteLine($"[LocalEnvir] 加载了 {MonsterInfoList.Count} 个怪物信息");
        }

        private void LoadMapInfo()
        {
            // TODO: 从 Server/Envir/MapInfo.DB 加载
        }

        private void LoadMagicInfo()
        {
            // TODO: 从 Server/Envir/MagicInfo.DB 加载
        }

        /// <summary>
        /// 更新游戏环境（每帧调用）
        /// </summary>
        public void Update()
        {
            GameTime = CMain.Time;

            // 更新所有玩家（UserObject不需要Process）
            // for (int i = Players.Count - 1; i >= 0; i--)
            // {
            //     Players[i].Process();
            // }

            // 更新怪物刷新系统
            LocalMonsterSpawner.Instance.Update();

            // 更新所有怪物
            for (int i = Monsters.Count - 1; i >= 0; i--)
            {
                var monster = Monsters[i];
                if (monster.Dead)
                {
                    // 移除死亡怪物
                    RemoveMonster(monster.ObjectID);
                    continue;
                }

                // 更新本地怪物AI
                if (_localMonsters.TryGetValue(monster.ObjectID, out LocalMonster localMonster))
                {
                    localMonster.Process();
                }
            }
        }

        /// <summary>
        /// 生成新的对象ID
        /// </summary>
        private uint _nextObjectId;

        public uint NewObjectID()
        {
            return (uint)(_nextObjectId++);
        }

        /// <summary>
        /// 获取物品信息
        /// </summary>
        public LocalItemInfo GetItemInfo(int index)
        {
            return ItemInfoList.TryGetValue(index, out LocalItemInfo info) ? info : null;
        }

        /// <summary>
        /// 获取怪物信息
        /// </summary>
        public LocalMonsterInfo GetMonsterInfo(int index)
        {
            return MonsterInfoList.TryGetValue(index, out LocalMonsterInfo info) ? info : null;
        }

        /// <summary>
        /// 获取技能信息
        /// </summary>
        public LocalMagicInfo GetMagicInfo(int index)
        {
            return MagicInfoList.TryGetValue(index, out LocalMagicInfo info) ? info : null;
        }

        /// <summary>
        /// 添加对象到环境
        /// </summary>
        public void AddObject(MapObject obj)
        {
            if (!Objects.ContainsKey(obj.ObjectID))
            {
                Objects[obj.ObjectID] = obj;

                if (obj is UserObject player)
                    Players.Add(player);
                else if (obj is MonsterObject monster)
                    Monsters.Add(monster);
            }
        }

        /// <summary>
        /// 从环境移除对象
        /// </summary>
        public void RemoveObject(MapObject obj)
        {
            if (Objects.Remove(obj.ObjectID))
            {
                if (obj is UserObject player)
                    Players.Remove(player);
                else if (obj is MonsterObject monster)
                    Monsters.Remove(monster);
            }
        }

        /// <summary>
        /// 清理所有对象
        /// </summary>
        public void ClearObjects()
        {
            Objects.Clear();
            Players.Clear();
            Monsters.Clear();
            _localMonsters.Clear();
        }

        #region 怪物管理

        /// <summary>
        /// 创建怪物
        /// </summary>
        internal MonsterObject CreateMonster(int monsterIndex, Point location)
        {
            var monsterInfo = GetMonsterInfo(monsterIndex);
            if (monsterInfo == null)
            {
                Console.WriteLine($"[LocalEnvir] 怪物信息不存在: {monsterIndex}");
                return null;
            }

            // 创建怪物对象
            uint objectId = NewObjectID();
            MonsterObject monster = new MonsterObject(objectId);

            // 设置基本属性
            monster.Name = monsterInfo.Name;
            monster.CurrentLocation = location;
            monster.MapLocation = location;
            monster.Dead = false;

            // TODO: 加载怪物外观（需要从MonsterInfo设置图像）

            // 添加到地图
            var scene = GameScene.Scene;
            if (scene?.MapControl != null)
            {
                scene.MapControl.AddObject(monster);
            }

            // 创建本地怪物扩展
            LocalMonster localMonster = new LocalMonster(monster, monsterInfo);
            _localMonsters[objectId] = localMonster;

            // 添加到环境
            AddObject(monster);

            Console.WriteLine($"[LocalEnvir] 创建怪物: {monsterInfo.Name} 在 ({location.X}, {location.Y})");
            return monster;
        }

        /// <summary>
        /// 移除怪物
        /// </summary>
        private void RemoveMonster(uint objectId)
        {
            if (_localMonsters.Remove(objectId))
            {
                var obj = Objects.TryGetValue(objectId, out MapObject mapObj) ? mapObj : null;
                if (obj != null)
                {
                    var scene = GameScene.Scene;
                    if (scene?.MapControl != null)
                    {
                        scene.MapControl.RemoveObject(obj);
                    }
                    RemoveObject(obj);
                }
            }
        }

        /// <summary>
        /// 获取本地怪物
        /// </summary>
        internal LocalMonster GetLocalMonster(uint objectId)
        {
            return _localMonsters.TryGetValue(objectId, out LocalMonster monster) ? monster : null;
        }

        /// <summary>
        /// 怪物受到攻击
        /// </summary>
        public void MonsterTakeDamage(uint monsterId, MapObject attacker, int damage)
        {
            if (_localMonsters.TryGetValue(monsterId, out LocalMonster monster))
            {
                monster.TakeDamage(attacker, damage);
            }
        }

        #endregion
    }
}
