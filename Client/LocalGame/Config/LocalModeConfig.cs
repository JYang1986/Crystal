using System;
using System.IO;
using System.Text.Json;

namespace Client.LocalGame.Config
{
    /// <summary>
    /// 本地游戏模式配置
    /// </summary>
    public class LocalModeConfig
    {
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "LocalModeConfig.json");

        private static LocalModeConfig _instance;
        public static LocalModeConfig Instance
        {
            get
            {
                if (_instance == null)
                    Load();
                return _instance;
            }
        }

        // ===== 配置项 =====

        /// <summary>
        /// 是否启用本地游戏模式
        /// </summary>
        public bool EnableLocalMode { get; set; } = true;

        /// <summary>
        /// 游戏难度
        /// </summary>
        public GameDifficulty Difficulty { get; set; } = GameDifficulty.Normal;

        /// <summary>
        /// 怪物伤害倍率
        /// </summary>
        public float MonsterDamageRate { get; set; } = 1.0f;

        /// <summary>
        /// 怪物血量倍率
        /// </summary>
        public float MonsterHPRate { get; set; } = 1.0f;

        /// <summary>
        /// 掉落倍率
        /// </summary>
        public float DropRate { get; set; } = 2.0f;

        /// <summary>
        /// 经验倍率
        /// </summary>
        public float ExpRate { get; set; } = 2.0f;

        /// <summary>
        /// 是否启用GM命令
        /// </summary>
        public bool EnableGMCommands { get; set; } = true;

        /// <summary>
        /// 自动保存间隔（分钟）
        /// </summary>
        public int AutoSaveInterval { get; set; } = 5;

        /// <summary>
        /// 最大存档槽位数
        /// </summary>
        public int MaxSaveSlots { get; set; } = 5;

        /// <summary>
        /// 是否启用全服排行榜（需要轻量服务端）
        /// </summary>
        public bool EnableGlobalRanking { get; set; } = true;

        /// <summary>
        /// 轻量服务端地址
        /// </summary>
        public string StatsServerAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// 轻量服务端端口
        /// </summary>
        public int StatsServerPort { get; set; } = 7001;

        /// <summary>
        /// 统计数据上报间隔（秒）
        /// </summary>
        public int StatsReportInterval { get; set; } = 300;

        // ===== 方法 =====

        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    _instance = JsonSerializer.Deserialize<LocalModeConfig>(json);
                }
                else
                {
                    _instance = new LocalModeConfig();
                    _instance.Save(); // 创建默认配置
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 加载配置失败: {ex.Message}");
                _instance = new LocalModeConfig();
            }
        }

        /// <summary>
        /// 重置为默认配置
        /// </summary>
        public void Reset()
        {
            EnableLocalMode = true;
            Difficulty = GameDifficulty.Normal;
            MonsterDamageRate = 1.0f;
            MonsterHPRate = 1.0f;
            DropRate = 2.0f;
            ExpRate = 2.0f;
            EnableGMCommands = true;
            AutoSaveInterval = 5;
            MaxSaveSlots = 5;
            EnableGlobalRanking = true;
            Save();
        }
    }

    /// <summary>
    /// 游戏难度
    /// </summary>
    public enum GameDifficulty
    {
        Easy,       // 简单
        Normal,     // 普通
        Hard,       // 困难
        Hell        // 地狱
    }
}
