using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FastWalkHotkey
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        private bool isToggle = false;
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
        }
        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.keybindFastSpeed.name"),
                tooltip: () => this.Helper.Translation.Get("config.keybindFastSpeed.desc"),
                getValue: () => this.Config.SpeedHotKey,
                setValue: value => this.Config.SpeedHotKey = value
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
                this.isToggle = true;
            }
            this.Monitor.Log($"{Game1.player.Name} 按下了 {e.Button}.", LogLevel.Debug);
        }
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
            this.Monitor.Log($"{Game1.player.Name} 松开了 {e.Button}.", LogLevel.Debug);
            if (Config.SpeedHotKey.JustPressed() && Game1.activeClickableMenu == null)
            {
                this.isToggle = false;
            }
        }
    }
}
