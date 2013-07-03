function Monitor(socket, parseFunction, frequency, pids) {
    var Socket = socket;
    var Frequency = frequency;
    var ParseFunction = parseFunction;
    var Pids = pids;
    var request;

    function setupTimers() {
        return setTimeout(function () {
            clearTimeout(request);
            socket.emit("request", { pids: Pids });
            var timeout = setTimeout(function () {
                clearTimeout(timeout);
                request = setupTimers();
            }, frequency * 2);
        }, frequency);
    }

    Monitor.prototype.newDataRecieved = function (command, output) {
        var all = Pids.split(' ');
        all.forEach(function (pid) {
            if (command.indexOf(pid) != -1) {
                ParseFunction(command, output);
                request = setupTimers();
                return false;
            }
        });
    }

    Monitor.prototype.Start = function () {
        request = setupTimers();
    }

    Monitor.prototype.Stop = function () {
        clearInterval(request);
        clearInterval(timeout);
    }
}