using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketIOClient.Messages;
using Elm327.Driver;

namespace SocketClient
{
    class Program
    {
        static SocketIOClient.Client client = new SocketIOClient.Client("http://localhost:8080/server.js");
        static void Main(string[] args)
        {
            ElmDriver driver = new ElmDriver("COM7", 115200);
            Console.Read();
        }        
    }
}
