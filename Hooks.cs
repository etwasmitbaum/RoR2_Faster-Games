using RoR2;
using UnityEngine.Networking;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Android;

namespace FasterGames
{
    public class Hooks
    {
        public BepInEx.Logging.ManualLogSource pluginLogger;

        private float baseExpMultiplier, expPerPlayerMultiplier;
        private float baseMoney, moneyPerPlayer;
        private float moveSpeed;
        private float baseInteractableMultiplier, perPlayerInteractableMultiplier;
        private int maxPurchases; private float costMult;
        private float teleporterChargeMultiplier;

        private bool isIncreaseSpawnSpeedEnabled = false,
            isIncreaseExpCoefficientEnabled = false,
            isIncreaseMoneyMultiplierEnabled = false,
            isIncreaseBaseStatsEnabled = false,
            isIncreaseChestSpawnRateEnabled = false,
            isOverhaulChanceShrinesEnabled = false,
            isIncreaseTeleporterChargeSpeedEnabled = false,
            isNoCoolDown3dPrinterEnabled = false;

        public void RemoveAllHook()
        {
            RemoveIncreaseSpawnSpeed();
            RemoveIncreaseExpCoefficient();
            RemoveIncreaseMoneyMultiplier();
            RemoveIncreaseBaseStats();
            RemoveIncreaseChestSpawnRate();
            RemoveOverhaulChanceShrines();
            RemoveIncreaseTeleporterChargeSpeed();
            RemoveNoCoolDown3dPrinter();
            pluginLogger.LogInfo($"Removed all Hooks");
        }

        public void HookIncreaseSpawnSpeed()
        {
            if (!isIncreaseSpawnSpeedEnabled)
            {
                On.RoR2.CombatDirector.Simulate += IncreaseSpawnSpeed;
                isIncreaseSpawnSpeedEnabled = true;
            }
        }
        public void RemoveIncreaseSpawnSpeed()
        {
            if (isIncreaseSpawnSpeedEnabled)
            {
                On.RoR2.CombatDirector.Simulate -= IncreaseSpawnSpeed;
                pluginLogger.LogInfo($"Removed IncreaseSpawnSpeed Hook");
                isIncreaseSpawnSpeedEnabled = false;
            }
        }

        public void HookIncreaseExpCoefficient(float baseExpMultiplier, float expPerPlayerMultiplier)
        {
            if (!isIncreaseExpCoefficientEnabled)
            {
                this.baseExpMultiplier = baseExpMultiplier;
                this.expPerPlayerMultiplier = expPerPlayerMultiplier;
                On.RoR2.CombatDirector.Simulate += IncreaseExpCoefficient;
                isIncreaseExpCoefficientEnabled = true;
            }
        }
        public void RemoveIncreaseExpCoefficient()
        {
            if (isIncreaseExpCoefficientEnabled)
            {
                On.RoR2.CombatDirector.Simulate -= IncreaseExpCoefficient;
                pluginLogger.LogInfo($"Removed IncreaseExpCoefficient Hook");
                isIncreaseExpCoefficientEnabled = false;
            }
        }


        public void HookIncreaseMoneyMultiplier(float baseMoney, float moneyPerPlayer)
        {
            if (!isIncreaseMoneyMultiplierEnabled)
            {
                this.baseMoney = baseMoney;
                this.moneyPerPlayer = moneyPerPlayer;
                On.RoR2.CombatDirector.Simulate += IncreaseMoneyMultiplier;
                isIncreaseMoneyMultiplierEnabled = true;
            }

        }
        public void RemoveIncreaseMoneyMultiplier()
        {
            if (isIncreaseMoneyMultiplierEnabled)
            {
                On.RoR2.CombatDirector.Simulate -= IncreaseMoneyMultiplier;
                pluginLogger.LogInfo($"Removed IncreaseMoneyMultiplier Hook");
                isIncreaseMoneyMultiplierEnabled = false;
            }
        }

        public void HookIncreaseBaseStats(float moveSpeed)
        {
            if (!isIncreaseBaseStatsEnabled)
            {
                this.moveSpeed = moveSpeed;
                IncreaseBaseStats();
                isIncreaseBaseStatsEnabled = true;
            }
        }
        public void RemoveIncreaseBaseStats()
        {
            if (isIncreaseBaseStatsEnabled)
            {
                foreach (SurvivorDef survivor in RoR2.SurvivorCatalog.allSurvivorDefs)
                {
                    CharacterBody body = survivor.bodyPrefab.GetComponent<CharacterBody>();

                    pluginLogger.LogInfo($"Removed adjusted speed for {body.name}");
                    body.baseMoveSpeed = 7; // Default is 7
                                            // TODO get speed from internal variable and use it here
                }
                isIncreaseBaseStatsEnabled = false;
            }
        }


