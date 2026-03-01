using Robust.Shared.GameStates;

namespace Content.Server._Stalker_DeathmatchArena.Scorestreak;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class ScorestreakRewardComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinGoodScore = 3;
}
