using AmiumScripter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private System.Windows.Forms.Timer idleTimer;
        int Interval = 100;

        public void StartIdleLoop(int interval)
        {
            Interval = interval;
            if (idleTimer != null)
                return;

            idleTimer = new System.Windows.Forms.Timer();
            idleTimer.Interval = Interval;
            idleTimer.Tick += (s, e) => ViewIdleLoop();
            idleTimer.Start();
            Logger.DebugMsg("[PageView] Start IdleLoop (Timer)");
        }

        public void StopIdleLoop()
        {
            if (idleTimer != null)
            {
                idleTimer.Stop();
                idleTimer.Dispose();
                idleTimer = null;
                Logger.DebugMsg("[PageView] IdleLoop stopped (Timer)");
            }
        }

        public virtual void Initialize()
        {


        }

        public virtual void Run() { }

        public virtual void Destroy()
        {
            StopIdleLoop();
            Controls.Clear();
            Logger.DebugMsg("[PageView] TestPage Destroyed");
        }
        public virtual void ViewIdleLoop() { }
    }
}