        public void HookIncreaseChestSpawnRate(float baseInteractableMultiplier, float perPlayerInteractableMultiplier)
        {
            if (!isIncreaseChestSpawnRateEnabled)
            {
                this.baseInteractableMultiplier = baseInteractableMultiplier;
                this.perPlayerInteractableMultiplier = perPlayerInteractableMultiplier;
                On.RoR2.ClassicStageInfo.Awake += IncreaseChestSpawnRate;
                isIncreaseChestSpawnRateEnabled = true;
            }

        }
        public void RemoveIncreaseChestSpawnRate()
        {
            if (isIncreaseChestSpawnRateEnabled)
            {
                On.RoR2.ClassicStageInfo.Awake -= IncreaseChestSpawnRate;
                pluginLogger.LogInfo($"Removed IncreaseChestSpawnRate Hook");
                isIncreaseChestSpawnRateEnabled = false;
            }
        }


        public void HookOverhaulChanceShrines(int maxPurchases, float costMult)
        {
            if (!isOverhaulChanceShrinesEnabled)
            {
                this.maxPurchases = maxPurchases;
                this.costMult = costMult;
                On.RoR2.ShrineChanceBehavior.Awake += ChanceMaxPurchaseCostScaling;
                On.RoR2.ShrineChanceBehavior.AddShrineStack += RemoveChanceTimer;
                isOverhaulChanceShrinesEnabled = true;
            }

        }
        public void RemoveOverhaulChanceShrines()
        {
            if (isOverhaulChanceShrinesEnabled)
            {
                On.RoR2.ShrineChanceBehavior.Awake -= ChanceMaxPurchaseCostScaling;
                On.RoR2.ShrineChanceBehavior.AddShrineStack -= RemoveChanceTimer;
                pluginLogger.LogInfo($"Removed OverhaulChanceShrines Hook");
                isOverhaulChanceShrinesEnabled = false;
            }
        }


        public void HookIncreaseTeleporterChargeSpeed(float teleporterChargeMultiplier)
        {
            if (!isIncreaseTeleporterChargeSpeedEnabled)
            {
                this.teleporterChargeMultiplier = teleporterChargeMultiplier;
                On.RoR2.HoldoutZoneController.OnEnable += IncreaseTeleporterChargeSpeed;
                isIncreaseTeleporterChargeSpeedEnabled = true;
            }

        }
        public void RemoveIncreaseTeleporterChargeSpeed()
        {
            if (isIncreaseTeleporterChargeSpeedEnabled)
            {
                On.RoR2.HoldoutZoneController.OnEnable -= IncreaseTeleporterChargeSpeed;
                pluginLogger.LogInfo($"Removed IncreaseTeleporterChargeSpeed Hook");
                isIncreaseTeleporterChargeSpeedEnabled = false;
            }

        }


        public void HookNoCoolDown3dPrinter()
        {
            if (!isNoCoolDown3dPrinterEnabled)
            {
                //TODO fix this
                //On.RoR2.Stage.Start += StageStart3DPrinterNoCoolDown;
                On.EntityStates.Duplicator.Duplicating.DropDroplet += DropDroplet3DPrinterNoCoolDown;
                On.EntityStates.Duplicator.Duplicating.BeginCooking += BeginCooking3DPrinterNoCoolDown;
                isNoCoolDown3dPrinterEnabled = true;
            }
        }
        public void RemoveNoCoolDown3dPrinter()
        {
            if (isNoCoolDown3dPrinterEnabled)
            {
                //TODO fix this
                //On.RoR2.Stage.Start -= StageStart3DPrinterNoCoolDown;
                On.EntityStates.Duplicator.Duplicating.DropDroplet -= DropDroplet3DPrinterNoCoolDown;
                On.EntityStates.Duplicator.Duplicating.BeginCooking -= BeginCooking3DPrinterNoCoolDown;
                pluginLogger.LogInfo($"Removed NoCoolDown3dPrinter Hook");
                isNoCoolDown3dPrinterEnabled = false;
            }
        }

        private void IncreaseSpawnSpeed(On.RoR2.CombatDirector.orig_Simulate orig, CombatDirector self, float deltaTime)
        {
            // Dunno if this is actually working...
            orig(self, deltaTime);
            pluginLogger.LogInfo($"Increased Spawn Rate");
        }

