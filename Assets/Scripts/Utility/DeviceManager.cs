using UnityEngine;
using System.Collections;
using fNbt;
using System.IO;
using System;

public class DeviceManager : MonoBehaviour {
    public string lastTX = "", lastRX = "";

    public void Upload() {
        byte[] xferBuffer = new byte[768];

        using(MemoryStream xferBufferStream = new MemoryStream(xferBuffer))
        using(BarWriter writer = new BarWriter(xferBufferStream)) {
            writer.Write(new byte[] { 2, 0 }); // Write command

            NbtCompound patt = BarManager.inst.patts, func;
            short val = 0;
            foreach(string alpha in new string[] { "td", "lall", "rall", "l1", "l2", "l3", "l4", "l5", "dcw", "tdp", "afl", "icl", "ltai", "rtai", "cru", "cal", "emi", "dim" }) {
                func = patt.Get<NbtCompound>(alpha); // Add all the enables to the byte buffer
                foreach(string beta in new string[] { "ef1", "ef2", "er1", "er2" }) {
                    val = func.Get<NbtShort>(beta).Value;

                    writer.Write(val);
                }
            }

            func = patt.Get<NbtCompound>("traf");
            foreach(string beta in new string[] { "er1", "er2" }) {
                val = func.Get<NbtShort>(beta).Value; // Add the traffic director's enables

                writer.Write(val);
            }

            foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "dcw", "tdp", "icl", "afl" }) {
                func = patt.Get<NbtCompound>(alpha); // Add the phases
                foreach(string beta in new string[] { "pf1", "pf2", "pr1", "pr2" }) {
                    val = func.Get<NbtShort>(beta).Value;

                    writer.Write(val);
                }
            }


            foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "dcw", "tdp", "icl", "afl" }) {
                func = patt.Get<NbtCompound>(alpha); // Add the phases
                foreach(string beta in new string[] { "pf1", "pf2", "pr1", "pr2" }) {
                    val = func.Get<NbtShort>(beta).Value;

                    writer.Write(val);
                }
            }

            NbtCompound patternColor;
            foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "afl", "tdp", "icl" }) {
                func = patt.Get<NbtCompound>(alpha); // Add patterns
                foreach(string beta in new string[] { "pat1", "pat2" }) {
                    patternColor = func.Get<NbtCompound>(beta);
                    foreach(string charlie in new string[] { "fcen", "finb", "foub", "ffar", "fcor", "rcen", "rinb", "roub", "rfar", "rcor" }) {
                        val = patternColor.Get<NbtShort>(charlie).Value;

                        writer.Write(val);
                    }
                }
            }

            func = patt.Get<NbtCompound>("traf");
            val = func.Get<NbtShort>("patt").Value;
            for(byte alpha = 0; alpha < 3; alpha++) { // Add traffic director's patterns 3x (left, right, center) (they should be the same anyway, James was lazy and didn't take out the extra two I guess)
                writer.Write(val);
            }
            writer.Write(func.Get<NbtShort>("ctd").Value); // Cycles TD value
            writer.Write(func.Get<NbtShort>("cwn").Value); // Cycles Warn value

            func = patt.Get<NbtCompound>("dim");
            val = func.Get<NbtShort>("dimp").Value; // Dim Percentage, ignored last I checked
            writer.Write(val);

            if(func.Contains("prog")) {
                val = func.Get<NbtShort>("prog").Value; // Preset program number
            } else {
                val = 0; // Not a preset program
            }
            writer.Write(val);

            int[] mapping = FnDragTarget.inputMap.Value; // Then put in the input map.
            for(byte alpha = 0; alpha < 20; alpha++) {
                writer.Write(mapping[alpha]);
            }
        }

        lastTX = PrettyPrintByteArray(xferBuffer);

        SpiXferJob job = new SpiXferJob();
        job.Start(xferBuffer);
        StartCoroutine(WatchXfer(job, delegate(byte[] rxBuffer) {
            lastRX = PrettyPrintByteArray(rxBuffer);

            if(rxBuffer[2] != 2 || rxBuffer[3] != 0) {
                ErrorText.inst.DispError("Transfer complete, but data integrity is not verifiable.  Please try again.");
                return;
            }
            byte upper = 2, lower = 10;
            ushort addr = 4;
            while(addr < 768) {
                if(rxBuffer[addr++] != upper) {
                    ErrorText.inst.DispError("Transfer complete, but data integrity is not verifiable.  Please try again.");
                    return;
                }
                if(rxBuffer[addr++] != lower) {
                    ErrorText.inst.DispError("Transfer complete, but data integrity is not verifiable.  Please try again.");
                    return;
                }
                if(lower == 255) {
                    upper++;
                    lower = 0;
                } else {
                    lower++;
                }
            }
        }));
    }

    public void Download() {
        byte[] xferBuffer = new byte[768];
        using(MemoryStream xferBufferStream = new MemoryStream(xferBuffer))
        using(BarWriter writer = new BarWriter(xferBufferStream)) {
            writer.Write(new byte[] { 0, 10 }); // Write command
            for(ushort i = 11; i < 396; i++) {
                writer.Write(i); // Fill with bytes for PIC reference
            }
        }

        lastTX = PrettyPrintByteArray(xferBuffer);

        SpiXferJob job = new SpiXferJob();
        job.Start(xferBuffer);
        StartCoroutine(WatchXfer(job, delegate(byte[] rxBuffer) {
            lastRX = PrettyPrintByteArray(rxBuffer);
        }));
    }

    public void Wipe() {
        byte[] xferBuffer = new byte[768];
        using(MemoryStream xferBufferStream = new MemoryStream(xferBuffer))
        using(BarWriter writer = new BarWriter(xferBufferStream)) {
            writer.Write(new byte[] { 0x2, 0x0 }); // Write command
            // TODO: fill in the rest of the defaults
        }

        lastTX = PrettyPrintByteArray(xferBuffer);

        SpiXferJob job = new SpiXferJob();
        job.Start(xferBuffer);
        StartCoroutine(WatchXfer(job, delegate(byte[] rxBuffer) {
            lastRX = PrettyPrintByteArray(rxBuffer);

            if(rxBuffer[2] != 2 || rxBuffer[3] != 0) {
                ErrorText.inst.DispError("Transfer complete, but data integrity is not verifiable.  Please try again.");
                return;
            }
            byte upper = 2, lower = 10;
            ushort addr = 4;
            while(addr < 768) {
                if(rxBuffer[addr++] != upper) {
                    ErrorText.inst.DispError("Transfer complete, but data integrity is not verifiable.  Please try again.");
                    return;
                }
                if(rxBuffer[addr++] != lower) {
                    ErrorText.inst.DispError("Transfer complete, but data integrity is not verifiable.  Please try again.");
                    return;
                }
                if(lower == 255) {
                    upper++;
                    lower = 0;
                } else {
                    lower++;
                }
            }
        }));
    }

    public static string PrettyPrintByteArray(byte[] array) {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(2048);
        sb.Append("{");
        for(ushort i = 0; i < array.Length; i++) {
            if(i != 0) sb.Append(", ");
            sb.Append(array[i]);
        }
        sb.Append("}");
        return sb.ToString();
    }

    public delegate void AfterXfer(byte[] rxBuffer);

    public IEnumerator WatchXfer(SpiXferJob job, AfterXfer Afterward) {
        while(!job.Update()) {
            yield return null;
        }
        yield return null;
        if(job.thrownExcep != null)
            Afterward(job.recieveBuffer);
    }

    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "ChipBase.png", true);
    }

    public class BarWriter : IDisposable {
        protected Stream outStream;

        public BarWriter(Stream output) {
            outStream = output;
        }

        public void Write(byte[] bytes) {
            outStream.Write(bytes, 0, bytes.Length);
        }

        public void Write(byte b) {
            outStream.WriteByte(b);
        }

        public void Write(ushort s) {
            outStream.WriteByte((byte)((s >> 8) & 0xFF));
            outStream.WriteByte((byte)(s & 0xFF));
        }

        public void Write(short s) {
            outStream.WriteByte((byte)((s >> 8) & 0xFF));
            outStream.WriteByte((byte)(s & 0xFF));
        }

        public void Write(int i) {
            outStream.WriteByte((byte)((i >> 24) & 0xFF));
            outStream.WriteByte((byte)((i >> 16) & 0xFF));
            outStream.WriteByte((byte)((i >> 8) & 0xFF));
            outStream.WriteByte((byte)(i & 0xFF));
        }

        public void Dispose() {
            outStream.Dispose();
        }
    }

    public class BarReader : IDisposable {
        protected Stream inStream;

        public BarReader(Stream input) {
            inStream = input;
        }

        public int ReadInt() {
            int rtn = 0;
            rtn |= (byte)(ReadByte() << 24);
            rtn |= (byte)(ReadByte() << 16);
            rtn |= (byte)(ReadByte() << 8);
            rtn |= (byte)ReadByte();
            return rtn;
        }

        public short ReadShort() {
            short rtn = 0;
            rtn |= (short)(ReadByte() << 8);
            rtn |= (short)ReadByte();
            return rtn;
        }

        public byte ReadByte() {
            int val = inStream.ReadByte();
            if(val == -1) throw new EndOfStreamException();
            return (byte)val;
        }
    }
}

