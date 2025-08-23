using System;
using System.Linq;

namespace Common
{
    /// <summary>
    /// Model‚âView‚É‰½‚ªÀ‘•‚³‚ê‚Ä‚¢‚é‚Ì‚©‚ğ”»•Ê‚·‚éˆ×‚Ì‚à‚ÌB
    /// </summary>
    public interface IPresenter
    {
        object View { get; }


        bool IViewSearch<T>() where T : class
        {
            return View.GetType().GetInterfaces().Contains(typeof(T));
        }
    }
}