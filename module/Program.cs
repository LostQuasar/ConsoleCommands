using Aki.SinglePlayer.Utils;
using UnityEngine;
using EFT.UI;
using Newtonsoft.Json;
using EFT.UI.DragAndDrop;
using EFT.InventoryLogic;
using EFT;
using Aki.Common.Utils.Patching;
using System.Reflection;
using System;
using Comfort.Common;
using System.Collections.Generic;

namespace Spaceman.ConsoleCommands
{
    public class Program
    {
        static internal string basePath = "/mod/console_commands/";
        private static readonly string host = Config.BackendUrl;
        static void Main(string[] args)
        {
            Debug.LogError("Spaceman.ConsoleCommands: Adding Console Commands");


            //Change the pmc conversion chance with command
            ConsoleScreen.Commands.AddCommand(new GClass1954("pmcpercent (100|[0-9][0-9]?)", match =>
            {
                int pmcBot = int.Parse(match.Groups[1].Value);
                RequestHandler.PostJson(basePath+"pmcConversionChance", JsonConvert.SerializeObject(pmcBot));
            }));

            //Kill everything including server + client
            ConsoleScreen.Commands.AddCommand(new GClass1954("hcf", match =>
            {
                try
                {
                    RequestHandler.GetJson(basePath+"killServer");
                }
                catch {}
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }));

            ConsoleScreen.Commands.AddCommand(new GClass1954("give (\\S+) (\\d+)", match =>
            {
                RequestHandler.GetJson(basePath+"give/"+match.Groups[1].Value+"/"+match.Groups[2].Value); 
            }));


            //Change the chance of insurance being returned
            ConsoleScreen.Commands.AddCommand(new GClass1954("insurancechance (100|[0-9][0-9]?)", match =>
            {
                int chance = int.Parse(match.Groups[1].Value);
                RequestHandler.PostJson(basePath + "insuranceReturnChance", JsonConvert.SerializeObject(chance));
            }));

            //Change the chance of boss being spawned
            ConsoleScreen.Commands.AddCommand(new GClass1954("bosschance (100|[0-9][0-9]?)", match =>
            {
                int chance = int.Parse(match.Groups[1].Value);
                RequestHandler.PostJson(basePath + "bossChance", JsonConvert.SerializeObject(chance));
            }));
        }
    }
}
