using Robust.Client.Graphics;

namespace Content.Client._Stalker_DeathmatchArena.BlurredSprite;

public sealed class BlurredSpriteOverlaySystem : EntitySystem
{
    [Dependency] private readonly IDependencyCollection _dependencyCollection = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlayManager.AddOverlay(new BlurredSpriteOverlay(_dependencyCollection));
    }

    public override void Shutdown()
    {
        _overlayManager.RemoveOverlay<BlurredSpriteOverlay>();
        base.Shutdown();
    }
}
