using System;
using System.Drawing;
using Client.MirObjects;
using Client.MirScenes;

namespace Client.LocalGame
{
    /// <summary>
    /// 本地怪物对象扩展
    /// 为客户端 MonsterObject 添加服务端的AI和战斗功能
    /// </summary>
    internal class LocalMonster
    {
        internal MonsterObject _monsterObject;
        private LocalMonsterInfo _monsterInfo;

        // AI状态
        private MonsterAIState _aiState = MonsterAIState.Idle;
        private long _nextActionTime;
        private long _nextAttackTime;
        private long _nextMoveTime;

        // 仇恨系统
        private MapObject _target;
        private int _aggroRange;

        // 战斗属性（运行时）
        public uint CurrentHP;
        public uint MaxHP;

        public LocalMonster(MonsterObject monsterObject, LocalMonsterInfo monsterInfo)
        {
            _monsterObject = monsterObject;
            _monsterInfo = monsterInfo;

            MaxHP = monsterInfo.HP;
            CurrentHP = MaxHP;
            _aggroRange = monsterInfo.ViewRange;

            ResetActionTimes();
        }

        private void ResetActionTimes()
        {
            _nextActionTime = CMain.Time + 500;
            _nextAttackTime = CMain.Time + _monsterInfo.AttackSpeed;
            _nextMoveTime = CMain.Time + _monsterInfo.MoveSpeed;
        }

        /// <summary>
        /// 更新怪物AI（每帧调用）
        /// </summary>
        public void Process()
        {
            if (_monsterObject.Dead) return;

            // 检查是否可以执行动作
            if (CMain.Time < _nextActionTime) return;

            _nextActionTime = CMain.Time + 100;

            switch (_aiState)
            {
                case MonsterAIState.Idle:
                    ProcessIdle();
                    break;
                case MonsterAIState.Patrol:
                    ProcessPatrol();
                    break;
                case MonsterAIState.Chase:
                    ProcessChase();
                    break;
                case MonsterAIState.Attack:
                    ProcessAttack();
                    break;
                case MonsterAIState.Return:
                    ProcessReturn();
                    break;
            }
        }

        #region AI状态处理

        private void ProcessIdle()
        {
            // 搜索目标
            if (FindTarget())
            {
                _aiState = MonsterAIState.Chase;
                return;
            }

            // 随机巡逻
            if (CMain.Time >= _nextMoveTime && LocalEnvir.Instance.Random.Next(100) < 5)
            {
                _aiState = MonsterAIState.Patrol;
            }
        }

        private void ProcessPatrol()
        {
            // 随机移动
            if (CMain.Time >= _nextMoveTime)
            {
                RandomMove();
                _nextMoveTime = CMain.Time + _monsterInfo.MoveSpeed;
            }

            // 检查是否有目标
            if (FindTarget())
            {
                _aiState = MonsterAIState.Chase;
            }
            else if (LocalEnvir.Instance.Random.Next(100) < 10)
            {
                _aiState = MonsterAIState.Idle;
            }
        }

        private void ProcessChase()
        {
            // 检查目标是否有效
            if (_target == null || _target.Dead)
            {
                _target = null;
                _aiState = MonsterAIState.Return;
                return;
            }

            // 检查目标是否在视野内
            if (!IsTargetInRange(_target, _aggroRange * 2))
            {
                _target = null;
                _aiState = MonsterAIState.Return;
                return;
            }

            // 检查是否可以攻击
            if (IsTargetInRange(_target, 1))
            {
                _aiState = MonsterAIState.Attack;
                return;
            }

            // 追逐目标
            if (CMain.Time >= _nextMoveTime)
            {
                MoveTowardsTarget();
                _nextMoveTime = CMain.Time + _monsterInfo.MoveSpeed;
            }
        }

        private void ProcessAttack()
        {
            // 检查目标是否有效
            if (_target == null || _target.Dead)
            {
                _target = null;
                _aiState = MonsterAIState.Return;
                return;
            }

            // 检查目标是否还在攻击范围内
            if (!IsTargetInRange(_target, 1))
            {
                _aiState = MonsterAIState.Chase;
                return;
            }

            // 执行攻击
            if (CMain.Time >= _nextAttackTime)
            {
                PerformAttack();
                _nextAttackTime = CMain.Time + _monsterInfo.AttackSpeed;
            }
        }

        private void ProcessReturn()
        {
            // TODO: 返回初始位置
            _aiState = MonsterAIState.Idle;
        }

        #endregion

        #region AI行为

        private bool FindTarget()
        {
            if (_target != null && !_target.Dead && IsTargetInRange(_target, _aggroRange))
                return true;

            // 搜索最近的敌人
            MapObject nearestEnemy = null;
            int nearestDistance = int.MaxValue;

            // 优先攻击玩家
            if (LocalEnvir.Instance.LocalPlayer != null && !LocalEnvir.Instance.LocalPlayer.Dead)
            {
                int distance = GetDistance(LocalEnvir.Instance.LocalPlayer);
                if (distance <= _aggroRange && distance < nearestDistance)
                {
                    nearestEnemy = LocalEnvir.Instance.LocalPlayer;
                    nearestDistance = distance;
                }
            }

            _target = nearestEnemy;
            return _target != null;
        }

        private bool IsTargetInRange(MapObject target, int range)
        {
            return GetDistance(target) <= range;
        }

