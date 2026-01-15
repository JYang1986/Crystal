using System;
using System.Collections.Generic;
using System.Linq;
using Client.LocalGame;
using Client.MirObjects;
using Client.MirScenes;
using S = ServerPackets;

namespace Client.MirScenes.Integration
{
    /// <summary>
    /// 本地模式集成助手
    /// 提供本地模式和联机模式之间的切换和集成功能
    /// </summary>
    public static class LocalModeIntegration
    {
        private static bool _isLocalModeInitialized = false;

        /// <summary>
        /// 检查是否启用本地模式
        /// </summary>
        public static bool IsLocalModeEnabled => Settings.EnableLocalMode;

        /// <summary>
        /// 初始化本地模式
        /// </summary>
        public static void InitializeLocalMode()
        {
            if (_isLocalModeInitialized) return;

            try
            {
                Console.WriteLine("[LocalMode] 初始化本地游戏模式...");

                // 加载本地模式配置
                var config = Client.LocalGame.Config.LocalModeConfig.Instance;

                if (!config.EnableLocalMode)
                {
                    Console.WriteLine("[LocalMode] 本地模式未启用，使用联机模式");
                    return;
                }

                // 初始化本地环境
                LocalEnvir.Instance.Initialize();

                // 初始化网络桥接器
                LocalNetworkBridge.Instance.Initialize();

                _isLocalModeInitialized = true;
                Console.WriteLine("[LocalMode] 本地模式初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取本地存档的角色列表
        /// </summary>
        public static List<SelectInfo> GetLocalCharacters()
        {
            try
            {
                var saveDatas = LocalSaveManager.GetAllSaves();
                var characters = new List<SelectInfo>();

                foreach (var save in saveDatas)
                {
                    var selectInfo = new SelectInfo
                    {
                        Name = save.Name,
                        Class = save.Class,
                        Gender = save.Gender,
                        Level = save.Level,
                        LastAccess = save.SaveTime
                    };
                    characters.Add(selectInfo);
                }

                Console.WriteLine($"[LocalMode] 找到 {characters.Count} 个本地存档");
                return characters;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 获取本地存档失败: {ex.Message}");
                return new List<SelectInfo>();
            }
        }

        /// <summary>
        /// 创建新的本地角色
        /// </summary>
        public static bool CreateLocalCharacter(string name, MirClass mirClass, byte gender)
        {
            try
            {
                // 检查角色名是否已存在
                if (LocalSaveManager.CharacterExists(name))
                {
                    Console.WriteLine($"[LocalMode] 角色名已存在: {name}");
                    return false;
                }

                // 创建新角色数据
                var newSaveData = new CharacterSaveData
                {
                    CharacterId = name,
                    Name = name,
                    Class = mirClass,
                    Gender = gender,
                    Level = 1,
                    Experience = 0,
                    CurrentMap = "0",
                    CurrentLocation = new PointData(330, 330),
                    Direction = 2,
                    Hair = 0,
                    HP = 100,
                    MP = 50,
                    MaxHP = 100,
                    MaxMP = 50,
                    MinAC = 0,
                    MaxAC = 0,
                    MinMAC = 0,
                    MaxMAC = 0,
                    MinDC = 5,
                    MaxDC = 10,
                    MinMC = 0,
                    MaxMC = 0,
                    MinSC = 0,
                    MaxSC = 0,
                    Accuracy = 10,
                    Agility = 10,
                    Equipment = null,
                    Inventory = null,
                    Storage = null,
                    Magics = null,
                    Quests = null,
                    MonstersKilled = 0,
                    Deaths = 0,
                    PlayTime = 0,
                    SaveTime = DateTime.Now
                };

                // 保存角色数据
                bool success = LocalSaveManager.SaveCharacterData(newSaveData);
                if (success)
                {
                    Console.WriteLine($"[LocalMode] 创建新角色成功: {name}");
                }
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 创建角色失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除本地角色
        /// </summary>
        public static bool DeleteLocalCharacter(string characterName)
        {
            try
            {
                return LocalSaveManager.DeleteCharacter(characterName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 删除角色失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 启动本地游戏
        /// </summary>
        public static bool StartLocalGame(string characterName)
        {
            try
            {
                Console.WriteLine($"[LocalMode] 启动本地游戏: {characterName}");

                // 使用 LocalGameStarter 启动游戏
                return LocalGameStarter.StartGame(characterName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 启动游戏失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 处理本地模式的聊天命令（GM命令）
        /// </summary>
        public static bool HandleLocalChat(string message)
        {
            try
            {
                // 检查是否是GM命令
                if (message.StartsWith("/@") || message.StartsWith("@"))
                {
                    var localPlayer = LocalGameStarter.GetLocalPlayer();
                    if (localPlayer != null)
                    {
                        return localPlayer.HandleGMCommand(message);
                    }
                    else
                    {
                        Console.WriteLine("[LocalMode] 本地玩家对象未初始化，无法执行GM命令");
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 处理聊天命令失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 保存当前游戏
        /// </summary>
        public static bool SaveCurrentGame()
        {
            try
            {
                var localPlayer = LocalGameStarter.GetLocalPlayer();
                if (localPlayer != null)
                {
                    localPlayer.Save();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 保存游戏失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新本地环境（每帧调用）
        /// </summary>
        public static void UpdateLocalEnvironment()
        {
            try
            {
                if (_isLocalModeInitialized)
                {
                    LocalEnvir.Instance.Update();

                    // 更新本地玩家
                    var localPlayer = LocalGameStarter.GetLocalPlayer();
                    localPlayer?.Process();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 更新环境失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理本地模式资源
        /// </summary>
        public static void Cleanup()
        {
            try
            {
                if (_isLocalModeInitialized)
                {
                    // 保存游戏
                    SaveCurrentGame();

                    // 清理环境
                    LocalEnvir.Instance.ClearObjects();

                    _isLocalModeInitialized = false;
                    Console.WriteLine("[LocalMode] 本地模式已清理");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMode] 清理失败: {ex.Message}");
            }
        }
    }
}
