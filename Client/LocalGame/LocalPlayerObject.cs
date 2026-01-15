using System;
using System.Collections.Generic;
using System.Drawing;
using Client.MirObjects;
using Client.MirScenes;

namespace Client.LocalGame
{
    /// <summary>
    /// 本地玩家对象扩展
    /// 为客户端 UserObject 添加服务端 PlayerObject 的功能
    /// </summary>
    public class LocalPlayerObject
    {
        private UserObject _userObject;
        private GMCommandHandler _gmHandler;

        // GM模式状态
        private bool _godMode = false;
        private bool _infiniteHP = false;
        private bool _infiniteMP = false;

        public LocalPlayerObject(UserObject userObject)
        {
            _userObject = userObject;
            _gmHandler = new GMCommandHandler(this);
        }

        /// <summary>
        /// 初始化本地玩家
        /// </summary>
        public void Initialize()
        {
            // 加载角色数据
            LoadPlayerData();

            // 初始化环境
            LocalEnvir.Instance.LocalPlayer = _userObject;

            // 初始化战斗系统
            LocalCombatSystem.Instance.Initialize(_userObject);
        }

        private void LoadPlayerData()
        {
            // 从本地存档加载
            var saveData = LocalSaveManager.LoadCharacter(_userObject.Name);
            if (saveData != null)
            {
                ApplySaveData(saveData);
            }
        }

        private void ApplySaveData(CharacterSaveData saveData)
        {
            _userObject.Level = saveData.Level;
            _userObject.Experience = (long)saveData.Experience;
            _userObject.HP = (ushort)saveData.HP;
            _userObject.MP = (ushort)saveData.MP;
            // TODO: 应用其他数据
        }

        /// <summary>
        /// 更新玩家状态（每帧调用）
        /// </summary>
        public void Process()
        {
            // GM模式：无限HP/MP
            if (_infiniteHP)
            {
                int maxHP = _userObject.Stats[Stat.HP];
                if (_userObject.HP < maxHP)
                {
                    _userObject.HP = maxHP;
                }
            }
            if (_infiniteMP)
            {
                int maxMP = _userObject.Stats[Stat.MP];
                if (_userObject.MP < maxMP)
                {
                    _userObject.MP = maxMP;
                }
            }

            // 更新战斗状态
            ProcessCombat();

            // 更新状态效果
            ProcessBuffs();

            // 自动保存（每5分钟）
            if (CMain.Time - _lastSaveTime > 5 * 60 * 1000)
            {
                AutoSave();
            }
        }

        private void ProcessCombat()
        {
            // TODO: 本地战斗逻辑
        }

        private void ProcessBuffs()
        {
            // TODO: 处理状态效果
        }

        private long _lastSaveTime;
        private void AutoSave()
        {
            _lastSaveTime = CMain.Time;
            // 不自动保存，由玩家手动保存或退出时保存
        }

        /// <summary>
        /// 处理GM命令
        /// </summary>
        public bool HandleGMCommand(string command)
        {
            return _gmHandler.HandleCommand(command);
        }

        // ===== 属性访问器 =====

        public string Name => _userObject.Name;
        public MirClass Class => _userObject.Class;
        public byte Gender => (byte)_userObject.Gender;
        public ushort Level
        {
            get => _userObject.Level;
            set => _userObject.Level = value;
        }
        public ulong Experience
        {
            get => (ulong)_userObject.Experience;
            set => _userObject.Experience = (long)value;
        }
        public string CurrentMap { get; set; } = "0"; // UserObject没有此属性
        public Point CurrentLocation => _userObject.CurrentLocation;

        public int HP
        {
            get => _userObject.HP;
            set => _userObject.HP = (ushort)Math.Min(_userObject.Stats[Stat.HP], Math.Max(0, value));
        }

        public int MP
        {
            get => _userObject.MP;
            set => _userObject.MP = (ushort)Math.Min(_userObject.Stats[Stat.MP], Math.Max(0, value));
        }

