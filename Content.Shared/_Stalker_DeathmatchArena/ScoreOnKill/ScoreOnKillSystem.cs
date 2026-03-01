using Content.Shared._Stalker_DeathmatchArena.Scorestreak;
using Content.Shared.Mobs;
using Robust.Shared.Player;

namespace Content.Shared._Stalker_DeathmatchArena.RejuvenateOnKill;

public sealed class RejuvenateOnKillSystem : EntitySystem
{
    [Dependency] private readonly ScorestreakSystem _scorestreakSystem = default!;

    private EntityQuery<ActorComponent> _actorQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent args) // not by-ref
    {
        if (args.Origin == args.Target ||
            args.Origin is not { } originUid) // GG
            return;

        if (!_actorQuery.HasComponent(args.Target))
            return;

        if (args.NewMobState == args.OldMobState ||
            args.NewMobState != MobState.Dead)
            return;

        _scorestreakSystem.AddScore(originUid, 1);
    }
}
