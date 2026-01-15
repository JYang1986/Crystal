# CLAUDE.md

此文件为 Claude Code (claude.ai/code) 提供在此代码库中工作的指导。

## 构建解决方案

```bash
# 构建整个解决方案
dotnet build

# 构建特定配置
dotnet build -c Release
dotnet build -c Debug

# 构建特定项目
dotnet build Client/Client.csproj
dotnet build Server.MirForms/Server.csproj
```

构建输出位于 `Build/` 目录：
- 客户端: `Build/Client/`
- 服务器: `Build/Server/`

**目标框架**: .NET 8.0
- 客户端/服务器: `net8.0-windows7.0` (Windows Forms)
- 类库: `net8.0`

## 项目架构

这是一个传奇2 (Legend of Mir 2) MMORPG 服务器和客户端实现，采用传统的客户端-服务器架构。

### 核心项目

| 项目 | 用途 |
|---------|---------|
| **Client** | Windows Forms 游戏客户端 - 处理渲染、输入和网络通信 |
| **Server.MirForms** | Windows Forms 服务器管理界面 |
| **Server.Library** | 核心服务器逻辑 - 游戏世界、数据库、网络 |
| **Shared** | 客户端和服务器之间共享的代码 - 数据包定义、数据结构 |

### 辅助工具

- **AutoPatcherAdmin** - 客户端补丁分发系统
- **LibraryEditor** - 游戏数据编辑工具
- **LibraryViewer** - 游戏数据查看器
- **CustomFormControl** - 共享 UI 控件

## 客户端-服务器通信

游戏使用基于 TCP 的自定义协议：

1. **数据包结构**: 2 字节大小 + 2 字节数据包 ID + 可变数据（需要时压缩）
2. **数据包定义**: 定义在 `Shared/ClientPackets.cs` 和 `Shared/ServerPackets.cs`
3. **网络层**:
   - 客户端: `Client/MirNetwork/Network.cs`
   - 服务器: `Server/MirNetwork/MirConnection.cs`

### 数据包流程

服务器对所有游戏逻辑具有权威性。客户端主要发送输入请求并接收状态更新。

## 服务器架构

服务器组织为几个关键子系统：

| 目录 | 用途 |
|-----------|---------|
| `Server/MirEnvir/` | 游戏世界管理 - 地图、计时器、重生、环境状态 |
| `Server/MirDatabase/` | 数据持久化 - 角色、物品、拍卖、行会、任务 |
| `Server/MirNetwork/` | 客户端连接和数据包处理 |
| `Server/MirObjects/` | 游戏实体 - 玩家、怪物、NPC、物品、商人 |
| `Server/MirSystems/` | 游戏系统 - 战斗、魔法、交易、婚姻等 |

### 入口点

`Server.MirForms/Program.cs` → `SMain` 类启动服务器管理界面

## 客户端架构

客户端采用基于场景的渲染架构：

| 目录 | 用途 |
|-----------|---------|
| `Client/MirScenes/` | 游戏场景管理（登录、选择、游戏） |
| `Client/MirGraphics/` | DirectX 渲染引擎 (SlimDX) |
| `Client/MirControls/` | 自定义 UI 控件 |
| `Client/MirNetwork/` | 与服务器的网络通信 |
| `Client/MirObjects/` | 客户端对象表示 |
| `Client/MirSounds/` | 音频系统 (NAudio) |
| `Client/Forms/` | Windows Forms UI 对话框 |

### 入口点

`Client/Program.cs` → 启动 `AMain`（补丁程序）或 `CMain`（游戏客户端）

## 配置

- **服务器配置**: `Configs/Setup.ini`
- **环境数据**: `Envir/` 目录 (XML 文件)
- **地图**: `Maps/` 目录
- **本地化**: `Localization/*.json`
- **日志**: `log4net.config` (使用 log4net)

## 常见模式

1. **基于数据包的协议**: 所有客户端-服务器通信使用 Shared 中定义的结构化数据包
2. **基于组件的实体**: 游戏对象继承自具有模块化组件的基类
3. **事件驱动**: 使用消息队列和计时器进行游戏逻辑
4. **数据库抽象**: `Server/MirDatabase/` 中的自定义数据库层

## 外部依赖

- **SlimDX** - DirectX 图形封装库
- **NAudio** - 音频播放
- **log4net** - 日志框架
- **Microsoft.Web.WebView2** - 嵌入式浏览器（自动更新程序）

## 开发注意事项

- 此代码库没有正式的测试套件 - 开发依赖于手动测试和调试版本
- 注意 `#if DEBUG` 条件编译块，这些是仅调试代码
- 服务器设置在启动时从 `Configs/Setup.ini` 加载
- 地图文件使用自定义格式 - 使用 [地图编辑器](https://github.com/Suprcode/Crystal.MapEditor) 修改它们
- 数据库文件（DB、ZIR 格式）单独管理 - 参见 [Crystal.Database](https://github.com/Suprcode/Crystal.Database)
