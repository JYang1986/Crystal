using System;
using System.Collections.Generic;
using System.Drawing;
using Client.MirObjects;

namespace Client.LocalGame
{
    /// <summary>
    /// 怪物刷新点
    /// </summary>
    public class MonsterSpawnPoint
    {
        public int MonsterIndex { get; set; }
        public Point Location { get; set; }
        public int Count { get; set; }
        public int SpawnRange { get; set; }
        public long RespawnDelay { get; set; } // 重生延迟（毫秒）
        public bool Respawn { get; set; } // 是否重生

        private long _lastSpawnTime;
        private int _currentCount;
        private List<uint> _spawnedMonsters;

        public MonsterSpawnPoint()
        {
            _spawnedMonsters = new List<uint>();
        }

        public MonsterSpawnPoint(int monsterIndex, Point location, int count, int range = 10, long respawnDelay = 30000, bool respawn = true)
        {
            MonsterIndex = monsterIndex;
            Location = location;
            Count = count;
            SpawnRange = range;
            RespawnDelay = respawnDelay;
            Respawn = respawn;

            _spawnedMonsters = new List<uint>();
            _lastSpawnTime = 0;
            _currentCount = 0;
        }

        /// <summary>
        /// 检查是否需要刷新怪物
        /// </summary>
        public bool ShouldSpawn()
        {
            if (!Respawn) return false;

            // 检查是否达到最大数量
            CleanDeadMonsters();
            if (_currentCount >= Count) return false;

            // 检查重生时间
            return CMain.Time - _lastSpawnTime >= RespawnDelay;
        }

        /// <summary>
        /// 刷新怪物
        /// </summary>
        internal List<MonsterObject> Spawn()
        {
            List<MonsterObject> spawned = new List<MonsterObject>();

            CleanDeadMonsters();
            int needSpawn = Count - _currentCount;

            for (int i = 0; i < needSpawn; i++)
            {
                // 随机位置
                Point spawnLocation = GetRandomSpawnLocation();

                var monster = LocalEnvir.Instance.CreateMonster(MonsterIndex, spawnLocation);
                if (monster != null)
                {
                    spawned.Add(monster);
                    _spawnedMonsters.Add(monster.ObjectID);
                    _currentCount++;
                }
            }

            _lastSpawnTime = CMain.Time;
            return spawned;
        }

        /// <summary>
        /// 获取随机刷新位置
        /// </summary>
        private Point GetRandomSpawnLocation()
        {
            if (SpawnRange <= 0)
                return Location;

            var random = LocalEnvir.Instance.Random;
            int offsetX = random.Next(-SpawnRange, SpawnRange + 1);
            int offsetY = random.Next(-SpawnRange, SpawnRange + 1);

            return new Point(Location.X + offsetX, Location.Y + offsetY);
        }

        /// <summary>
        /// 清理已死亡的怪物
        /// </summary>
        private void CleanDeadMonsters()
        {
            List<uint> aliveMonsters = new List<uint>();

            foreach (uint monsterId in _spawnedMonsters)
            {
                if (LocalEnvir.Instance.Objects.TryGetValue(monsterId, out MapObject obj))
                {
                    if (!obj.Dead)
                    {
                        aliveMonsters.Add(monsterId);
                    }
                }
            }

            _spawnedMonsters = aliveMonsters;
            _currentCount = _spawnedMonsters.Count;
        }

        /// <summary>
        /// 清除所有怪物
        /// </summary>
        public void Clear()
        {
            _spawnedMonsters.Clear();
            _currentCount = 0;
            _lastSpawnTime = CMain.Time;
        }
    }

    /// <summary>
    /// 怪物刷新管理器
    /// </summary>
    public class LocalMonsterSpawner
    {
        private static LocalMonsterSpawner _instance;
        public static LocalMonsterSpawner Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LocalMonsterSpawner();
                return _instance;
            }
        }

        private List<MonsterSpawnPoint> _spawnPoints;

        private LocalMonsterSpawner()
        {
            _spawnPoints = new List<MonsterSpawnPoint>();
        }

        /// <summary>
        /// 初始化刷新点
        /// </summary>
        public void Initialize()
        {
            _spawnPoints.Clear();

            // 添加默认刷新点（新手村附近）
            // 鸡群
            _spawnPoints.Add(new MonsterSpawnPoint(1, new Point(330, 330), 5, 10, 30000, true));

            // 鹿群
            _spawnPoints.Add(new MonsterSpawnPoint(2, new Point(350, 350), 3, 15, 60000, true));

            // 钉耙猫（更远）
            _spawnPoints.Add(new MonsterSpawnPoint(3, new Point(300, 300), 2, 5, 120000, true));

            Console.WriteLine($"[MonsterSpawner] 初始化了 {_spawnPoints.Count} 个刷新点");
        }

        /// <summary>
        /// 更新刷新系统
        /// </summary>
        public void Update()
        {
            foreach (var spawnPoint in _spawnPoints)
            {
                if (spawnPoint.ShouldSpawn())
                {
                    var spawned = spawnPoint.Spawn();
                    if (spawned.Count > 0)
                    {
                        var monsterInfo = LocalEnvir.Instance.GetMonsterInfo(spawnPoint.MonsterIndex);
                        Console.WriteLine($"[MonsterSpawner] 刷新了 {spawned.Count} 个 {monsterInfo?.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// 添加刷新点
        /// </summary>
        public void AddSpawnPoint(MonsterSpawnPoint spawnPoint)
        {
            _spawnPoints.Add(spawnPoint);
        }

        /// <summary>
        /// 清除所有刷新点
        /// </summary>
        public void Clear()
        {
            _spawnPoints.Clear();
        }

        /// <summary>
        /// 获取刷新点数量
        /// </summary>
        public int GetSpawnPointCount()
        {
            return _spawnPoints.Count;
        }

        /// <summary>
        /// 立即刷新所有刷新点
        /// </summary>
        public void ForceSpawnAll()
        {
            int totalSpawned = 0;
            foreach (var spawnPoint in _spawnPoints)
            {
                var spawned = spawnPoint.Spawn();
                totalSpawned += spawned.Count;
            }
            Console.WriteLine($"[MonsterSpawner] 强制刷新了 {totalSpawned} 个怪物");
        }
    }
}