public class SpiXferJob : ThreadedJob {
    public byte[] sendBuffer, recieveBuffer;
    public System.Exception thrownExcep;

    Device d;

    public void Start(byte[] bytesToSend) {
        ErrorText.inst.DispInfo("Beginning transfer with bar...");

        d = new Device();

        d.SetAllSpiSettings(2000, 0xFFFF, 0xFFFE, 1, 1, 1, (ushort)sendBuffer.Length, 0);

        sendBuffer = bytesToSend;

        base.Start();
        m_Thread.Name = "SPI Transfer Thread";
    }

    public override void Abort() {
        d.Abort();

        base.Abort();
    }

    protected override void ThreadFunction() {
        try {
            recieveBuffer = d.SpiTransfer(sendBuffer);
        } catch(System.Exception ex) {
            thrownExcep = ex;
        }
    }

    protected override void OnFinished() {
        if(thrownExcep != null) {
            try {
                throw thrownExcep;
            } catch(DeviceErrorException ex) {
                switch(ex.errCode) {
                    case -2:
                    case -8:
                    case -10:
                        ErrorText.inst.DispError("Cannot communicate with bar.  Wait a few seconds and try again. (" + ex.errCode + ")");
                        break;
                    case -9:
                        ErrorText.inst.DispError("Cannot communicate with bar.  The communication channel might be damaged. (" + ex.errCode + ")");
                        break;
                    case -101:
                        ErrorText.inst.DispError("Cannot communicate with bar.  Is it connected? (" + ex.errCode + ")");
                        break;
                    default:
                        ErrorText.inst.DispError("Unknown error communicating with bar. (" + ex.errCode + ")");
                        break;
                }
            }
        } else {
            d.Dispose();

            ErrorText.inst.DispInfo("Transfer with bar complete.");
        }
    }
}