        public int MaxHP => _userObject.Stats[Stat.HP];
        public int MaxMP => _userObject.Stats[Stat.MP];

        public int MinAC => _userObject.Stats[Stat.MinAC];
        public int MaxAC => _userObject.Stats[Stat.MaxAC];
        public int MinMAC => _userObject.Stats[Stat.MinMAC];
        public int MaxMAC => _userObject.Stats[Stat.MaxMAC];
        public int MinDC => _userObject.Stats[Stat.MinDC];
        public int MaxDC => _userObject.Stats[Stat.MaxDC];
        public int MinMC => _userObject.Stats[Stat.MinMC];
        public int MaxMC => _userObject.Stats[Stat.MaxMC];
        public int MinSC => _userObject.Stats[Stat.MinSC];
        public int MaxSC => _userObject.Stats[Stat.MaxSC];
        public int Accuracy => _userObject.Stats[Stat.Accuracy];
        public int Agility => _userObject.Stats[Stat.Agility];

        // UserObject没有这些属性，使用默认值
        public uint Gold { get; set; }
        public byte MoveSpeed { get; set; } = 12;
        public int MonstersKilled { get; set; }
        public int Deaths { get; set; }
        public int PlayTime { get; set; }

        // GM模式状态
        public bool GodMode => _godMode;
        public bool InfiniteHP => _infiniteHP;
        public bool InfiniteMP => _infiniteMP;

        public object Equipment => _userObject.Equipment;
        public object Inventory => _userObject.Inventory;
        public object Storage => null;
        public object Magics => _userObject.Magics;
        public object Quests => null;

        // 内部访问UserObject
        internal UserObject UserObject => _userObject;

        // ===== 方法 =====

        /// <summary>
        /// 获得经验
        /// </summary>
        public void GainExperience(ulong exp)
        {
            _userObject.Experience += (long)exp;
            Console.WriteLine($"[LocalPlayer] 获得经验: {exp}, 当前经验: {_userObject.Experience}");

            // TODO: 检查升级
        }

        /// <summary>
        /// 设置等级
        /// </summary>
        public void SetLevel(ushort level)
        {
            _userObject.Level = level;
            _userObject.Experience = 0;

            // 根据等级调整属性
            // TODO: 实现等级属性计算
            Console.WriteLine($"[LocalPlayer] 等级设置为: {level}");
        }

        /// <summary>
        /// 传送到指定坐标
        /// </summary>
        public void Teleport(int x, int y)
        {
            var scene = GameScene.Scene;
            if (scene?.MapControl == null || _userObject == null)
            {
                Console.WriteLine("[LocalPlayer] 无法传送：地图或玩家未初始化");
                return;
            }

            // 从旧位置移除
            scene.MapControl.RemoveObject(_userObject);

            // 更新位置
            _userObject.CurrentLocation = new Point(x, y);
            _userObject.MapLocation = new Point(x, y);

            // 添加到新位置
            scene.MapControl.AddObject(_userObject);

            // 刷新地图
            scene.MapControl.FloorValid = false;

            Console.WriteLine($"[LocalPlayer] 传送到: ({x}, {y})");
        }

