using Godot;
using Irrelephant.Outcast.Client.Constants;
using Irrelephant.Outcast.Client.Entities;
using Irrelephant.Outcast.Client.Networking;

namespace Irrelephant.Outcast.Client.Ui.Camera;

public partial class CameraController : Camera3D
{
    [Export] public float CameraMaxRange { get; set; } = 15f;
    [Export] public float CameraMinRange { get; set; } = 3f;
    [Export] public float AnglePerPercentViewportAzimuth { get; set; } = 4f;
    [Export] public float AnglePerPercentViewportPitch { get; set; } = 2f;
    [Export] public float ZoomSpeedPerStep { get; set; } = 0.2f;
    [Export] public Node3D? Anchor { get; set; }

    private float _desiredAzimuth;
    private float _desiredPitch;
    private float _desiredDistance;

    private bool _unlocked;

    private Vector2 _viewportSize;
    private Vector2 _lockedMousePosition;

    private Vector3 _lastTargetPosition;

    private const float RaycastLength = 1500f;

    [Signal]
    public delegate void OnLeftClickEventHandler(Vector3 position);

    [Signal]
    public delegate void OnEntityClickEventHandler(NetworkedEntity entity);

    public override void _Ready()
    {
        var viewport = GetViewport();
        _viewportSize = viewport.GetVisibleRect().Size;
        viewport.SizeChanged += () => { _viewportSize = viewport.GetVisibleRect().Size; };

        _desiredAzimuth = 0f;
        _desiredPitch = -45f;
        _desiredDistance = Mathf.Lerp(CameraMinRange, CameraMaxRange, 0.5f);

        base._Ready();
    }

    public override void _Process(double delta)
    {
        if (Anchor is null)
        {
            return;
        }

        var offset = Vector3.Back
            .Rotated(Vector3.Right, Mathf.DegToRad(_desiredPitch))
            .Rotated(Vector3.Up, Mathf.DegToRad(_desiredAzimuth))
            * _desiredDistance;

        LookAtFromPosition(
            position: Position.Lerp(Anchor.Position + offset, 0.8f),
            target: _lastTargetPosition.Lerp(
                Anchor.Position + Vector3.Up,
                0.7f
            ),
            Vector3.Up
        );

        _lastTargetPosition = Anchor.Position;

        base._Process(delta);
    }

    public override void _Input(InputEvent evt)
    {
        if (evt.IsCanceled() || Anchor is null)
        {
            return;
        }

        if (evt is InputEventMouseButton { ButtonIndex: MouseButton.Right } buttonEvent)
        {
            HandleEnableOrbit(buttonEvent);
        }

        if (_unlocked && evt is InputEventMouseMotion motion)
        {
            HandleMouseMove(motion);
        }

        if (evt is InputEventMouseButton { ButtonIndex: MouseButton.WheelUp or MouseButton.WheelDown} wheelEvent)
        {
            HandleZoom(wheelEvent);
        }

        if (evt is InputEventMouseButton { ButtonIndex: MouseButton.Left } clickEvent)
        {
            HandleLeftClickCommand(clickEvent);
        }

        base._Input(evt);
    }

    private void HandleLeftClickCommand(InputEventMouseButton clickEvent)
    {
        var from = ProjectRayOrigin(clickEvent.Position);
        var to = from + ProjectRayNormal(clickEvent.Position) * RaycastLength;
        var state = GetWorld3D().DirectSpaceState;

        var raycastQuery = PhysicsRayQueryParameters3D.Create(
            from, to,
            collisionMask: Collision.GeoData | Collision.Entities
        );
        var result = state.IntersectRay(raycastQuery);
        if (result.Count > 0)
        {
            var clickedCollider = result["collider"].Obj;
            if (clickedCollider is CharacterBody3D { CollisionLayer: Collision.Entities } body)
            {
                EmitSignalOnEntityClick(body.GetParent<NetworkedEntity>());
            }

            EmitSignal(SignalName.OnLeftClick, result["position"]);
        }
    }

    private void HandleZoom(InputEventMouseButton wheelEvent)
    {
        _desiredDistance = Mathf.Max(
            CameraMinRange,
            _desiredDistance + ZoomSpeedPerStep * (wheelEvent.ButtonIndex is MouseButton.WheelUp ? -1 : +1)
        );
    }

    private void HandleMouseMove(InputEventMouseMotion motion)
    {
        _desiredAzimuth -= motion.Relative.X / _viewportSize.X * 100 * AnglePerPercentViewportAzimuth;
        _desiredPitch = Mathf.Clamp(
            _desiredPitch - motion.Relative.Y / _viewportSize.Y * 100 * AnglePerPercentViewportPitch,
            -80f,
            +10f
        );
    }

    private void HandleEnableOrbit(InputEventMouseButton buttonEvent)
    {
        _unlocked = buttonEvent.Pressed;
        Input.MouseMode = _unlocked ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
        if (_unlocked)
        {
            _lockedMousePosition = buttonEvent.Position;
        }
        else
        {
            GetViewport().WarpMouse(_lockedMousePosition);
        }
    }
}
