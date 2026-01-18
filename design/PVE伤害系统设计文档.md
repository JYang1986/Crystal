# Crystal PVE 伤害系统设计文档

## 目录
1. [系统概述](#系统概述)
2. [属性系统](#属性系统)
3. [伤害计算流程](#伤害计算流程)
4. [玩家攻击怪物](#玩家攻击怪物)
5. [怪物攻击玩家](#怪物攻击玩家)
6. [特殊机制](#特殊机制)
7. [配置参数](#配置参数)

---

## 系统概述

Crystal 游戏采用经典传奇2的伤害计算系统，具有以下特点：

- **三种攻击类型**：物理（DC）、魔法（MC）、毒系（SC）
- **三种防御类型**：物理防御（AC）、魔法防御（MAC）
- **随机浮动机制**：攻击力和防御力都有随机浮动
- **幸运值系统**：影响伤害的随机范围
- **暴击系统**：独立的暴击率和暴击伤害计算
- **特殊效果**：吸血、反伤、能量护盾等

---

## 属性系统

### 攻击属性

| 属性 | 英文全称 | 说明 | 计算方式 |
|------|----------|------|----------|
| **MinDC** | Min Damage Combat | 最小物理攻击力 | 基础值 + 装备加成 + 技能加成 |
| **MaxDC** | Max Damage Combat | 最大物理攻击力 | 基础值 + 装备加成 + 技能加成 |
| **MinMC** | Min Magic Combat | 最小魔法攻击力 | 基础值 + 装备加成 + 元素球加成 |
| **MaxMC** | Max Magic Combat | 最大魔法攻击力 | 基础值 + 装备加成 + 元素球加成 |
| **MinSC** | Min Poison Combat | 最小毒系攻击力 | 基础值 + 装备加成 |
| **MaxSC** | Max Poison Combat | 最大毒系攻击力 | 基础值 + 装备加成 |

### 防御属性

| 属性 | 英文全称 | 说明 | 计算方式 |
|------|----------|------|----------|
| **MinAC** | Min Armour Combat | 最小物理防御力 | 基础值 + 装备加成 |
| **MaxAC** | Max Armour Combat | 最大物理防御力 | 基础值 + 装备加成 |
| **MinMAC** | Min Magic Armour Combat | 最小魔法防御力 | 基础值 + 装备加成 |
| **MaxMAC** | Max Magic Armour Combat | 最大魔法防御力 | 基础值 + 装备加成 |

### 特殊属性

| 属性 | 说明 | 取值范围 |
|------|------|----------|
| **Luck** | 幸运值 | -10 ~ +10 |
| **CriticalRate** | 暴击率 | 0 ~ 100 |
| **CriticalDamage** | 暴击伤害 | 0 ~ 1000 |
| **HPDrainRatePercent** | 吸血百分比 | 0 ~ 100 |
| **Reflect** | 反伤概率 | 0 ~ 100 |
| **MagicResist** | 魔法抗性 | 0 ~ 100 |
| **Agility** | 敏捷（闪避率） | 0 ~ 100 |
| **Accuracy** | 准确度 | 0 ~ 100 |
| **DamageReductionPercent** | 伤害减免百分比 | 0 ~ 100 |

---

## 伤害计算流程

### 完整计算流程图

```
攻击开始
  ↓
命中判定
  ├─ 物理攻击：敏捷 vs 准确度
  ├─ 魔法攻击：魔法抗性判定
  └─ 未命中 → 返回 0 伤害
  ↓
计算攻击力
  ├─ GetAttackPower(MinDC, MaxDC)
  ├─ 幸运值影响随机范围
  └─ 元素球加成（仅魔法攻击）
  ↓
暴击判定
  ├─ 暴击率：CriticalRate * CriticalRateWeight
  ├─ 暴击伤害：damage * (1 + CriticalDamage / CriticalDamageWeight / 10)
  └─ 未暴击 → 继续下一步
  ↓
计算防御力
  ├─ GetDefencePower(MinAC, MaxAC)
  └─ 防御力随机浮动
  ↓
应用伤害倍率
  ├─ DamageRate：伤害倍率（技能/装备加成）
  ├─ ArmourRate：防御倍率（技能/装备加成）
  └─ damage = damage * DamageRate
  └─ armour = armour * ArmourRate
  ↓
计算最终伤害
  ├─ if (armour >= damage) → 返回 0（未破防）
  ├─ finalDamage = damage - armour
  └─ 应用伤害减免
  ↓
特殊效果触发
  ├─ 吸血判定
  ├─ 反伤判定
  ├─ 能量护盾判定
  └─ 其他状态效果
  ↓
返回最终伤害
```

---

## 玩家攻击怪物

### 1. 攻击力计算

#### 物理攻击力（DC）

```csharp
// 获取攻击力（随机值）
int damage = GetAttackPower(MinDC, MaxDC);

// 幸运值影响
if (Stats[Stat.Luck] > Envir.Random.Next(Settings.MaxLuck))
{
    damage = MaxDC; // 幸运值高时，有概率打出最大攻击
}

// 远程攻击距离衰减
if (distance > 3)
{
    damage = GetRangeAttackPower(MinDC, MaxDC, distance);
}
```

**关键参数**：
- `Settings.MaxLuck`：最大幸运值（默认 10）
- 幸运值为正：提高打出最大伤害的概率
- 幸运值为负：提高打出最小伤害的概率

#### 魔法攻击力（MC）

```csharp
// 基础魔法攻击力
int damage = GetAttackPower(MinMC, MaxMC);

// 元素球加成（Archers 专属）
damage += GetElementalOrbPower();

// 技能伤害
int skillDamage = magicInfo.GetDamage(damage);
```

#### 毒系攻击力（SC）

```csharp
// 毒系攻击力计算方式与物理类似
int damage = GetAttackPower(MinSC, MaxSC);

// 毒素效果
ApplyPoison(target, poison);
```

### 2. 暴击系统

#### 暴击率计算

```csharp
// 暴击率判定
if (Envir.Random.Next(100) < Stats[Stat.CriticalRate] * Settings.CriticalRateWeight)
{
    // 触发暴击
    int criticalDamage = damage + (int)Math.Floor(
        damage * (((double)Stats[Stat.CriticalDamage] / (double)Settings.CriticalDamageWeight) * 10)
    );
    return criticalDamage;
}
```

**关键参数**：
- `Settings.CriticalRateWeight`：暴击率权重（默认 5）
- `Settings.CriticalDamageWeight`：暴击伤害权重（默认 50）

**示例**：
- CriticalRate = 10，CriticalRateWeight = 5
- 实际暴击率 = 10 * 5 = 50%
- CriticalDamage = 100，CriticalDamageWeight = 50
- 暴击伤害加成 = damage * (100 / 50 / 10) = damage * 20%

### 3. 技能伤害计算

```csharp
public int GetDamage(int DamageBase)
{
    // 基础伤害 + 技能威力
    int baseDamage = DamageBase + GetPower();

    // 应用技能倍率
    float multiplier = GetMultiplier();
    return (int)(baseDamage * multiplier);
}

private float GetMultiplier()
{
    // 基础倍率 + 等级加成
    return BaseMultiplier + (Level * 0.1f);
}
```

### 4. 武器特殊效果

#### Holy（神圣）属性
```csharp
if (weapon.HasHoly && target.IsUndead)
{
    damage = (int)(damage * 1.5); // 对亡灵生物伤害加成 50%
}
```

#### Strong（强力）属性
```csharp
if (weapon.HasStrong)
{
    damage = (int)(damage * 1.2); // 伤害加成 20%
}
```

---

## 怪物攻击玩家

### 1. 怪物攻击力计算

```csharp
// 怪物攻击力（从 MonsterInfo 读取）
int damage = GetAttackPower(MonsterInfo.MinDC, MonsterInfo.MaxDC);

// 怪物技能伤害
if (MonsterInfo.HasSkill)
{
    damage += GetSkillDamage();
}
```

### 2. 玩家防御力计算

#### 物理防御（AC）

```csharp
// 获取防御力（随机值）
int armour = GetDefencePower(MinAC, MaxAC);

// 防御力受倍率影响
armour = (int)(armour * ArmourRate);

// 防御力不能超过伤害值
if (armour > damage)
{
    armour = damage; // 伤害减免后为 0
}
```

#### 魔法防御（MAC）

```csharp
// 魔法防御力计算
int armour = GetDefencePower(MinMAC, MaxMAC);

// 魔法抗性额外减免
if (Envir.Random.Next(Settings.MagicResistWeight) < Stats[Stat.MagicResist])
{
    damage = (int)(damage * 0.5); // 魔法抗性成功，伤害减半
}
```

### 3. 命中与闪避

#### 物理攻击闪避

```csharp
// 敏捷 vs 准确度
if (Envir.Random.Next(Stats[Stat.Agility] + 1) > attacker.Stats[Stat.Accuracy])
{
    // 闪避成功，不受到伤害
    BroadcastDamageIndicator(DamageType.Miss);
    return 0;
}
```

#### 魔法攻击抗性

```csharp
// 魔法抗性判定
if (Envir.Random.Next(Settings.MagicResistWeight) < Stats[Stat.MagicResist])
{
    // 魔法抗性成功，伤害减半
    damage = (int)(damage * 0.5);
}
```

**关键参数**：
- `Settings.MagicResistWeight`：魔法抗性权重（默认 10）

### 4. 伤害减免

```csharp
// 伤害减免百分比
if (Stats[Stat.DamageReductionPercent] > 0)
{
    int reduction = (damage * Stats[Stat.DamageReductionPercent]) / 100;
    damage -= reduction;
}

// 能量护盾
if (Envir.Random.Next(100) < Stats[Stat.EnergyShieldPercent])
{
    // 能量护盾触发，恢复固定 HP
    int shieldAmount = Stats[Stat.EnergyShieldAmount];
    ChangeHP(shieldAmount);
}
```

---

## 特殊机制

### 1. 吸血机制

```csharp
// 吸血判定
if (attacker.Stats[Stat.HPDrainRatePercent] > 0 && damageWeapon)
{
    // 计算吸血量
    attacker.HpDrain += Math.Max(0, ((float)(damage - armour) / 100) * attacker.Stats[Stat.HPDrainRatePercent]);

    // 累积吸血量超过 2 时生效
    if (attacker.HpDrain > 2)
    {
        int HpGain = (int)Math.Floor(attacker.HpDrain);
        attacker.ChangeHP(HpGain);
        attacker.HpDrain -= HpGain;
    }
}
```

**说明**：
- `HPDrainRatePercent`：吸血百分比（例如 10 表示 10% 吸血）
- 吸血量基于实际造成的伤害（damage - armour）
- 吸血累积到 2 点以上才会生效

### 2. 反伤机制

```csharp
// 反伤判定
if (Envir.Random.Next(100) < Stats[Stat.Reflect])
{
    if (attacker.IsAttackTarget(this))
    {
        // 反射伤害给攻击者
        attacker.Attacked(this, damage, type, false);
        CurrentMap.Broadcast(new S.ObjectEffect { ObjectID = ObjectID, Effect = SpellEffect.Reflect }, CurrentLocation);
    }
    return 0; // 自身不受到伤害
}
```

**说明**：
- `Reflect`：反伤概率（0-100）
- 反射的伤害等于原伤害值
- 触发反伤后，自身不受到伤害

### 3. 毒系伤害

```csharp
// 毒素应用
public void ApplyPoison(MapObject target, Poison p)
{
    // 毒素类型
    // - Poison: 持续伤害
    // - Paralysis: 麻痹（无法移动）
    // - Slow: 减速
    // - Frozen: 冰冻

    // 毒素伤害计算
    int poisonDamage = GetAttackPower(MinSC, MaxSC);
    target.ApplyPoison(p, poisonDamage);
}
```

### 4. 状态效果

#### 冰冻效果
```csharp
if (Envir.Random.Next(Settings.FreezingAttackWeight) < Stats[Stat.FreezingAttack])
{
    target.ApplyFrozen(3); // 冰冻 3 秒
}
```

#### 麻痹效果
```csharp
if (Envir.Random.Next(100) < Stats[Stat.ParalysisRate])
{
    target.ApplyParalysis(2); // 麻痹 2 秒
}
```

---

## 配置参数

### 服务器配置（Settings.cs）

```csharp
// 暴击系统
public static byte CriticalRateWeight = 5;        // 暴击率权重
public static byte CriticalDamageWeight = 50;     // 暴击伤害权重

// 魔法系统
public static byte MagicResistWeight = 10;        // 魔法抗性权重

// 状态效果
public static byte FreezingAttackWeight = 10;     // 冰冻攻击权重
public static byte PoisonAttackWeight = 10;       // 毒攻击权重

// 幸运值系统
public static sbyte MaxLuck = 10;                 // 最大幸运值
```

### 怪物配置（MonsterInfo.txt）

```ini
[Monster]
Name=鸡
Image=1
AI=1
# 攻击力设置
MinDC=1
MaxDC=5
# 防御力设置
MinAC=0
MaxAC=2
# 生命值
HP=50
# 经验值
Exp=10
```

---

## 附录：伤害计算示例

### 示例 1：物理攻击

**场景**：
- 玩家 MinDC=100, MaxDC=150, Luck=5
- 怪物 MinAC=30, MaxAC=60
- 无暴击，无特殊效果

**计算过程**：
1. 计算攻击力：`GetAttackPower(100, 150)` → 随机到 125
2. 幸运值判定：`Random.Next(10) < 5` → false（未触发最大伤害）
3. 计算防御力：`GetDefencePower(30, 60)` → 随机到 45
4. 计算最终伤害：`125 - 45 = 80`

**结果**：造成 80 点伤害

### 示例 2：暴击攻击

**场景**：
- 玩家 MinDC=100, MaxDC=150, CriticalRate=8, CriticalDamage=100
- 怪物 MinAC=30, MaxAC=60

**计算过程**：
1. 计算攻击力：`GetAttackPower(100, 150)` → 随机到 125
2. 暴击判定：`Random.Next(100) < 8 * 5` → true（触发暴击）
3. 计算暴击伤害：`125 * (1 + 100 / 50 / 10)` → `125 * 1.2 = 150`
4. 计算防御力：`GetDefencePower(30, 60)` → 随机到 45
5. 计算最终伤害：`150 - 45 = 105`

**结果**：造成 105 点伤害（暴击）

### 示例 3：吸血攻击

**场景**：
- 玩家 MinDC=100, MaxDC=150, HPDrainRatePercent=10
- 怪物 MinAC=30, MaxAC=60

**计算过程**：
1. 计算攻击力：`GetAttackPower(100, 150)` → 随机到 125
2. 计算防御力：`GetDefencePower(30, 60)` → 随机到 45
3. 计算最终伤害：`125 - 45 = 80`
4. 计算吸血：`80 * 10 / 100 = 8`
5. 玩家恢复 HP：`+8`

**结果**：造成 80 点伤害，玩家恢复 8 点 HP

---

## 文档版本

- **版本**：1.0
- **日期**：2026-01-17
- **作者**：Claude Code
- **基于代码**：Crystal dev 分支
