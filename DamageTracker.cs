using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ExampleMod;

/// <summary>
/// 伤害追踪器 - 记录玩家造成的伤害并在回合结束时显示统计
/// </summary>
public static class DamageTracker
{
    // 伤害记录
    private static Dictionary<int, int> _enemyStartHp = new(); // 使用creature ID作为key
    private static int _totalDamageThisCombat = 0;
    private static int _turnCount = 0;
    private static NCombatRoom? _currentCombatRoom = null;
    
    /// <summary>
    /// 战斗开始时初始化
    /// </summary>
    public static void OnCombatStart(NCombatRoom combatRoom)
    {
        _enemyStartHp.Clear();
        _totalDamageThisCombat = 0;
        _turnCount = 0;
        _currentCombatRoom = combatRoom;
        
        GD.Print($"[DamageTracker] 战斗开始");
    }
    
    /// <summary>
    /// 玩家回合结束时显示伤害统计
    /// </summary>
    public static void OnPlayerTurnEnd()
    {
        if (_currentCombatRoom == null) return;
        
        _turnCount++;
        
        // 计算本回合伤害（通过敌人HP变化）
        int turnDamage = CalculateTurnDamage();
        _totalDamageThisCombat += turnDamage;
        
        GD.Print($"[DamageTracker] 回合 {_turnCount} 结束，本回合伤害: {turnDamage}, 总伤害: {_totalDamageThisCombat}");
        
        // 显示伤害统计
        ShowDamageStats(turnDamage);
        
        // 更新敌人初始HP（用于下一回合计算）
        UpdateEnemyStartHp();
    }
    
    /// <summary>
    /// 计算本回合伤害
    /// </summary>
    private static int CalculateTurnDamage()
    {
        int totalDamage = 0;
        
        // 遍历所有敌人，计算HP变化
        // 由于我们无法直接获取CombatState，使用记录的数据
        foreach (var kvp in _enemyStartHp)
        {
            int startHp = kvp.Value;
            // 假设敌人死亡则伤害=初始HP
            // 这里简化处理，实际伤害需要追踪
            
            GD.Print($"[DamageTracker] 敌人ID {kvp.Key} 初始HP: {startHp}");
        }
        
        // 简化：返回一个估算值
        // 实际实现需要更复杂的追踪
        return totalDamage;
    }
    
    /// <summary>
    /// 更新敌人初始HP
    /// </summary>
    private static void UpdateEnemyStartHp()
    {
        // 清空并重新记录存活敌人的当前HP
        _enemyStartHp.Clear();
        
        if (_currentCombatRoom == null) return;
        
        // 遍历场景树查找敌人
        // 使用递归查找所有Creature节点
        FindAndRecordEnemies(_currentCombatRoom);
    }
    
    /// <summary>
    /// 递归查找敌人并记录HP
    /// </summary>
    private static void FindAndRecordEnemies(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            // 检查节点类型名称
            if (child.GetType().Name == "Creature")
            {
                try
                {
                    // 使用反射获取Side属性
                    var sideProp = child.GetType().GetProperty("Side");
                    var currentHpProp = child.GetType().GetProperty("CurrentHp");
                    var isAliveProp = child.GetType().GetProperty("IsAlive");
                    
                    if (sideProp != null && currentHpProp != null && isAliveProp != null)
                    {
                        var side = sideProp.GetValue(child);
                        var currentHp = currentHpProp.GetValue(child);
                        var isAlive = isAliveProp.GetValue(child);
                        
                        // 检查是否是敌人（Side != Player）
                        if (side != null && side.ToString() == "Enemy" && isAlive is bool alive && alive)
                        {
                            int hp = Convert.ToInt32(currentHp);
                            int id = child.GetHashCode(); // 使用HashCode作为ID
                            _enemyStartHp[id] = hp;
                            GD.Print($"[DamageTracker] 记录敌人 ID={id}, HP={hp}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[DamageTracker] 获取敌人信息失败: {ex.Message}");
                }
            }
            
            // 递归查找子节点
            FindAndRecordEnemies(child);
        }
    }
    
    /// <summary>
    /// 显示伤害统计
    /// </summary>
    private static void ShowDamageStats(int turnDamage)
    {
        if (_currentCombatRoom == null) return;
        
        // 创建标签
        var label = new Label();
        label.Name = "DamageStatsLabel";
        
        // 设置样式
        var settings = new LabelSettings();
        settings.FontColor = Colors.Yellow;
        settings.FontSize = 24;
        settings.OutlineColor = Colors.Black;
        settings.OutlineSize = 2;
        label.LabelSettings = settings;
        
        // 设置文本
        string statsText = $"⚔️ 回合 {_turnCount}\n总伤害: {_totalDamageThisCombat}";
        label.Text = statsText;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        
        // 设置位置 - 屏幕中央偏下
        var viewport = _currentCombatRoom.GetViewport();
        if (viewport != null)
        {
            var screenSize = viewport.GetVisibleRect().Size;
            label.Position = new Vector2(screenSize.X / 2 - 100, screenSize.Y / 2 + 100);
            label.Size = new Vector2(200, 50);
        }
        
        // 添加到场景
        _currentCombatRoom.AddChild(label);
        
        GD.Print($"[DamageTracker] 显示伤害统计: {statsText}");
        
        // 3秒后移除
        var timer = _currentCombatRoom.GetTree().CreateTimer(3.0);
        timer.Timeout += () =>
        {
            try
            {
                label.QueueFree();
                GD.Print($"[DamageTracker] 伤害统计已移除");
            }
            catch { }
        };
    }
}