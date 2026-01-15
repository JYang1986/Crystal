using System;
using System.IO;
using System.Text.Json;
using Client.MirObjects;

namespace Client.LocalGame
{
    /// <summary>
    /// 本地存档管理器
    /// 负责保存和加载角色数据
    /// </summary>
    public class LocalSaveManager
    {
        private static readonly string SavePath = Path.Combine(LocalEnvir.SavePath, "Characters");

        /// <summary>
        /// 保存角色数据（从CharacterSaveData）
        /// </summary>
        public static bool SaveCharacterData(CharacterSaveData saveData)
        {
            try
            {
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);

                string fileName = GetSaveFileName(saveData.Name);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(saveData, options);

                File.WriteAllText(fileName, json);

                Console.WriteLine($"[LocalSave] 角色已保存: {saveData.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalSave] 保存失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 保存角色数据
        /// </summary>
        public static bool SaveCharacter(UserObject player)
        {
            try
            {
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);

                var saveData = new CharacterSaveData
                {
                    CharacterId = player.Name,
                    Name = player.Name,
                    Class = player.Class,
                    Gender = (byte)player.Gender,
                    Level = player.Level,
                    Experience = (ulong)player.Experience,
                    CurrentMap = "0", // UserObject没有CurrentMap属性
                    CurrentLocation = new PointData(player.CurrentLocation.X, player.CurrentLocation.Y),
                    Direction = (byte)player.Direction,
                    Hair = player.Hair,
                    HP = player.HP,
                    MP = player.MP,
                    MaxHP = player.MaxHP,
                    MaxMP = player.MaxMP,
                    MinAC = player.MinAC,
                    MaxAC = player.MaxAC,
                    MinMAC = player.MinMAC,
                    MaxMAC = player.MaxMAC,
                    MinDC = player.MinDC,
                    MaxDC = player.MaxDC,
                    MinMC = player.MinMC,
                    MaxMC = player.MaxMC,
                    MinSC = player.MinSC,
                    MaxSC = player.MaxSC,
                    Accuracy = player.Accuracy,
                    Agility = player.Agility,

                    // 装备
                    Equipment = player.Equipment,

                    // 背包
                    Inventory = player.Inventory,

                    // 仓库 - UserObject没有此属性
                    Storage = null,

                    // 技能
                    Magics = player.Magics,

                    // 任务
                    Quests = null,

                    // 统计数据
                    MonstersKilled = 0,
                    Deaths = 0,
                    PlayTime = 0,

                    SaveTime = DateTime.Now
                };

                string fileName = GetSaveFileName(player.Name);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(saveData, options);

                File.WriteAllText(fileName, json);

                Console.WriteLine($"[LocalSave] 角色已保存: {player.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalSave] 保存失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载角色数据
        /// </summary>
        public static CharacterSaveData LoadCharacter(string characterName)
        {
            try
            {
                string fileName = GetSaveFileName(characterName);

                if (!File.Exists(fileName))
                    return null;

                string json = File.ReadAllText(fileName);
                var saveData = JsonSerializer.Deserialize<CharacterSaveData>(json);

                Console.WriteLine($"[LocalSave] 角色已加载: {characterName}");
                return saveData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalSave] 加载失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有存档列表
        /// </summary>
        public static CharacterSaveData[] GetAllSaves()
        {
            try
            {
                if (!Directory.Exists(SavePath))
                    return new CharacterSaveData[0];

                var files = Directory.GetFiles(SavePath, "*.json");
                var saves = new System.Collections.Generic.List<CharacterSaveData>();

                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var save = JsonSerializer.Deserialize<CharacterSaveData>(json);
                        saves.Add(save);
                    }
                    catch { }
                }

                return saves.ToArray();
            }
            catch
            {
                return new CharacterSaveData[0];
            }
        }

        /// <summary>
        /// 删除角色存档
        /// </summary>
        public static bool DeleteCharacter(string characterName)
        {
            try
            {
                string fileName = GetSaveFileName(characterName);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    Console.WriteLine($"[LocalSave] 角色已删除: {characterName}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalSave] 删除失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查角色是否存在
        /// </summary>
        public static bool CharacterExists(string characterName)
        {
            string fileName = GetSaveFileName(characterName);
            return File.Exists(fileName);
        }

        private static string GetSaveFileName(string characterName)
        {
            // 清理文件名中的非法字符
            string safeName = string.Join("_", characterName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(SavePath, $"{safeName}.json");
        }

        /// <summary>
        /// 快速存档（用于游戏中的临时保存点）
        /// </summary>
        public static bool QuickSave(UserObject player, int slot)
        {
            try
            {
                string quickSavePath = Path.Combine(LocalEnvir.SavePath, "QuickSaves");
                if (!Directory.Exists(quickSavePath))
                    Directory.CreateDirectory(quickSavePath);

                // 实现快速存档逻辑
                // ...
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 角色存档数据结构
    /// </summary>
    public class CharacterSaveData
    {
        public string CharacterId { get; set; }
        public string Name { get; set; }
        public MirClass Class { get; set; }
        public byte Gender { get; set; }
        public ushort Level { get; set; }
        public ulong Experience { get; set; }
        public string CurrentMap { get; set; }
        public PointData CurrentLocation { get; set; }
        public byte Direction { get; set; }
        public byte Hair { get; set; }

        // 属性
        public int HP { get; set; }
        public int MP { get; set; }
        public int MaxHP { get; set; }
        public int MaxMP { get; set; }
        public ushort MinAC { get; set; }
        public ushort MaxAC { get; set; }
        public ushort MinMAC { get; set; }
        public ushort MaxMAC { get; set; }
        public ushort MinDC { get; set; }
        public ushort MaxDC { get; set; }
        public ushort MinMC { get; set; }
        public ushort MaxMC { get; set; }
        public ushort MinSC { get; set; }
        public ushort MaxSC { get; set; }
        public byte Accuracy { get; set; }
        public byte Agility { get; set; }

        // 装备和物品（TODO: 定义具体的数据结构）
        public object Equipment { get; set; }
        public object Inventory { get; set; }
        public object Storage { get; set; }
        public object Magics { get; set; }
        public object Quests { get; set; }

        // 统计
        public int MonstersKilled { get; set; }
        public int Deaths { get; set; }
        public int PlayTime { get; set; }

        public DateTime SaveTime { get; set; }
    }

    /// <summary>
    /// 坐标数据
    /// </summary>
    public class PointData
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PointData() { }

        public PointData(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
