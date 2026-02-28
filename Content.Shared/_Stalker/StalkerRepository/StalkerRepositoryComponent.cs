using Content.Shared.Whitelist;

namespace Content.Shared._Stalker.StalkerRepository;

[RegisterComponent]
public sealed partial class StalkerRepositoryComponent : Component
{
    // ST14EN-ARENA-DEATMATCH
    [DataField]
    public bool LoadoutsCloneItems = false;

    [DataField("Owner")]
    public string StorageOwner = "";

    [DataField("LoadedDBJson")]
    public string LoadedDbJson = "";

    [ViewVariables]
    public List<RepositoryItemInfo> ContainedItems = new();

    [DataField("whitelist")] public EntityWhitelist? Whitelist;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float MaxWeight = 75f; // kilos

    public float CurrentWeight = 0f;

    /// <summary>
    /// Set to true during async loadout operations to prevent race conditions
    /// with manual stash operations (eject/inject).
    /// </summary>
    [ViewVariables]
    public bool LoadoutOperationInProgress;
}
/// <summary>
/// Raised on user entity
/// </summary>
public sealed class RepositoryItemInjectedEvent : HandledEntityEventArgs
{
    public EntityUid RepositoryEnt;
    public RepositoryItemInfo Item;

    public RepositoryItemInjectedEvent(EntityUid repositoryEnt, RepositoryItemInfo item)
    {
        RepositoryEnt = repositoryEnt;
        Item = item;
    }
}
