using Aki.Common.Utils.Patching;
using System.Reflection;

namespace Spaceman.ConsoleCommands
{
    internal class InfiniteStaminaPatch : GenericPatch<InfiniteStaminaPatch>
    {
        static public bool infiniteStaminaEnabled = false;

        static InfiniteStaminaPatch()
        {
        }

        public InfiniteStaminaPatch() : base(prefix: nameof(PatchPrefix))
        {
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass423).GetMethod("Consume", BindingFlags.Public | BindingFlags.Instance);
        }

        static bool PatchPrefix()
        {
            return !infiniteStaminaEnabled;
        }
    }
}