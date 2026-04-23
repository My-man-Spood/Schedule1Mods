using HarmonyLib;
using MelonLoader;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;

namespace Spood.Mono.PickyCustomers
{
    [HarmonyPatch(typeof(Customer), "GetProductEnjoyment")]
    public static class GetProductEnjoymentPatch
    {
        public static string lastDrugChecked = "";
        public static string lastCustomerProductChecked = "";

        public static Dictionary<ECustomerStandard, int> StandardsPropertiesFloor = new Dictionary<ECustomerStandard, int>
        {
            { ECustomerStandard.VeryLow, 0 },
            { ECustomerStandard.Low, 1 },
            { ECustomerStandard.Moderate, 1 },
            { ECustomerStandard.High, 2 },
            { ECustomerStandard.VeryHigh, 2 }
        };

        public static Dictionary<ECustomerStandard, float> StandardsQualityFloor = new Dictionary<ECustomerStandard, float>
        {
            { ECustomerStandard.VeryLow, 0f },
            { ECustomerStandard.Low, 0.25f },
            { ECustomerStandard.Moderate, 0.5f },
            { ECustomerStandard.High, 0.75f },
            { ECustomerStandard.VeryHigh, 1f }
        };
        
        public static bool Prefix(ProductDefinition product, EQuality quality, Customer __instance, ref float __result)
        {
            var affinityData = Traverse.Create(__instance).Field("currentAffinityData").GetValue<CustomerAffinityData>();
            var customerData = Traverse.Create(__instance).Field("customerData").GetValue<CustomerData>();
            var currentCustomerProduct = $"{__instance.NPC.fullName}_{product.Name}";
            if(lastCustomerProductChecked != currentCustomerProduct)
            {
                Log($"Customer {__instance.NPC.fullName} is considering {product.Name}!", currentCustomerProduct);
                Log($"This is how they feel about drugs:", currentCustomerProduct);
                var output = "";
                foreach (var aff in affinityData.ProductAffinities)
                {
                    output += $"{aff.DrugType.GetName()[0]}({aff.Affinity}) | ";
                }
                Log(output, currentCustomerProduct);
            }

            var affinityMod = GetDrugTypeAffinityEnjoymentModifier(product, affinityData, currentCustomerProduct);
            var propertiesMod = GetPropertiesEnjoymentModifier(product, customerData, currentCustomerProduct);
            var qualityMod = GetQualityEnjoymentModifier(product, quality, customerData, currentCustomerProduct);

            __result = Mathf.Clamp01(0.75f + affinityMod + propertiesMod + qualityMod);
            if (lastCustomerProductChecked != currentCustomerProduct)
            {
                Log($"So {__instance.NPC.fullName} likes {product.Name} at {(int)(__result*100)}%", currentCustomerProduct);
            }

            lastCustomerProductChecked = currentCustomerProduct;
            return false;
        }

        /// <summary>
        /// Calculate quality modifier based on customer standards
        /// 30% deduction for each tier below the customer's minimum quality
        /// 10% bonus for each tier above the customer's preferred quality
        /// </summary>
        private static float GetQualityEnjoymentModifier(ProductDefinition product, EQuality quality, CustomerData customerData, string currentCustomerProduct)
        {
            var cat = MelonPreferences.GetCategory(Mod.PreferencesCategory);
            float qualityDeduction = 0f;
            float qualityBonus = 0f;

            float productQualityScalar = CustomerData.GetQualityScalar(quality);
            float customerStandardQualityScalar = StandardsQualityFloor[customerData.Standards];

            float scalarDifference = customerStandardQualityScalar - productQualityScalar;
            int tiersBelow = Mathf.CeilToInt(scalarDifference / 0.25f);

            if (tiersBelow > 0)
            {
                var QualityPenaltyPercent = cat.GetEntry<float>(Mod.QualityPenaltyPercent).Value;
                qualityDeduction = QualityPenaltyPercent * tiersBelow;
            }
            else
            {
                var tiersAbove = Mathf.CeilToInt(-scalarDifference / 0.25f);
                qualityBonus = cat.GetEntry<float>(Mod.QualityBonusPercent).Value * tiersAbove;
            }

            if (lastCustomerProductChecked != currentCustomerProduct)
            {
                Log($"Quality {Mod.RedColor}-{qualityDeduction * 100}%|{Mod.GreenColor}+{qualityBonus * 100}%", currentCustomerProduct);
            }

            return qualityBonus - qualityDeduction;
        }

        /// <summary>
        /// Calculate property modifier based on how many properties the customer prefers
        /// 40% deduction for each property that is not matched, 10% bonus for each additional property that is matched
        /// after the minimum number of properties required
        /// </summary>
        private static float GetPropertiesEnjoymentModifier(ProductDefinition product, CustomerData customerData, string currentCustomerProduct)
        {
            var cat = MelonPreferences.GetCategory(Mod.PreferencesCategory);
            float propertyBonus = 0f;
            float propertyDeduction = 0f;

            var minPropertiesRequired = StandardsPropertiesFloor[customerData.Standards];
            int numberOfMatchedProperties = customerData.PreferredProperties.Count(p => product.Properties.Contains(p));

            if (numberOfMatchedProperties < minPropertiesRequired)
            {
                propertyDeduction = cat.GetEntry<float>(Mod.PropertyPenaltyPercent).Value * (minPropertiesRequired - numberOfMatchedProperties);
            }
            else
            {
                propertyBonus = cat.GetEntry<float>(Mod.PropertyBonusPercent).Value * (numberOfMatchedProperties - minPropertiesRequired);
            }

            if (lastCustomerProductChecked != currentCustomerProduct)
            {
                Log($"Properties {Mod.RedColor}-{propertyDeduction * 100}%|{Mod.GreenColor}+{propertyBonus * 100}%", currentCustomerProduct);
            }

            return propertyBonus - propertyDeduction;
        }

        /// <summary>
        /// Calculate affinity deduction based on how much the customer hates the product
        /// disregard affinities that are positive, these customer are PICKY.
        /// </summary>
        private static float GetDrugTypeAffinityEnjoymentModifier(ProductDefinition product, CustomerAffinityData affinityData, string currentCustomerProduct)
        {
            var cat = MelonPreferences.GetCategory(Mod.PreferencesCategory);
            float affinityDeduction = 0f;

            for (int i = 0; i < product.DrugTypes.Count; i++)
            {
                var affinity = affinityData.GetAffinity(product.DrugTypes[i].DrugType);
                if (affinity > 0f) continue;
                affinityDeduction += Mathf.Lerp(0f, cat.GetEntry<float>(Mod.DrugTypePenaltyPercent).Value, -affinity);
            }

            if(lastCustomerProductChecked != currentCustomerProduct)
            {
                Log($"Affinity {Mod.RedColor}-{affinityDeduction * 100}%", currentCustomerProduct);
            }

            return -affinityDeduction; 
        }

        private static void Log(string message, string currentCustomerProduct)
        {
            if(lastCustomerProductChecked != currentCustomerProduct)
            {
                Melon<Mod>.Logger.Msg($"[Product Enjoyment]{message}");
            }
        }
    }
}
