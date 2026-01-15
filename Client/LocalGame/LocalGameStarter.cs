using System;
using System.Drawing;
using Client.MirObjects;
using Client.MirScenes;
using Client.MirControls;
using S = ServerPackets;

namespace Client.LocalGame
{
    /// <summary>
    /// 本地游戏启动器
    /// 负责从本地存档加载角色并启动游戏场景
    /// </summary>
    public class LocalGameStarter
    {
        /// <summary>
        /// 从本地存档启动游戏
        /// </summary>
        public static bool StartGame(string characterName)
        {
            try
            {
                Console.WriteLine($"[LocalGame] 正在加载角色: {characterName}");

                // 1. 加载角色存档数据
                var saveData = LocalSaveManager.LoadCharacter(characterName);
                if (saveData == null)
                {
                    Console.WriteLine($"[LocalGame] 角色存档不存在: {characterName}");
                    return false;
                }

                // 2. 初始化本地环境
                LocalEnvir.Instance.Initialize();

                // 3. 创建并切换到游戏场景
                GameScene newScene = new GameScene();
                MirScene.ActiveScene.Dispose();
                MirScene.ActiveScene = newScene;

                Console.WriteLine("[LocalGame] 游戏场景已创建");

                // 4. 创建本地玩家对象
                UserObject user = CreateLocalPlayer(saveData);
                if (user == null)
                {
                    Console.WriteLine("[LocalGame] 创建玩家对象失败");
                    return false;
                }

                // 5. 初始化游戏地图
                if (!InitializeGameMap(saveData, user))
                {
                    Console.WriteLine("[LocalGame] 地图初始化失败");
                    return false;
                }

                // 6. 初始化游戏UI
                InitializeGameUI(saveData);

                Console.WriteLine($"[LocalGame] 游戏启动成功: {characterName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalGame] 启动游戏失败: {ex.Message}");
                return false;
            }
        }

        // 本地玩家对象（用于GM命令等扩展功能）
        private static LocalPlayerObject _localPlayerObject;

        /// <summary>
        /// 创建本地玩家对象
        /// </summary>
        private static UserObject CreateLocalPlayer(CharacterSaveData saveData)
        {
            try
            {
                // 创建 UserObject（客户端玩家对象）
                uint objectId = LocalEnvir.Instance.NewObjectID();
                UserObject user = new UserObject(objectId);

                // 加载角色数据
                LoadPlayerData(user, saveData);

                // 设置为当前玩家
                GameScene.User = user;

                // 创建LocalPlayerObject扩展（用于GM命令）
                _localPlayerObject = new LocalPlayerObject(user);
                _localPlayerObject.Initialize();

                // 添加到本地环境
                LocalEnvir.Instance.LocalPlayer = user;
                LocalEnvir.Instance.AddObject(user);

                Console.WriteLine($"[LocalGame] 玩家对象创建成功: {saveData.Name}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalGame] 创建玩家对象失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取本地玩家对象
        /// </summary>
        public static LocalPlayerObject GetLocalPlayer()
        {
            return _localPlayerObject;
        }

        /// <summary>
        /// 将存档数据加载到玩家对象
        /// </summary>
        private static void LoadPlayerData(UserObject user, CharacterSaveData saveData)
        {
            // 基本信息
            user.Name = saveData.Name;
            user.Class = saveData.Class;
            user.Gender = (MirGender)saveData.Gender;
            user.Level = saveData.Level;
            user.Hair = saveData.Hair;

            // 位置
            user.CurrentLocation = new Point(saveData.CurrentLocation.X, saveData.CurrentLocation.Y);
            user.MapLocation = user.CurrentLocation; // 设置MapLocation
            user.Direction = (MirDirection)saveData.Direction;

            // 属性 - 通过 Stats 字典设置
            user.HP = (ushort)saveData.HP;
            user.MP = (ushort)saveData.MP;
            user.Stats[Stat.HP] = saveData.MaxHP;
            user.Stats[Stat.MP] = saveData.MaxMP;
            user.Stats[Stat.MinAC] = saveData.MinAC;
            user.Stats[Stat.MaxAC] = saveData.MaxAC;
            user.Stats[Stat.MinMAC] = saveData.MinMAC;
            user.Stats[Stat.MaxMAC] = saveData.MaxMAC;
            user.Stats[Stat.MinDC] = saveData.MinDC;
            user.Stats[Stat.MaxDC] = saveData.MaxDC;
            user.Stats[Stat.MinMC] = saveData.MinMC;
            user.Stats[Stat.MaxMC] = saveData.MaxMC;
            user.Stats[Stat.MinSC] = saveData.MinSC;
            user.Stats[Stat.MaxSC] = saveData.MaxSC;
            user.Stats[Stat.Accuracy] = saveData.Accuracy;
            user.Stats[Stat.Agility] = saveData.Agility;

            // 装备和物品
            // TODO: 加载装备和背包数据

            // 统计数据
            // TODO: 加载统计数据

            Console.WriteLine($"[LocalGame] 角色数据加载完成: Lv.{saveData.Level} {saveData.Class}");
        }

        /// <summary>
        /// 初始化游戏地图
        /// </summary>
        private static bool InitializeGameMap(CharacterSaveData saveData, UserObject user)
        {
            try
            {
                var scene = GameScene.Scene;
                if (scene == null)
                {
                    Console.WriteLine("[LocalGame] GameScene.Scene 为空");
                    return false;
                }

                // 创建地图控制器
                var mapControl = new MapControl
                {
                    FileName = $"{saveData.CurrentMap}.map",
                    Title = saveData.CurrentMap,
                    MiniMap = 0, // TODO: 从配置获取
                    BigMap = 0,
                    Lights = LightSetting.Normal,
                    Lightning = false,
                    Fire = false,
                    MapDarkLight = 0
                };

                // 加载地图
                mapControl.LoadMap();

                // 设置到场景
                scene.MapControl = mapControl;

                // 将地图控件插入到场景的最底层
                scene.InsertControl(0, mapControl);

                // 添加玩家到地图
                mapControl.AddObject(user);

                // 设置场景移动时间
                GameScene.MoveTime = CMain.Time;
                GameScene.CanMove = true;
                GameScene.CanRun = true;

                // 刷新地图
                mapControl.FloorValid = false;

                Console.WriteLine($"[LocalGame] 地图加载成功: {saveData.CurrentMap}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalGame] 地图加载失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 初始化游戏UI
        /// </summary>
        private static void InitializeGameUI(CharacterSaveData saveData)
        {
            try
            {
                var scene = GameScene.Scene;
                if (scene == null) return;

                // 设置初始金币
                GameScene.Gold = 0; // TODO: 从存档加载
                GameScene.Credit = 0;

                // 刷新技能栏
                foreach (var skillBar in scene.SkillBarDialogs)
                {
                    skillBar.Update();
                }

                // 刷新背包
                scene.InventoryDialog?.RefreshInventory();

                // 更新职业相关UI
                if (scene.MainDialog != null && GameScene.User != null)
                {
                    scene.MainDialog.PModeLabel.Visible =
                        GameScene.User.Class == MirClass.Wizard ||
                        GameScene.User.Class == MirClass.Taoist;
                }

                Console.WriteLine("[LocalGame] 游戏UI初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalGame] UI初始化失败: {ex.Message}");
            }
        }
    }
}
