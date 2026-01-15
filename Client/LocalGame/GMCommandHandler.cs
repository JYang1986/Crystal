using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Client.MirObjects;
using Client.MirScenes;

namespace Client.LocalGame
{
    /// <summary>
    /// GM命令处理器
    /// 用于本地测试和调试
    /// </summary>
    public class GMCommandHandler
    {
        private LocalPlayerObject _player;

        public GMCommandHandler(LocalPlayerObject player)
        {
            _player = player;
        }

        /// <summary>
        /// 处理GM命令
        /// </summary>
        public bool HandleCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return false;

            // 移除命令前缀
            string cmd = command.Trim();
            if (cmd.StartsWith("/@") || cmd.StartsWith("@"))
            {
                cmd = cmd.Substring(cmd.StartsWith("/@") ? 2 : 1);
            }

            string[] parts = cmd.Split(' ');
            string cmdName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            try
            {
                return ProcessCommand(cmdName, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GM] 命令执行失败: {ex.Message}");
                return false;
            }
        }

        private bool ProcessCommand(string cmdName, string[] args)
        {
            switch (cmdName)
            {
                // 移动相关
                case "move":
                case "goto":
                    return CommandMove(args);
                case "go":
                case "map":
                    return CommandGo(args);
                case "town":
                case "recall":
                    return CommandTown();

                // 角色属性
                case "level":
                case "lvl":
                    return CommandLevel(args);
                case "exp":
                case "xp":
                    return CommandExp(args);
                case "hp":
                    return CommandHP(args);
                case "mp":
                    return CommandMP(args);
                case "stats":
                    return CommandStats(args);

                // 物品相关
                case "item":
                case "additem":
                    return CommandItem(args);
                case "gold":
                case "money":
                    return CommandGold(args);
                case "clearbag":
                case "clean":
                    return CommandClearBag();

                // 怪物相关
                case "mob":
                case "monster":
                    return CommandMob(args);
                case "clearmob":
                case "killmob":
                    return CommandClearMob();

                // 技能相关
                case "skill":
                case "learn":
                    return CommandSkill(args);
                case "magic":
                    return CommandMagic();

                // GM模式
                case "god":
                case "immortal":
                    return CommandGod();
                case "infhp":
                case "infinitehp":
                    return CommandInfiniteHP();
                case "infmp":
                case "infinitemp":
                    return CommandInfiniteMP();

                // 状态相关
                case "speed":
                    return CommandSpeed(args);

                // 战斗相关
                case "kill":
                case "attack":
                    return CommandAttack(args);

                // 测试相关
                case "test":
                    return CommandTest(args);
                case "spawn":
                    return CommandSpawn(args);

                // 信息和保存
                case "info":
                case "status":
                    return CommandInfo();
                case "save":
                    return CommandSave();

                // 帮助
                case "help":
                case "?":
                    return CommandHelp();

                default:
                    Console.WriteLine($"[GM] 未知命令: {cmdName}，输入 /@help 查看帮助");
                    return false;
            }
        }

        #region 命令实现

        /// <summary>
        /// 传送到指定坐标
        /// </summary>
        private bool CommandMove(string[] args)
        {
            if (args.Length >= 2)
            {
                if (int.TryParse(args[0], out int x) && int.TryParse(args[1], out int y))
                {
                    _player.Teleport(x, y);
                    OutputMessage($"已传送到: ({x}, {y})");
                    return true;
                }
            }

            OutputMessage("用法: /@move X Y");
            return false;
        }

        /// <summary>
        /// 切换地图
        /// </summary>
        private bool CommandGo(string[] args)
        {
            if (args.Length >= 1)
            {
                string mapName = args[0];
                _player.ChangeMap(mapName);
                OutputMessage($"切换到地图: {mapName}");
                return true;
            }

            OutputMessage("用法: /@go 地图名称");
            OutputMessage("常用地图: 0(新手村), 1(比奇), 2(毒蛇), 3(沃玛), 4(祖玛)");
            return false;
        }

        /// <summary>
        /// 回城
        /// </summary>
        private bool CommandTown()
        {
            _player.TeleportTown();
            OutputMessage("已回城");
            return true;
        }

