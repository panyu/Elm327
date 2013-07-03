using Elm327.Driver.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Elm327.Driver
{
    public enum OBDPid : int
    {
        [StringValue("atz")]
        Elm327Initialize,
        [StringValue("ati")]
        Elm327Info,
        [StringValue("ate0")]
        Elm327EchoOff,
        [StringValue("ate1")]
        Elm327EchoOn,
        [StringValue("0100")]
        PidSupport_01_20 = 0,
        [StringValue("0101")]
        MonitorStatus = 1,
        [StringValue("0102")]
        FreezeDTC = 2,
        [StringValue("0103")]
        FuelSystemStatus = 3,
        [StringValue("0104")]
        CalcEngineLoad = 4,
        [StringValue("0105")]
        EngineCoolantTemp = 5,
        [StringValue("0106")]
        ShortTermFuelPercentTrimBankOne = 6,
        [StringValue("0107")]
        LongTermFuelPercentTrimBankOne = 7,
        [StringValue("0108")]
        ShortTermFuelPercentTrimBankTwo = 8,
        [StringValue("0109")]
        LongTermFuelPercentTrimBankTwo = 9,
        [StringValue("010A")]
        FuelPressure = 10,
        [StringValue("010B")]
        IntakeManifoldAbsolutePressure = 11,
        [StringValue("010C")]
        EngineRPM = 12,
        [StringValue("010D")]
        VehicleSpeed = 13,
        [StringValue("010E")]
        TimingAdvance = 14,
        [StringValue("010F")]
        IntakeAirTemperature = 15,
        [StringValue("0110")]
        MAFRate = 16,
        [StringValue("0111")]
        ThrottlePosition = 17,
        [StringValue("0112")]   
        CommandedSecondaryAirStatus = 18,
        [StringValue("0113")]
        OxygenSensorsPresent_BankOne_Sensors_1_4 = 19,
        [StringValue("011C")]
        OBDStandard = 20,
        [StringValue("011D")]
        OxygenSensorsPresent_A0_7 = 21,
        [StringValue("011E")]
        AuxilaryInputStatus = 22,
        [StringValue("011F")]
        RunTimeSinceEngineStart = 23,
        [StringValue("0120")]
        PidSupport_21_40 = 24,
        [StringValue("0121")]
        DistanceTraveledWithMILOn = 25,
        [StringValue("0122")]
        FuelRailPressureRelativeToManifoldVacuum = 26,
        [StringValue("0123")]
        FuelRailPressureDirectInject = 27,
        [StringValue("012E")]
        CommandedEvaporativePurge = 28,
        [StringValue("012F")]
        FuelLevelInput = 29,
        [StringValue("0130")]
        NumberWarmUpsSinceCodeCleared = 30,
        [StringValue("0131")]
        FuelLevelInput_2 = 31,
        [StringValue("0132")]
        EvaporationSystemVaporPressure = 32,
        [StringValue("0133")]
        BarometricPressure = 33,
        [StringValue("0140")]
        PidSupport_41_60 = 34,
        [StringValue("0141")]
        MonitorStatusThisDriveCycle = 35,
        [StringValue("0142")]
        ControlModuleVoltage = 36,
        [StringValue("0143")]
        AbsoluteLoadValue = 37,
        [StringValue("0144")]
        CommandEquivalenceRatio = 38,
        [StringValue("0145")]
        RelativeThrottlePosition = 39,
        [StringValue("0146")]
        AmbientAirTemperature = 40,
        [StringValue("0147")]
        AbsoluteThrottlePosB = 41,
        [StringValue("0148")]
        AbsoluteThrottlePosC = 42,
        [StringValue("0149")]
        AbsoluteThrottlePosD = 43,
        [StringValue("014A")]
        AbsoluteThrottlePosE = 44,
        [StringValue("014B")]
        AbsoluteThrottlePosF = 45,
        [StringValue("014C")]
        CommandedThrottleActuator = 46,
        [StringValue("014D")]
        TimeRunWithMILOn = 47,
        [StringValue("014E")]
        TimeSinceTroubleCodesCleared = 48,
        [StringValue("0151")]
        FuelRate = 49,
        [StringValue("0152")]
        EthanolFuelPercent = 50,
        [StringValue("0159")]
        FuelRailPressure = 51,
        [StringValue("015A")]
        RelativeAcceleratorPedalPosition = 53,
        [StringValue("015B")]
        HybridBatteryPackRemainingLife = 54,
        [StringValue("015C")]
        EngineOilTemperature = 55,
        [StringValue("015D")]
        FuelInjectionTiming = 56,
        [StringValue("015E")]
        EngineFuelRate = 57,
        [StringValue("015F")]
        EmissionRequirements = 58,
        [StringValue("0160")]
        PidSupport_61_80 = 59,
        [StringValue("0161")]
        DriverDemandEnginePercentTorque = 60,
        [StringValue("0162")]
        ActualEnginePercentTorque = 61,
        [StringValue("0163")]
        EngineReferenceTorque = 62,
        [StringValue("0164")]
        EnginePercentTorqueData = 63,
        [StringValue("0165")]
        AuxilaryIOSupported = 64,
        [StringValue("0166")]
        MassAirFlowSensor = 65,
        [StringValue("0167")]
        EngineCoolantTemperature = 66,
        [StringValue("0168")]
        IntakeAirTemperatureSensor = 67,
        [StringValue("0169")]
        CommandedEGRAndError = 68,
        [StringValue("016A")]
        CommandedDieselIntakeAirFlowControl = 69,
        [StringValue("016B")]
        FuelPressureControlSystem = 70,
        [StringValue("016E")]
        InjectionPressureControlSystem = 71,
        [StringValue("016F")]
        TurbochargerCompressorInletPressure = 72,
        [StringValue("0170")]
        BoostPressureControl = 73,
        [StringValue("0171")]
        VariableGeometryTurboControl = 74,
        [StringValue("0172")]
        WastegateControl = 75,
        [StringValue("0173")]
        TurbochargerRPM = 76,
        [StringValue("0174")]
        TurbochargerTemperature = 77,
        [StringValue("0175")]
        TurbochargerTemperature_2 = 78,
        [StringValue("0176")]
        ChargeAirCoolerTemperature = 79,
        [StringValue("0177")]
        EngineGasTemperatureBank_1 = 80,
        [StringValue("0178")]
        EngineGasTemperatureBank_2 = 81,
        [StringValue("017F")]
        EngineRunTime = 82,
        [StringValue("0180")]
        PidSupport_80_A0 = 83,
        [StringValue("0181")]
        EngineRunTimeAECD = 84,
        [StringValue("0182")]
        EngineRunTimeAECD_2 = 85,
        [StringValue("0183")]
        NOxSensor = 86,
        [StringValue("0184")]
        ManifoldSurfaceTemperature = 87,
        [StringValue("0185")]
        NOxReagentSystem = 88,
        [StringValue("0186")]
        ParticulateMatterSensor = 89,
        [StringValue("0187")]
        IntakeManifoldAbsolutePressure_2 = 90,
        [StringValue("0202")]
        FreezeFrameTroubleCode = 91,
        [StringValue("03")]
        RequestTroubleCodes = 92,
        [StringValue("04")]
        ClearTroubleCodes = 93,
        [StringValue("0901")]
        VinMessageCount = 94,
        [StringValue("0902")]
        VINNumber = 95,
        [StringValue("0903")]
        CalibrationID = 96,
        [StringValue("0904")]
        Calibration = 97,
    }

    public class PidRequest
    {
        public Guid Guid { get; set; }
        public OBDPid[] Pids { get; set; }
    }

    public class ElmDriver : DataRecievedBase
    {
        SerialPort port;
        private string _output;
        public string Output
        {
            get { return _output; }
            set {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _output = value;
                    if (processing.Count > 0 && value.Length >= (processing.Peek() + " \r\n").Length)
                    {
                        Debug.WriteLine(processing.Peek(), "completed");
                        DataRecieved(processing.Count == 0 ? "output" : processing.Dequeue(), value);
                    }
                    Debug.WriteLine(value, "output");
                }
            }
        }
        string OutputBuffer;
        public Queue<PidRequest> requests = new Queue<PidRequest>();
        public Queue<string> processing = new Queue<string>();
        const int MAX_SIMULTANEOUS_COMMANDS = 6;

        Timer timer = new Timer(50);

        public ElmDriver(string portName, int baudRate)
        {
            port = new SerialPort(portName, baudRate, Parity.None, 8);
            port.NewLine = "\r\n";
            port.DataReceived += port_DataReceived;
            timer.Elapsed += timer_Elapsed;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (port.IsOpen)
            {
                timer.Stop();
                port.DiscardOutBuffer();
                port.DataReceived -= port_DataReceived;
                string command = "";
                if (requests.Count > 0)
                {
                    if (requests.Count > MAX_SIMULTANEOUS_COMMANDS)
                    {
                        for (int i = 0; i < MAX_SIMULTANEOUS_COMMANDS; i++)
                        {
                            var request = requests.Dequeue();
                            foreach (OBDPid pid in request.Pids)
                            {
                                if (!command.Contains(pid.GetStringValue()))
                                    command += pid.GetStringValue() + " ";
                            }
                        }
                        Debug.WriteLine(command, "command");
                        processing.Enqueue(command);
                        port.Write(command + "\r\n");
                    }
                    else
                    {
                        while (requests.Count > 0)
                        {
                            var request = requests.Dequeue();
                            if (request != null)
                            {
                                foreach (OBDPid pid in request.Pids)
                                {
                                    if (!command.Contains(pid.GetStringValue()))
                                        command += pid.GetStringValue();
                                }
                            }
                        }
                        Debug.WriteLine(command, "command");
                        processing.Enqueue(command);
                        port.Write(command + "\r\n");
                    }
                }
                port.DataReceived += port_DataReceived;
                timer.Start();
            }
        }
            
        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var bytesToRead = port.BytesToRead;
            char[] text = new char[bytesToRead];
            port.Read(text, 0, bytesToRead);
            var asStr = new string(text);
            if (asStr.Contains('>'))
            {
                var separated = asStr.Split('>');
                foreach (var line in separated)
                {
                    OutputBuffer += line;
                    Output = OutputBuffer;
                    if (!line.Equals(separated.Last()))
                        OutputBuffer = string.Empty;
                }
            }
            else
                OutputBuffer += asStr;
        }

        public void OpenConnection()
        {
            if (port == null)
            {
                Debug.Fail("Port not initialized");
                throw new Exception();
            }
            else
            {
                try
                {
                    if (port.IsOpen)
                        port.Close();
                    port.Open();
                    port.DtrEnable = true;
                    timer.Start();
                    Debug.WriteLine(string.Format("Port {0} opened", port.PortName));
                }
                catch (Exception err)
                {
                    Debug.Fail("Failed to open port", err.Message);
                }
            }
        }        

        public void SendCommand(PidRequest request)
        {
            requests.Enqueue(request);
        }

        public void CloseConnection()
        {
            if (port == null)
            {
                Debug.Fail("Port not initialized");
                throw new Exception();
            }
            else
            {
                if (port.IsOpen)
                {
                    timer.Stop();
                    port.Close();
                    Debug.WriteLine(string.Format("Port {0} closed", port.PortName));
                }
                else
                    Debug.WriteLine("Port is already closed");
            }
        }
    }
}
