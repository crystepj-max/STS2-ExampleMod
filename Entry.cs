using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ExampleMod
{
    /// <summary>
    /// 增强版Mod - 实现可见效果
    /// 功能：战斗开始时显示"为了部落"（红色大字）
    /// </summary>
    [ModInitializer("Initialize")]
    public static class Entry
    {
        public const string Version = "1.1.0";
        
        public static void Initialize()
        {
            try
            {
                GD.Print("[ExampleMod] 正在加载增强版...");
                GD.Print("[ExampleMod] 版本: " + Version);
                
                // 初始化Harmony
                var harmony = new Harmony("com.example.sts2mod");
                
                // 应用补丁
                harmony.PatchAll(typeof(Entry).Assembly);
                
                GD.Print("[ExampleMod] Harmony补丁已应用");
                GD.Print("[ExampleMod] 功能：战斗开始显示'为了部落'");
                GD.Print("[ExampleMod] 加载完成！");
            }
            catch (Exception ex)
            {
                GD.PrintErr("[ExampleMod] 加载失败: " + ex.Message);
                GD.PrintErr("[ExampleMod] 详细: " + ex.StackTrace);
            }
        }
    }
    
    /// <summary>
    /// 补丁: 战斗开始时显示"为了部落"
    /// 在NCombatRoom._Ready后触发
    /// </summary>
    [HarmonyPatch(typeof(NCombatRoom), "_Ready")]
    static class CombatStartPatch
    {
        static void Postfix(NCombatRoom __instance)
        {
            try
            {
                GD.Print("[ExampleMod] NCombatRoom._Ready触发！");
                GD.Print("[ExampleMod] 战斗开始！准备显示'为了部落'...");
                
                // 延迟显示文字（等待战斗UI初始化）
                __instance.GetTree().CreateTimer(0.5).Timeout += () =>
                {
                    try
                    {
                        ShowCombatStartMessage(__instance);
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr("[ExampleMod] 显示文字失败: " + ex.Message);
                    }
                };
            }
            catch (Exception ex)
            {
                GD.PrintErr("[ExampleMod] CombatStartPatch失败: " + ex.Message);
            }
        }
        
        static void ShowCombatStartMessage(NCombatRoom combatRoom)
        {
            try
            {
                GD.Print("[ExampleMod] 创建'为了部落'文字...");
                
                // 创建Label
                var label = new Label();
                label.Text = "为了部落！";
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.VerticalAlignment = VerticalAlignment.Center;
                
                // 红色大字样式
                var settings = new LabelSettings();
                settings.FontColor = new Color(1.0f, 0.2f, 0.2f); // 红色
                settings.FontSize = 32;
                settings.OutlineColor = new Color(0.1f, 0.1f, 0.1f);
                settings.OutlineSize = 3;
                label.LabelSettings = settings;
                
                // 位置（屏幕中央偏上）
                label.Position = new Vector2(0, 80);
                label.Size = new Vector2(800, 60);
                
                // 添加到场景
                combatRoom.AddChild(label);
                
                GD.Print("[ExampleMod] '为了部落'已显示！");
                
                // 3秒后移除
                var timer = combatRoom.GetTree().CreateTimer(3.0);
                timer.Timeout += () => 
                {
                    try 
                    {
                        label.QueueFree();
                        GD.Print("[ExampleMod] '为了部落'已移除");
                    } 
                    catch {}
                };
            }
            catch (Exception ex)
            {
                GD.PrintErr("[ExampleMod] 创建失败: " + ex.Message);
            }
        }
    }
}