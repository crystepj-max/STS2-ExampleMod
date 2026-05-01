using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;

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
/// 每帧检测Patch - 检测回合切换并在回合结束时显示伤害统计
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), "_Process")]
public class CombatProcessDamagePatch
{
    static void Postfix(NCombatRoom __instance, double delta)
    {
        try
        {
            DamageTracker.OnProcess();
        }
        catch (Exception ex)
        {
            // 避免频繁打印错误影响性能
            if (DamageTracker.TurnCount == 0)
            {
                GD.PrintErr($"[CombatProcessDamagePatch] 错误: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// 战斗结束Patch - 清理状态
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), "EndCombat")]
public class CombatEndDamagePatch
{
    static void Prefix(NCombatRoom __instance)
    {
        try
        {
            DamageTracker.OnCombatEnd();
            GD.Print("[DamagePatch] 战斗结束，伤害统计已清理");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CombatEndDamagePatch] 错误: {ex}");
        }
    }
}