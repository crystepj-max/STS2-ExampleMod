# Agent 协作入口

本仓库是 Slay the Spire 2 Mod 基础模板。`README.md` 负责项目介绍、构建命令和手动安装步骤；本文件提供代码地图及验证边界。

## 项目地图

- `Entry.cs`：Mod 初始化入口，并包含战斗房间开始时的示例提示补丁。
- `DamageTracker.cs`：伤害追踪、回合统计和界面显示逻辑。
- `Patches/DamagePatch.cs`：连接战斗房间生命周期与伤害追踪器。
- `Patches/ExamplePatch.cs`：Harmony 补丁示例说明；示例代码当前以注释形式保留。
- `Relics/ExampleRelic.cs`：遗物示例占位；当前实现以注释形式保留。
- `ExampleMod.csproj`：目标框架、游戏引用和本机游戏路径配置。
- `modid.json`：Mod 元数据。
- `project/project.godot`：Godot 项目配置。

具体行为以当前源码和配置为准；文件或目录存在不代表对应内容已在游戏中运行验收。

## 验证边界

- 按 `README.md` 中的命令执行构建。构建结果只证明该次编译步骤的结果。
- README 中的文件复制命令是单独的手动部署步骤；不要把构建成功描述为自动安装或已部署。
- Mod 加载、Patch 效果和游戏内行为需要在真实游戏环境中单独验证。

修改构建或安装流程时，同步更新 `README.md` 中对应说明。
