using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Content.Shared._Stalker_DeathmatchArena.Scorestreak;
using Content.Shared.Administration.Systems;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server._Stalker_DeathmatchArena.ScorestreakReward;

public sealed class ScorestreakRewardSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly ActorSystem _actorSystem = default!;
    [Dependency] private readonly ScorestreakSystem _scorestreakSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScorestreakRewardComponent, ScorestreakChangedEvent>(OnScore);
        SubscribeLocalEvent<ScorestreakRewardComponent, MobStateChangedEvent>(OnStateChanged);
    }

    private string GetName(EntityUid uid) => _actorSystem.GetSession(uid)?.Name ?? $"{Name(uid)} [non-player]";

    private void OnScore(Entity<ScorestreakRewardComponent> entity, ref ScorestreakChangedEvent args)
    {
        if (args.NewScore <= args.OldScore)
            return;

        _rejuvenateSystem.PerformRejuvenate(entity.Owner);
        _popupSystem.PopupEntity("You are rejuvenated!", entity, entity, PopupType.Medium);

        if (args.NewScore >= entity.Comp.MinScore)
            _chatManager.DispatchServerAnnouncement($"{GetName(entity.Owner)} has a scorestreak of {args.NewScore}!");
    }

    private void OnStateChanged(Entity<ScorestreakRewardComponent> targetEntity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead ||
            args.Origin is not { } originUid)
            return;

        var score = _scorestreakSystem.GetScorestreak(targetEntity.Owner);
        _scorestreakSystem.SetScore(targetEntity.Owner, 0);
        if (score < targetEntity.Comp.MinScore)
            return;

        var originName = GetName(originUid);
        var targetName = GetName(targetEntity.Owner);

        _chatManager.DispatchServerAnnouncement($"{originName} just ended {targetName}s scorestreak of {score}!");
    }
}
