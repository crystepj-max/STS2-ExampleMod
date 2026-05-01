using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;

namespace ExampleMod;

/// <summary>
/// 伤害追踪器 - 记录玩家每回合造成的伤害并在回合结束时显示统计
/// </summary>
public static class DamageTracker
{
    // 伤害记录
    private static Dictionary<int, int> _enemyHpAtTurnStart = new();
    private static int _totalDamageThisCombat = 0;
    private static int _turnCount = 0;
    private static NCombatRoom? _currentCombatRoom = null;
    private static CombatSide _lastSide = CombatSide.Enemy; // 初始设为Enemy，这样第一个玩家回合开始时就会触发记录
    private static bool _isInitialized = false;
    
    /// <summary>
    /// 战斗开始时初始化
    /// </summary>
    public static void OnCombatStart(NCombatRoom combatRoom)
    {
        _enemyHpAtTurnStart.Clear();
        _totalDamageThisCombat = 0;
        _turnCount = 0;
        _currentCombatRoom = combatRoom;
        _lastSide = CombatSide.Enemy;
        _isInitialized = true;
        
        GD.Print("[DamageTracker] 战斗开始，等待玩家回合...");
    }
    
    /// <summary>
    /// 每帧检查回合状态变化
    /// </summary>
    public static void OnProcess()
    {
        if (!_isInitialized || _currentCombatRoom == null) return;
        
        // 获取当前战斗状态
        var combatState = BetaMainCompatibility.Creature_.WrappedCombatState(_currentCombatRoom.Player?.Creature);
        if (combatState == null) return;
        
        var currentSide = combatState.CurrentSide;
        
        // 检测回合切换：玩家回合结束（Player -> Enemy）
        if (_lastSide == CombatSide.Player && currentSide == CombatSide.Enemy)
        {
            OnPlayerTurnEnd();
        }
        
        // 检测回合切换：玩家回合开始（Enemy -> Player）
        if (_lastSide == CombatSide.Enemy && currentSide == CombatSide.Player)
        {
            OnPlayerTurnStart(combatState);
        }
        
        _lastSide = currentSide;
    }
    
    /// <summary>
    /// 玩家回合开始时记录敌人初始HP
    /// </summary>
    private static void OnPlayerTurnStart(CombatStateWrapper combatState)
    {
        _turnCount++;
        _enemyHpAtTurnStart.Clear();
        
        // 记录所有敌人的当前HP
        foreach (var enemy in combatState.Enemies)
        {
            if (enemy.IsAlive)
            {
                int id = enemy.GetHashCode();
                int hp = enemy.CurrentHp;
                _enemyHpAtTurnStart[id] = hp;
                GD.Print($"[DamageTracker] 回合 {_turnCount} 开始 - 记录敌人 ID={id}, HP={hp}");
            }
        }
        
        GD.Print($"[DamageTracker] ========== 回合 {_turnCount} 开始 ========== ");
    }
    
    /// <summary>
    /// 玩家回合结束时计算并显示伤害统计
    /// </summary>
    private static void OnPlayerTurnEnd()
    {
        if (_currentCombatRoom == null) return;
        
        // 计算本回合伤害（敌人HP减少量）
        int turnDamage = CalculateTurnDamage();
        _totalDamageThisCombat += turnDamage;
        
        GD.Print($"[DamageTracker] ========== 回合 {_turnCount} 结束 ========== ");
        GD.Print($"[DamageTracker] 本回合伤害: {turnDamage}, 总伤害: {_totalDamageThisCombat}");
        
        // 显示伤害统计
        ShowDamageStats(turnDamage);
    }
    
    /// <summary>
    /// 计算本回合伤害
    /// </summary>
    private static int CalculateTurnDamage()
    {
        int totalDamage = 0;
        
        if (_currentCombatRoom == null) return 0;
        
        var combatState = BetaMainCompatibility.Creature_.WrappedCombatState(_currentCombatRoom.Player?.Creature);
        if (combatState == null) return 0;
        
        // 遍历敌人，计算HP变化
        foreach (var enemy in combatState.Enemies)
        {
            int id = enemy.GetHashCode();
            
            if (_enemyHpAtTurnStart.TryGetValue(id, out int startHp))
            {
                int currentHp = enemy.IsAlive ? enemy.CurrentHp : 0;
                int damage = startHp - currentHp;
                
                if (damage > 0)
                {
                    totalDamage += damage;
                    GD.Print($"[DamageTracker] 敌人 ID={id}: {startHp} -> {currentHp}, 伤害={damage}");
                }
            }
        }
        
        return totalDamage;
    }
    
    /// <summary>
    /// 显示伤害统计
    /// </summary>
    private static void ShowDamageStats(int turnDamage)
    {
        if (_currentCombatRoom == null) return;
        
        // 移除旧的标签（如果存在）
        var oldLabel = _currentCombatRoom.FindChild("DamageStatsLabel", true, false);
        if (oldLabel != null)
        {
            oldLabel.QueueFree();
        }
        
        // 创建新标签
        var label = new Label();
        label.Name = "DamageStatsLabel";
        
        // 设置样式
        var settings = new LabelSettings();
        settings.FontColor = Colors.Yellow;
        settings.FontSize = 28;
        settings.OutlineColor = Colors.Black;
        settings.OutlineSize = 3;
        label.LabelSettings = settings;
        
        // 设置文本
        string statsText = $"回合 {_turnCount}\n本回合伤害: {turnDamage}\n总伤害: {_totalDamageThisCombat}";
        label.Text = statsText;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        
        // 设置位置 - 屏幕右上角
        var viewport = _currentCombatRoom.GetViewport();
        if (viewport != null)
        {
            var screenSize = viewport.GetVisibleRect().Size;
            label.Position = new Vector2(screenSize.X - 220, 100);
            label.Size = new Vector2(200, 80);
        }
        
        // 添加到场景
        _currentCombatRoom.AddChild(label);
        
        GD.Print($"[DamageTracker] 显示伤害统计: {statsText}");
        
        // 2秒后移除（比回合切换间隔短）
        var timer = _currentCombatRoom.GetTree().CreateTimer(2.0);
        timer.Timeout += () =>
        {
            try
            {
                if (label != null && IsInstanceValid(label))
                {
                    label.QueueFree();
                }
            }
            catch { }
        };
    }
    
    /// <summary>
    /// 战斗结束时清理
    /// </summary>
    public static void OnCombatEnd()
    {
        GD.Print($"[DamageTracker] 战斗结束 - 总回合数: {_turnCount}, 总伤害: {_totalDamageThisCombat}");
        _isInitialized = false;
        _currentCombatRoom = null;
    }
}