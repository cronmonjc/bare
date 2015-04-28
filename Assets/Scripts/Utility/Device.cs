using System;
using System.Runtime.InteropServices;

public class Device : IDisposable {
#if !UNITY_EDITOR
    [DllImport("mcp2210")]
    private static extern int DllInit(uint vendorID, uint productID);
    [DllImport("mcp2210")]
    private static extern int SetGpioConfig(int whichToSet, byte[] pinDesignations, int defaultOutput, int pinDir);

    public const int CURRENT_SETTINGS_ONLY = 0;

    public bool busy = false;

    private ushort xferSize;

    public const uint DEFAULT_VENDOR = 0x04D8, DEFAULT_PRODUCT = 0x00DE;

    public Device()
        : this(DEFAULT_VENDOR, DEFAULT_PRODUCT) { }

    public Device(uint vendorID, uint productID) {
        DllInit(vendorID, productID);
        int res = SetGpioConfig(CURRENT_SETTINGS_ONLY, new byte[] { 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1, 0x1 }, 0xFFFF, 0xFFFF);
        if(res != 0) throw new DeviceErrorException(res);
    }

    [DllImport("mcp2210")]
    private static extern int GetSpiBitRate(int whichToGet);
    [DllImport("mcp2210")]
    private static extern int SetSpiBitRate(int whichToSet, uint bitRate);
    public uint BitRate {
        get {
            int res;
            lock(this) { res = GetSpiBitRate(CURRENT_SETTINGS_ONLY); }
            if(res < 0) throw new DeviceErrorException(res); else return (uint)res;
        }
        set {
            int res;
            lock(this) { res = SetSpiBitRate(CURRENT_SETTINGS_ONLY, value); }
            if(res < 0) throw new DeviceErrorException(res);
        }
    }

    [DllImport("mcp2210")]
    private static extern int GetSpiActiveCsValue(int whichToGet);
    [DllImport("mcp2210")]
    private static extern int SetSpiCsValues(int whichToSet, ushort idleCs, ushort activeCs);
    public ushort ActiveCS {
        get {
            int res;
            lock(this) { res = GetSpiActiveCsValue(CURRENT_SETTINGS_ONLY); }
            if(res < 0) throw new DeviceErrorException(res);
            return (ushort)res;
        }
        set {
            int res;
            lock(this) { res = SetSpiCsValues(CURRENT_SETTINGS_ONLY, IdleCS, value); }
            if(res < 0) throw new DeviceErrorException(res);
        }
    }

    [DllImport("mcp2210")]
    private static extern int GetSpiIdleCsValue(int whichToGet);
    public ushort IdleCS {
        get {
            int res;
            lock(this) { res = GetSpiIdleCsValue(CURRENT_SETTINGS_ONLY); }
            if(res < 0) throw new DeviceErrorException(res); else return (ushort)res;
        }
        set {
            int res;
            lock(this) { res = SetSpiCsValues(CURRENT_SETTINGS_ONLY, value, ActiveCS); }
            if(res < 0) throw new DeviceErrorException(res);
        }
    }

    [DllImport("mcp2210")]
    private static extern int SetSpiTxferSize(int whichToSet, ushort size);
    public ushort XferSize {
        get { return xferSize; }
        set {
            int res;
            lock(this) { res = SetSpiTxferSize(CURRENT_SETTINGS_ONLY, value); }
            if(res < 0) throw new DeviceErrorException(res);

            xferSize = value;
        }
    }

    [DllImport("mcp2210")]
    private static extern bool GetConnectionStatus();
    public bool Connected {
        get {
            lock(this) { return GetConnectionStatus(); }
        }
    }

    [DllImport("mcp2210")]
    private static extern int SetAllSpiSettings(int whichToSet, uint bitrate, ushort idleCS, ushort activeCS, ushort csToDataDelay, ushort dataToDataDelay, ushort dataToCsDelay, ushort xferSize, byte spiMode);

