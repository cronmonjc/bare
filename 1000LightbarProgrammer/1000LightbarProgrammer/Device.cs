using System;
using System.Runtime.InteropServices;
using MCP2210;

public class Device : IDisposable {
    public const int CURRENT_SETTINGS_ONLY = 0;

    public bool busy = false;

    private ushort xferSize;

    public const uint DEFAULT_VENDOR = 0x04D8, DEFAULT_PRODUCT = 0xF2CF;

    private DevIO dev;

    public Device()
        : this(DEFAULT_VENDOR, DEFAULT_PRODUCT) { }

    public Device(uint vendorID, uint productID) {
        dev = new DevIO(vendorID, productID);
        if(!Connected) throw new DeviceErrorException(-101);
    }

    public int BitRate {
        get {
            int res;
            lock(this) {
                res = dev.Settings.GetSpiBitRate(CURRENT_SETTINGS_ONLY);
            }
            if(res < 0) throw new DeviceErrorException(res); else return res;
        }
        set {
            int res;
            lock(this) {
                res = dev.Settings.SetSpiBitRate(CURRENT_SETTINGS_ONLY, (uint)value);
            }
            if(res != 0) throw new DeviceErrorException(res);
        }
    }

    public ushort ActiveCS {
        get {
            int res;
            lock(this) {
                res = dev.Settings.GetSpiCsActiveValue(CURRENT_SETTINGS_ONLY);
            }
            if(res < 0) throw new DeviceErrorException(res); else return (ushort)res;
        }
        set {
            int res;
            lock(this) {
                res = dev.Settings.SetSpiCsValues(CURRENT_SETTINGS_ONLY, IdleCS, value);
            }
            if(res != 0) throw new DeviceErrorException(res);
        }
    }

    public ushort IdleCS {
        get {
            int res;
            lock(this) {
                res = dev.Settings.GetSpiCsIdleValue(CURRENT_SETTINGS_ONLY);
            }
            if(res < 0) throw new DeviceErrorException(res); else return (ushort)res;
        }
        set {
            int res;
            lock(this) {
                res = dev.Settings.SetSpiCsValues(CURRENT_SETTINGS_ONLY, value, ActiveCS);
            }
            if(res != 0) throw new DeviceErrorException(res);
        }
    }

    public ushort XferSize {
        get { return xferSize; }
        set {
            int res;
            lock(this) {
                res = dev.Settings.SetSpiTxferSize(CURRENT_SETTINGS_ONLY, value);
            }
            if(res != 0) throw new DeviceErrorException(res);

            xferSize = value;
        }
    }

    public uint VendorID {
        get {
            long res;
            lock(this) {
                res = dev.Settings.GetVid();
            }
            if(res < 0) throw new DeviceErrorException((int)res); else return (uint)res;
        }
        set {
            int res;
            lock(this) {
                res = dev.Settings.SetVidPid(value, ProductID);
            }
            if(res != 0) throw new DeviceErrorException(res);
        }
    }

    public uint ProductID {
        get {
            long res;
            lock(this) {
                res = dev.Settings.GetPid();
            }
            if(res < 0) throw new DeviceErrorException((int)res); else return (uint)res;
        }
        set {
            int res;
            lock(this) {
                res = dev.Settings.SetVidPid(VendorID, value);
            }
            if(res != 0) throw new DeviceErrorException(res);
        }
    }

    public string ProdDescriptor {
        get { lock(this) return dev.Settings.GetStringDescriptor(); }
        set {
            int res;
            lock(this) {
                res = dev.Settings.SetStringDescriptor(value);
            }
            if(res != 0) throw new DeviceErrorException(res);
        }
    }

    public string Manufacturer {
        get { lock(this) return dev.Settings.GetStringManufacturer(); }
        set {
            int res;
            lock(this) {
                res = dev.Settings.SetStringManufacturer(value);
            }
            if(res != 0) throw new DeviceErrorException(res);
        }
    }

    public bool Connected {
        get {
            lock(this) { return dev.Settings.GetConnectionStatus(); }
        }
    }

