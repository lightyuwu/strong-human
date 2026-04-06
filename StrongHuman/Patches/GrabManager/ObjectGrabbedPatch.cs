using HarmonyLib;
using UnityEngine;
using MGrabManager = global::GrabManager;

namespace StrongHuman.Patches.GrabManager
{
    [HarmonyPatch(typeof(MGrabManager), nameof(MGrabManager.ObjectGrabbed))]
    public class ObjectGrabbedPatch
    {
        static void Postfix(MGrabManager __instance, GameObject grabObject)
        {
            Debug.Log($"[Postfix] Grabbed: {grabObject.name}");
            var joint = grabObject.GetComponent<ConfigurableJoint>();
            var rb = grabObject.GetComponent<Rigidbody>();
            if (!joint)
            {
                if (!rb) return; // No Grabbable Physics object we can change strength for
                
                // Grabbable Simple object
                StrongHumanPlugin.RememberObject(rb);
            }
            else
            {
                if(rb)
                    StrongHumanPlugin.RememberJointObject(rb);
                else if (joint.connectedBody)
                    StrongHumanPlugin.RememberJointObject(joint.connectedBody);
            }
        }
    }
}