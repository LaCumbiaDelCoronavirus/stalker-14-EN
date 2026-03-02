using System.Runtime.InteropServices;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    protected override void InitializeBallistic()
    {
        base.InitializeBallistic();
        SubscribeLocalEvent<BallisticAmmoProviderComponent, UpdateAmmoCounterEvent>(OnBallisticAmmoCount);
    }

    private void OnBallisticAmmoCount(EntityUid uid, BallisticAmmoProviderComponent component, UpdateAmmoCounterEvent args)
    {
        if (args.Control is DefaultStatusControl control)
        {
            control.Update(GetBallisticShots(component), component.Capacity);
        }
    }

    protected override void Cycle(EntityUid uid, BallisticAmmoProviderComponent component, MapCoordinates coordinates)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        EntityUid? ent = null;

        // TODO: Combine with TakeAmmo
        if (component.Entities.Count > 0)
        {
            var existing = component.Entities[^1];
            component.Entities.RemoveAt(component.Entities.Count - 1);

            // An unhandled exception of type 'System.ArgumentOutOfRangeException' occurred in System.Private.CoreLib.dll: 'Index was out of range. Must be non-negative and less than the size of the collection.'
            // An unhandled exception of type 'System.ArgumentOutOfRangeException' occurred in System.Private.CoreLib.dll: 'Index was out of range. Must be non-negative and less than the size of the collection.'
            // An unhandled exception of type 'System.ArgumentOutOfRangeException' occurred in System.Private.CoreLib.dll: 'Index was out of range. Must be non-negative and less than the size of the collection.'
            // An unhandled exception of type 'System.ArgumentOutOfRangeException' occurred in System.Private.CoreLib.dll: 'Index was out of range. Must be non-negative and less than the size of the collection.'
            // An unhandled exception of type 'System.ArgumentOutOfRangeException' occurred in System.Private.CoreLib.dll: 'Index was out of range. Must be non-negative and less than the size of the collection.'
            // GG, STALKERCODERS
            //component.EntProtos.RemoveAt(component.EntProtos.Count - 1); // stalker-changes
            // STDA fix:
            var epI = component.EntProtos.Count - 1;
            if (component.EntProtos.Count <= epI)
                component.EntProtos.RemoveAt(epI);

            Containers.Remove(existing, component.Container);
            EnsureShootable(existing);
        }
        else if (component.UnspawnedCount > 0)
        {
            component.UnspawnedCount--;
            var proto = component.EntProtos.Count > 0 // stalker-changes-start
                ? (Robust.Shared.Prototypes.EntProtoId?)component.EntProtos[^1]
                : null;
            if (proto != null)
            {
                ent = Spawn(proto.Value, coordinates);
                EnsureShootable(ent.Value);
                component.EntProtos.RemoveAt(component.EntProtos.Count - 1);
            }
            else
            {
                ent = Spawn(component.Proto, coordinates);
                EnsureShootable(ent.Value);
            } // stalker-changes-end
        }

        if (ent != null && IsClientSide(ent.Value))
            Del(ent.Value);

        var cycledEvent = new GunCycledEvent();
        RaiseLocalEvent(uid, ref cycledEvent);
    }
}
