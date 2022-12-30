using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using cynofield.mods.ui;
using cynofield.mods.utils;

namespace cynofield.mods
{
    public class AugmentedRealityEntry
    {
        public static AugmentedRealityEntry Instance;

        static public void Create()
        {
            Instance?.Destroy();
            Instance = new AugmentedRealityEntry();
        }

        private readonly AugmentedUiManager uiManager;
        private readonly ArStateManager stateManager;
        public AugmentedRealityEntry()
        {
            PlayerProvider playerProvider = new PlayerProvider();
            stateManager = new ArStateManager(playerProvider);
            uiManager = AugmentedUiManager.Create();
        }

        public void Destroy()
        {
            uiManager.Destroy();
            Instance = null;
        }

        public void EyesOn(Thing thing)
        {
            if (stateManager.IsVisible())
            {
                uiManager.EyesOn(thing);
            }
        }

        public void MouseOn(Thing thing)
        {
            if (stateManager.IsVisible())
            {
                uiManager.MouseOn(thing);
            }
        }
    }

    public class ArStateManager
    {
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

        private readonly PlayerProvider playerProvider;
        private State st = State.DISABLED;

        void UpdateState()
        {
            var oldState = st;
            switch (st)
            {
                case State.DISABLED:
                    { }
                    break;
                case State.HIDDEN:
                    {
                        if (MustDisable()) st = State.DISABLED;
                    }
                    break;
                case State.VISIBLE:
                    {
                        if (MustDisable()) st = State.DISABLED;
                        if (MustHide()) st = State.HIDDEN;
                    }
                    break;
            }

            if (oldState == st)
                return;

            switch (st)
            {
                case State.DISABLED:
                    { }
                    break;
                case State.HIDDEN:
                    { }
                    break;
                case State.VISIBLE:
                    { }
                    break;
            }
        }

        private bool MustDisable()
        {
            return !playerProvider.IsGameInitialized();
        }

        private bool MustHide()
        {
            var human = playerProvider.GetPlayerAvatar();
            if (!human || human.State != EntityState.Alive)
                return false;

            var glasses = human.GlassesSlot.Occupant;
            if (glasses == null)
                return false;

            if (!(glasses is SensorLenses))
                return false;

            var lenses = glasses as SensorLenses;
            // only check battery state and ignore inserted sensor:
            var power = (lenses.Battery == null) ? 0 : lenses.Battery.PowerStored;
            return lenses.OnOff && power > 0;
        }
    }
}
