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
            // Presenter relies on external state control.
            // If `Present` is called for non-existent, hidden or otherwise inconsistent objects,
            //  it is the problem on the caller side.
            foreach (var binding in bindings)
            {
                binding.Present(data);
            }
        }

        public void AddBinding(IPresenterBinding<T> presenterBinding)
        {
            bindings.Add(presenterBinding);
        }

        public void AddBinding(PresenterAction<T> action)
        {
            var presenterBinding = new PresenterBindingBase<T>(action);
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

    // For now it is only a marker interface. Reconsider if it is usefull later.
    public interface IPresenterView { }
}
