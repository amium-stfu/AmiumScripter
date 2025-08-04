using AmiumScripter.Helpers;
using AmiumScripter.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AmiumScripter.Core
{
    public interface IView
    {

        void Initialize();
        void Run();
        void Destroy();

        void ViewIdleLoop();
    }
    public abstract class BaseView : UserControl, IView
    {

        public AThread IdleLoop;
        int Interval = 100;
        public void StartIdleLoop(int interval)
        {
            Interval = interval;
            if (IdleLoop != null && IdleLoop.IsRunning) return;

            IdleLoop = new("updateView", () => _idle(), true);

            if (IdleLoop == null || !IdleLoop.IsRunning)
            {
                IdleLoop.Start();
            }
            Logger.Log("[PageView] Start IdleLoop");
        }

        public virtual void Initialize()
        {


        }

        public virtual void Run() { }

        public void Destroy()
        {
            Controls.Clear();
            Logger.Log("[PageView] TestPage Destroy");
        }
        public virtual void ViewIdleLoop() { }

        void _idle()
        {
            while (IdleLoop.IsRunning)
            {
                System.Threading.Thread.Sleep(Interval);
                ViewIdleLoop();
            }
        
        }

        public void SafeInvoke(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }


    }
}
