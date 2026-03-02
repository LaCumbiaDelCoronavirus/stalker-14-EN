namespace Content.Server._Stalker_DeathmatchArena.ScorestreakReward;

[RegisterComponent]
public sealed partial class ScorestreakRewardComponent : Component
{
    [DataField, ViewVariables]
    public int MinScore = 3;
}