        /// <summary>
        /// 切换地图
        /// </summary>
        public void ChangeMap(string mapName)
        {
            try
            {
                var scene = GameScene.Scene;
                if (scene == null) return;

                // 创建新地图控制器
                var mapControl = new MapControl
                {
                    FileName = $"{mapName}.map",
                    Title = mapName,
                    MiniMap = 0,
                    BigMap = 0,
                    Lights = LightSetting.Normal,
                    Lightning = false,
                    Fire = false,
                    MapDarkLight = 0
                };

                mapControl.LoadMap();

                // 移除旧地图
                if (scene.MapControl != null)
                {
                    scene.MapControl.RemoveObject(_userObject);
                }

                // 设置新地图
                scene.MapControl = mapControl;
                scene.Controls.Remove(scene.MapControl);

                // 添加到场景
                scene.InsertControl(0, mapControl);

                // 设置玩家到新地图的出生点
                _userObject.CurrentLocation = new Point(330, 330);
                _userObject.MapLocation = new Point(330, 330);

                mapControl.AddObject(_userObject);

                CurrentMap = mapName;

                Console.WriteLine($"[LocalPlayer] 切换到地图: {mapName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalPlayer] 切换地图失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 传送到城镇
        /// </summary>
        public void TeleportTown()
        {
            // 传送到安全区
            Teleport(330, 330);
            Console.WriteLine("[LocalPlayer] 回城成功");
        }

        /// <summary>
        /// 切换无敌模式
        /// </summary>
        public void ToggleGodMode()
        {
            _godMode = !_godMode;
            Console.WriteLine($"[LocalPlayer] 无敌模式: {(_godMode ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 切换无限HP
        /// </summary>
        public void ToggleInfiniteHP()
        {
            _infiniteHP = !_infiniteHP;
            Console.WriteLine($"[LocalPlayer] 无限HP: {(_infiniteHP ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 切换无限MP
        /// </summary>
        public void ToggleInfiniteMP()
        {
            _infiniteMP = !_infiniteMP;
            Console.WriteLine($"[LocalPlayer] 无限MP: {(_infiniteMP ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 设置移动速度
        /// </summary>
        public void SetMoveSpeed(byte speed)
        {
            MoveSpeed = speed;
            Console.WriteLine($"[LocalPlayer] 移动速度设置为: {speed}");
        }

        /// <summary>
        /// 添加金币
        /// </summary>
        public void AddGold(uint amount)
        {
            Gold += amount;
            Console.WriteLine($"[LocalPlayer] 获得金币: {amount}, 当前金币: {Gold}");
        }

        /// <summary>
        /// 保存角色
        /// </summary>
        public void Save()
        {
            LocalSaveManager.SaveCharacter(_userObject);
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        public string GetInfo()
        {
            return $@"
=== 角色信息 ===
名称: {Name}
职业: {Class}
性别: {(Gender == 0 ? "男" : "女")}
等级: {Level}
经验: {Experience}
HP: {HP}/{MaxHP}
MP: {MP}/{MaxMP}
金币: {Gold}
地图: {CurrentMap}
位置: ({CurrentLocation.X}, {CurrentLocation.Y})
移动速度: {MoveSpeed}
=== 属性 ===
DC: {MinDC}-{MaxDC}
MC: {MinMC}-{MaxMC}
SC: {MinSC}-{MaxSC}
AC: {MinAC}-{MaxAC}
MAC: {MinMAC}-{MaxMAC}
准确: {Accuracy}
敏捷: {Agility}
=== 统计 ===
击杀怪物: {MonstersKilled}
死亡次数: {Deaths}
游玩时间: {PlayTime}分钟
=== GM状态 ===
无敌模式: {(GodMode ? "开启" : "关闭")}
无限HP: {(InfiniteHP ? "开启" : "关闭")}
无限MP: {(InfiniteMP ? "开启" : "关闭")}
";
        }

        // ===== 战斗相关方法 =====

        /// <summary>
        /// 攻击指定位置的怪物
        /// </summary>
        public bool Attack(Point targetLocation)
        {
            return LocalCombatSystem.Instance.PlayerAttack(targetLocation);
        }

        /// <summary>
        /// 攻击指定的怪物
        /// </summary>
        public bool Attack(MapObject target)
        {
            return LocalCombatSystem.Instance.PlayerAttack(target);
        }

        /// <summary>
        /// 检查是否可以攻击
        /// </summary>
        public bool CanAttack(MapObject target)
        {
            return LocalCombatSystem.Instance.CanAttack(target);
        }
    }
}
