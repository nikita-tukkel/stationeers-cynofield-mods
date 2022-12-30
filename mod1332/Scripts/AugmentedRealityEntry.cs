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
            uiManager = AugmentedUiManager.Create(playerProvider);
            stateManager.OnHide += OnHideHandler;
            stateManager.OnShow += OnShowHandler;
        }

        public void Destroy()
        {
            uiManager.Destroy();
            Instance = null;
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
            if (!stateManager.IsVisible())
                return;

            // var thingName = thing ? thing.DisplayName : null;
            // Debug.Log($"{ToString()} EyesOn {thingName}");
            uiManager.EyesOn(thing);
        }

        public void MouseOn(Thing thing)
        {
            if (!stateManager.IsVisible())
                return;

            // var thingName = thing ? thing.DisplayName : null;
            // Debug.Log($"{ToString()} MouseOn {thingName}");
            uiManager.MouseOn(thing);
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
                        //Debug.Log($"{ToString()} 1 {st}");
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

            //Debug.Log($"{ToString()} {oldState} -> {st}");

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
            //Debug.Log($"{ToString()} MustHide 1");
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
