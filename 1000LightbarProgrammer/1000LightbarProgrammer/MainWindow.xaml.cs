using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using fNbt;

using Path = System.IO.Path;

namespace LightbarProg {
    public partial class MainWindow : Window {
        private Device d;

        public MainWindow() {
            InitializeComponent();


            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += delegate(object sender, EventArgs e) {
                Device dev = TryGetDevice();

                if(dev != null && dev.Connected) {
                    connLbl.Content = "Connected";
                    connImg.Source = ((Image)this.Resources["conn"]).Source;
                } else {
                    connLbl.Content = "Disconnected";
                    connImg.Source = ((Image)this.Resources["disconn"]).Source;
                }
            };
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void ReadBrowse_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog() { DefaultExt = ".bar.nbt", Filter = "Bar Files|*.bar.nbt", OverwritePrompt = true, DereferenceLinks = true, Title = "Browsing for Output File" };
            bool? result = dlg.ShowDialog(this);
            if(result == false) return;
            ReadBox.Text = dlg.FileName;
        }

        private void WriteBrowse_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".bar.nbt", Filter = "Bar Files|*.bar.nbt", Multiselect = false, CheckFileExists = true, DereferenceLinks = true, Title = "Browsing for Input File" };
            bool? result = dlg.ShowDialog(this);
            if(result == false) return;
            WriteBox.Text = dlg.FileName;
        }

        private void FileDragEnter(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach(string file in files) {
                if(file.EndsWith(".bar.nbt")) {
                    e.Effects = DragDropEffects.Link;
                    e.Handled = true;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void FileDragDrop(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            TextBox tb = sender as TextBox;
            if(tb != null && files != null && files.Length != 0) {
                foreach(string file in files) {
                    if(file.EndsWith(".bar.nbt")) {
                        tb.Text = file;
                        return;
                    }
                }
            }
        }

        private void ReadBar(object sender, MouseButtonEventArgs e) {
            Device dev = TryGetDevice();

            if(dev == null || !dev.Connected) {
                MessageBox.Show(this, "No bar was found.  Are you certain that one is connected?", "No Bar Connected", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            }
            if(String.IsNullOrEmpty(ReadBox.Text)) {
                MessageBox.Show(this, "Please specify a destination to put the output file.", "No Destination Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            }
            try {
                string path = Path.GetFullPath(ReadBox.Text);

                NbtFile file = null;

                if(File.Exists(path)) {
                    try {
                        file = new NbtFile(path);
                    } catch(NbtFormatException) {
                        if(MessageBox.Show(this, "Destination file appears corrupt.  Is it alright to overwrite the whole file?", "Destination Corrupt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                            return;
                        file = null;
                    } catch(EndOfStreamException) {
                        if(MessageBox.Show(this, "Destination file appears corrupt.  Is it alright to overwrite the whole file?", "Destination Corrupt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                            return;
                        file = null;
                    } catch(InvalidCastException) {
                        if(MessageBox.Show(this, "Destination file appears corrupt.  Is it alright to overwrite the whole file?", "Destination Corrupt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                            return;
                        file = null;
                    } catch(NullReferenceException) {
                        if(MessageBox.Show(this, "Destination file appears corrupt.  Is it alright to overwrite the whole file?", "Destination Corrupt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                            return;
                        file = null;
                    }
                }

                if(file == null) {
                    file = new NbtFile();
                    file.RootTag.Name = "root";

                    file.RootTag.Add(new NbtCompound("opts", new NbtTag[] { new NbtByte("size", 3), new NbtByte("tdop", 0), new NbtByte("can", 0), new NbtByte("cabt", 0), new NbtByte("cabl", 0) }));
                    file.RootTag.Add(new NbtCompound("ordr", new NbtTag[] { new NbtString("name", ""), new NbtString("num", ""), new NbtString("note", "Program imported by Lightbar Programmer") }));
                    file.RootTag.Add(new NbtCompound("pats"));
                    file.RootTag.Add(new NbtList("lite", NbtTagType.Compound));
                    file.RootTag.Add(new NbtList("soc", NbtTagType.Compound));
                    file.RootTag.Add(new NbtList("lens", NbtTagType.Compound));
                }

                NbtCompound patts = file.RootTag.Get<NbtCompound>("pats");
                patts.Clear();

                foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim", "traf" })
                    patts.Add(new NbtCompound(alpha));

                patts.Get<NbtCompound>("traf").AddRange(new NbtShort[] { new NbtShort("er1", 0), new NbtShort("er2", 0), new NbtShort("ctd", 0), new NbtShort("cwn", 0) });

                foreach(string alpha in new string[] { "td", "lall", "rall", "ltai", "rtai", "cru", "cal", "emi", "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw", "dim" })
                    patts.Get<NbtCompound>(alpha).AddRange(new NbtShort[] { new NbtShort("ef1", 0), new NbtShort("ef2", 0), new NbtShort("er1", 0), new NbtShort("er2", 0) });

                patts.Get<NbtCompound>("dim").Add(new NbtShort("dimp", 15));

                foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl", "dcw" })
                    patts.Get<NbtCompound>(alpha).AddRange(new NbtShort[] { new NbtShort("pf1", 0), new NbtShort("pf2", 0), new NbtShort("pr1", 0), new NbtShort("pr2", 0) });

                patts.Get<NbtCompound>("traf").AddRange(new NbtShort[] {new NbtShort("patt", 0), new NbtShort("ctd", 0), new NbtShort("cwn", 0)});

                foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "tdp", "icl", "afl" }) {
                    patts.Get<NbtCompound>(alpha).Add(new NbtCompound("pat1", new NbtTag[] { new NbtShort("fcen", 0), new NbtShort("finb", 0), new NbtShort("foub", 0), new NbtShort("ffar", 0), new NbtShort("fcor", 0),
                                                                                             new NbtShort("rcen", 0), new NbtShort("rinb", 0), new NbtShort("roub", 0), new NbtShort("rfar", 0), new NbtShort("rcor", 0) }));
                    patts.Get<NbtCompound>(alpha).Add(new NbtCompound("pat2", new NbtTag[] { new NbtShort("fcen", 0), new NbtShort("finb", 0), new NbtShort("foub", 0), new NbtShort("ffar", 0), new NbtShort("fcor", 0),
                                                                                             new NbtShort("rcen", 0), new NbtShort("rinb", 0), new NbtShort("roub", 0), new NbtShort("rfar", 0), new NbtShort("rcor", 0) }));
                }

                patts.Add(new NbtIntArray("map", new int[20]));


                byte[] xferBuffer = new byte[768];
                using(MemoryStream xferBufferStream = new MemoryStream(xferBuffer))
                using(BarWriter writer = new BarWriter(xferBufferStream)) {
                    writer.Write(new byte[] { 0, 10 }); // Write command
                    for(ushort i = 11; i < 394; i++) {
                        writer.Write(i); // Fill with bytes for PIC reference
                    }
                }

                dev.XferSize = 768;
                byte[] rxBuffer = dev.SpiTransfer(xferBuffer);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Read Op @ {0:G}\n\nSent: [{1} bytes]\n", DateTime.Now, xferBuffer.Length);
                for(short i = 0; i < xferBuffer.Length; i++) {
                    sb.Append(xferBuffer[i]);
                    if(i % 2 == 1) sb.Append("\n");
                    else sb.Append(" ");
                }
                sb.AppendFormat("\n\nRecieved: [{0} bytes]\n", rxBuffer.Length);
                for(short i = 0; i < rxBuffer.Length; i++) {
                    sb.Append(rxBuffer[i]);
                    if(i % 2 == 1) sb.Append("\n");
                    else sb.Append(" ");
                }
                sb.Append("\n<End of transfer>\n\n");

                File.AppendAllText("log.txt", sb.ToString());

                using(MemoryStream rxBufferStream = new MemoryStream(rxBuffer))
                using(BarReader reader = new BarReader(rxBufferStream)) {
                    NbtCompound patt = patts, func;
                    short val = 0;

                    reader.ReadShort();

                    foreach(string alpha in new string[] { "td", "lall", "rall", "l1", "l2", "l3", "l4", "l5", "dcw", "tdp", "afl", "icl", "ltai", "rtai", "cru", "cal", "emi", "dim" }) {
                        func = patt.Get<NbtCompound>(alpha); // Add all the enables to the byte buffer
                        foreach(string beta in new string[] { "ef1", "ef2", "er1", "er2" }) {
                            func.Get<NbtShort>(beta).Value = reader.ReadShort();
                        }
                    }

                    func = patt.Get<NbtCompound>("traf");
                    foreach(string beta in new string[] { "er1", "er2" }) {
                        func.Get<NbtShort>(beta).Value = reader.ReadShort(); // Add the traffic director's enables
                    }

                    foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "dcw", "tdp", "icl", "afl" }) {
                        func = patt.Get<NbtCompound>(alpha); // Add the phases
                        foreach(string beta in new string[] { "pf1", "pf2", "pr1", "pr2" }) {
                            func.Get<NbtShort>(beta).Value = reader.ReadShort();
                        }
                    }

                    NbtCompound patternColor;
                    foreach(string alpha in new string[] { "l1", "l2", "l3", "l4", "l5", "afl", "tdp", "icl" }) {
                        func = patt.Get<NbtCompound>(alpha); // Add patterns
                        foreach(string beta in new string[] { "pat1", "pat2" }) {
                            patternColor = func.Get<NbtCompound>(beta);
                            foreach(string charlie in new string[] { "fcen", "finb", "foub", "ffar", "fcor", "rcen", "rinb", "roub", "rfar", "rcor" }) {
                                patternColor.Get<NbtShort>(charlie).Value = reader.ReadShort();
                            }
                        }
                    }

                    func = patt.Get<NbtCompound>("traf");
                    func.Get<NbtShort>("patt").Value = reader.ReadShort();
                    for(byte alpha = 0; alpha < 2; alpha++) { // Traffic director's patterns 3x (left, right, center) (they should be the same anyway, James was lazy and didn't take out the extra two I guess)
                        reader.ReadShort();  // Eat two Shorts to discard extra Traffic Director patterns.
                    }
                    func.Get<NbtShort>("ctd").Value = reader.ReadShort(); // Cycles TD value
                    func.Get<NbtShort>("cwn").Value = reader.ReadShort(); // Cycles Warn value

                    func = patt.Get<NbtCompound>("dim");
                    func.Get<NbtShort>("dimp").Value = reader.ReadShort(); // Dim Percentage, ignored last I checked

                    val = reader.ReadShort();
                    if(val != 0) {
                        patt.Add(new NbtShort("prog", val)); // Preset program number
                    }

                    int[] mapping = patt.Get<NbtIntArray>("map").Value; // Then put in the input map.
                    for(byte alpha = 0; alpha < 20; alpha++) {
                        mapping[alpha] = reader.ReadInt();
                    }
                }

                file.SaveToFile(path, NbtCompression.None);
                MessageBox.Show(this, "Read operation completed.", "Done", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
                return;
            } catch(ArgumentException) {
                MessageBox.Show(this, "Please don't use any invalid characters in the path.", "Invalid Destination Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(NotSupportedException) {
                MessageBox.Show(this, "Please don't use any invalid characters in the path.", "Invalid Destination Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(PathTooLongException) {
                MessageBox.Show(this, "The specified output path is too long for Windows to use.", "Invalid Destination Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(System.Security.SecurityException) {
                MessageBox.Show(this, "You do not have permission to write to the specified output path.", "Invalid Destination Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(DeviceErrorException ex) {
                switch(ex.errCode) {
                    case -2:
                    case -8:
                    case -10:
                        MessageBox.Show(this, "Cannot communicate with bar.  Wait a few seconds and try again. (" + ex.errCode + ")", "Bar Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                        break;
                    case -9:
                        MessageBox.Show(this, "Cannot communicate with bar.  The communication channel might be damaged. (" + ex.errCode + ")", "Bar Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                        break;
                    case -101:
                        MessageBox.Show(this, "Cannot communicate with bar.  Is it connected? (" + ex.errCode + ")", "Bar Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                        break;
                    default:
                        MessageBox.Show(this, "Unknown error communicating with bar.  Wait a few seconds and try again. (" + ex.errCode + ")", "Bar Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                        break;
                }
            }
        }

        private void WriteBar(object sender, MouseButtonEventArgs e) {
            Device dev = TryGetDevice();

            if(dev == null || !dev.Connected) {
                MessageBox.Show(this, "No bar was found.  Are you certain that one is connected?", "No Bar Connected", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            }
            if(String.IsNullOrEmpty(WriteBox.Text)) {
                MessageBox.Show(this, "Please specify a source file.", "No Source Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            }
            try {
                string path = Path.GetFullPath(WriteBox.Text);

                NbtFile file = null;

                if(!File.Exists(path)) {
                    MessageBox.Show(this, "The file name you indicated does not exist.", "Invalid Source Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                    return;
                }

                file = new NbtFile(path);
                
                byte[] xferBuffer = new byte[768];

                using(MemoryStream xferBufferStream = new MemoryStream(xferBuffer))
                using(BarWriter writer = new BarWriter(xferBufferStream)) {
                    writer.Write(new byte[] { 2, 0 }); // Write command

                    NbtCompound patt = file.RootTag.Get<NbtCompound>("pats"), func;
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
                    if(func.Contains("ctd"))
                        writer.Write(func.Get<NbtShort>("ctd").Value); // Cycles TD value
                    else
                        writer.Write((short)0);
                    if(func.Contains("cwn"))
                        writer.Write(func.Get<NbtShort>("cwn").Value); // Cycles Warn value
                    else
                        writer.Write((short)0);

                    func = patt.Get<NbtCompound>("dim");
                    val = func.Get<NbtShort>("dimp").Value; // Dim Percentage, ignored last I checked
                    writer.Write(val);

                    if(patt.Contains("prog")) {
                        val = patt.Get<NbtByte>("prog").ShortValue; // Preset program number
                    } else {
                        val = 0; // Not a preset program
                    }
                    writer.Write(val);

                    int[] mapping = patt.Get<NbtIntArray>("map").Value; // Then put in the input map.
                    for(byte alpha = 0; alpha < 20; alpha++) {
                        writer.Write(mapping[alpha]);
                    }
                }

                dev.XferSize = 768;
                byte[] rxBuffer = dev.SpiTransfer(xferBuffer);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Write Op @ {0:G}\n\nSent: [{1} bytes]\n", DateTime.Now, xferBuffer.Length);
                for(short i = 0; i < xferBuffer.Length; i++) {
                    sb.Append(xferBuffer[i]);
                    if(i % 2 == 1) sb.Append("\n");
                    else sb.Append(" ");
                }
                sb.AppendFormat("\n\nRecieved: [{0} bytes]\n", rxBuffer.Length);
                for(short i = 0; i < rxBuffer.Length; i++) {
                    sb.Append(rxBuffer[i]);
                    if(i % 2 == 1) sb.Append("\n");
                    else sb.Append(" ");
                }
                sb.Append("\n<End of transfer>\n\n");

                File.AppendAllText("log.txt", sb.ToString());

                if(rxBuffer[2] != 2 || rxBuffer[3] != 0) {
                    MessageBox.Show(this, "Write operation complete, but data integrity is not verifiable.  Another attempt is recommended.", "Complete (With Complications)", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    return;
                }
                byte upper = 2, lower = 1;
                ushort addr = 4;
                while(addr < 768) {
                    if(rxBuffer[addr++] != upper) {
                        MessageBox.Show(this, "Write operation complete, but data integrity is not verifiable.  Another attempt is recommended.", "Complete (With Complications)", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        return;
                    }
                    if(rxBuffer[addr++] != lower) {
                        MessageBox.Show(this, "Write operation complete, but data integrity is not verifiable.  Another attempt is recommended.", "Complete (With Complications)", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        return;
                    }
                    if(lower == 255) {
                        upper++;
                        lower = 0;
                    } else {
                        lower++;
                    }
                }

                MessageBox.Show(this, "Write operation complete.", "Complete", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
                return;
            } catch(ArgumentException) {
                MessageBox.Show(this, "Please don't use any invalid characters in the path.", "Invalid Source Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(NotSupportedException) {
                MessageBox.Show(this, "Please don't use any invalid characters in the path.", "Invalid Source Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(PathTooLongException) {
                MessageBox.Show(this, "The specified source path is too long for Windows to use.", "Invalid Source Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(System.Security.SecurityException) {
                MessageBox.Show(this, "You do not have permission to read from the specified source path.", "Invalid Source Designated", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(NbtFormatException) {
                MessageBox.Show(this, "Source file appears corrupt.", "Source Corrupt", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(EndOfStreamException) {
                MessageBox.Show(this, "Source file appears corrupt.", "Source Corrupt", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(InvalidCastException) {
                MessageBox.Show(this, "Source file appears corrupt.", "Source Corrupt", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(NullReferenceException) {
                MessageBox.Show(this, "Source file appears corrupt.", "Source Corrupt", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                return;
            } catch(DeviceErrorException ex) {
                switch(ex.errCode) {
                    case -2:
                    case -8:
                    case -10:
                        MessageBox.Show(this, "Cannot communicate with bar.  Wait a few seconds and try again. (" + ex.errCode + ")", "Bar Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                        break;
                    case -9:
                        MessageBox.Show(this, "Cannot communicate with bar.  The communication channel might be damaged. (" + ex.errCode + ")", "Bar Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                        break;
                    case -101:
                        MessageBox.Show(this, "Cannot communicate with bar.  Is it connected? (" + ex.errCode + ")", "Bar Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                        break;
                    default:
                        MessageBox.Show(this, "Unknown error communicating with bar.  Wait a few seconds and try again. (" + ex.errCode + ")", "Bar Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                        break;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Device dev = TryGetDevice();
            if(dev != null) dev.Dispose();
        }

        private Device TryGetDevice() {
            if(d != null) {
                if(d.Connected)
                    return d;
                else {
                    d.Dispose();
                    d = null;
                }
            }

            try {
                d = new Device();
            } catch(DeviceErrorException ex) {
                if(ex.errCode == -101) {
                    if(d != null) d.Dispose();
                    try {
                        d = new Device(0x04D8, 0x00DE);
                        d.ProductID = 0xF2CF;
                        d.ProdDescriptor = "Light Bar Breakout Box";
                        d.Manufacturer = "Star Headlight & Lantern Co.";
                        d.SetAllSpiSettings(100000, 0xFFFF, 0xFFFE, 100, 1, 1, 768, 1, true);
                        d.SetGpioConfig(new byte[] { 0x1, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0, 0x0 }, 0, 0, true);
                    } catch(DeviceErrorException ex2) {
                        if(ex2.errCode == -101) {
                            if(d != null) d.Dispose();
                            d = null;
                        }
                    }
                }
            }
            return d;
        }
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

        public void Dispose() {
            inStream.Dispose();
        }
    }
}
