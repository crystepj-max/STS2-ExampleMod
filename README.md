# ExampleMod - StS2 Mod项目模板

这是一个杀戮尖塔2 mod开发的基础模板，展示mod的基本结构和Harmony补丁用法。

## 项目结构

```
ExampleMod/
├── modid.json          # Mod配置文件（必需）
├── ExampleMod.csproj   # .NET项目配置
├── Entry.cs            # Mod入口初始化
├── Patches/
│   └── ExamplePatch.cs # Harmony补丁示例
├── Relics/
│   └── ExampleRelic.cs # 遗物示例（占位）
└── README.md           # 说明文档
```

## 核心命名空间

从sts2.dll中的实际命名空间：

| 命名空间 | 内容 |
|----------|------|
| `MegaCrit.Sts2.Core.Modding` | Mod初始化API |
| `MegaCrit.Sts2.Core.Entities.Cards` | 卡牌实体 |
| `MegaCrit.Sts2.Core.Entities.Creatures` | 生物（敌人/玩家） |
| `MegaCrit.Sts2.Core.Entities.Players` | 玩家 |
| `MegaCrit.Sts2.Core.Entities.Potions` | 药水 |
| `MegaCrit.Sts2.Core.Combat` | 战斗系统 |
| `MegaCrit.Sts2.Core.Runs` | 运行管理 |
| `MegaCrit.Sts2.Core.Nodes.*` | Godot节点 |
| `HarmonyLib` | 代码补丁库 |
| `Godot` | Godot引擎API |

## 编译步骤

```bash
cd ~/STS2Mods/ExampleMod

# 恢复依赖
dotnet restore

# 编译（Release模式）
dotnet build -c Release
```

## 安装到游戏

```bash
# 目标目录
MODS_DIR=~/Library/Application\ Support/Steam/steamapps/common/Slay\ the\ Spire\ 2/SlayTheSpire2.app/Contents/MacOS/mods

# 复制DLL
cp bin/Release/net9.0/ExampleMod.dll "$MODS_DIR/"

# 复制配置文件（文件名=modid.json中的id+.json）
cp modid.json "$MODS_DIR/ExampleMod.json"
```

## 在游戏中启用

1. 启动游戏
2. 主菜单 → Mods
3. 找到 "Example Mod" 并启用
4. 重启游戏让Godot编译新mod

## Harmony补丁示例

```csharp
// 在战斗开始时输出日志
[HarmonyPatch(typeof(CombatController), "StartCombat")]
public static class CombatStartPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        GD.Print("[ExampleMod] 战斗开始！");
    }
}
```

## 下一步开发

1. 使用ILSpy反编译 `sts2.dll` 查看游戏API
2. 参考 `~/STS2MCP/` 项目查看实际mod代码
3. 加入Discord #sts2-modding 频道交流

## 参考资源

- STS2MCP项目: ~/STS2MCP/ (实际可运行的mod示例)
- 游戏DLL: `.../SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll`
- 开发文档: https://zcnhvtwb47ab.feishu.cn/docx/JCFsd03ZsookoSxAj5fcEWyYndc
- Spire Codex: https://spire-codex.com (游戏数据API)
