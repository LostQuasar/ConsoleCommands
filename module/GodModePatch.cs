using Aki.Common.Utils.Patching;
using EFT;
using System.Reflection;
using ActiveHealthController = GClass1475;

namespace Spaceman.ConsoleCommands
{
    internal class GodModePatch : GenericPatch<GodModePatch>
    {
        static public bool godModeEnabled = false;

        static GodModePatch()
        {
        }

        public GodModePatch() : base(prefix: nameof(PatchPrefix))
        {
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(ActiveHealthController).GetMethod("ApplyDamage", BindingFlags.Public | BindingFlags.Instance );
        }

        static bool PatchPrefix(ActiveHealthController __instance)
        {
            if (__instance.Player.IsYourPlayer)
            {
                return !godModeEnabled;
            }
            else
            {
                return true;
            }

        }
    }
}