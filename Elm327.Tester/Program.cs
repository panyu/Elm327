using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elm327.Driver;
using System.Diagnostics;
using SocketIOClient;
using Newtonsoft.Json.Linq;
using Elm327.Driver.Extensions;
using System.Timers;

namespace Elm327.Tester
{
    class Program
    {
        static ElmDriver driver;
        static Monitor Speed, RPM, MAFRate, IntakeAirTemp;
        static Client socket = new Client("http://localhost:8080/server.js");
        static List<Monitor> monitors;
        static double? LastSpeed;
        static DateTime start_0_60, end_0_60, end_0_120;

        static void Main(string[] args)
        {
            Console.Write("Enter COM Port: ");
            var port = Console.ReadLine();
            driver = new ElmDriver(port, 115200);
            InitializeMonitors();
            monitors = new List<Monitor>() { Speed, RPM };//, MAFRate, IntakeAirTemp };
            monitors.ForEach(m => m.PropertyChanged += m_PropertyChanged);
            socket.Opened += socket_Opened;
            socket.Message += socket_Message;
            socket.HeartBeatTimerEvent += socket_HeartBeatTimerEvent;
            socket.ConnectionRetryAttempt += socket_ConnectionRetryAttempt;
            socket.Error += socket_Error;
            socket.SocketConnectionClosed += socket_SocketConnectionClosed;
            socket.Connect();
            Console.Read();
            monitors.ForEach(m => m.Stop());
        }

        static void m_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string source = "";
            var monitor = sender as Monitor;
            if (sender.Equals(Speed)){
                source = "speed";
                if(monitor.Value == 0){
                    start_0_60 = DateTime.MinValue;
                    end_0_60 = DateTime.MinValue;
                    end_0_120 = DateTime.MinValue;
                }
                else if(LastSpeed == 0 && monitor.Value != 0){
                    start_0_60 = DateTime.Now;
                }
                else if (end_0_60 == DateTime.MinValue && monitor.Value >= 60 && monitor.Value < 100)
                {
                    end_0_60 = DateTime.Now;
                    socket.Emit("stats", new { Stat = "0_60", Value = new TimeSpan(end_0_60.Ticks - start_0_60.Ticks) });
                }
                else if (end_0_120 == DateTime.MinValue && monitor.Value >= 100)
                {
                    end_0_120 = DateTime.Now;
                    socket.Emit("stats", new { Stat = "0_120", Value = new TimeSpan(end_0_120.Ticks - start_0_60.Ticks) });
                }
                LastSpeed = monitor.Value;
            }
            else if (sender.Equals(RPM))
                source = "rpm";            
            else if (sender.Equals(IntakeAirTemp))
                source = "intaketemp";
            else if (sender.Equals(MAFRate))
                source = "mafrate";
            else
                source = "output";
            socket.Emit("response", new { Source = source, Value = monitor.Value });
        }

        static void socket_SocketConnectionClosed(object sender, EventArgs e)
        {
            Debug.WriteLine("socket closed");
        }

        static void socket_Error(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine("socket error: " + e.Message);
        }

        static void socket_ConnectionRetryAttempt(object sender, EventArgs e)
        {
            Debug.WriteLine("retrying connection");
        }

        static void socket_HeartBeatTimerEvent(object sender, EventArgs e)
        {
            Debug.WriteLine("heartbeat");
        }

        static void socket_Message(object sender, MessageEventArgs e)
        {
            if (e.Message.Event == "open")
                socket.Emit("provider", "o hai there. got some data for youz");
            else if (e.Message.Event == "provided")
                driver.OpenConnection();
            else if (e.Message.Event == "start")
            {
                driver.SendCommand(new PidRequest() { Guid = Guid.NewGuid(), Pids = new OBDPid[] { OBDPid.Elm327Initialize } });
                driver.Elm327DataRecieved += driver_Elm327DataRecieved;
            }
            else if (e.Message.Event == "stop")
            {
                monitors.ForEach(m => m.Stop());
                driver.CloseConnection();
            }              
        }

        static void driver_Elm327DataRecieved(string command, string output)
        {
            if (command == "atz")
            {
                driver.Elm327DataRecieved -= driver_Elm327DataRecieved;
                monitors.ForEach(m => m.Start());
            }
        }

        static void socket_Opened(object sender, EventArgs e)
        {
            Debug.WriteLine("socket opened. attempting to connect to ECU");
        }

        static void InitializeMonitors()
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
                        speed = (int)((double)Convert.ToInt32(speedBits[0], 16) / 1.609);
                        return speed;
                    }
                    else if (output.Contains("NO DATA"))
                        return Speed.Value;
                    return Speed.Value;
                }
                catch (Exception err) { return -1; }
            }, 100, OBDPid.VehicleSpeed);
            RPM = new Monitor(driver, (command, output) =>
            {
                try
                {
                    var commands = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (commands.Length > 1)
                    {
                        var rpmLoc = command.IndexOf(OBDPid.EngineRPM.GetStringValue()) / 4;
                        var com = commands.Length > 2 ? commands[rpmLoc + 2].Substring(3) : commands[1];
                        if (com.StartsWith("41 0C"))
                        {
                            double rpm;
                            var rpmBits = com.Substring(6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            rpm = (double)((Convert.ToInt16(rpmBits[0], 16) * 256) + Convert.ToInt32(rpmBits[1], 16)) / 4.0;
                            return rpm;
                        }
                        else if (output.Contains("NO DATA"))
                            return -1;
                    }
                    return -1;
                }
                catch (Exception) { return -1; }
            }, 50, OBDPid.EngineRPM);
            MAFRate = new Monitor(driver, (command, output) =>
            {
                try
                {
                    var commands = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var loadLoc = command.IndexOf(OBDPid.MAFRate.GetStringValue()) / 4;
                    var com = commands[loadLoc + 2].Substring(3);
                    if (com.StartsWith("41 10"))
                    {
                        double maf;
                        var mafBits = com.Substring(6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        maf = (double)((Convert.ToInt16(mafBits[0], 16) * 256) + Convert.ToInt32(mafBits[1], 16)) / 100.0;
                        return maf;
                    }
                    return -1;
                }
                catch (Exception err) { return -1; }
            }, 200, OBDPid.MAFRate);
            IntakeAirTemp = new Monitor(driver, (command, output) =>
            {
                try
                {
                    var commands = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var tempLoc = command.IndexOf(OBDPid.EngineCoolantTemp.GetStringValue()) / 4;
                    var com = commands[tempLoc + 2].Substring(3);
                    if (com.StartsWith("41 0F"))
                    {
                        double temp;
                        var tempBits = com.Substring(6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        temp = Convert.ToInt16(tempBits[0], 16) - 40;
                        return temp;
                    }
                    return -1;
                }
                catch (Exception err) { return -1; }
            }, 500, OBDPid.IntakeAirTemperature);
        }
    }
}
