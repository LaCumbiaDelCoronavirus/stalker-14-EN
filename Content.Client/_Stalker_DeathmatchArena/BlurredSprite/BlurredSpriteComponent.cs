namespace Content.Client._Stalker_DeathmatchArena.BlurredSprite;

[RegisterComponent]
public sealed partial class BlurredSpriteComponent : Component
{
    /// <summary>
    ///     Layers to render.
    /// </summary>
    [DataField]
    public List<PrototypeLayerData> Layers = new();
}
