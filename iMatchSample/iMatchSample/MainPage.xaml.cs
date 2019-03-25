using Gridler.iMatch;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace iMatchSample
{
    public partial class MainPage : ContentPage
    {

        IiMatch iMatch;
        IAdapter adapter;
        IBluetoothLE bluetoothBLE;

        private ObservableCollection<String> messageList = new ObservableCollection<string>();
        public ObservableCollection<String> MessageList
        {
            get { return messageList; }
            set
            {
                if (messageList != value)
                {
                    messageList = value;
                    OnPropertyChanged(nameof(MessageList));
                }
            }
        }

        private bool connected = false;
        public bool Connected
        {
            get { return connected; }
            set
            {
                connected = value;
                OnPropertyChanged(nameof(Connected));
            }
        }

        private double firmwareUpdateProgress = 0;
        public double FirmwareUpdateProgress
        {
            get { return firmwareUpdateProgress; }
            set
            {
                firmwareUpdateProgress = value;
                OnPropertyChanged(nameof(FirmwareUpdateProgress));
            }
        }

        private String firmwareUpdateText;
        public String FirmwareUpdateText
        {
            get { return firmwareUpdateText; }
            set
            {
                firmwareUpdateText = value;
                OnPropertyChanged(nameof(FirmwareUpdateText));
            }
        }

        private bool updatingFirmware = false;
        public bool UpdatingFirmware
        {
            get { return updatingFirmware; }
            set
            {
                updatingFirmware = value;
                OnPropertyChanged(nameof(UpdatingFirmware));
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void ConnectButton_Clicked(object sender, EventArgs e)
        {
            if (iMatch == null || !iMatch.IsConnected)
            {
                bluetoothBLE = CrossBluetoothLE.Current;
                adapter = CrossBluetoothLE.Current.Adapter;

                if (iMatch == null)
                {
                    iMatch = new iMatch(adapter);
                    iMatch.MessageReceived += Imatch_MessageReceived;
                    iMatch.ConnectionStatusChanged += Imatch_ConnectionStatusChanged;
                    iMatch.LogMessage += Imatch_LogMessage;
                    iMatch.FirmwareProgressChanged += IMatch_FirmwareProgressChanged;
                }

                if (!iMatch.IsRefreshing)
                {
                    iMatch.ScanForDevices();
                }
            }
            else
            {
                iMatch.Disconnect();
            }
        }

        private void BatteryButton_Clicked(object sender, EventArgs e)
        {
            iMatch.sendCommand(Devices.Board, Methods.Status, "");
        }

        private void InfoButton_Clicked(object sender, EventArgs e)
        {
            iMatch.sendCommand(Devices.Board, Methods.Info, "");
        }

        private async void UpdateFirmware()
        {
            UpdatingFirmware = true;
            await iMatch.updateFirmware(Device.RuntimePlatform);
            UpdatingFirmware = false;
        }

        private void FWUpdateButton_Clicked(object sender, EventArgs e)
        {
            UpdateFirmware();
        }

        private void FPRButton_Clicked(object sender, EventArgs e)
        {
            iMatch.sendCommand(Devices.FingerPrintReader, Methods.PowerOn, "");
            System.Threading.Thread.Sleep(1000);
            iMatch.sendCommand(Devices.FingerPrintReader, Methods.Send, new byte[] { 0x21, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x3d, 0x06, 0x00, 0x00, 0x3e, 0x02, 0x00, 0x9c, 0x0f });
        }

        private void SCRButton_Clicked(object sender, EventArgs e)
        {
            iMatch.sendCommand(Devices.SmartCardReader, Methods.PowerOn, "readKnownATRs");
        }

        private void NFCButton_Clicked(object sender, EventArgs e)
        {
            iMatch.sendCommand(Devices.RfidReader, Methods.MrtdRead, "D1NLD2509496211136GP2N3MN8SB56");
        }

        private void Imatch_LogMessage(object sender, LogEventArgs e)
        {

        }

        private void Imatch_ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            Connected = iMatch.IsConnected;
            if (Connected)
            {
                MessageList.Add("Connected to " + e.DeviceName);
            }
            else
            {
                MessageList.Add("Disconnected");
            }
        }

        private void Imatch_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            String message = "";
            if (e.Message == null)
            {
                return;
            }

            message = e.Message.Device + " " + e.Message.Method + ": " + e.Message.Data;

            if (e.Message.Device == "fpr")
            {
                if (e.Message.Method == "notify")
                {
                    byte[] fpData = Convert.FromBase64String(e.Message.Data);
                    if (fpData[0] == 0x71) // fingerprint notification
                    {

                        message = e.Message.Device + " " + e.Message.Method + ": " + BitConverter.ToString(fpData).Replace("-", "");
                    }
                    else if (fpData[0] == 0x21) // fingerprint data
                    {
                        message = e.Message.Device + " " + e.Message.Method + ": " + fpData.Length + " bytes";
                    }
                    else
                    {
                        message = e.Message.Device + " " + e.Message.Method + ": " + fpData.Length + " bytes";
                    }
                    iMatch.sendCommand(Devices.FingerPrintReader, Methods.PowerOff, "");
                }
            }
            else if (e.Message.Device == "nfc")
            {
                if (e.Message.Method == "read_sod" || e.Message.Method == "read_dg2")
                {
                    byte[] nfcData = Convert.FromBase64String(e.Message.Data);
                    message = e.Message.Device + " " + e.Message.Method + ": " + nfcData.Length + " bytes";
                }
            }
            else if (e.Message.Device == "sys")
            {
                if (e.Message.Method == "flash")
                {
                    return;
                }
            }

            MessageList.Insert(0, message);
        }

        private void IMatch_FirmwareProgressChanged(object sender, FirmwareUpdateEventArgs e)
        {
            double percentage = (double)e.BytesSent / (double)e.TotalBytes * 100;
            FirmwareUpdateText = e.BytesSent + " B / " + e.TotalBytes + " B (" + Math.Round(percentage, 0) + "%)";
            FirmwareUpdateProgress = Math.Round(percentage / 100, 2);
        }
    }
}
