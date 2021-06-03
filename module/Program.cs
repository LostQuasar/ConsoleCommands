using Aki.SinglePlayer.Utils;
using UnityEngine;
using EFT.UI;
using Newtonsoft.Json;
using EFT;
using System.Collections.Generic;
using Aki.Common.Utils.Patching;
using System;
using Comfort.Common;
using System.Linq;
using EFT.Weather;
using RegexCommand = GClass1954;

namespace Spaceman.ConsoleCommands
{
    public class Program
    {
        private static GameObject HookObject;
        static internal string basePath = "/mod/console_commands/";
        static private bool moonGravityEnabled = false;
        static private bool infiniteCarryWeightEnabled = false;

        public static void AddConsoleLog(string log)
        {
            if (PreloaderUI.Instantiated)
                PreloaderUI.Instance.Console.AddLog("ConsoleCommands: " + log, "");
        }

        static Player GetPlayerInstance()
        {
            List<Player> players = Singleton<GameWorld>.Instance.RegisteredPlayers;
            foreach (Player player in players)
            {
                if (player.IsYourPlayer)
                {
                    return player;
                }
            }
            return null;
        }

        static void Main(string[] args)
        {
            Debug.LogError("Spaceman.ConsoleCommands: Adding Console Commands");

            PatcherUtil.Patch<GodModePatch>();
            PatcherUtil.Patch<InfiniteStaminaPatch>();

            HookObject = new GameObject();
            HookObject.AddComponent<FreecamController>();
            UnityEngine.Object.DontDestroyOnLoad(HookObject);

            //Change the pmc conversion chance with command
            ConsoleScreen.Commands.AddCommand(new RegexCommand("pmcpercent (100|[0-9][0-9]?)", match =>
            {
                int chance = int.Parse(match.Groups[1].Value);
                RequestHandler.PostJson(basePath + "pmcConversionChance", JsonConvert.SerializeObject(chance));

                AddConsoleLog($"Set pmc conversion chance to {chance}%");
            }));

            //Kill everything including server + client
            ConsoleScreen.Commands.AddCommand(new RegexCommand("hcf", match =>
            {
                try
                {
                    RequestHandler.GetJson(basePath + "killServer");
                }
                catch { }
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }));

            ConsoleScreen.Commands.AddCommand(new RegexCommand("give (\\S+) (\\d+)", match =>
            {
                RequestHandler.GetJson(basePath + "give/" + match.Groups[1].Value + "/" + match.Groups[2].Value);

                AddConsoleLog($"Gave {match.Groups[2].Value} {match.Groups[1].Value} to player");
            }));


            //Change the chance of insurance being returned
            ConsoleScreen.Commands.AddCommand(new RegexCommand("insurancechance (100|[0-9][0-9]?)", match =>
            {
                int chance = int.Parse(match.Groups[1].Value);
                RequestHandler.PostJson(basePath + "insuranceReturnChance", JsonConvert.SerializeObject(chance));

                AddConsoleLog($"Set insurance return chance to {chance}%");
            }));

            //Change the chance of boss being spawned
            ConsoleScreen.Commands.AddCommand(new RegexCommand("bosschance (100|[0-9][0-9]?)", match =>
            {
                int chance = int.Parse(match.Groups[1].Value);
                RequestHandler.PostJson(basePath + "bossChance", JsonConvert.SerializeObject(chance));

                AddConsoleLog($"Set boss chance to {chance}%");
            }));


            //Kills all ai or the player
            ConsoleScreen.Commands.AddCommand(new RegexCommand("kill (ai|me)", match =>
            {
                string killGroup = match.Groups[1].Value;
                int killCount = 0;
                List<Player> players = Singleton<GameWorld>.Instance.RegisteredPlayers;
                foreach (Player player in players.ToList())
                {
                    if (player.IsYourPlayer && killGroup == "me")
                    {
                        player.KillMe(EBodyPart.Head, 999999F);

                    }
                    else if (killGroup == "ai" && player.IsAI)
                    {
                        player.KillMe(EBodyPart.Head, 999999F);
                        killCount += 1;
                    }
                }

                AddConsoleLog($"Killed " + killGroup == "ai" ? $"{killCount} AI" : "Player");
            }));

            //Heal player
            ConsoleScreen.Commands.AddCommand(new RegexCommand("heal", match =>
            {
                Player player = GetPlayerInstance();
                GClass1476 healthController = player.PlayerHealthController;

                healthController.RestoreFullHealth();
                foreach (EBodyPart bodyPart in Enum.GetValues(typeof(EBodyPart)))
                {
                    healthController.RemoveNegativeEffects(bodyPart);
                }
                healthController.ChangeEnergy(healthController.Energy.Maximum - healthController.Energy.Current);
                healthController.ChangeHydration(healthController.Hydration.Maximum - healthController.Hydration.Current);
                player.PlayerHealthController.ManualUpdate(.25F);

                AddConsoleLog("Healed the player");
            }));

            //Toggle God mode
            ConsoleScreen.Commands.AddCommand(new RegexCommand("godmode", match =>
            {
                GodModePatch.godModeEnabled = !GodModePatch.godModeEnabled;
                string parsedStatus = GodModePatch.godModeEnabled ? "enabled" : "disabled";

                AddConsoleLog("God mode " + parsedStatus);
            }));

            //Toggle infinite stamina
            ConsoleScreen.Commands.AddCommand(new RegexCommand("infstamina", match =>
            {
                InfiniteStaminaPatch.infiniteStaminaEnabled = !InfiniteStaminaPatch.infiniteStaminaEnabled;
                Player player = GetPlayerInstance();

                player.Physical.StaminaParameters.SprintDrainRate = InfiniteStaminaPatch.infiniteStaminaEnabled ? 0F : 4F;

                string parsedStatus = InfiniteStaminaPatch.infiniteStaminaEnabled ? "enabled" : "disabled";
                AddConsoleLog("Infinite Stamina " + parsedStatus);

            }));

            //Toggle infinite carry weight
            ConsoleScreen.Commands.AddCommand(new RegexCommand("infweight", match =>
            {
                infiniteCarryWeightEnabled = !infiniteCarryWeightEnabled;
                Player player = GetPlayerInstance();

                player.Physical.EncumberDisabled = infiniteCarryWeightEnabled;

                string parsedStatus = infiniteCarryWeightEnabled ? "enabled" : "disabled";
                AddConsoleLog("Infinite carry weight " + parsedStatus);
            }));


            //Freecam thanks to Terkoiz
            ConsoleScreen.Commands.AddCommand(new RegexCommand("freecam", match =>
            {
                HookObject.GetComponent<FreecamController>().ToggleCamera();

                string parsedStatus = HookObject.GetComponent<Freecam>().IsActive ? "enabled" : "disabled";
                AddConsoleLog("Freecam " + parsedStatus);
            }));

            //move the player to the freecam thanks to Terkoiz
            ConsoleScreen.Commands.AddCommand(new RegexCommand("moveplayer freecam", match =>
            {
                HookObject.GetComponent<FreecamController>().MovePlayerToCamera();

                AddConsoleLog("Moved the player to the freecam");
            }));

            //Toggle UI When in freecam thanks to Terkoiz
            ConsoleScreen.Commands.AddCommand(new RegexCommand("toggleui", match =>
            {
                HookObject.GetComponent<FreecamController>().ToggleUi();

                string parsedStatus = HookObject.GetComponent<FreecamController>().UIHiddenStatus ? "enabled" : "disabled";
                AddConsoleLog("Freecam " + parsedStatus);
            }));

            //Set the freecam movement speed
            ConsoleScreen.Commands.AddCommand(new RegexCommand("camspeed (\\d+)", match =>
            {
                if (float.TryParse(match.Groups[1].Value, out float speed))
                {
                    Freecam.MovementSpeed = speed;
                    AddConsoleLog("Freecam speed set to " + speed);
                }
                else
                {
                    AddConsoleLog("Unable to parse freecam speed "+ match.Groups[1].Value);
                }
            }));

            //Set the freecam look sensitivity
            ConsoleScreen.Commands.AddCommand(new RegexCommand("camlookspeed (\\d+)", match =>
            {
                if (float.TryParse(match.Groups[1].Value, out float speed))
                {
                    Freecam.FreeLookSensitivity = speed;
                    AddConsoleLog("Freecam look speed set to " + speed);
                }
                else
                {
                    AddConsoleLog("Unable to parse freecam look speed " + match.Groups[1].Value);
                }
            }));

            //Set time
            ConsoleScreen.Commands.AddCommand(new RegexCommand("time (1?[0-9]|2[0-4])", match =>
            {
                if (int.TryParse(match.Groups[1].Value, out int targetTime))
                {
                    GClass965 gameDateTime = Singleton<GameWorld>.Instance.GameDateTime;
                    DateTime currentDateTime = gameDateTime.Calculate();
                    DateTime newDateTime = new DateTime();
                    newDateTime = currentDateTime.AddHours((double)targetTime - currentDateTime.Hour);
                    gameDateTime.Reset(newDateTime);    
                    AddConsoleLog("Set time to " + targetTime);
                }
                else
                {
                    AddConsoleLog("Unable to parse int " + match.Groups[1].Value);
                }
            }));

            //Easter Egg Spaceman
            ConsoleScreen.Commands.AddCommand(new RegexCommand("spaceman", match =>
            {
                moonGravityEnabled = !moonGravityEnabled;
                Physics.gravity = new Vector3(0, moonGravityEnabled ? -1.62F : -9.81F, 0);

                string parsedStatus = moonGravityEnabled ? "enabled" : "disabled";
                AddConsoleLog("Moon gravity " + parsedStatus);
            }));
        }
    }
}