        private int GetDistance(MapObject target)
        {
            int dx = Math.Abs(_monsterObject.CurrentLocation.X - target.CurrentLocation.X);
            int dy = Math.Abs(_monsterObject.CurrentLocation.Y - target.CurrentLocation.Y);
            return Math.Max(dx, dy);
        }

        private void RandomMove()
        {
            MirDirection direction = (MirDirection)LocalEnvir.Instance.Random.Next(8);
            Move(direction);
        }

        private void MoveTowardsTarget()
        {
            if (_target == null) return;

            int dx = _target.CurrentLocation.X - _monsterObject.CurrentLocation.X;
            int dy = _target.CurrentLocation.Y - _monsterObject.CurrentLocation.Y;

            MirDirection direction = MirDirection.Up;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                direction = dx > 0 ? MirDirection.Right : MirDirection.Left;
            }
            else
            {
                direction = dy > 0 ? MirDirection.Down : MirDirection.Up;
            }

            Move(direction);
        }

        private void Move(MirDirection direction)
        {
            _monsterObject.Direction = direction;

            var scene = GameScene.Scene;
            if (scene?.MapControl == null) return;

            // 计算新位置
            Point newLocation = GetNextLocation(_monsterObject.CurrentLocation, direction);

            // 简单的位置检查（TODO: 实现完整的碰撞检测）
            // 暂时不检查是否可以移动，直接移动
            scene.MapControl.RemoveObject(_monsterObject);
            _monsterObject.CurrentLocation = newLocation;
            _monsterObject.MapLocation = newLocation;
            scene.MapControl.AddObject(_monsterObject);
            scene.MapControl.FloorValid = false;
        }

        private Point GetNextLocation(Point location, MirDirection direction)
        {
            switch (direction)
            {
                case MirDirection.Up: return new Point(location.X, location.Y - 1);
                case MirDirection.UpRight: return new Point(location.X + 1, location.Y - 1);
                case MirDirection.Right: return new Point(location.X + 1, location.Y);
                case MirDirection.DownRight: return new Point(location.X + 1, location.Y + 1);
                case MirDirection.Down: return new Point(location.X, location.Y + 1);
                case MirDirection.DownLeft: return new Point(location.X - 1, location.Y + 1);
                case MirDirection.Left: return new Point(location.X - 1, location.Y);
                case MirDirection.UpLeft: return new Point(location.X - 1, location.Y - 1);
                default: return location;
            }
        }

        private void PerformAttack()
        {
            if (_target == null) return;

            // 计算伤害
            int damage = CalculateDamage();

            // 对目标造成伤害
            if (_target is UserObject player)
            {
                DamagePlayer(player, damage);
            }

            Console.WriteLine($"[Monster] {_monsterInfo.Name} 攻击了 {_target.Name}，造成 {damage} 点伤害");
        }

        private int CalculateDamage()
        {
            // 随机伤害范围
            int minDC = _monsterInfo.MinDC;
            int maxDC = _monsterInfo.MaxDC;

            if (maxDC <= minDC) maxDC = minDC + 1;

            return LocalEnvir.Instance.Random.Next(minDC, maxDC + 1);
        }

        private void DamagePlayer(UserObject player, int damage)
        {
            // 计算实际伤害（考虑防御） - 使用 Stats 字典
            int minAC = player.Stats[Stat.MinAC];
            int maxAC = player.Stats[Stat.MaxAC];
            int actualDamage = Math.Max(1, damage - (minAC + maxAC) / 2);

            // 应用伤害
            player.HP = (ushort)Math.Max(0, player.HP - actualDamage);

            // TODO: 显示伤害数字、播放受伤动画
        }

        #endregion

        #region 被攻击处理

        /// <summary>
        /// 怪物受到攻击
        /// </summary>
        public void TakeDamage(MapObject attacker, int damage)
        {
            CurrentHP = (uint)Math.Max(0, CurrentHP - damage);

            Console.WriteLine($"[Monster] {_monsterInfo.Name} 受到 {damage} 点伤害，剩余HP: {CurrentHP}/{MaxHP}");

            // 设置攻击者为仇恨目标
            if (_target == null)
            {
                _target = attacker;
                _aiState = MonsterAIState.Chase;
            }

            // 检查死亡
            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Console.WriteLine($"[Monster] {_monsterInfo.Name} 死亡");

            // 标记为死亡
            _monsterObject.Dead = true;

            // 给予击杀者经验
            if (_target != null && _target is UserObject player)
            {
                GiveExperience(player);
            }

            // 掉落物品
            DropItems();

            // TODO: 播放死亡动画、移除对象
        }

        private void GiveExperience(UserObject player)
        {
            ulong exp = _monsterInfo.Experience;
            player.Experience += (long)exp;
            Console.WriteLine($"[Monster] {player.Name} 获得 {exp} 点经验");
        }

        private void DropItems()
        {
            // TODO: 实现物品掉落
            Console.WriteLine($"[Monster] {_monsterInfo.Name} 掉落物品（待实现）");
        }

        #endregion

        // 属性访问器
        public string Name => _monsterInfo.Name;
        public byte AI => _monsterInfo.AI;
        public MonsterAIState AIState => _aiState;
        public bool IsDead => CurrentHP <= 0;
    }

    /// <summary>
    /// 怪物AI状态
    /// </summary>
    public enum MonsterAIState
    {
        Idle,       // 闲置
        Patrol,     // 巡逻
        Chase,      // 追逐
        Attack,     // 攻击
        Return      // 返回
    }
}
