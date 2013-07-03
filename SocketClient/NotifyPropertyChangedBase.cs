using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elm327.Driver
{
    public class DataRecievedBase
    {
        public void DataRecieved(string command, string output)
        {
            if (Elm327DataRecieved != null)
                Elm327DataRecieved(command, output);
        }        

        public delegate void Elm327DataRecievedEventHandler(string command, string output);

        public event Elm327DataRecievedEventHandler Elm327DataRecieved;
    }

    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
