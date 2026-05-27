using GenericModConfigMenu;
using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Audio;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System.Reflection;

namespace FastWalkHotkey
{
    internal class FasterPathSpeed
    {

        public static void PostGetFarmerMovementSpeed(Farmer who, ref float refMovementSpeed)
        {
            if (who == null || Game1.currentLocation == null ||(Game1.CurrentEvent == null && who.hasBuff("19")) || (Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence))
            {
                return;
            }

            if (ModEntry.isToggle)
            {
                refMovementSpeed *= ModEntry.Config.FastSpeed;
                return;
            }
        }


    }

    public class FarmerPatches
    {
        private static IMonitor Monitor;
        public static void Init(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static void GetMovementSpeed_Postfix(Farmer __instance, ref float __result)
        {
            try
            {
                FastWalkHotkey.FasterPathSpeed.PostGetFarmerMovementSpeed(__instance, ref __result);
            }
            catch (Exception value)
            {
                //Monitor.Log($"Fail in {"GetMovementSpeed_Postfix"}:\n{value}", LogLevel.Debug);
            }
        }
    }



    public class ModEntry : Mod
    {
        public static ModConfig Config;
        public static bool isToggle = false;

        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            //helper.Events.Input.ButtonReleased += OnButtonReleased;
        }
        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Harmony harmony = new Harmony(this.ModManifest.UniqueID);
            FarmerPatches.Init(this.Monitor);
            harmony.Patch(AccessTools.Method(typeof(Farmer), "getMovementSpeed", null, null), null, new HarmonyMethod(typeof(FarmerPatches), "GetMovementSpeed_Postfix", null), null, null);
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(Config)
            );

            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.keybindFastSpeed.name"),
                tooltip: () => this.Helper.Translation.Get("config.keybindFastSpeed.desc"),
                getValue: () => Config.SpeedHotKey,
                setValue: value => Config.SpeedHotKey = value
                );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.fastspeed.name"),
                tooltip: () => this.Helper.Translation.Get("config.fastspeed.desc"),
                getValue: () => Config.FastSpeed,
                min: 2,
                max: 8,
                interval: 1,
                setValue: value => Config.FastSpeed = value
                );
        }
        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            isToggle = false;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(!Context.IsWorldReady)
            {
                return;
            }
            if(Config.SpeedHotKey.JustPressed() && Game1.activeClickableMenu == null)
            {
                isToggle = !isToggle;
            }
            //this.Monitor.Log($"{Game1.player.Name} 按下了 {e.Button}.", LogLevel.Debug);
        }
        //private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        //{
        //    if (!Context.IsWorldReady)
        //    {
        //        return;
        //    }
        //    this.Monitor.Log($"{Game1.player.Name} 松开了 {e.Button}.", LogLevel.Debug);
        //    if (Config.SpeedHotKey.JustPressed() && Game1.activeClickableMenu == null)
        //    {
        //        this.isToggle = false;
        //    }
        //}
    }
}
