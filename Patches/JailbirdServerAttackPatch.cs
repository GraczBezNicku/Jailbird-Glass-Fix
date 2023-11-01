using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Jailbird;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using RelativePositioning;
using UnityEngine;
using Utils.Networking;
using Utils.NonAllocLINQ;

namespace JailbirdGlassFix.Patches;

// I'm not good with IL, so have this pretty prefix instead :D

[HarmonyPatch(typeof(JailbirdHitreg), nameof(JailbirdHitreg.ServerAttack))]
public static class JailbirdServerAttackPatch
{
    public static bool Prefix(ref bool __result, JailbirdHitreg __instance, bool isCharging, NetworkReader reader)
    {
        ReferenceHub owner = __instance._item.Owner;
        bool result = false;
        if (reader != null)
        {
            RelativePosition relativePosition = reader.ReadRelativePosition();
            Quaternion claimedRot = reader.ReadQuaternion();
            JailbirdHitreg.BacktrackedPlayers.Add(new FpcBacktracker(owner, relativePosition.Position, claimedRot, 0.1f, 0.15f));
            byte b = reader.ReadByte();
            for (int i = 0; i < (int)b; i++)
            {
                try
                {
                    ReferenceHub target;
                    bool flag = reader.TryReadReferenceHub(out target);
                    RelativePosition relativePosition2 = reader.ReadRelativePosition();
                    if (flag)
                    {
                        JailbirdHitreg.BacktrackedPlayers.Add(new FpcBacktracker(target, relativePosition2.Position, 0.4f));
                    }
                }
                catch
                {
                    // Ignore.
                }
            }
        }
        __instance.DetectDestructibles();
        Vector3 forward = __instance._item.Owner.PlayerCameraReference.forward;
        float num = isCharging ? __instance._damageCharge : __instance._damageMelee;
        for (int j = 0; j < JailbirdHitreg._detectionsLen; j++)
        {
            IDestructible destructible = JailbirdHitreg.DetectedDestructibles[j];
            if (destructible.Damage(num, new JailbirdDamageHandler(owner, num, forward), destructible.CenterOfMass))
            {
                result = true;
                if (!isCharging)
                {
                    __instance.TotalMeleeDamageDealt += num;
                }
                else
                {
                    HitboxIdentity hitboxIdentity = destructible as HitboxIdentity;
                    if (hitboxIdentity != null)
                    {
                        hitboxIdentity.TargetHub.playerEffectsController.EnableEffect<Flashed>(__instance._flashDuration, true);
                    }
                }
            }
        }
        JailbirdHitreg.BacktrackedPlayers.ForEach(delegate (FpcBacktracker x)
        {
            x.RestorePosition();
        });
        JailbirdHitreg.BacktrackedPlayers.Clear();
        __result = result;

        return false;
    }
}
