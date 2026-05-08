using Robust.Shared.GameStates;

namespace Content.Shared._Stalker_DeathmatchArena.MapText;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MapTextOverrideComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    [AutoNetworkedField]
    public string Text = "";
}
