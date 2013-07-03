using Elm327.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Elm327.Driver.Extensions;
using Elm327.Driver;

namespace Elm327.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ElmDriver driver = new ElmDriver("COM7", 115200);
        Monitor Speed, RPM, EngineOilTemp, EngineLoad, CoolantTemp, EngineFuelRate;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Speed.Stop();
            RPM.Stop();
            EngineLoad.Stop();
            CoolantTemp.Stop();
            driver.CloseConnection();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            driver.OpenConnection();
            InitializeMonitors();
            driver.SendCommand(new PidRequest() { Guid = Guid.NewGuid(), Pids = new OBDPid[] { OBDPid.Elm327Initialize } });
            Speed.Start();
            RPM.Start();
            EngineLoad.Start();
            CoolantTemp.Start();
        }

        void InitializeMonitors()
        {
            Speed = new Monitor(driver, (command, output) =>
            {
                try
                {
                    var commands = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var speedLoc = command.IndexOf(OBDPid.VehicleSpeed.GetStringValue()) / 4;
                    var com = commands.Length > 2 ? commands[speedLoc + 2].Substring(3) : commands[1];
                    if (com.StartsWith("41 0D"))
                    {
                        double speed;
                        var speedBits = com.Substring(6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        speed = (double)Convert.ToInt32(speedBits[0], 16) / 1.609;
                        Dispatcher.InvokeAsync(() => { SpeedBlock.Text = string.Format("Speed: {0}", speed); });
                        return speed;
                    }
                    return -1;
                }
                catch (Exception err) { return -1; }
            }, 100, OBDPid.VehicleSpeed);
            RPM = new Monitor(driver, (command, output) =>
            {
                try
                {
                    var commands = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var rpmLoc = command.IndexOf(OBDPid.EngineRPM.GetStringValue()) / 4;
                    var com = commands.Length > 2 ? commands[rpmLoc + 2].Substring(3) : commands[1];
                    if (com.StartsWith("41 0C") || com.StartsWith("E5 00"))
                    {
                        double rpm;
                        var rpmBits = com.Substring(6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        rpm = (double)((Convert.ToInt16(rpmBits[0], 16) * 256) + Convert.ToInt32(rpmBits[1], 16)) / 4.0;
                        Dispatcher.InvokeAsync(() => { RpmBlock.Text = string.Format("RPM: {0}", rpm); });
                        if (rpm > 10000)
                            Debugger.Break();
                        return rpm;
                    }
                    return -1;
                }
                catch (Exception) { return -1; }
            }, 50, OBDPid.EngineRPM);
            EngineLoad = new Monitor(driver, (command, output) =>
            {
                try
                {
                    var commands = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var loadLoc = command.IndexOf(OBDPid.CalcEngineLoad.GetStringValue()) / 4;
                    var com = commands[loadLoc + 2].Substring(3);
                    if (com.StartsWith("41 04") || com.StartsWith("E5 00"))
                    {
                        double load;
                        var loadBits = com.Substring(6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        load = (((double)Convert.ToInt16(loadBits[0], 16) * 100.0) / 255.0);
                        Dispatcher.InvokeAsync(() => { LoadBlock.Text = string.Format("Load: {0}", load); });
                        return load;
                    }
                    return -1;
                }
                catch (Exception err) { return -1; }
            }, 200, OBDPid.CalcEngineLoad);
            CoolantTemp = new Monitor(driver, (command, output) =>
            {
                try
                {
                    var commands = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var tempLoc = command.IndexOf(OBDPid.EngineCoolantTemp.GetStringValue()) / 4;
                    var com = commands[tempLoc + 2].Substring(3);
                    if (com.StartsWith("41 05"))
                    {
                        double temp;
                        var tempBits = com.Substring(6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        temp = Convert.ToInt16(tempBits[0], 16) - 40;
                        Dispatcher.InvokeAsync(() => { TempBlock.Text = string.Format("Temperature: {0}", temp); });
                        return temp;
                    }
                    return -1;
                }
                catch (Exception err) { return -1; }
            }, 1000, OBDPid.EngineCoolantTemp);
        }
    }
}
