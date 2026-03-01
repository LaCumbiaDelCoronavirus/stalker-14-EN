using Content.Shared.Administration.Systems;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Shared._Stalker_DeathmatchArena.RejuvenateOnKill;

public sealed class RejuvenateOnKillSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;

    private EntityQuery<ActorComponent> _actorQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(ref MobStateChangedEvent args)
    {
        if (!_actorQuery.HasComponent(args.Origin) ||
            !_actorQuery.HasComponent(args.Target))
            return;

        if (args.NewMobState == args.OldMobState ||
            args.NewMobState != MobState.Dead)
            return;

        _rejuvenateSystem.PerformRejuvenate(args.Origin.Value);
        _popupSystem.PopupClient("You are rejuvenated!", args.Origin.Value, args.Origin, PopupType.Medium);
    }
}
