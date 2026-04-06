using HarmonyLib;
using UnityEngine;
using MGrabManager = global::GrabManager;

namespace StrongHuman.Patches.GrabManager
{
    [HarmonyPatch(typeof(MGrabManager), nameof(MGrabManager.ObjectReleased))]
    public class ObjectReleasedPatch
    {
        static void Postfix(MGrabManager __instance, GameObject grabObject)
        {
            Debug.Log($"[Postfix] Released: {grabObject.name}");
            var joint = grabObject.GetComponent<ConfigurableJoint>();
            var rb = grabObject.GetComponent<Rigidbody>();
            if (!joint)
            {
                if (!rb) return; // No Grabbable Physics object we can change strength for
                
                // Grabbable Simple object
                StrongHumanPlugin.ForgetObject(rb);
            }
            else
            {
                if(rb)
                    StrongHumanPlugin.ForgetJointObject(rb);
                else if (joint.connectedBody)
                    StrongHumanPlugin.ForgetJointObject(joint.connectedBody);
            }
        }
    }
}