using System;
using System.Drawing;
using Client.MirObjects;
using Client.MirScenes;

namespace Client.LocalGame
{
    /// <summary>
    /// 本地模式战斗处理器
    /// 处理本地游戏中的攻击、伤害计算等战斗逻辑
    /// </summary>
    public static class LocalCombatHandler
    {
        /// <summary>
        /// 处理普通攻击
        /// </summary>
        public static void HandleAttack(UserObject user, MirDirection direction, Spell spell)
        {
            if (user == null) return;

            user.Direction = direction;
            user.Spell = spell;

            // 计算攻击时间
            GameScene.AttackTime = CMain.Time + user.AttackSpeed;
            MapControl.NextAction = CMain.Time + 2500;

            // 查找攻击目标
            var target = FindTarget(user, direction);
            if (target != null)
            {
                // 计算并造成伤害
                int damage = CalculateDamage(user, target);
                DealDamage(target, damage);

                Console.WriteLine($"[本地模式] 攻击 {target.Name}，造成 {damage} 点伤害");
            }
            else
            {
                Console.WriteLine($"[本地模式] 攻击方向 {direction}，没有目标");
            }

            // 清除技能状态
            if (spell == Spell.Slaying)
                GameScene.User.Slaying = false;
            if (spell == Spell.TwinDrakeBlade)
                GameScene.User.TwinDrakeBlade = false;
            if (spell == Spell.FlamingSword)
                GameScene.User.FlamingSword = false;
        }

        /// <summary>
        /// 处理远程攻击
        /// </summary>
        public static void HandleRangeAttack(UserObject user, MirDirection direction, Point targetLocation, uint targetID)
        {
            if (user == null) return;

            user.Direction = direction;

            // 计算攻击时间
            GameScene.AttackTime = CMain.Time + user.AttackSpeed + 200;
            MapControl.NextAction = CMain.Time + 2500;

            // 查找目标
            var target = FindObjectByID(targetID);
            if (target != null)
            {
                int damage = CalculateDamage(user, target);
                DealDamage(target, damage);

                Console.WriteLine($"[本地模式] 远程攻击 {target.Name}，造成 {damage} 点伤害");
            }
        }

        /// <summary>
        /// 查找攻击目标
        /// </summary>
        private static MapObject FindTarget(UserObject user, MirDirection direction)
        {
            try
            {
                var mapControl = GameScene.Scene?.MapControl;
                if (mapControl == null) return null;

                // 检查当前方向前方1格的位置
                Point targetLocation = Functions.PointMove(user.CurrentLocation, direction, 1);

                // 获取该位置的对象
                if (targetLocation.X >= 0 && targetLocation.X < mapControl.Width &&
                    targetLocation.Y >= 0 && targetLocation.Y < mapControl.Height)
                {
                    var cell = mapControl.M2CellInfo[targetLocation.X, targetLocation.Y];
                    if (cell?.CellObjects != null)
                    {
                        // 查找可攻击的对象（怪物等）
                        foreach (var obj in cell.CellObjects)
                        {
                            if (obj != user && !obj.Dead && obj.Blocking)
                            {
                                return obj;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalCombatHandler] 查找目标失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据ID查找对象
        /// </summary>
        private static MapObject FindObjectByID(uint objectID)
        {
            try
            {
                // 从地图控件的对象字典中查找
                if (MapControl.Objects != null && MapControl.Objects.TryGetValue(objectID, out var obj))
                {
                    return obj;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalCombatHandler] 查找对象失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 计算伤害
        /// </summary>
        private static int CalculateDamage(UserObject attacker, MapObject defender)
        {
            try
            {
                // 基础伤害计算
                int minDC = attacker.Stats[Stat.MinDC];
                int maxDC = attacker.Stats[Stat.MaxDC];

                // 随机伤害
                Random random = new Random();
                int damage = random.Next(minDC, maxDC + 1);

                // TODO: 添加更复杂的伤害计算
                // - 防御力减免
                // - 技能加成
                // - 暴击等

                return Math.Max(1, damage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalCombatHandler] 计算伤害失败: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// 对目标造成伤害
        /// </summary>
        private static void DealDamage(MapObject target, int damage)
        {
            try
            {
                // 更新目标的生命值百分比
                // 注意：客户端中 PercentHealth 是从服务器更新的
                // 这里我们只是记录伤害，实际的生命值管理需要更复杂的系统

                Console.WriteLine($"[本地模式] 对 {target.Name} 造成 {damage} 点伤害");

                // TODO: 实现完整的伤害系统
                // - 显示伤害数字
                // - 更新生命值
                // - 检查死亡
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalCombatHandler] 造成伤害失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理技能攻击
        /// </summary>
        public static void HandleMagicAttack(UserObject user, ClientMagic magic, MirDirection direction, Point targetLocation, uint targetID)
        {
            if (user == null || magic == null) return;

            user.Direction = direction;

            Console.WriteLine($"[本地模式] 施放技能: {magic.Spell}");

            // 根据技能类型处理
            switch (magic.Spell)
            {
                case Spell.FireBall:
                    HandleFireBall(user, magic, targetLocation);
                    break;
                // TODO: 添加更多技能处理
                default:
                    Console.WriteLine($"[本地模式] 技能 {magic.Spell} 尚未实现");
                    break;
            }
        }

        /// <summary>
        /// 处理火球术
        /// </summary>
        private static void HandleFireBall(UserObject user, ClientMagic magic, Point targetLocation)
        {
            try
            {
                // 查找目标位置的对象
                var mapControl = GameScene.Scene?.MapControl;
                if (mapControl == null) return;

                if (targetLocation.X >= 0 && targetLocation.X < mapControl.Width &&
                    targetLocation.Y >= 0 && targetLocation.Y < mapControl.Height)
                {
                    var cell = mapControl.M2CellInfo[targetLocation.X, targetLocation.Y];
                    if (cell?.CellObjects != null)
                    {
                        foreach (var obj in cell.CellObjects)
                        {
                            if (obj != user && !obj.Dead)
                            {
                                // 计算魔法伤害
                                int minMC = user.Stats[Stat.MinMC];
                                int maxMC = user.Stats[Stat.MaxMC];
                                Random random = new Random();
                                int damage = random.Next(minMC, maxMC + 1) + (magic.Level * 2);

                                DealDamage(obj, damage);
                                Console.WriteLine($"[本地模式] 火球术对 {obj.Name} 造成 {damage} 点伤害");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalCombatHandler] 火球术失败: {ex.Message}");
            }
        }
    }
}
