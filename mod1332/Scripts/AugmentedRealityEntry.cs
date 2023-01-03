using System.Collections.Generic;
using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using cynofield.mods.ui;
using cynofield.mods.utils;
using cynofield.mods.ui.styles;

namespace cynofield.mods
{
    public class AugmentedRealityEntry
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedRealityEntry Instance;

        static public void Create()
        {
            CLogger.SetConfig(new Dictionary<string, (LoggerConfig.LogLevel, bool)>
            {
                ["cynofield.mods.utils.ModDirLocator"] = (LoggerConfig.LogLevel.WARN, true),
                ["cynofield.mods.utils.AssetsLoader"] = (LoggerConfig.LogLevel.DEBUG, false),
                ["cynofield.mods.utils.PlayerProvider"] = (LoggerConfig.LogLevel.DEBUG, true),
                ["cynofield.mods.ArStateManager"] = (LoggerConfig.LogLevel.WARN, true),
            });

            var modDirLocator = new ModDirLocator("mod1332");
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

                PlayerProvider playerProvider = new PlayerProvider();
                //PlayerProvider.DebugInfo();
                stateManager = new ArStateManager(playerProvider);
                HiddenPool.Create();
                uiManager = AugmentedUiManager.Create(playerProvider, skin, fonts2d);
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

        void OnShowHandler()
        {
            uiManager.Show();
        }

        public void EyesOn(Thing thing)
        {
            if (stateManager == null || uiManager == null || !stateManager.IsVisible())
                return;

            uiManager.EyesOn(thing);
        }

        public void MouseOn(Thing thing)
        {
            if (stateManager == null || uiManager == null || !stateManager.IsVisible())
                return;

            uiManager.MouseOn(thing);
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
                        OnHide();
                    }
                    break;
                case State.VISIBLE:
                    {
                        OnShow();
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