    /// <summary>
    /// Set all SPI settings in one command
    /// </summary>
    /// <param name="bitrate">Bit rate, in units of bits per second.</param>
    /// <param name="idleCS">The state of the Chip Select pins when the device is not communicating over SPI.</param>
    /// <param name="activeCS">The state of the Chip Select pins when the device is communicating over SPI.</param>
    /// <param name="csToDataDelay">How much time should be between the Chip Select being chosen and the first byte being sent, in units of 100 microseconds.</param>
    /// <param name="dataToDataDelay">How much time should be between two consecutive bytes, in units of 100 microseconds.</param>
    /// <param name="dataToCsDelay">How much time should be between the last byte being sent and the Chip Select being dropped, in units of 100 microseconds.</param>
    /// <param name="xferSize">How many bytes are being sent per transfer?</param>
    /// <param name="spiMode">Which SPI mode are you using?</param>
    public void SetAllSpiSettings(uint bitrate, ushort idleCS, ushort activeCS, ushort csToDataDelay, ushort dataToDataDelay, ushort dataToCsDelay, ushort xferSize, byte spiMode) {
        int res;
        lock(this) {
            res = SetAllSpiSettings(CURRENT_SETTINGS_ONLY, bitrate, idleCS, activeCS, csToDataDelay, dataToDataDelay, dataToCsDelay, xferSize, spiMode);
        }
        if(res < 0) throw new DeviceErrorException(res);
        this.xferSize = xferSize;
    }

    /// <summary>
    /// Select which chip the MCP2210 is communicating with.
    /// </summary>
    /// <param name="which">Which chip, from 0 to 8, are we communicating with?</param>
    public void Select(int which) {
        if(which > 8 || which < 0)
            throw new ArgumentException("Invalid argument for which chip to select.");

        IdleCS = 0xFFFF;
        ActiveCS = (ushort)(~(0x01 << which));
    }

    [DllImport("mcp2210")]
    private static extern int TxferSpiData(byte[] txData, byte[] rxData);
    public byte[] SpiTransfer(byte[] toSend) {
        if(toSend.Length != xferSize)
            throw new ArgumentException("The device is configured to send " + xferSize + " bytes, whereas you're trying to send " + toSend.Length + " bytes.");

        byte[] rxData = new byte[xferSize];

        int res;
        lock(this) {
            res = TxferSpiData(toSend, rxData);
        }
        if(res < 0) throw new DeviceErrorException(res);

        return rxData;
    }

    [DllImport("mcp2210")]
    private static extern int WriteEEProm(byte addr, byte value);
    public void UploadToLocalEeprom(byte[] bytes) {
        if(bytes.Length > 256) throw new ArgumentException("Cannot send more than 256 bytes to the EEPROM on the MCP2210 - it only has 256 bytes of space!");

        int res = 0;
        busy = true;
        for(byte i = 0; i < 255; i++) {
            lock(this) {
                res = WriteEEProm(i, (bytes.Length > i ? bytes[i] : (byte)0x0));
                if(res < 0) throw new DeviceErrorException(res);
            }
        }
        busy = false;

    }

    [DllImport("mcp2210")]
    private static extern int ReadEEProm(byte addr);
    public byte[] DownloadFromLocalEeprom() {
        byte[] rtn = new byte[256];

        int res = 0;
        busy = true;
        for(byte i = 0; i <= 255; i++) {
            lock(this) {
                res = ReadEEProm(i);
                if(res < 0) throw new DeviceErrorException(res);
                rtn[i] = (byte)res;
            }
        }
        busy = false;

        return rtn;
    }
    
    [DllImport("mcp2210")]
    private static extern void DllCleanUp();
    public void Dispose() {
        DllCleanUp();
    }
#else
    public void Dispose() { }
#endif
}

public class DeviceErrorException : Exception {
    public int errCode;

    public DeviceErrorException(int errCode)
        : base("Error interfacing with MCP2210: " + errCode) {
        this.errCode = errCode;
    }
}
