using System;
using System.Linq;
using Godot;

namespace Irrelephant.Outcast.Client.Entities;

public partial class Entity : Node3D
{
    private AnimationPlayer? _animationPlayer;

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
        _animationPlayer?.Play("Run");
    }

    public void NotifyMovementEnd()
    {
        _animationPlayer?.Play("Idle");
    }
}
