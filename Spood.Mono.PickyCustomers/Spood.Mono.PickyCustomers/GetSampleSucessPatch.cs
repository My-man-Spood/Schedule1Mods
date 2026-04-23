using HarmonyLib;
using MelonLoader;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;

namespace Spood.Mono.PickyCustomers
{
    [HarmonyPatch(typeof(Customer), "GetSampleSuccess")]
    public static class GetSampleSucessPatch
    {
        public static string lastCustomerProduct = "";

        public static bool Prefix(List<ItemInstance> items, float price, Customer __instance, ref float __result)
        {
            float sampleScore = -1000f;
            string currentCustomerProduct = "";
            foreach (ItemInstance item in items)
            {
                if (item is ProductItemInstance)
                {
                    ProductItemInstance productItemInstance = item as ProductItemInstance;
                    float productEnjoyment = __instance.GetProductEnjoyment(item.Definition as ProductDefinition, productItemInstance.Quality);
                    if (productEnjoyment > sampleScore)
                    {
                        sampleScore = productEnjoyment;
                        currentCustomerProduct = $"{__instance.NPC.fullName}_{productItemInstance.Name}";
                    }
                }
            }
            Log($"Customer {__instance.NPC.fullName} is considering a sample.", currentCustomerProduct);
            Log($"Product enjoyment score: {sampleScore:F2}", currentCustomerProduct);
            float relationDelta = __instance.NPC.RelationData.RelationDelta / 5f;
            if (relationDelta >= 0.5f)
            {
                sampleScore += Mathf.Lerp(0f, 0.2f, (relationDelta - 0.5f) * 2f);
            }
            Log($"Relationship bonus: {Mathf.Lerp(0f, 0.2f, (relationDelta - 0.5f) * 2f):F2}", currentCustomerProduct);
            sampleScore += Mathf.Lerp(0f, 0.2f, __instance.CurrentAddiction);
            Log($"Addiction bonus: {Mathf.Lerp(0f, 0.2f, __instance.CurrentAddiction):F2}", currentCustomerProduct);
            float avgFriendScore = __instance.NPC.RelationData.GetAverageMutualRelationship() / 5f;
            if (avgFriendScore > 0.5f)
            {
                sampleScore += Mathf.Lerp(0f, 0.2f, (avgFriendScore - 0.5f) * 2f);
            }
            Log($"Friend of friends bonus: {Mathf.Lerp(0f, 0.2f, (avgFriendScore - 0.5f) * 2f):F2}", currentCustomerProduct);

            __result = Mathf.Clamp01(sampleScore);
            Log($"Total sample score before curve: {__result:F2}", currentCustomerProduct);
            lastCustomerProduct = currentCustomerProduct;
            return false;
        }

        private static void Log(string message, string currentCustomerProduct)
        {
            if(lastCustomerProduct != currentCustomerProduct)
            {
                Melon<Mod>.Logger.Msg($"[Sample] {message}");
            }
        }
    }
}
