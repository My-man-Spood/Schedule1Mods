using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MelonLoader;
using ScheduleOne.Cartel;
using ScheduleOne.Economy;
using ScheduleOne.Graffiti;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs.Relation;

namespace Spood.Mono.CartelRebalance;

internal static class CartelInfluencePatches
{
    private static readonly MethodInfo RemoveCartelGraffitiPlayerDeltaGetter =
        AccessTools.PropertyGetter(typeof(CartelInfluenceConfig), nameof(CartelInfluenceConfig.RemoveCartelGraffitiPlayerDelta));

    private static readonly MethodInfo RemoveCartelGraffitiNpcInterruptedDeltaGetter =
        AccessTools.PropertyGetter(typeof(CartelInfluenceConfig), nameof(CartelInfluenceConfig.RemoveCartelGraffitiNpcInterruptedDelta));

    private static readonly MethodInfo AmbushClearedDeltaGetter =
        AccessTools.PropertyGetter(typeof(CartelInfluenceConfig), nameof(CartelInfluenceConfig.AmbushClearedDelta));

    private static readonly MethodInfo CartelDealerDefeatedDeltaGetter =
        AccessTools.PropertyGetter(typeof(CartelInfluenceConfig), nameof(CartelInfluenceConfig.CartelDealerDefeatedDelta));

    private static readonly MethodInfo NewCustomerUnlockedDeltaGetter =
        AccessTools.PropertyGetter(typeof(CartelInfluenceConfig), nameof(CartelInfluenceConfig.NewCustomerUnlockedDelta));

