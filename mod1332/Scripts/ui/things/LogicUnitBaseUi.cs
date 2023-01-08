using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using UnityEngine;

namespace cynofield.mods.ui.things
{
    class LogicUnitBaseUi : IThingDescriber, IThingExtendedSupport, IThingWatcher
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private readonly ViewLayoutFactory lf;
        private readonly BaseSkin skin;
        public LogicUnitBaseUi(ViewLayoutFactory lf, BaseSkin skin)
        {
            this.lf = lf;
            this.skin = skin;
        }

        public Type SupportedType() => typeof(LogicUnitBase);

        public bool IsSupported(Thing thing)
        {
            // This is bullshit, but Stationeers.Addons blacklists methods of System.Type.
            // If someday Stationeers.Addons will become more practical, this code must be replaced with Type.IsAssingableFrom.
            try
            {
                LogicUnitBase check = (LogicUnitBase)thing;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string Describe(Thing thing)
        {
            var obj = thing as LogicUnitBase;
            if (obj.Powered && obj.Error == 0)
                return $"{obj.DisplayName} {skin.MathDisplay(obj.Setting)}";
            else
                return $"{obj.DisplayName} OFF";
        }

        public GameObject RenderWatch(Thing thing, RectTransform parentRect, TagParser.Tag watcherTag)
        {
            PresenterDefault presenter = parentRect.GetComponentInChildren<PresenterDefault>();

            if (presenter == null)
            {
                //Log.Debug(() => $"Creating new watch for {thing.DisplayName}");
                presenter = parentRect.GetOrAddComponent<PresenterDefault>();
                {
                    var view = lf.Text1(parentRect.gameObject, $"");
                    presenter.AddBinding((th) => view.value.text = Utils.GetName(th));
                }
                var hl = lf.CreateRow(parentRect.gameObject);
                {
                    var setting = lf.Text2(hl.gameObject, "", width: 60);
                    setting.value.margin = new Vector4(5, 0, 5, 0);
                    presenter.AddBinding((th) =>
                    {
                        var obj = th as LogicUnitBase;
                        if (obj.Powered && obj.Error == 0)
                            setting.value.text = skin.MathDisplay(obj.Setting);
                        else
                            setting.value.text = "OFF";
                    });
                }
            }

            presenter.Present(thing);
            return presenter.gameObject;
        }
    }
}
