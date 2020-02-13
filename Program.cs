using System;
using System.IO;

namespace m88mem {

    class Program {
        static int ReadValue(string v) {
            if (v.StartsWith("0x")) return Convert.ToInt32(v, 16);
            return Convert.ToInt32(v);
        }

        static void Main(string[] args) {
            if (args.Length < 5) {
                Usage();
                return;
            }

            var cm = new CommMemory();
            if (!cm.OpenMemory("m88mem")) {
                Console.WriteLine("memory not found!");
                return;
            }
            var Send = false;

            switch(args[0]) {
                case "recv":
                    break;
                case "send":
                    Send = true;
                    break;
                default:
                    Usage();
                    return;
            }

            var Memory = ReadValue(args[1]);
            var IsMainRam = Memory == 0;
            var Bank = ReadValue(args[2]);
            var Address = ReadValue(args[3]);
            var Filename = args[4];
            int Length = (args.Length > 5 ? ReadValue(args[5]) : 0x8000);

            if (Send) {
                var Data = File.ReadAllBytes(Filename);
                Length = Length > Data.Length ? Data.Length : Length;
                cm.SendData(IsMainRam, Bank, Address, Length, Data);
            } else {
                var Data = cm.RecvData(IsMainRam, Bank, Address, Length);
                File.WriteAllBytes(Filename, Data);
            }

            Console.WriteLine($"{(Send ? "Send" : "Recv")} {(IsMainRam ? "MainRam" : "ExtRam")} Bank:{Bank} Address:{Address} Length:{Length}");

        }

        private static void Usage() {
            Console.WriteLine(@"m88mem ver 1.0
Usage m88mem <recv|send> <memory> <bank> <address> <filename> [length]
memory: 0 = main, 1 = ext
");
        }
    }
}
