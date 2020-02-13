using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace m88mem {
    class CommMemory : IDisposable {

        enum MemoryCommand {
            NoOperation,
            ReadMainRam,
            ReadExtRam,
            WriteMainRam,
            WriteExtRam,
        };

        const int MemorySize = 16 + 65536;
        MemoryMappedFile mmf;

        public bool OpenMemory(string Name) {
            try {
                mmf = MemoryMappedFile.OpenExisting(Name);
            } catch (FileNotFoundException) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 処理完了まで待機
        /// </summary>
        public void WaitCommand() {
            byte[] buf = new byte[16];

            using (var accessor = mmf.CreateViewAccessor()) {
                while (true) {
                    accessor.ReadArray(0, buf, 0, 16);
                    // PacketId
                    if (buf[0] == buf[1]) break;
                    Thread.Sleep(10);
                }
            }
        }

        public void SetCommand(int Command, int Bank, int Address, int Length) {
            byte[] buf = new byte[16];

            using (var accessor = mmf.CreateViewAccessor()) {
                accessor.ReadArray(0, buf, 0, 16);
                // PacketId
                buf[0] = (byte)(buf[0] + 1);

                buf[2] = (byte)(Command);
                buf[3] = (byte)(Bank);
                buf[4] = (byte)(Address & 0xff);
                buf[5] = (byte)(Address >> 8);
                buf[6] = (byte)(Length & 0xff);
                buf[7] = (byte)(Length >> 8);

                accessor.WriteArray(0, buf, 0, 16);
            }
        }

        public void SetData(byte[] Data, int Length) {
            using (var accessor = mmf.CreateViewAccessor()) {
                accessor.WriteArray(16, Data, 0, Length);
            }
        }

        public byte[] GetData(int Length) {
            byte[] Result = new byte[Length];
            using (var accessor = mmf.CreateViewAccessor()) {
                accessor.ReadArray(16, Result, 0, Length);
            }
            return Result;
        }

        // VMに送信する
        public void SendData(bool MainRam, int Bank, int Address, int Length, byte[] Data) {
            WaitCommand();
            SetData(Data, Length);
            int Command = MainRam ? (int)MemoryCommand.WriteMainRam : (int)MemoryCommand.WriteExtRam;
            SetCommand(Command, Bank, Address, Length);
            WaitCommand();
        }

        // VMより受信する
        public byte[] RecvData(bool MainRam, int Bank, int Address, int Length) {
            WaitCommand();
            int Command = MainRam ? (int)MemoryCommand.ReadMainRam : (int)MemoryCommand.ReadExtRam;
            SetCommand(Command, Bank, Address, Length);
            WaitCommand();
            return GetData(Length);
        }


        public void Dispose() {
            mmf.Dispose();
        }
    }
}
