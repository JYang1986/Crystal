using System;
using System.Drawing;
using Client.MirObjects;
using Client.MirScenes;

namespace Client.LocalGame
{
    /// <summary>
    /// 本地战斗系统
    /// 处理玩家攻击、伤害计算、战斗交互等
    /// </summary>
    public class LocalCombatSystem
    {
        private static LocalCombatSystem _instance;
        public static LocalCombatSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LocalCombatSystem();
                return _instance;
            }
        }

        private UserObject _player;
        private long _lastAttackTime;
        private const long ATTACK_COOLDOWN = 1000; // 1秒攻击间隔

        private LocalCombatSystem()
        {
        }

        /// <summary>
        /// 初始化战斗系统
        /// </summary>
        public void Initialize(UserObject player)
        {
            _player = player;
            _lastAttackTime = 0;
        }

        /// <summary>
        /// 玩家攻击
        /// </summary>
        public bool PlayerAttack(Point targetLocation)
        {
            if (_player == null)
            {
                Console.WriteLine("[Combat] 玩家未初始化");
                return false;
            }

            // 检查攻击冷却
            if (CMain.Time - _lastAttackTime < ATTACK_COOLDOWN)
            {
                Console.WriteLine("[Combat] 攻击冷却中");
                return false;
            }

            // 检查攻击范围
            int distance = GetDistance(_player.CurrentLocation, targetLocation);
            if (distance > 1) // 近战攻击范围为1
            {
                Console.WriteLine($"[Combat] 目标太远: {distance}");
                return false;
            }

            // 查找目标位置的怪物
            MapObject target = FindTargetAt(targetLocation);
            if (target == null)
            {
                Console.WriteLine("[Combat] 目标位置没有怪物");
                return false;
            }

            // 执行攻击
            _lastAttackTime = CMain.Time;
            PerformAttack(target);
            return true;
        }

        /// <summary>
        /// 玩家攻击指定怪物
        /// </summary>
        public bool PlayerAttack(MapObject target)
        {
            if (target == null || target.Dead)
            {
                Console.WriteLine("[Combat] 目标无效或已死亡");
                return false;
            }

            return PlayerAttack(target.CurrentLocation);
        }

        /// <summary>
        /// 执行攻击
        /// </summary>
        private void PerformAttack(MapObject target)
        {
            // 计算伤害
            int damage = CalculatePlayerDamage();

            Console.WriteLine($"[Combat] {_player.Name} 攻击 {target.Name}");

            // 对目标造成伤害
            if (target is MonsterObject monster)
            {
                DamageMonster(monster, damage);
            }
            else if (target is UserObject player)
            {
                DamagePlayer(player, damage);
            }

            // TODO: 播放攻击动画、音效
        }

        /// <summary>
        /// 计算玩家伤害
        /// </summary>
        private int CalculatePlayerDamage()
        {
            // 获取玩家属性 - 从 Stats 字典读取
            int minDC = _player.Stats[Shared.Data.Stat.MinDC];
            int maxDC = _player.Stats[Shared.Data.Stat.MaxDC];

            // 确保有伤害范围
            if (maxDC <= minDC)
                maxDC = minDC + 1;

            // 随机伤害
            int baseDamage = LocalEnvir.Instance.Random.Next(minDC, maxDC + 1);

            // 考虑准确度
            int accuracy = _player.Stats[Shared.Data.Stat.Accuracy];
            // TODO: 考虑目标的敏捷

            // 随机浮动 (80% - 120%)
            float variance = 0.8f + (float)(LocalEnvir.Instance.Random.NextDouble() * 0.4f);
            int finalDamage = (int)(baseDamage * variance);

            return Math.Max(1, finalDamage);
        }

        /// <summary>
        /// 对怪物造成伤害
        /// </summary>
        private void DamageMonster(MonsterObject monster, int damage)
        {
            // 计算怪物防御
            var localMonster = LocalEnvir.Instance.GetLocalMonster(monster.ObjectID);
            if (localMonster == null)
            {
                Console.WriteLine("[Combat] 无法找到本地怪物数据");
                return;
            }

            // 获取怪物防御
            // TODO: 从 LocalMonsterInfo 获取防御数据
            int defense = 0; // 暂时设为0

            // 计算最终伤害
            int actualDamage = Math.Max(1, damage - defense);

            // 对怪物造成伤害
            LocalEnvir.Instance.MonsterTakeDamage(monster.ObjectID, _player, actualDamage);

            Console.WriteLine($"[Combat] 对 {monster.Name} 造成 {actualDamage} 点伤害");
        }

        /// <summary>
        /// 对玩家造成伤害（PVP）
        /// </summary>
        private void DamagePlayer(UserObject player, int damage)
        {
            // 计算玩家防御 - 从 Stats 字典读取
            int minAC = player.Stats[Shared.Data.Stat.MinAC];
            int maxAC = player.Stats[Shared.Data.Stat.MaxAC];
            int defense = (minAC + maxAC) / 2;

            // 计算最终伤害
            int actualDamage = Math.Max(1, damage - defense);

            // 应用伤害
            player.HP = (ushort)Math.Max(0, player.HP - actualDamage);

            Console.WriteLine($"[Combat] 对 {player.Name} 造成 {actualDamage} 点伤害");
        }

        /// <summary>
        /// 查找指定位置的目标
        /// </summary>
        private MapObject FindTargetAt(Point location)
        {
            var scene = GameScene.Scene;
            if (scene?.MapControl == null)
                return null;

            // 搜索地图上的对象
            foreach (var obj in LocalEnvir.Instance.Objects.Values)
            {
                if (obj == null || obj.Dead)
                    continue;

                if (obj.CurrentLocation == location)
                {
                    // 优先攻击怪物
                    if (obj is MonsterObject)
                        return obj;
                    // 其次是玩家（PVP）
                    if (obj is UserObject && obj != _player)
                        return obj;
                }
            }

            return null;
        }

        /// <summary>
        /// 计算两点距离
        /// </summary>
        private int GetDistance(Point a, Point b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return Math.Max(dx, dy);
        }

        /// <summary>
        /// 检查是否可以攻击
        /// </summary>
        public bool CanAttack(MapObject target)
        {
            if (_player == null || target == null || target.Dead)
                return false;

            int distance = GetDistance(_player.CurrentLocation, target.CurrentLocation);
            return distance <= 1 && CMain.Time - _lastAttackTime >= ATTACK_COOLDOWN;
        }

        /// <summary>
        /// 获取攻击冷却进度 (0-1)
        /// </summary>
        public float GetAttackCooldown()
        {
            long elapsed = CMain.Time - _lastAttackTime;
            return Math.Min(1f, (float)elapsed / ATTACK_COOLDOWN);
        }
    }
}
