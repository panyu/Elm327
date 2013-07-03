using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Elm327.Common
{
    public sealed class Monitor
    {
        public string Pids { get; set; }
        public dynamic Value { get; set; }
        protected Guid guid;

        public Guid Guid { get { return guid; } }

        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer timeoutTimer = new DispatcherTimer();

        public long Frequency { get { return timer.Interval.Ticks; } set { timer.Interval = new TimeSpan(value); } }

        public Monitor(long frequency, string pids)
        {
            guid = Guid.NewGuid();
            ParseResponseFunction = parseFunction;
            Pids = pids;
            Frequency = frequency;
            timer.Tick += timer_Tick;
            timeoutTimer.Tick += timeoutTimer_Tick;
        }

        void timeoutTimer_Tick(object sender, object e)
        {
            timeoutTimer.Stop();
            timer.Start();      
        }

        void timer_Tick(object sender, object e)
        {
            timer.Stop();
            //driver.SendCommand(new PidRequest() { Pids = Pids, Guid = guid }); need to send command with this
            timeoutTimer.Interval = new TimeSpan(timer.Interval.Ticks * 2);
            timeoutTimer.Start();      
        }

        public void newDataRecieved(string command, string output)
        {
            Value = ParseResponseFunction.DynamicInvoke(command, output);
            timer.Start();
        }

        public void Start()
        {
            //driver.SendCommand(new PidRequest() { Pids = Pids, Guid = guid });    //send command
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public Delegate ParseResponseFunction;

        public void Dispose()
        {
            timer.Stop();
            timeoutTimer.Stop();
        }
    }
}
