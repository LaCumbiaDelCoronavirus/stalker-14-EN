using Robust.Shared.GameStates;

namespace Content.Shared._Stalker_DeathmatchArena.Scorestreak;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ScorestreakComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [Access(typeof(ScorestreakSystem))]
    [AutoNetworkedField]
    public int Score = 0;
}

[ByRefEvent]
public record struct ScorestreakChangedEvent(int OldScore, int NewScore);
