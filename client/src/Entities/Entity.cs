using System;
using System.Linq;
using Godot;

namespace Irrelephant.Outcast.Client.Entities;

public partial class Entity : Node3D
{
    private AnimationPlayer? _animationPlayer;

    private int? _currentHealth;

    public int? CurrentHealth
    {
        get => _currentHealth;
        protected set
        {
            _currentHealth = value;
            if (value.HasValue)
            {
                EmitSignalOnCurrentHealthUpdated(value.Value);
            }
        }
    }

    private int? _maxHealth;
    public int? MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            if (value.HasValue)
            {
                EmitSignalOnMaxHealthUpdated(value.Value);
            }
        }
    }

    public bool IsInCombat { get; private set; } = false;

    [Signal]
    public delegate void OnCurrentHealthUpdatedEventHandler(int currentHealth);

    [Signal]
    public delegate void OnMaxHealthUpdatedEventHandler(int maxHealth);

    [Export]
    public Node3D? DisplayModel { get; set; }

    public override void _Ready()
    {
        if (DisplayModel is not null)
        {
            PrepareAnimationPlayer();
        }
        base._Ready();
    }

    private void PrepareAnimationPlayer()
    {
        _animationPlayer = DisplayModel?.GetChildren()
            .OfType<AnimationPlayer>()
            .FirstOrDefault();

        if (_animationPlayer is not null)
        {
            var allAnimations = _animationPlayer.GetAnimationList();

            var walkAnimations = allAnimations.Where(it => it.Contains("Walk", StringComparison.OrdinalIgnoreCase));
            var runAnimations = allAnimations.Where(it => it.Contains("Run", StringComparison.OrdinalIgnoreCase));
            var idleAnimationName = allAnimations.FirstOrDefault(it => it.Contains("Idle", StringComparison.OrdinalIgnoreCase));

            if (idleAnimationName is not null)
            {
                _animationPlayer.Play(idleAnimationName);
            }

            var allLooping = walkAnimations.Concat(runAnimations).Append(idleAnimationName);
            foreach (var loopingAnimationName in allLooping)
            {
                var animation = _animationPlayer.GetAnimation(loopingAnimationName!);
                animation.SetLoopMode(Animation.LoopModeEnum.Linear);
            }
        }
    }

    public void NotifyMovementStart()
    {
        if (IsInCombat)
        {
            _animationPlayer?.Play("Run_Combat");
        }
        else
        {
            _animationPlayer?.Play("Run");
        }
    }

    public void NotifyMovementEnd()
    {
        if (IsInCombat)
        {
            _animationPlayer?.Play("Idle_Combat");
        }
        else
        {
            _animationPlayer?.Play("Idle");
        }
    }

    public void NotifyAttack()
    {
        _animationPlayer?.Play("Attack_One_H_Slash");
    }

    public void NotifyDeath()
    {
        _animationPlayer?.Play("Death");
    }

    public void NotifyDead()
    {
        _animationPlayer?.Play("Death");
        _animationPlayer?.Seek(_animationPlayer.CurrentAnimationLength);
    }

    public void NotifyEnterCombat()
    {
        var current = _animationPlayer?.CurrentAnimation;
        if (current is "Idle")
        {
            _animationPlayer?.Play("Idle_Combat");
        }
        else if (current is "Run")
        {
            _animationPlayer?.Play("Run_Combat");
        }
        IsInCombat = true;
    }

    public void NotifyLeaveCombat()
    {
        var current = _animationPlayer?.CurrentAnimation;
        if (current is "Idle_Combat")
        {
            _animationPlayer?.Play("Idle");
        }
        else if (current is "Run_Combat")
        {
            _animationPlayer?.Play("Run");
        }
        IsInCombat = false;
    }
}
