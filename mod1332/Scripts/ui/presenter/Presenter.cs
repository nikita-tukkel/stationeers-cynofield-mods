using System.Collections.Generic;
using cynofield.mods.utils;
using UnityEngine;

namespace cynofield.mods.ui.presenter
{
    public class PresenterBase<T> : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private readonly List<IPresenterBinding<T>> bindings = new List<IPresenterBinding<T>>();

        public PresenterBase()
        {
            this.name = this.ToString();
        }

        virtual public void Present(T data)
        {
            if (!isActiveAndEnabled)
                return;

            foreach (var binding in bindings)
            {
                binding.Present(data);
            }
        }

        public void AddBinding(IPresenterBinding<T> presenterBinding)
        {
            bindings.Add(presenterBinding);
        }
    }

    public delegate void PresenterAction<T>(T data);

    public interface IPresenterBinding<T>
    {
        void Present(T data);
    }

    public class PresenterBindingBase<T> : IPresenterBinding<T>
    {
        private readonly PresenterAction<T> action;
        public PresenterBindingBase(PresenterAction<T> action)
        {
            this.action = action;
        }

        public void Present(T data) => action(data);
    }
}
