using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using XRL.World.Parts;
using XRL.World;

namespace Petrak.Yank {

    /// This class patches ITeleporter.AttemptTeleport to reflect the replacement of recoilers with yanks.
    /// Specifically, it does three things:
    ///
    /// 1. Fire an event named TeleportPreflight before anything else.
    ///    This allows geofold yanks to configure themselves to point to the surface before the 'port happens.
    ///
    /// 2. Find all string literals referenced (the ldstr opcode) that contain the substring 'recoil',
    ///    and replace them with 'yank' instead. This is done in FindAndReplaceStrings().
    ///
    ///    This particular transform relies on find-and-replace properties and prevents recoilers and yanks from coexisting.
    ///    A nicer way might be to inject a call to YankMixinCallee before Popup::Show passes the string to the user,
    ///    which only performs s/recoil/yank/ if the ITeleporter is in fact a yank. (We do something similar for goal 3 below).
    ///
    /// 3. This is the meat and potatoes: when we call out to GameObject::ZoneTeleport at the end of AttemptTeleport,
    ///    after all preflight checks have passed, we inject a call to modify the message and destroy the yank.
    ///    We do this by injecting a call right after a string-type argument (the SuccessMessage) is pushed to the stack.
    ///    This is done in PatchTeleportMessage().
    ///
    ///    Since arguments in C# are pushed bottom-to-top, we do this by finding the call to Teleport, then walking backwards
    ///    through the args list; conveniently in the current invocation, the final two arguments (after SuccessMessage)
    ///    are constant true and constant null, so we can unambiguously identify these opcodes before the call (as ldc.i4.1
    ///    and ldnull respectively). The opcodes before this, then, must finish by pushing SuccessMessage to the stack.
    ///
    ///    Once our argument target is found, we'd like to call out to YankMixinCallee::ModifyMessageAndDestroy with it.
    ///    This function accepts a string as its first argument; conveniently SuccessMessage is already there on the stack.
    ///    It then takes a second argument and returns a string, IL calling convention being that the return is left on top of the stack.
    ///    Thus if we push the second argument and call out, we'll have successfully replaced the message without touching anything else.
    ///    The second argument in question is 'this', the ITeleporter instance in question; in a nonstatic method, this is in the
    ///    zeroth argument register. Thus pushing our second argument is as simple as 'ldarg.0'.
    [HarmonyPatch(typeof(ITeleporter))]
    [HarmonyPatch(nameof(ITeleporter.AttemptTeleport))]
    public class ITeleporter_AttemptTeleport_Patch {
        // 1. There are ldstrs that reference 'recoil'ing; change these to 'yank'ing.
        // 2. The end of AttemptTeleport calls out to ZoneTeleport. We want to intercept this.
        public static MethodInfo MODIFY_MESSAGE = AccessTools.Method(typeof(YankMixinCallee), "ModifyMessageAndDestroy");
        public static MethodInfo ZONE_TELEPORT = AccessTools.Method(
            typeof(GameObject),
            "ZoneTeleport",
            new System.Type[] {
                typeof(string),
                typeof(int),
                typeof(int),
                typeof(IEvent), 
                typeof(GameObject),
                typeof(GameObject),
                typeof(IPart),
                typeof(string), // This is our target, the SuccessMessage.
                typeof(bool), // This should be a const (ldc.i4.1)
                typeof(Cell) // This should be null (ldnull)
            }
        );

        static bool Prefix(ref bool __result, ITeleporter __instance, GameObject who, IEvent FromEvent) {
            // 10 is ground level; higher numbers are deeper
            if (!__instance.ParentObject.FireEvent(new Event("TeleportPreflight", "Who", who))) {
                __result = false;
                return false;
            }
            return true;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns) {
            if (ZONE_TELEPORT is null) {
                throw new System.Exception("[Yanks] Can't find a method signature in ITeleporter, aborting!");
            }

            var codeblock = new List<CodeInstruction>(insns);
            FindAndReplaceStrings(codeblock, "recoil", "yank");
            PatchTeleportMessage(codeblock);
            return codeblock;
        }

        public static void FindAndReplaceStrings(List<CodeInstruction> insns, string needle, string replace) {
            int count = 0;
            foreach (var insn in insns) {
                if (insn.opcode == OpCodes.Ldstr && insn.operand is string s) {
                    var replacement = s.Replace(needle, replace);
                    if (replacement != s) {
                        insn.operand = replacement;
                        count++;
                    }
                }
            }
            UnityEngine.Debug.Log("[Yanks] Successfully patched " + count + " strings in ITeleporter::AttemptTeleport!");
        }
                

        // Locate the SuccessMessage in the final call to ZoneTeleport,
        // then push 'this' (ldarg.0) and call out to MODIFY_MESSAGE,
        // which should leave the modified SuccessMessage on the stack.
        public static void PatchTeleportMessage(List<CodeInstruction> insns) {
            int ixAfterStringPush = FindIxAfterStringInsn(insns);
            if (ixAfterStringPush == -1) {
                throw new System.Exception("[Yanks] Can't find a success message to modify in ITeleporter::AttemptTeleport!");
            }
            InsertInstructionsAt(insns, GenerateCallout(), ixAfterStringPush);
            UnityEngine.Debug.Log("[Yanks] Successfully patched return in ITeleporter::AttemptTeleport!");
        }
        public static int FindIxAfterStringInsn(List<CodeInstruction> insns) {
            for (int i = 0; i < insns.Count - 2; i++) {
                if (TestZoneTeleportSequence(insns[i], insns[i + 1], insns[i + 2])) {
                    return i;
                }
            }
            return -1;
        }
        public static bool TestZoneTeleportSequence(CodeInstruction first, CodeInstruction second, CodeInstruction third) {
            return first.opcode == OpCodes.Ldc_I4_1
                && second.opcode == OpCodes.Ldnull
                && third.Calls(ZONE_TELEPORT);
        }
        public static List<CodeInstruction> GenerateCallout() {
            var effectiveCode = new List<CodeInstruction>();
            effectiveCode.Add(new CodeInstruction(OpCodes.Ldarg_0)); // push 'this'
            effectiveCode.Add(new CodeInstruction(OpCodes.Call, MODIFY_MESSAGE)); // invoke
            return effectiveCode;
        }

        public static void InsertInstructionsAt(List<CodeInstruction> insns, List<CodeInstruction> splice, int index) {
            var range = insns.GetRange(index, insns.Count - index);
            insns.RemoveRange(index, insns.Count - index);
            insns.AddRange(splice);
            insns.AddRange(range);
        }
    }
}
