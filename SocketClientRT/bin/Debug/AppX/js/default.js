// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkId=232509
(function () {
    "use strict";

    WinJS.Binding.optimizeBindingReferences = true;

    var app = WinJS.Application;
    var activation = Windows.ApplicationModel.Activation;

    app.onactivated = function (args) {
        if (args.detail.kind === activation.ActivationKind.launch) {
            if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
                // TODO: This application has been newly launched. Initialize
                // your application here.
            } else {
                // TODO: This application has been reactivated from suspension.
                // Restore application state here.
            }
            args.setPromise(WinJS.UI.processAll());
        }
    };

    app.oncheckpoint = function (args) {
        // TODO: This application is about to be suspended. Save any state
        // that needs to persist across suspensions here. You might use the
        // WinJS.Application.sessionState object, which is automatically
        // saved and restored across suspension. If you need to complete an
        // asynchronous operation before your application is suspended, call
        // args.setPromise().
    };

    var gauges = new Array();

    app.onloaded = function (args) {
            var gauge = new Gauge({
                renderTo: 'gauge',
                width: 400,
                height: 400,
                glow: true,
                units: 'rpm',
                title: false,
                minValue: 0,
                maxValue: 8000,
                majorTicks: 8,
                minorTicks: 2,
                strokeTicks: false,
                highlights: [
                    { from: 0, to: 6500, color: 'rgba(0,   255, 0, .15)' },
                    { from: 6500, to: 8000, color: 'rgba(0, 0,  255, .25)' }
                ],
                colors: {
                    plate: '#222',
                    majorTicks: '#f5f5f5',
                    minorTicks: '#ddd',
                    title: '#fff',
                    units: '#ccc',
                    numbers: '#eee',
                    needle: { start: 'rgba(240, 128, 128, 1)', end: 'rgba(255, 160, 122, .9)' }
                }
            });
            gauge.draw();
            gauges[0] = gauge;

        var socket = io.connect('http://localhost:8080');
        socket.on('connect', function () {
            rawMessage("Connected to server");
            socket.emit('subscriber');
        });

        var monitors = [
            new Monitor(socket, monitorSpeed, 50, OBDPids.VehicleSpeed),
            new Monitor(socket, monitorRPM, 50, OBDPids.EngineRPM)
        ];

        socket.on('subscribed', function (args) {
            socket.emit('request', { pids: 'ati' });
        });

        socket.on('disconnect', function () {
            rawMessage("Disconnected to server");
        });

        socket.on('response', function (message) {
            //add message for debug purposes
            //rawMessage(message);

            //parse JSON
            var result = JSON.parse(message);

            if (result.Command.indexOf("ati") != -1) {
                //initialize completed. startup the monitors
                monitors.forEach(function (monitor) {
                    monitor.Start();
                });
            }
            else {
                //notify monitors of new data
                monitors.forEach(function (monitor) {
                    monitor.newDataRecieved(result.Command, result.Output);
                });
            }
        });
    }

    function rawMessage(message) {
        var item = document.createElement("li");
        item.appendChild(document.createTextNode(message));
        document.querySelector('#raw').appendChild(item);
    }

    var monitorSpeed = function (command, output) {
        var commands = output.split("\r\n");
        var speedLoc = command.indexOf(OBDPids.VehicleSpeed) / 4;
        var com = commands.length > 3 ? commands[speedLoc + 2].substring(3) : commands[1];
        if (com.indexOf("41 0D") == 0) {
            var bits = com.substring(6).split(' ');
            var speed = parseInt(bits[0], 16) / 1.609;  //parse and convert kilometers to mph
            document.querySelector('#speed').innerText = speed;
        }
    }

    var below4500 = true;
    var below6500 = true;

    var monitorRPM = function (command, output) {
        var commands = output.split("\r\n");
        var speedLoc = command.indexOf(OBDPids.EngineRPM) / 4;
        var com = commands.length > 3 ? commands[speedLoc + 2].substring(3) : commands[1];
        if (com.indexOf("41 0C") == 0 || com.indexOf("E5 00") == 0) {
            var bits = com.substring(6).split(' ');
            var rpm = (parseInt(bits[0], 16) * 256 + parseInt(bits[1], 16)) / 4;
            gauges[0].setValue(rpm);
            if (rpm < 4500) {
                below4500 = true;
                below6500 = true;
            }
            else if (rpm > 4500 && below4500 && rpm < 6500) {
                YeahToast.show({ title: "Making it rumble", textContent: "Max torque" });
                below4500 = false;
                below6500 = true;
            }
            if (rpm > 6500) {
                YeahToast.show({ title: "Overrev warning", textContent: "Exceeded safe RPM limit" });
                below4500 = false;
                below6500 = false;
            }
            document.querySelector('#rpm').innerText = rpm;
        }
    }

    app.start();
})();
