﻿using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API.Utils;
using UnityEngine;
using System.Security.Permissions;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace FasterGames
{
    [R2APISubmoduleDependency("DifficultyAPI")]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.CwakrJax.FasterGames", "FasterGames", "1.0.0")]
    public class FasterGames : BaseUnityPlugin
    {
        public static AssetBundle MainAssets;

        public static ConfigEntry<bool> IsModEnabled { get; set; }
        public static ConfigEntry<float> spawnRate { get; set; }

        public static ConfigEntry<float> baseExpMultiplier { get; set; }
        public static ConfigEntry<float> expPerPlayerMultiplier { get; set; }

        public static ConfigEntry<float> baseMoney { get; set; }
        public static ConfigEntry<float> moneyPerPlayer { get; set; }

        public static ConfigEntry<float> moveSpeed { get; set; }

        public static ConfigEntry<float> scalingPercentage { get; set; }

        public static ConfigEntry<float> baseInteractableMultiplier { get; set; }
        public static ConfigEntry<float> perPlayerInteractableMultiplier { get; set; }

        public static ConfigEntry<int> chanceShrineItemCount { get; set; }
        public static ConfigEntry<float> chanceShrineCostMultiplier { get; set; }

        public static ConfigEntry<float> teleporterChargeMultiplier { get; set; }


        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        private Hooks myHooks = new Hooks();

        [System.Obsolete]
        public void Awake()
        {
            InitConfig();
            if (!IsModEnabled.Value)
            {
                return;
            }
            string InitMessage = "Your game is Faster!";
            Logger.LogInfo(InitMessage);
            myHooks.pluginLogger = Logger;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FasterGames.assets.assetbundle"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }

            DifficultyIndex diffIndex = FasterGamesDifficulty.AddDifficulty(scalingPercentage.Value);

            NoChestArtifact noChestArtifact = null;

            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config, Logger);
                }
                if (artifactType == typeof(NoChestArtifact))
                {
                    noChestArtifact = (NoChestArtifact)artifact;
                    Logger.LogDebug("Found NoChestArtifact");
                }
            }

            RoR2.Run.onRunStartGlobal += (RoR2.Run run) =>
            {
                // Handle selected Difficulty
                if (run.selectedDifficulty == diffIndex)
                {
                    ChatMessage.SendColored(InitMessage, new Color(0.78f, 0.788f, 0.3f));
                    if (noChestArtifact.ArtifactEnabled)
                    {
                        Logger.LogInfo("NoChestArtifact is enabled. Disabling chest based hooks");
                    }
                    else
                    {
                        myHooks.HookIncreaseExpCoefficient(baseExpMultiplier.Value, expPerPlayerMultiplier.Value);
                        myHooks.HookIncreaseMoneyMultiplier(baseMoney.Value, moneyPerPlayer.Value);
                        myHooks.HookIncreaseChestSpawnRate(baseInteractableMultiplier.Value, perPlayerInteractableMultiplier.Value);
                        myHooks.HookOverhaulChanceShrines(chanceShrineItemCount.Value, chanceShrineCostMultiplier.Value);
                    }
                    myHooks.HookNoCoolDown3dPrinter();
                    // myHooks.IncreaseSpawnSpeed();
                    myHooks.HookIncreaseBaseStats(moveSpeed.Value);
                    myHooks.HookIncreaseTeleporterChargeSpeed(teleporterChargeMultiplier.Value);
                    ChatMessage.SendColored("[FasterGames] If you plan on playing different difficulty, you must restart your game!", new Color(0.78f, 0.788f, 0.3f));
                }
                else { }

                // Handle selected Artifact
                // TODO Does this artifact make sense without the new difficulty?
                if (noChestArtifact.ArtifactEnabled)
                {
                    Logger.LogInfo("NoChestArtifact is enabled.");
                    noChestArtifact.Hooks();
                }
                else
                {
                    Logger.LogInfo("NoChestArtifact is NOT enabled.");
                    noChestArtifact.removeHooks();
                }
            };
        }

        public void InitConfig()
        {
            IsModEnabled = Config.Bind<bool>(
                "Enabled",
                "isModEnabled",
                true,
                "Turn this off if you want to play the game normally again. \nBut why would you?"
            );

            baseExpMultiplier = Config.Bind<float>(
                "Player",
                "baseExpMultiplier",
                2.5f,
                "Base Exp Gain. \nBase Game value: 1"
            );

            expPerPlayerMultiplier = Config.Bind<float>(
                "Player",
                "expPerPlayerMultiplier",
                0f,
                "Extra Exp Per player. Helps with multiplayer scaling. \nBase Game value: 0"
            );

            baseMoney = Config.Bind<float>(
                "Player",
                "baseMoneyMultiplier",
                1.5f,
                "Increases money gain for killing stuff. \nBase Game value: 1"
            );

            moneyPerPlayer = Config.Bind<float>(
                "Player",
                "moneyPerPlayer",
                0.1f,
                "Increases money gain per player. Helps with multiplayer scaling. \nBase Game value: 0"
            );

            moveSpeed = Config.Bind<float>(
                "Player",
                "moveSpeed",
                10f,
                "Increases base speed so you don't move so slow. \nBase Game value: 7"
            );

            scalingPercentage = Config.Bind<float>(
                "Game",
                "scalingIncreaseMultiplier",
                3f,
                "Increases game scaling. \nNormal Mode value: 1 \nMonsoon value: 1.5"
            );

            baseInteractableMultiplier = Config.Bind<float>(
                "Game",
                "interactableSpawnRateMultiplier",
                2.5f,
                "Increases Interactable spawn Rate. \nBase Game value: 1"
            );

            perPlayerInteractableMultiplier = Config.Bind<float>(
                "Game",
                "interactableSpawnRatePerPlayerMultiplier",
                0.5f,
                "Increases Interactable spawn Rate per extra Player Past 1. \nBase Game value: 0"
            );

            chanceShrineItemCount = Config.Bind<int>(
                "Game",
                "chanceShrineItemAmount",
                5,
                "Increases amount of items you get from chance shrine before it's disabled. \nBase Game value: 2"
            );

            chanceShrineCostMultiplier = Config.Bind<float>(
                "Game",
                "chanceShrineCostMultiplier",
                1.2f,
                "Increases cost of chance shrine by this amount every use (exponential).  \nBase Game value: 1.4"
            );

            teleporterChargeMultiplier = Config.Bind<float>(
                "Game",
                "teleporterChargeSpeedMultiplier",
                2.5f,
                "Increases Speed of Teleporter Charge.  \nBase Game value: 1"
            );
        }

        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;
            if (enabled)
            {
                artifactList.Add(artifact);
            }
            return enabled;
        }
    }
}
