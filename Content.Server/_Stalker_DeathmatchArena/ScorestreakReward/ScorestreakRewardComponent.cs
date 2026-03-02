using Robust.Shared.GameStates;

namespace Content.Server._Stalker_DeathmatchArena.ScorestreakReward;

[RegisterComponent, NetworkedComponent]
public sealed partial class ScorestreakRewardComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinGoodScore = 3;
}
