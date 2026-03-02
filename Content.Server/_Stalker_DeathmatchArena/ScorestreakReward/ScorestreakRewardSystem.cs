using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Content.Shared._Stalker_DeathmatchArena.Scorestreak;
using Content.Shared.Administration.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;

namespace Content.Server._Stalker_DeathmatchArena.ScorestreakReward;

public sealed class ScorestreakRewardSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScorestreakRewardComponent, ScorestreakChangedEvent>(OnScore);
    }

    private void OnScore(Entity<ScorestreakRewardComponent> entity, ref ScorestreakChangedEvent args)
    {
        if (args.NewScore <= args.OldScore)
            return;

        _rejuvenateSystem.PerformRejuvenate(entity.Owner);
        _popupSystem.PopupEntity("You are rejuvenated!", entity, entity, PopupType.Medium);

        if (args.NewScore >= entity.Comp.MinGoodScore)
            _chatManager.DispatchServerAnnouncement($"{Identity.Name(entity.Owner, EntityManager)} has a scorestreak of {args.NewScore}!");
    }
}
