using System;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace ExampleMod.Patches;

/// <summary>
/// 战斗开始Patch - 初始化伤害追踪
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), "_Ready")]
public class CombatStartDamagePatch
{
    static void Postfix(NCombatRoom __instance)
    {
        try
        {
            DamageTracker.OnCombatStart(__instance);
            GD.Print("[DamagePatch] 战斗开始，伤害追踪已初始化");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CombatStartDamagePatch] 错误: {ex}");
        }
    }
}

/// <summary>
/// 简化版：直接在战斗结束时显示统计
/// 使用NCombatRoom的结束方法作为触发点
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), "EndCombat")]
public class CombatEndDamagePatch
{
    static void Prefix(NCombatRoom __instance)
    {
        try
        {
            DamageTracker.OnPlayerTurnEnd();
            GD.Print("[DamagePatch] 战斗结束，显示最终伤害统计");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CombatEndDamagePatch] 错误: {ex}");
        }
    }
}