    [HarmonyPatch(typeof(WorldSpraySurface), nameof(WorldSpraySurface.CleanGraffiti))]
    private static class WorldSpraySurface_CleanGraffiti_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            return ReplaceInfluenceDelta(
                instructions,
                __originalMethod,
                originalDelta: -0.05f,
                replacementGetter: RemoveCartelGraffitiPlayerDeltaGetter,
                replacementName: nameof(CartelInfluenceConfig.RemoveCartelGraffitiPlayerDelta),
                expectedReplacementCount: 1);
        }
    }

    [HarmonyPatch(typeof(GraffitiBehaviour), nameof(GraffitiBehaviour.Disable))]
    private static class GraffitiBehaviour_Disable_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            return ReplaceInfluenceDelta(
                instructions,
                __originalMethod,
                originalDelta: -0.1f,
                replacementGetter: RemoveCartelGraffitiNpcInterruptedDeltaGetter,
                replacementName: nameof(CartelInfluenceConfig.RemoveCartelGraffitiNpcInterruptedDelta),
                expectedReplacementCount: 1);
        }
    }

    [HarmonyPatch]
    private static class Ambush_MonitorAmbush_Patch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            var declaredIteratorMethods = AccessTools.GetDeclaredMethods(typeof(Ambush))
                .Where(method => typeof(IEnumerator).IsAssignableFrom(method.ReturnType))
                .Cast<MethodBase>();

            var nestedIteratorMoveNextMethods = EnumerateNestedTypesRecursively(typeof(Ambush))
                .Where(type => typeof(IEnumerator).IsAssignableFrom(type))
                .Select(type => (MethodBase)AccessTools.Method(type, "MoveNext"))
                .Where(method => method != null);

            var targetMethods = declaredIteratorMethods
                .Concat(nestedIteratorMoveNextMethods)
                .Distinct()
                .ToList();

            if (targetMethods.Count == 0)
            {
                MelonLogger.Warning("[CartelRebalance] Ambush patch: no iterator methods found; using safe fallback target.");
                targetMethods.Add(AccessTools.DeclaredMethod(typeof(Ambush), "MinPassed"));
            }
            else
            {
                MelonLogger.Msg($"[CartelRebalance] Ambush patch: targeting {targetMethods.Count} method(s): {string.Join(", ", targetMethods.Select(FormatMethodName))}");
            }

            return targetMethods;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            var expectedReplacementCount = __originalMethod.Name.Equals("MinPassed", StringComparison.Ordinal) ? 0 : 1;
            return ReplaceInfluenceDelta(
                instructions,
                __originalMethod,
                originalDelta: -0.1f,
                replacementGetter: AmbushClearedDeltaGetter,
                replacementName: nameof(CartelInfluenceConfig.AmbushClearedDelta),
                expectedReplacementCount: expectedReplacementCount);
        }

        private static IEnumerable<Type> EnumerateNestedTypesRecursively(Type rootType)
        {
            var pending = new Stack<Type>(rootType.GetNestedTypes(AccessTools.all));
            while (pending.Count > 0)
            {
                var type = pending.Pop();
                yield return type;

                foreach (var nestedType in type.GetNestedTypes(AccessTools.all))
                {
                    pending.Push(nestedType);
                }
            }
        }

        private static string FormatMethodName(MethodBase method)
        {
            if (method.DeclaringType == null)
            {
                return method.Name;
            }

            return $"{method.DeclaringType.FullName}.{method.Name}";
        }
    }

    [HarmonyPatch(typeof(CartelDealer), "DiedOrKnockedOut")]
    private static class CartelDealer_DiedOrKnockedOut_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            return ReplaceInfluenceDelta(
                instructions,
                __originalMethod,
                originalDelta: -0.1f,
                replacementGetter: CartelDealerDefeatedDeltaGetter,
                replacementName: nameof(CartelInfluenceConfig.CartelDealerDefeatedDelta),
                expectedReplacementCount: 1);
        }
    }

    [HarmonyPatch(typeof(Customer), "OnCustomerUnlocked")]
    [HarmonyPatch(new[] { typeof(NPCRelationData.EUnlockType), typeof(bool) })]
    private static class Customer_OnCustomerUnlocked_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            return ReplaceInfluenceDelta(
                instructions,
                __originalMethod,
                originalDelta: -0.075f,
                replacementGetter: NewCustomerUnlockedDeltaGetter,
                replacementName: nameof(CartelInfluenceConfig.NewCustomerUnlockedDelta),
                expectedReplacementCount: 1);
        }
    }

    private static IEnumerable<CodeInstruction> ReplaceInfluenceDelta(
        IEnumerable<CodeInstruction> instructions,
        MethodBase originalMethod,
        float originalDelta,
        MethodInfo replacementGetter,
        string replacementName,
        int expectedReplacementCount)
    {
        var codeInstructions = instructions.ToList();
        var replacementCount = 0;

        for (var i = 0; i < codeInstructions.Count; i++)
        {
            var instruction = codeInstructions[i];
            if (instruction.opcode != OpCodes.Ldc_R4)
            {
                continue;
            }

            if (instruction.operand is not float loadedValue)
            {
                continue;
            }

            if (!ApproximatelyEqual(loadedValue, originalDelta))
            {
                continue;
            }

            if (!HasChangeInfluenceCallSoon(codeInstructions, i))
            {
                continue;
            }

            codeInstructions[i] = new CodeInstruction(OpCodes.Call, replacementGetter);
            replacementCount++;
        }

        LogReplacementResult(originalMethod, originalDelta, replacementName, replacementCount, expectedReplacementCount);

        return codeInstructions;
    }

    private static void LogReplacementResult(
        MethodBase originalMethod,
        float originalDelta,
        string replacementName,
        int replacementCount,
        int expectedReplacementCount)
    {
        var methodName = originalMethod.DeclaringType == null
            ? originalMethod.Name
            : $"{originalMethod.DeclaringType.FullName}.{originalMethod.Name}";

        var message =
            $"[CartelRebalance] {methodName}: replaced {replacementCount} occurrence(s) of {originalDelta} with {replacementName}";

        if (replacementCount == expectedReplacementCount)
        {
            MelonLogger.Msg(message);
            return;
        }

        MelonLogger.Warning(message);
    }

    private static bool HasChangeInfluenceCallSoon(List<CodeInstruction> codeInstructions, int startIndex)
    {
        var endIndexExclusive = Math.Min(codeInstructions.Count, startIndex + 7);
        for (var i = startIndex; i < endIndexExclusive; i++)
        {
            if ((codeInstructions[i].opcode != OpCodes.Callvirt && codeInstructions[i].opcode != OpCodes.Call) ||
                codeInstructions[i].operand is not MethodInfo method)
            {
                continue;
            }

            if (method.Name.Equals("ChangeInfluence", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ApproximatelyEqual(float left, float right)
    {
        return Math.Abs(left - right) <= 0.0001f;
    }
}
