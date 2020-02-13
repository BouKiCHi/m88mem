using m88mem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sendem {
    class SendEm {
        static int ReadValue(string v) {
            if (v.StartsWith("0x")) return Convert.ToInt32(v, 16);
            return Convert.ToInt32(v);
        }

        static void Main(string[] args) {
            if (args.Length == 0) {
                Usage();
                return;
            }

            var cm = new CommMemory();
            if (!cm.OpenMemory("m88mem")) {
                Console.WriteLine("memory not found!");
                return;
            }

            var IsMainRam = false;
            var Bank = 0;
            var Address = 0;
            var Filename = args[0];

            var MubData = File.ReadAllBytes(Filename);
            var Magic = System.Text.Encoding.ASCII.GetString(GetBytes(MubData, 0, 4));
            if (Magic != "MUB8") {
                Console.WriteLine("unknown format!!");
            }

            var DataStart = (int)GetDword(MubData, 4);
            var Length = (int)GetDword(MubData, 8);

            var SongData = GetBytes(MubData, DataStart, Length);

            cm.SendData(IsMainRam, Bank, Address, Length, SongData);

            Console.WriteLine($"Send File:{Filename} Length:{Length}");
        }

        static byte[] GetBytes(byte[] source, int index, int length) {
            var d = new byte[length];
            Array.Copy(source, index, d, 0, length);
            return d;
        }

        static uint GetDword(byte[] source, int index) {
            return (uint)source[index+0] + ((uint)source[index+1] << 8) + ((uint)source[index+2] << 16) + ((uint)source[index+3] << 24);
        }

        static uint GetWord(byte[] source, int index) {
            return (uint)source[index+0] + ((uint)source[index+1] << 8);
        }

        private static void Usage() {
            Console.WriteLine(@"sendem ver 1.0
Usage sendem <mub filename>");
        }
    }
}
