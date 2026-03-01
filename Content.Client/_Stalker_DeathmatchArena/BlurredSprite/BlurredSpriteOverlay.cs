using System.Numerics;
using Content.Client.Graphics;
using Content.Client.Light;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Stalker_DeathmatchArena.BlurredSprite;

public sealed class BlurredSpriteOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> MixShaderId = "Mix";

    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    private readonly TransformSystem _transformSystem = default!;
    private readonly SpriteSystem _spriteSystem = default!;

    private readonly OverlayResourceCache<CachedResources> _resources = new();
    public override OverlaySpace Space => OverlaySpace.BeforeLighting;

    public BlurredSpriteOverlay(IDependencyCollection dependencyCollection)
    {
        dependencyCollection.InjectDependencies(this);

        _entityManager.EntitySysManager.Resolve(ref _transformSystem);
        _entityManager.EntitySysManager.Resolve(ref _spriteSystem);
        ZIndex = AfterLightTargetOverlay.ContentZIndex + 1;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        foreach (var _ in _entityManager.EntityQuery<BlurredSpriteComponent>(includePaused: false))
            return true;

        return false;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = args.Viewport;
        if (viewport.Eye is not { } eye)
            return;

        var res = _resources.GetForViewport(args.Viewport, static _ => new CachedResources());
        var targetSize = viewport.LightRenderTarget.Size;

        if (res.Target?.Size != targetSize)
        {
            res.Target = _clyde.CreateRenderTarget(targetSize, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "stda-shadow-target");

            if (res.BlurTarget?.Size != targetSize)
                res.BlurTarget = _clyde.CreateRenderTarget(targetSize, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "stda-shadow-blur");
        }

        var lightScale = viewport.LightRenderTarget.Size / (Vector2)viewport.Size;
        var scale = viewport.RenderScale / (Vector2.One / lightScale);
        var worldBounds = args.WorldBounds;

        var scaleMatrix = Matrix3x2.CreateScale(Vector2.One); // scale
        var rotationMatrix = Matrix3x2.CreateRotation(-(float)eye.Rotation.Degrees);

        var worldHandle = args.WorldHandle;
        worldHandle.RenderInRenderTarget(res.Target,
            () =>
            {
                var shadowEnumerator = _entityManager.EntityQueryEnumerator<BlurredSpriteComponent, TransformComponent>();
                while (shadowEnumerator.MoveNext(out var spriteComponent, out var transformComponent))
                {
                    var matrix = _transformSystem.GetWorldPositionRotationMatrix(transformComponent).WorldMatrix;
                    worldHandle.SetTransform(matrix);

                    foreach (var layer in spriteComponent.Layers)
                    {
                        SpriteSpecifier specifier;
                        if (layer is { RsiPath: not null, State: not null })
                            specifier = new SpriteSpecifier.Rsi(new ResPath(layer.RsiPath), layer.State);
                        else if (layer.TexturePath is { } texturePath)
                            specifier = new SpriteSpecifier.Texture(new ResPath(texturePath));
                        else
                            continue;

                        worldHandle.DrawTexture(_spriteSystem.GetFrame(specifier, _gameTiming.RealTime, loop: true), Vector2.Zero);
                    }
                }
            },
            Color.Transparent);

        worldHandle.SetTransform(Matrix3x2.Identity);
        var maskShader = _prototypeManager.Index(MixShaderId).Instance();

        _clyde.BlurRenderTarget(viewport, res.Target, res.BlurTarget!, eye, 1f);

        // Draw stencil (see roofoverlay).
        worldHandle.UseShader(maskShader);
        worldHandle.RenderInRenderTarget(viewport.LightRenderTarget,
            () =>
            {
                var invMatrix =
                    viewport.LightRenderTarget.GetWorldToLocalMatrix(eye, scale);
                worldHandle.SetTransform(invMatrix);

                worldHandle.DrawTextureRect(res.Target.Texture, worldBounds, Color.Black.WithAlpha(0.5f));
            }, null);

        worldHandle.UseShader(null);
        worldHandle.SetTransform(Matrix3x2.Identity);
    }

    private sealed class CachedResources : IDisposable
    {
        public IRenderTexture? BlurTarget;
        public IRenderTexture? Target;

        public void Dispose()
        {
            BlurTarget?.Dispose();
            Target?.Dispose();
        }
    }
}