        private void IncreaseExpCoefficient(On.RoR2.CombatDirector.orig_Simulate orig, CombatDirector self, float deltaTime)
        {
            //TODO figure out why this is spamming the console
            self.expRewardCoefficient = new float();
            self.expRewardCoefficient = this.baseExpMultiplier / 5 + Run.instance.participatingPlayerCount * this.expPerPlayerMultiplier / 5; // Default is 0.2
            orig(self, deltaTime);
            pluginLogger.LogInfo($"Increased Exp Rate: {this.baseExpMultiplier}x; Per Player: {this.expPerPlayerMultiplier}x");
        }

        private void IncreaseMoneyMultiplier(On.RoR2.CombatDirector.orig_Simulate orig, CombatDirector self, float deltaTime)
        {
            //TODO figure out why this is spamming the console
            self.creditMultiplier = new float();
            self.creditMultiplier = this.baseMoney + Run.instance.participatingPlayerCount * this.moneyPerPlayer; // Default is 1
            orig(self, deltaTime);
            pluginLogger.LogInfo($"Increased Money on Kill: {this.baseMoney}x; Per Player: {this.moneyPerPlayer}x");
        }

        private void IncreaseBaseStats()
        {
            foreach (SurvivorDef survivor in RoR2.SurvivorCatalog.allSurvivorDefs)
            {
                CharacterBody body = survivor.bodyPrefab.GetComponent<CharacterBody>();

                pluginLogger.LogInfo($"Updated speed for {body.name}");
                body.baseMoveSpeed = moveSpeed; // Default is 7
            }
            pluginLogger.LogInfo($"Increased Base Move Speed to {moveSpeed}");
        }

        private void IncreaseChestSpawnRate(On.RoR2.ClassicStageInfo.orig_Awake orig, ClassicStageInfo self)
        {
            self.sceneDirectorInteractibleCredits = (int)(200 * baseInteractableMultiplier) + (int)((Run.instance.participatingPlayerCount - 1) * 200 * perPlayerInteractableMultiplier); // Default is 200
            orig(self);
            pluginLogger.LogInfo($"Increased Chest Spawn Rate: {baseInteractableMultiplier}x; Per Player: {perPlayerInteractableMultiplier}x");
        }

        // Credit to https://github.com/HimeTwyla/TwylaRoR2Mods/blob/master/InfiniteChance/InfiniteChance.cs
        private void ChanceMaxPurchaseCostScaling(On.RoR2.ShrineChanceBehavior.orig_Awake orig, ShrineChanceBehavior self)
        {
            orig(self);
            self.maxPurchaseCount = maxPurchases;

            // Default is 1.4
            HarmonyLib.AccessTools.Field(HarmonyLib.AccessTools.TypeByName("RoR2.ShrineChanceBehavior"), "costMultiplierPerPurchase").SetValue(self, costMult);
            pluginLogger.LogInfo($"Overhauled Chance Shrine - Max Items: {maxPurchases}; Cost Multiplier: {costMult}x");
        }

        private void RemoveChanceTimer(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor interactor)
        {
            orig(self, interactor);
            HarmonyLib.AccessTools.Field(HarmonyLib.AccessTools.TypeByName("RoR2.ShrineChanceBehavior"), "refreshTimer").SetValue(self, 0.1f);
            pluginLogger.LogInfo($"Removed ChanceShrine Cooldown");
        }

        //Credit to https://thunderstore.io/package/der10pm/ChargeInHalf/
        private void IncreaseTeleporterChargeSpeed(On.RoR2.HoldoutZoneController.orig_OnEnable orig, HoldoutZoneController self)
        {
            self.baseChargeDuration /= teleporterChargeMultiplier;
            orig(self);

            pluginLogger.LogInfo($"Increased Teleporter Charge Speed: {teleporterChargeMultiplier}x");
        }

        //Credit to https://github.com/TheRealElysium/R2Mods/blob/master/Faster3DPrinters
        private void StageStart3DPrinterNoCoolDown(On.RoR2.Stage.orig_Start orig, Stage self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("initialDelayDuration", 0.1f);
                typeof(EntityStates.Duplicator.Duplicating).SetFieldValue("timeBetweenStartAndDropDroplet", 0f);
            }
        }

        private void DropDroplet3DPrinterNoCoolDown(On.EntityStates.Duplicator.Duplicating.orig_DropDroplet orig, EntityStates.Duplicator.Duplicating self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                self.outer.GetComponent<PurchaseInteraction>().Networkavailable = true;
            }
        }

        private void BeginCooking3DPrinterNoCoolDown(On.EntityStates.Duplicator.Duplicating.orig_BeginCooking orig, EntityStates.Duplicator.Duplicating self)
        {
            if (!NetworkServer.active)
            {
                orig(self);
            }
            pluginLogger.LogInfo("Removed 3d printer cooldown");
        }

    }

}