    /// <summary>
    /// Set all SPI settings in one command.
    /// </summary>
    /// <param name="bitrate">Bit rate, in units of bits per second.</param>
    /// <param name="idleCS">The state of the Chip Select pins when the device is not communicating over SPI.</param>
    /// <param name="activeCS">The state of the Chip Select pins when the device is communicating over SPI.  A low bit means comm line is open.</param>
    /// <param name="csToDataDelay">How much time should be between the Chip Select being chosen and the first byte being sent, in units of 100 microseconds.</param>
    /// <param name="dataToDataDelay">How much time should be between two consecutive bytes, in units of 100 microseconds.</param>
    /// <param name="dataToCsDelay">How much time should be between the last byte being sent and the Chip Select being dropped, in units of 100 microseconds.</param>
    /// <param name="xferSize">How many bytes are being sent per transfer?</param>
    /// <param name="spiMode">Which SPI mode are you using?</param>
    /// <param name="powerOnToo">Should this setting also include power-on defaults?</param>
    public void SetAllSpiSettings(uint bitrate, ushort idleCS, ushort activeCS, ushort csToDataDelay, ushort dataToDataDelay, ushort dataToCsDelay, ushort xferSize, byte spiMode, bool powerOnToo = false) {
        int res;
        lock(this) {
            res = dev.Settings.SetAllSpiSettings(powerOnToo ? DllConstants.BOTH : DllConstants.CURRENT_SETTINGS_ONLY, bitrate, idleCS, activeCS, csToDataDelay, dataToDataDelay, dataToCsDelay, xferSize, spiMode);
        }
        if(res != 0) throw new DeviceErrorException(res);
        this.xferSize = xferSize;
    }

    public void SetGpioConfig(byte[] pinDesig, int output, int dir, bool powerOnToo = false) {
        int res;
        lock(this) {
            res = dev.Settings.SetGpioConfig(powerOnToo ? DllConstants.BOTH : DllConstants.CURRENT_SETTINGS_ONLY, pinDesig, output, dir);
        }
        if(res != 0) throw new DeviceErrorException(res);
    }

    public byte[] GetPinDesig() {
        byte[] pinDesig = null;
        int res;
        lock(this) {
            res = dev.Settings.GetGpioPinDesignations(DllConstants.CURRENT_SETTINGS_ONLY, pinDesig);
        }
        if(res != 0 || pinDesig == null) throw new DeviceErrorException(res);
        return pinDesig;
    }

    public void SetPinDesig(byte[] all, bool powerOnToo = false) {
        if(all == null) {
            throw new ArgumentNullException("all");
        }
        if(all.Length != 9) {
            throw new ArgumentException("Parameter \"all\" must be 9 elements long", "all");
        }
        foreach(byte b in all) {
            if(b > 2 || b < 0)
                throw new ArgumentException("Invalid argument for pin designation.", "all");
        }

        SetGpioConfig(all, GetGpioOutput(), GetGpioDir(), powerOnToo);
    }

    public void SetPinDesig(byte which, byte to, bool powerOnToo = false) {
        if(which > 8 || which < 0)
            throw new ArgumentException("Invalid argument for which pin to modify.", "which");
        if(to > 2 || to < 0)
            throw new ArgumentException("Invalid argument for pin designation.", "to");

        byte[] pinDesig = GetPinDesig();

        pinDesig[which] = to;

        SetPinDesig(pinDesig, powerOnToo);
    }

    public int GetGpioOutput() {
        int res;
        lock(this) {
            res = dev.Settings.GetGpioDfltOutput(DllConstants.CURRENT_SETTINGS_ONLY);
        }
        if(res < 0) throw new DeviceErrorException(res);
        return res;
    }

    public void SetGpioOutput(int to) {
        SetGpioConfig(GetPinDesig(), to, GetGpioDir());
    }

    public int GetGpioDir() {
        int res;
        lock(this) {
            res = dev.Settings.GetGpioDfltDirection(DllConstants.CURRENT_SETTINGS_ONLY);
        }
        if(res < 0) throw new DeviceErrorException(res);
        return res;
    }

    public void SetGpioDir(int to) {
        SetGpioConfig(GetPinDesig(), GetGpioOutput(), to);
    }

    /// <summary>
    /// Select which chip the MCP2210 is communicating with.
    /// </summary>
    /// <param name="which">Which chip, from 0 to 8, are we communicating with?</param>
    public void Select(byte which) {
        if(which > 8 || which < 0)
            throw new ArgumentException("Invalid argument for which chip to select.", "which");

        IdleCS = 0xFFFF;
        ActiveCS = (ushort)(~(0x01 << which));
    }

    public byte[] SpiTransfer(byte[] toSend) {
        if(toSend.Length != xferSize)
            throw new ArgumentException("The device is configured to send " + xferSize + " bytes, whereas you're trying to send " + toSend.Length + " bytes.");

        byte[] rxData = new byte[xferSize];

        int res;
        lock(this) {
            res = dev.Functions.TxferSpiData(toSend, rxData);
        }
        if(res != 0) throw new DeviceErrorException(res);

        return rxData;
    }

    public void Abort() {
        int res;
        lock(this) {
            res = dev.Functions.CancelSpiTxfer();
        }
        if(res != 0) throw new DeviceErrorException(res);
    }

    public void Dispose() {
        dev.Dispose();
    }
}

public class DeviceErrorException : Exception {
    public int errCode;

    public DeviceErrorException(int errCode)
        : base("Error interfacing with MCP2210: " + errCode) {
        this.errCode = errCode;
    }
}
