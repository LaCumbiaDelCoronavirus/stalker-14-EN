using Content.Shared.CombatMode.Pacification;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Shared._Stalker_DeathmatchArena.BlockPacifiedUse;

public sealed class BlockPacifiedUseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private EntityQuery<PacifiedComponent> _pacifiedQuery;

    public override void Initialize()
    {
        base.Initialize();
        _pacifiedQuery = GetEntityQuery<PacifiedComponent>();

        SubscribeLocalEvent<BlockPacifiedUseComponent, AttemptUseInHandEvent>(OnAttemptUse);
    }

    private void OnAttemptUse(Entity<BlockPacifiedUseComponent> entity, ref AttemptUseInHandEvent args)
    {
        if (!_pacifiedQuery.HasComponent(args.User))
            return;

        args.Cancelled = true;
        _popupSystem.PopupClient("You may not use this, you are pacified!", args.User, args.User, PopupType.MediumCaution);
    }
}