        /// <summary>
        /// 设置等级
        /// </summary>
        private bool CommandLevel(string[] args)
        {
            if (args.Length >= 1 && ushort.TryParse(args[0], out ushort level))
            {
                level = Math.Min((ushort)255, Math.Max((ushort)1, level));
                _player.SetLevel(level);
                OutputMessage($"等级设置为: {level}");
                return true;
            }

            OutputMessage($"当前等级: {_player.Level}");
            return true;
        }

        /// <summary>
        /// 添加经验
        /// </summary>
        private bool CommandExp(string[] args)
        {
            if (args.Length >= 1 && ulong.TryParse(args[0], out ulong exp))
            {
                _player.GainExperience(exp);
                OutputMessage($"获得经验: {exp}");
                return true;
            }

            // 默认添加10万经验
            _player.GainExperience(100000);
            OutputMessage("获得经验: 100000");
            return true;
        }

        /// <summary>
        /// 恢复HP
        /// </summary>
        private bool CommandHP(string[] args)
        {
            _player.HP = _player.MaxHP;
            OutputMessage("HP已完全恢复");
            return true;
        }

        /// <summary>
        /// 恢复MP
        /// </summary>
        private bool CommandMP(string[] args)
        {
            _player.MP = _player.MaxMP;
            OutputMessage("MP已完全恢复");
            return true;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        private bool CommandStats(string[] args)
        {
            if (args.Length < 2)
            {
                OutputMessage("用法: /@stats 属性名 值");
                OutputMessage("属性: DC, MC, SC, AC, MAC");
                return false;
            }

            string stat = args[0].ToLower();
            if (ushort.TryParse(args[1], out ushort value))
            {
                // TODO: 实现属性设置
                OutputMessage($"设置 {stat} = {value}");
                return true;
            }

            OutputMessage("无效的属性值");
            return false;
        }

        /// <summary>
        /// 创建物品
        /// </summary>
        private bool CommandItem(string[] args)
        {
            if (args.Length >= 1)
            {
                string itemName = string.Join(" ", args);
                // TODO: 实现物品创建
                OutputMessage($"创建物品: {itemName} (待实现)");
                return true;
            }

            OutputMessage("用法: /@item 物品名称");
            OutputMessage("例如: /@item 裁决  或  /@item 金币");
            return false;
        }

        /// <summary>
        /// 添加金币
        /// </summary>
        private bool CommandGold(string[] args)
        {
            uint amount = 100000; // 默认10万
            if (args.Length >= 1 && uint.TryParse(args[0], out uint customAmount))
            {
                amount = customAmount;
            }

            _player.AddGold(amount);
            OutputMessage($"获得金币: {amount}");
            return true;
        }

        /// <summary>
        /// 清空背包
        /// </summary>
        private bool CommandClearBag()
        {
            // TODO: 实现清空背包
            OutputMessage("背包已清空 (待实现)");
            return true;
        }

        /// <summary>
        /// 召唤怪物
        /// </summary>
        private bool CommandMob(string[] args)
        {
            if (args.Length >= 1)
            {
                string mobNameOrIndex = args[0];
                int count = 1;
                if (args.Length >= 2 && int.TryParse(args[1], out int customCount))
                {
                    count = Math.Min(100, Math.Max(1, customCount));
                }

                // 尝试解析怪物索引
                int monsterIndex = -1;
                if (int.TryParse(mobNameOrIndex, out int parsedIndex))
                {
                    monsterIndex = parsedIndex;
                }
                else
                {
                    // 根据名称查找怪物索引
                    monsterIndex = FindMonsterIndexByName(mobNameOrIndex);
                }

                if (monsterIndex < 0)
                {
                    OutputMessage($"找不到怪物: {mobNameOrIndex}");
                    OutputMessage("可用怪物ID: 1(鸡), 2(鹿), 3(钉耙猫), 4(钉耙猫王)");
                    return false;
                }

                // 在玩家附近召唤怪物
                var scene = GameScene.Scene;
                if (scene?.MapControl == null || _player.CurrentLocation.IsEmpty)
                {
                    OutputMessage("地图未初始化");
                    return false;
                }

                int spawned = 0;
                for (int i = 0; i < count; i++)
                {
                    // 在玩家周围随机位置生成
                    int offsetX = LocalEnvir.Instance.Random.Next(-5, 6);
                    int offsetY = LocalEnvir.Instance.Random.Next(-5, 6);

                    Point spawnLocation = new Point(
                        _player.CurrentLocation.X + offsetX,
                        _player.CurrentLocation.Y + offsetY
                    );

                    // 暂时不检查位置是否可行走，直接生成
                    var monster = LocalEnvir.Instance.CreateMonster(monsterIndex, spawnLocation);
                    if (monster != null)
                    {
                        spawned++;
                    }
                }

                OutputMessage($"已召唤 {spawned} 个怪物 (ID: {monsterIndex})");
                return true;
            }

            OutputMessage("用法: /@mob 怪物ID或名称 [数量]");
            OutputMessage("例如: /@mob 1 10");
            OutputMessage("可用怪物ID: 1(鸡), 2(鹿), 3(钉耙猫), 4(钉耙猫王)");
            return false;
        }

        private int FindMonsterIndexByName(string name)
        {
            // 简单名称匹配
            foreach (var kvp in LocalEnvir.Instance.MonsterInfoList)
            {
                if (kvp.Value.Name.Contains(name) || name.Contains(kvp.Value.Name))
                {
                    return kvp.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// 清除所有怪物
        /// </summary>
        private bool CommandClearMob()
        {
            // TODO: 实现清除怪物
            var scene = GameScene.Scene;
            if (scene?.MapControl != null)
            {
                int count = LocalEnvir.Instance.Monsters.Count;
                LocalEnvir.Instance.Monsters.Clear();
                OutputMessage($"清除了 {count} 个怪物");
                return true;
            }

            OutputMessage("没有怪物可清除");
            return true;
        }

        /// <summary>
        /// 学习技能
        /// </summary>
        private bool CommandSkill(string[] args)
        {
            if (args.Length >= 1 && int.TryParse(args[0], out int skillId))
            {
                // TODO: 实现技能学习
                OutputMessage($"学习技能ID: {skillId} (待实现)");
                return true;
            }

            OutputMessage("用法: /@skill 技能ID");
            return false;
        }

        /// <summary>
        /// 显示技能列表
        /// </summary>
        private bool CommandMagic()
        {
            // TODO: 显示技能列表
            OutputMessage("技能列表 (待实现)");
            return true;
        }

        /// <summary>
        /// 攻击附近的怪物
        /// </summary>
        private bool CommandAttack(string[] args)
        {
            int range = 1; // 默认攻击范围
            if (args.Length >= 1 && int.TryParse(args[0], out int customRange))
            {
                range = Math.Min(10, Math.Max(1, customRange));
            }

            var scene = GameScene.Scene;
            if (scene?.MapControl == null)
            {
                OutputMessage("地图未初始化");
                return false;
            }

            // 查找范围内的怪物
            int attackedCount = 0;
            foreach (var obj in LocalEnvir.Instance.Objects.Values)
            {
                if (obj is MonsterObject monster && !monster.Dead)
                {
                    int distance = Math.Max(
                        Math.Abs(_player.CurrentLocation.X - monster.CurrentLocation.X),
                        Math.Abs(_player.CurrentLocation.Y - monster.CurrentLocation.Y)
                    );

                    if (distance <= range)
                    {
                        if (_player.Attack(monster))
                        {
                            attackedCount++;
                        }
                    }
                }
            }

            OutputMessage($"攻击了 {attackedCount} 个怪物");
            return true;
        }

        /// <summary>
        /// 切换无敌模式
        /// </summary>
        private bool CommandGod()
        {
            _player.ToggleGodMode();
            OutputMessage($"无敌模式: {(_player.GodMode ? "开启" : "关闭")}");
            return true;
        }

        /// <summary>
        /// 切换无限HP
        /// </summary>
        private bool CommandInfiniteHP()
        {
            _player.ToggleInfiniteHP();
            OutputMessage($"无限HP: {(_player.InfiniteHP ? "开启" : "关闭")}");
            return true;
        }

        /// <summary>
        /// 切换无限MP
        /// </summary>
        private bool CommandInfiniteMP()
        {
            _player.ToggleInfiniteMP();
            OutputMessage($"无限MP: {(_player.InfiniteMP ? "开启" : "关闭")}");
            return true;
        }

        /// <summary>
        /// 设置移动速度
        /// </summary>
        private bool CommandSpeed(string[] args)
        {
            if (args.Length >= 1 && byte.TryParse(args[0], out byte speed))
            {
                speed = Math.Min((byte)50, Math.Max((byte)1, speed));
                _player.SetMoveSpeed(speed);
                OutputMessage($"移动速度设置为: {speed}");
                return true;
            }

            OutputMessage($"当前移动速度: {_player.MoveSpeed}");
            return true;
        }

        /// <summary>
        /// 测试命令
        /// </summary>
        private bool CommandTest(string[] args)
        {
            if (args.Length >= 1)
            {
                switch (args[0].ToLower())
                {
                    case "damage":
                        OutputMessage("测试伤害计算 (待实现)");
                        return true;
                    case "drop":
                        OutputMessage("测试物品掉落 (待实现)");
                        return true;
                    case "spawn":
                        OutputMessage("测试怪物刷新 (待实现)");
                        return true;
                }
            }

            OutputMessage("用法: /@test [damage|drop|spawn]");
            return false;
        }

        /// <summary>
        /// 强制刷新所有怪物
        /// </summary>
        private bool CommandSpawn(string[] args)
        {
            if (args.Length >= 1 && args[0].ToLower() == "all")
            {
                LocalMonsterSpawner.Instance.ForceSpawnAll();
                OutputMessage("已强制刷新所有怪物");
                return true;
            }

            OutputMessage("用法: /@spawn all - 强制刷新所有怪物");
            return false;
        }

        /// <summary>
        /// 显示角色信息
        /// </summary>
        private bool CommandInfo()
        {
            string info = _player.GetInfo();
            Console.WriteLine(info);
            OutputMessage("角色信息已输出到控制台");
            return true;
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        private bool CommandSave()
        {
            try
            {
                _player.Save();
                OutputMessage("游戏已保存");
                return true;
            }
            catch (Exception ex)
            {
                OutputMessage($"保存失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 显示帮助
        /// </summary>
        private bool CommandHelp()
        {
            string help = @"
╔════════════════════════════════════════════════════════╗
║              GM命令列表                                  ║
╠════════════════════════════════════════════════════════╣
║ 移动:                                                  ║
║   /@move X Y        - 传送到坐标                        ║
║   /@go 地图名        - 切换地图                          ║
║   /@town            - 回城                              ║
╠════════════════════════════════════════════════════════╣
║ 属性:                                                  ║
║   /@level [N]        - 设置/查看等级                     ║
║   /@exp [N]          - 添加经验(默认10万)                ║
║   /@hp               - 恢复HP                            ║
║   /@mp               - 恢复MP                            ║
╠════════════════════════════════════════════════════════╣
║ 物品:                                                  ║
║   /@item 名称        - 创建物品(待实现)                  ║
║   /@gold [N]         - 添加金币(默认10万)                ║
║   /@clearbag         - 清空背包(待实现)                  ║
╠════════════════════════════════════════════════════════╣
║ 怪物:                                                  ║
║   /@mob ID [数量]    - 召唤怪物                          ║
║   /@clearmob         - 清除所有怪物                      ║
║   /@attack [范围]    - 攻击附近怪物                      ║
╠════════════════════════════════════════════════════════╣
║ GM模式:                                                ║
║   /@god              - 切换无敌模式                      ║
║   /@infhp            - 切换无限HP                        ║
║   /@infmp            - 切换无限MP                        ║
║   /@speed [N]        - 设置移动速度                      ║
╠════════════════════════════════════════════════════════╣
║ 其他:                                                  ║
║   /@info             - 查看角色信息                      ║
║   /@save             - 保存游戏                          ║
║   /@test [类型]       - 测试功能                         ║
╠════════════════════════════════════════════════════════╣
║ 怪物ID: 1(鸡), 2(鹿), 3(钉耙猫), 4(钉耙猫王)           ║
╠════════════════════════════════════════════════════════╣
║ 提示: 所有命令可以用 /@ 或 @ 开头                       ║
║      例如: /@mob 1 10  召唤10只鸡                       ║
╚════════════════════════════════════════════════════════╝
";
            Console.WriteLine(help);
            OutputMessage("帮助信息已输出到控制台");
            return true;
        }

        #endregion

        /// <summary>
        /// 输出消息到游戏界面（如果可用）
        /// </summary>
        private void OutputMessage(string message)
        {
            Console.WriteLine($"[GM] {message}");

            // 尝试输出到游戏聊天框
            try
            {
                var scene = GameScene.Scene;
                if (scene?.ChatDialog != null)
                {
                    scene.ChatDialog.ReceiveChat($"[GM] {message}", ChatType.System);
                }
            }
            catch
            {
                // 忽略错误，继续使用控制台输出
            }
        }
    }
}
