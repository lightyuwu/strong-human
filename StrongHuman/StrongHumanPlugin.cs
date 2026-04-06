using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace StrongHuman
{
    [BepInPlugin(PluginInformation.Guid, PluginInformation.Name, PluginInformation.Version)]
    public class StrongHumanPlugin : BaseUnityPlugin
    {
        internal static float StrengthMultiplier = 1;
        public static void SetStrength(float newStrength)
        {
            StrengthMultiplier = newStrength;
            // Handle setting other objects

            foreach (var obj in StoredBodies)
            {
                obj.Key.mass = obj.Value / StrengthMultiplier; // Update currently held items to the same strength multiplier
            }
        }

        private static void ShowHelp()
        {
            Debug.Log($"[ StrongHuman ] {Command}\t\t- Get Current Strength Multiplier");
            Debug.Log($"[ StrongHuman ] {Command} [value]\t- Set Strength Multiplier");
            Debug.Log($"[ StrongHuman ] {Command} reset\t- Reset Strength Multiplier");
        }
        
        
        internal static readonly Dictionary<Rigidbody, float> StoredBodies = new Dictionary<Rigidbody, float>();

        public static void RememberObject(Rigidbody rb)
        {
            if (StoredBodies.ContainsKey(rb)) return;

            StoredBodies.Add(rb, rb.mass);
            // Change mass
            rb.mass /= StrengthMultiplier;
        }

        public static void ForgetObject(Rigidbody rb)
        {
            if (!StoredBodies.TryGetValue(rb, out var originalMass)) return;

            // Change mass
            rb.mass = originalMass;
            StoredBodies.Remove(rb);
        }
        
        public static void RememberJointObject(Rigidbody rb)
        {
            if (rb == null) return;
            if (StoredBodies.ContainsKey(rb)) return; // Already remembered

            if (rb.GetComponentInChildren<Human>() != null) return; // Don't allow touching humans
            
            // Reduce this mass only once
            StoredBodies.Add(rb, rb.mass);
            rb.mass /= StrengthMultiplier;

            // Optional: handle connectedBody if you really want chain effects
            var joint = rb.GetComponent<ConfigurableJoint>();
            if (joint?.connectedBody != null && !StoredBodies.ContainsKey(joint.connectedBody))
            {
                RememberJointObject(joint.connectedBody);
            }
        }
        
        public static void ForgetJointObject(Rigidbody rb)
        {
            if (rb == null) return;
            if (!StoredBodies.TryGetValue(rb, out var originalMass)) return;

            rb.mass = originalMass;
            StoredBodies.Remove(rb);

            var joint = rb.GetComponent<ConfigurableJoint>();
            if (joint?.connectedBody != null)
            {
                ForgetJointObject(joint.connectedBody);
            }
        }

        private const string Command = "playerstrength";
        
        public void Awake()
        {
            Logger.LogInfo("Setting up Patches...");
            new Harmony(PluginInformation.Guid).PatchAll();
            Shell.RegisterCommand(Command, (args) =>
            {
                var argv = args.Split(' ');
                switch (argv.Length)
                {
                    case 1 when argv[0].Trim() == "reset":
                        Debug.Log("[ StrongHuman ] - Resetting Strength to: " + 1);
                        SetStrength(1);
                        break;
                    case 1:
                    {
                        var success = float.TryParse(argv[0], out float playerStrength);
                        if (success)
                        {
                            if (playerStrength == 0) playerStrength = 1; // No divided by zero in this household
                            Debug.Log("[ StrongHuman ] - Setting Strength to: " + playerStrength);
                            SetStrength(playerStrength);
                        }
                        else
                        {
                            Debug.LogError("[ StrongHuman ] - Strength must be a valid Number! Usage:");
                            ShowHelp();
                        }

                        break;
                    }
                    case 0:
                        Debug.Log("[ StrongHuman ] - Current Strength: ");
                        break;
                    default:
                        Debug.LogError("[ StrongHuman ] - Invalid Usage! Usage:");
                        ShowHelp();
                        break;
                }
            });
        }
    }
}