using System;

namespace Client.LocalGame
{
    // 本地数据库类型定义（简化版，避免引用Server项目）

    /// <summary>
    /// 地图信息
    /// </summary>
    public class LocalMapInfo
    {
        public int Index { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public ushort MiniMap { get; set; }
        public ushort BigMap { get; set; }
        public byte Light { get; set; }
        public byte MapDarkLight { get; set; }
        public bool NeedHole { get; set; }
        public bool NeedBridging { get; set; }
        public bool CanReconnect { get; set; }
        public bool CanMount { get; set; }
        public bool CanHack { get; set; }
        public bool CanFly { get; set; }
        public bool NeedMoveDetect { get; set; }
        public bool Fight { get; set; }
        public bool NoThrowItem { get; set; }
        public bool NoDropItem { get; set; }
        public bool NoHorse { get; set; }
        public bool NoPosition { get; set; }
        public bool NoRecall { get; set; }
        public bool NoGuild { get; set; }
        public bool NoDrug { get; set; }
        public bool NoReconnect { get; set; }
        public bool NoTimer { get; set; }
        public bool NoTownTeleport { get; set; }
        public bool NoRandom { get; set; }
        public bool NoHero { get; set; }
        public bool NoMerchant { get; set; }
        public bool NoStorage { get; set; }
        public bool NoDealer { get; set; }
        public bool NoHunter { get; set; }
        public bool NoFishing { get; set; }
        public bool NoMine { get; set; }
        public byte NoMountExp { get; set; }
        public byte NoSkillExp { get; set; }
        public byte NoExp { get; set; }
        public byte NoTrap { get; set; }
        public byte NoMakeDrug { get; set; }
        public byte NoHeroCall { get; set; }
        public byte NoMoney { get; set; }
        public byte NoOverPower { get; set; }
    }

    /// <summary>
    /// 物品信息
    /// </summary>
    public class LocalItemInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        // TODO: 添加更多属性
    }

    /// <summary>
    /// 怪物信息
    /// </summary>
    public class LocalMonsterInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }

        // 外观
        public ushort Image; // Monster 枚举的值
        public byte AI;
        public byte Effect;
        public byte ViewRange = 7;
        public byte CoolEye;

        // 等级和生命
        public ushort Level;
        public uint HP;

        // 属性
        public byte Accuracy;
        public byte Agility;
        public byte Light;
        public ushort MinAC, MaxAC; // 防御力
        public ushort MinMAC, MaxMAC;
        public ushort MinDC, MaxDC; // 近战攻击
        public ushort MinMC, MaxMC; // 魔法攻击
        public ushort MinSC, MaxSC; // 道具攻击

        // 速度
        public ushort AttackSpeed = 2500;
        public ushort MoveSpeed = 1800;

        // 奖励
        public uint Experience;

        // 特性
        public bool CanTame = true;
        public bool CanPush = true;
        public bool AutoRev = true;
        public bool Undead = false;
    }

    /// <summary>
    /// 掉落信息
    /// </summary>
    public class LocalDropInfo
    {
        public int Chance; // 掉落概率 (1-100)
        public int ItemIndex; // 物品索引
        public uint Gold; // 金币数量
    }

    /// <summary>
    /// NPC信息
    /// </summary>
    public class LocalNPCInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        // TODO: 添加更多属性
    }

    /// <summary>
    /// 技能信息
    /// </summary>
    public class LocalMagicInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        // TODO: 添加更多属性
    }
}
