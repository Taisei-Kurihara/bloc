using System;
using System.Linq;

namespace Common
{
    /// <summary>
    /// ModelやViewに何が実装されているのかを判別する為のもの。
    /// </summary>
    public interface Presenter_interface
    {
        object View { get; }


        bool IViewSearch<T>() where T : class
        {
            return View.GetType().GetInterfaces().Contains(typeof(T));
        }
    }
}