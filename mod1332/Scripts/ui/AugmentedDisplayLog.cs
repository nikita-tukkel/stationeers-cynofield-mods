using cynofield.mods.utils;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayLog : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedDisplayLog Create(VerticalLayoutGroup parent, ThingsUi thingsUi, Fonts2d fonts2d)
        {
            var result = Utils.CreateGameObject<AugmentedDisplayLog>();
            result.Init(fonts2d);
            return result;
        }

        private Fonts2d fonts2d;
        private void Init(Fonts2d fonts2d)
        {
        }
    }

}
