using TaleWorlds.MountAndBlade;
using HarmonyLib;
using TaleWorlds.Core;


namespace SiegeBreakInOutsideBattleFix
{
    public class SubModule : MBSubModuleBase
    {
        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);
            if (_lateHarmonyPatchApplied) {return;}
            Harmony harmony = new Harmony("com.SiegeBreakInOutsideBattleFix");
            harmony.PatchAll();
            _lateHarmonyPatchApplied = true;
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            //Harmony harmony = new Harmony("com.SiegeBreakInOutsideBattleFix");
            //harmony.PatchAll();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

        }

        private bool _lateHarmonyPatchApplied = false;
    }
}