using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using cynofield.mods.ui;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using static cynofield.mods.ui.AugmentedDisplayLog;
using static cynofield.mods.ui.AugmentedUiManager;
using System;
using System.Collections.Generic;

namespace cynofield.mods
{
    public class AugmentedRealityEntry
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedRealityEntry Instance;

        static AugmentedRealityEntry()
        {
            ModInfo.Instance = new ModInfo()
            {
                name = "mod1332",
                longName = "Augmented Reality",
                version = "1.0"
            };
        }

        static public void Create()
        {
            CLogger.SetConfig(new Dictionary<string, (LoggerConfig.LogLevel, bool)>
            {
                ["cynofield.mods.utils.ModDirLocator"] = (LoggerConfig.LogLevel.WARN, true),
                ["cynofield.mods.utils.AssetsLoader"] = (LoggerConfig.LogLevel.DEBUG, false),
                ["cynofield.mods.utils.PlayerProvider"] = (LoggerConfig.LogLevel.DEBUG, true),
                ["cynofield.mods.ArStateManager"] = (LoggerConfig.LogLevel.WARN, true),
            });

            var modDirLocator = new ModDirLocator(ModInfo.Instance.name);
            AssetsLoader.SetConfig(bundleFiles: new List<string>
            {
                $"{modDirLocator.GetContentDir()}/cynofieldmods.assetbundle",
            });

            Instance?.Destroy();
            Instance = new AugmentedRealityEntry();
        }

        private readonly AugmentedUiManager uiManager;
        private readonly ArStateManager stateManager;
        public AugmentedRealityEntry()
        {
            AssetsLoader assetsLoader;
            try
            {
                assetsLoader = AssetsLoader.Load();
                assetsLoader.DebugInfo();
                var fonts2d = new Fonts2d(assetsLoader);

                var skin = new BaseSkin(fonts2d);
                var colorSchemes = new List<ColorScheme> {
                    new ColorSchemeLightText(), new ColorSchemeDarkText()
                };

                PlayerProvider playerProvider = new PlayerProvider();
                //PlayerProvider.DebugInfo();
                stateManager = new ArStateManager(playerProvider);
                HiddenPool.Create();
                uiManager = AugmentedUiManager.Create(playerProvider, stateManager, skin, colorSchemes, fonts2d);
                stateManager.OnHide += OnHideHandler;
                stateManager.OnShow += OnShowHandler;

                Log.Info(() => "started successfully");
            }
            catch (Exception e)
            {
                Log.Error(e);
                AssetsLoader.Destroy();
            }
        }

        public void Destroy()
        {
            HiddenPool.Destroy();
            uiManager.Destroy();
            AssetsLoader.Destroy();
            Instance = null;
            Log.Info(() => "unloaded");
        }

        void OnHideHandler()
        {
            uiManager.Hide();
        }

        private AnnouncementWorkflow welcomeAnnouncement = new AnnouncementWorkflow();
        void OnShowHandler()
        {
            uiManager.Show();
            welcomeAnnouncement.Reset();
            welcomeAnnouncement.ShowAnnouncement($"Welcome to <color=red>HEROs v{ModInfo.Instance.version}</color>!");
        }

        public void EyesOn(Thing thing)
        {
            if (stateManager == null || uiManager == null || !stateManager.IsVisible())
                return;

            uiManager.EyesOn(CursorEventInfo.FromCursorManager(thing));
        }

        public void MouseOn(Thing thing, Interactable interactable)
        {
            if (stateManager == null || uiManager == null || !stateManager.IsVisible())
                return;

            //Log.Debug(() => $"MouseOn thing={thing}, interactable={interactable}");

            uiManager.MouseOn(CursorEventInfo.FromInputMouse(thing, interactable));
        }

        public void LogToHud(string message)
        {
            uiManager.LogToHud(message);
        }

        public void LogToHud(LogAction logRenderAction)
        {
            uiManager.LogToHud(logRenderAction);
        }

        public AugmentedDisplayBanner GetBanner()
        {
            return uiManager.banner;
        }
    }

    public class ArStateManager
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public ArStateManager(PlayerProvider playerProvider)
        {
            this.playerProvider = playerProvider;
        }

        enum State
        {
            DISABLED, // not visible and don't react on events
            HIDDEN,   // not visible, but react on some events
            VISIBLE,  // visible to the user and react on all events
        }

        public bool IsVisible()
        {
            UpdateState();
            return st == State.VISIBLE;
        }

        public delegate void StateCallback();
        public event StateCallback OnHide;
        public event StateCallback OnShow;

        private readonly PlayerProvider playerProvider;
        private State st = State.DISABLED;

        void UpdateState()
        {
            var oldState = st;
            switch (st)
            {
                case State.DISABLED:
                    {
                        if (!MustDisable())
                        {
                            if (MustHide()) st = State.HIDDEN;
                            else st = State.VISIBLE;
                        }
                    }
                    break;
                case State.HIDDEN:
                    {
                        if (MustDisable()) st = State.DISABLED;
                        else if (!MustHide()) st = State.VISIBLE;
                    }
                    break;
                case State.VISIBLE:
                    {
                        if (MustDisable()) st = State.DISABLED;
                        else if (MustHide()) st = State.HIDDEN;
                    }
                    break;
            }

            if (oldState == st)
                return;

            Log.Info(() => $"{oldState} -> {st}");

            switch (st)
            {
                case State.DISABLED:
                    { }
                    break;
                case State.HIDDEN:
                    {
                        OnHide?.Invoke();
                    }
                    break;
                case State.VISIBLE:
                    {
                        OnShow?.Invoke();
                    }
                    break;
            }
        }

        private bool MustDisable()
        {
            return !playerProvider.IsGameInitialized();
        }

        private bool MustHide()
        {
            //Log.Debug(() => $"MustHide 1");
            var human = playerProvider.GetPlayerAvatar();
            if (!human || human.State != EntityState.Alive)
                return true;

            var glasses = human.GlassesSlot.Occupant;
            if (glasses == null)
                return true;

            if (!(glasses is SensorLenses))
                return true;

            var lenses = glasses as SensorLenses;
            // only check battery state and ignore inserted sensor:
            var power = (lenses.Battery == null) ? 0 : lenses.Battery.PowerStored;
            return !lenses.OnOff || power <= 0;
        }
    }
}
