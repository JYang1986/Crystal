# Mir2 客户端本地化改造

## 📋 改造进度

### 已完成 ✅

- [x] 创建本地游戏引擎架构 (`LocalEnvir.cs`)
- [x] 实现本地存档系统 (`LocalSaveManager.cs`)
- [x] 实现 GM 命令处理器 (`GMCommandHandler.cs`)
- [x] 创建本地网络桥接器 (`LocalNetworkBridge.cs`)
- [x] 创建本地玩家对象扩展 (`LocalPlayerObject.cs`)
- [x] 创建本地游戏管理器 (`LocalGameManager.cs`)
- [x] 创建本地模式配置 (`LocalModeConfig.cs`)
- [x] 创建 Git 分支 (`feature/local-single-player`)

### 进行中 🚧

- [ ] 修改客户端主程序集成本地引擎
- [ ] 从服务端移植核心游戏逻辑
- [ ] 实现本地存档读取和显示
- [ ] 测试基本游戏功能

### 待开始 📝

- [ ] 移除网络依赖（保留 GM 命令）
- [ ] 完善本地战斗系统
- [ ] 实现怪物 AI 本地化
- [ ] 实现 NPC 对话系统
- [ ] 实现物品掉落系统
- [ ] 添加存档/读档 UI
- [ ] 集成轻量服务端（排行榜功能）
- [ ] 完整测试和 Bug 修复

---

## 📁 新增文件结构

```
Client/LocalGame/
├── LocalEnvir.cs              # 本地游戏环境管理
├── LocalSaveManager.cs        # 本地存档管理
├── GMCommandHandler.cs        # GM 命令处理器
├── LocalNetworkBridge.cs      # 本地网络桥接器
├── LocalPlayerObject.cs       # 本地玩家对象
├── Config/
│   └── LocalModeConfig.cs     # 本地模式配置
└── README.md                  # 本文件
```

---

## 🎮 核心功能说明

### 1. LocalEnvir（本地游戏环境）

**作用**：替代服务端的 `Envir.cs`，管理所有本地游戏数据

**主要功能**：
- 加载游戏数据（物品、怪物、地图、NPC）
- 管理游戏对象（玩家、怪物、NPC）
- 更新游戏状态（每帧调用）
- 生成对象 ID

**使用示例**：
```csharp
// 初始化
LocalEnvir.Instance.Initialize();

// 获取物品信息
ItemInfo item = LocalEnvir.Instance.GetItemInfo(itemId);

// 更新游戏环境
LocalEnvir.Instance.Update();
```

### 2. LocalSaveManager（本地存档管理）

**作用**：管理角色数据的保存和加载

**主要功能**：
- 保存角色数据到 JSON 文件
- 加载角色数据
- 获取所有存档列表
- 删除存档

**存档位置**：`Saves/Characters/角色名.json`

**使用示例**：
```csharp
// 保存角色
LocalSaveManager.SaveCharacter(player);

// 加载角色
CharacterSaveData data = LocalSaveManager.LoadCharacter("英雄");

// 获取所有存档
var saves = LocalSaveManager.GetAllSaves();
```

### 3. GMCommandHandler（GM 命令处理器）

**作用**：处理 GM 测试命令

**支持的命令**：
```
/@move X Y          - 传送到坐标
/@go 地图索引        - 切换地图
/@level N           - 设置等级
/@exp N             - 获得经验
/@hp                - 恢复 HP
/@mp                - 恢复 MP
/@item 物品名称      - 创建物品
/@gold N            - 获得金币
/@mob 怪物名称 [数量] - 召唤怪物
/@clearmob          - 清除所有怪物
/@skill ID          - 学习技能
/@save              - 保存游戏
/@info              - 显示角色信息
/@help              - 显示帮助
```

**使用示例**：
```csharp
var gmHandler = new GMCommandHandler(player);
gmHandler.HandleCommand("/@level 50");
gmHandler.HandleCommand("/@gold 100000");
```

### 4. LocalNetworkBridge（本地网络桥接器）

**作用**：替代网络层，将网络请求转换为本地调用

