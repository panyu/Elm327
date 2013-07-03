using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elm327.Driver.Extensions;
using System.Timers;

namespace Elm327.Driver
{
    public class Monitor : NotifyPropertyChangedBase, IDisposable
    {
        protected ElmDriver driver;
        protected Elm327.Driver.DataRecievedBase.Elm327DataRecievedEventHandler propertyChangedHandler;
        public OBDPid[] Pids { get; set; }
        private dynamic _value { get; set; }
        public dynamic Value
        {
            get { return _value; }
            set
            {
                if (value != -1)
                {
                    _value = value; NotifyPropertyChanged(Identifier);
                }
            }
        }
        protected Guid guid;

        public Guid Guid { get { return guid; } }

        public string Identifier { get { string handler = ""; foreach (var pid in Pids) { handler += pid.GetStringValue(); } return handler; } }

        private Timer timer = new Timer();
        private Timer timeoutTimer = new Timer();
        
        public double Frequency { set { timer.Interval = value; } }

        public Monitor(ElmDriver elmDriver, Func<string, string, dynamic> parseFunction, double frequency, params OBDPid[] pids)
        {
            guid = Guid.NewGuid();
            driver = elmDriver;
            ParseResponseFunction = parseFunction;
            Pids = pids;
            Frequency = frequency;
            timer.Elapsed += timer_Elapsed;
            timeoutTimer.Elapsed += timeoutTimer_Elapsed;
        }

        void timeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeoutTimer.Stop();
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            driver.SendCommand(new PidRequest() { Pids = Pids, Guid = guid });
            timeoutTimer.Interval = timer.Interval * 2;
            timeoutTimer.Start();
        }

        void driver_PropertyChanged(string command, string output)
        {
            foreach(OBDPid pid in Pids)
            if (command.Contains(pid.GetStringValue()))
            {
                Value = ParseResponseFunction(command, output);
                timer.Start();
                break;
            }
        }

        public void Start()
        {
            driver.Elm327DataRecieved += driver_PropertyChanged;
            driver.SendCommand(new PidRequest() { Pids = Pids, Guid = guid });
            timer.Start();
        }

        public void Stop()
        {
            if (propertyChangedHandler != null)
                driver.Elm327DataRecieved -= propertyChangedHandler;
            timer.Stop();
        }

        public Func<string, string, dynamic> ParseResponseFunction;

        public void Dispose()
        {
            if (propertyChangedHandler != null)
                driver.Elm327DataRecieved -= propertyChangedHandler;
        }
    }
}
