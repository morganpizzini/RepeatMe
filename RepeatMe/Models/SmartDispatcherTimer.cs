using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RepeatMe.Models
{
    public class SmartDispatcherTimer : DispatcherTimer
    {
        public SmartDispatcherTimer()
        {
            base.Tick += SmartDispatcherTimer_Tick;
        }

        async void SmartDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (TickTask == null || (IsRunning && !IsReentrant))
                return;
            try
            {
                // we're running it now
                IsRunning = true;
                await TickTask.Invoke();
            }
            finally
            {
                // allow it to run again
                IsRunning = false;
            }
        }

        public bool IsReentrant { get; set; }
        public bool IsRunning { get; private set; }

        public Func<Task> TickTask { get; set; }
    }
}