**主要功能**：
- 拦截客户端数据包
- 将数据包转换为本地函数调用
- 向客户端发送模拟的服务端响应

**使用示例**：
```csharp
// 初始化
LocalNetworkBridge.Instance.Initialize();

// 处理数据包
LocalNetworkBridge.Instance.ProcessPacket(packet);
```

### 5. LocalGameManager（本地游戏管理器）

**作用**：管理整个本地游戏模式

**主要功能**：
- 初始化本地游戏模式
- 更新游戏环境
- 关闭本地游戏模式

**使用示例**：
```csharp
// 初始化（程序启动时）
LocalGameManager.Instance.Initialize();

// 更新（每帧）
LocalGameManager.Instance.Update();

// 关闭（程序退出时）
LocalGameManager.Instance.Shutdown();
```

---

## 🔧 配置文件

### LocalModeConfig.json

配置文件位置：`Client/bin/Release/LocalModeConfig.json`

```json
{
  "EnableLocalMode": true,
  "Difficulty": "Normal",
  "MonsterDamageRate": 1.0,
  "MonsterHPRate": 1.0,
  "DropRate": 2.0,
  "ExpRate": 2.0,
  "EnableGMCommands": true,
  "AutoSaveInterval": 5,
  "MaxSaveSlots": 5,
  "EnableGlobalRanking": true,
  "StatsServerAddress": "127.0.0.1",
  "StatsServerPort": 7001,
  "StatsReportInterval": 300
}
```

**配置说明**：
- `EnableLocalMode`: 是否启用本地模式
- `Difficulty`: 游戏难度（Easy/Normal/Hard/Hell）
- `MonsterDamageRate`: 怪物伤害倍率
- `MonsterHPRate`: 怪物血量倍率
- `DropRate`: 掉落倍率
- `ExpRate`: 经验倍率
- `EnableGMCommands`: 是否启用 GM 命令
- `AutoSaveInterval`: 自动保存间隔（分钟）
- `EnableGlobalRanking`: 是否启用全服排行榜

---

## 🚀 下一步工作

### 短期目标（1-2周）

1. **修改客户端主程序**
   - 在 `CMain.cs` 中集成 `LocalGameManager`
   - 修改游戏循环调用本地环境更新

2. **实现基本游戏功能**
   - 角色创建和存档
   - 基本移动和攻击
   - 物品使用

3. **完善 GM 系统**
   - 添加更多测试命令
   - 实现 GM 权限检查

### 中期目标（3-4周）

1. **移植服务端逻辑**
   - 战斗计算
   - 怪物 AI
   - 物品掉落
   - NPC 对话

2. **实现存档 UI**
   - 选角界面改造
   - 存档槽位管理
   - 快速存档

3. **性能优化**
   - 帧率优化
   - 内存优化

### 长期目标（5-8周）

1. **轻量服务端集成**
   - 统计数据上报
   - 排行榜查询

2. **完善游戏体验**
   - 任务系统
   - 技能系统
   - 装备系统

3. **测试和发布**
   - 完整功能测试
   - Bug 修复
   - 打包发布

---

## 📝 开发说明

### 分支管理

当前工作分支：`feature/local-single-player`

主分支：`master`

### 提交规范

```bash
# 添加新功能
git commit -m "feat: 添加本地存档系统"

# 修复 Bug
git commit -m "fix: 修复角色数据保存问题"

# 文档更新
git commit -m "docs: 更新本地化开发文档"

# 测试相关
git commit -m "test: 添加 GM 命令测试"
```

### 代码规范

- 所有新增类放在 `Client/LocalGame/` 目录
- 使用中文注释说明功能
- GM 命令保持中文提示
- 保持与原有代码风格一致

---

## 🐛 已知问题

- [ ] 服务端数据文件尚未加载
- [ ] 本地战斗逻辑未实现
- [ ] 物品系统需要完善
- [ ] 怪物 AI 需要移植

---

## 📞 联系方式

如有问题或建议，请在项目中提 Issue。

---

**最后更新**：2026-01-15
**开发者**：Claude Code
**分支**：feature/local-single-player
