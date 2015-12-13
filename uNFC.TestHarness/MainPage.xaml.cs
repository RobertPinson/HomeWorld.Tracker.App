using System;
using System.Diagnostics;
using uPLibrary.Hardware.Nfc;
using uPLibrary.Nfc;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace uNFC.TestHarness
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string UartBridgeName1 = "CP2102 USB to UART Bridge Controller";
        private const string UartBridgeName = "Silicon Labs CP210x USB to UART Bridge(COM3)";
        private const string UartBridgeName3 = "USB to UART Bridge";
        private const string Onboard = "MINWINPC";

        private INfcReader nfc;

        public MainPage()
        {
            this.InitializeComponent();

            try
            {
                // var UISyncContext = TaskScheduler.FromCurrentSynchronizationContext();
                // var commLayer = new PN532CommunicationHSU(UartBridgeName);
                CreateNfcReader();
            }
            catch
            {
                SetStatus("Reader not configured");
            }
        }

        private void CreateNfcReader()
        {
            SetStatus("Connecting to RFID Reader through UART Bridge ...");

            nfc?.Close();

            Pn532CommunicationHsu.CreateSerialPort(UartBridgeName3).ContinueWith(t =>
            {
                if (t.IsFaulted || t.Result == null)
                {
                    SetStatus("Reader port configuration failed");

                    return;
                }

                nfc = new NfcPN532Reader(t.Result);
                nfc.TagDetected += nfc_TagDetected;
                nfc.TagLost += nfc_TagLost;

                try
                {
                    var openResult = nfc.Open(NfcTagType.MifareUltralight).Wait(5000);
                    SetStatus(openResult ? "Reader ready" : "Reader open failed");
                }
                catch (Exception)
                {
                    SetStatus("Reader open failed");
                }
            });
        }

        private async void SetStatus(string value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { txtStatus.Text = value; });
        }

        private async void nfc_TagLost(object sender, NfcTagEventArgs e)
        {
            Debug.WriteLine("LOST " + BitConverter.ToString(e.Connection.ID));

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { txtCardId.Text = string.Empty; });
        }

        private async void nfc_TagDetected(object sender, NfcTagEventArgs e)
        {
            var id = BitConverter.ToString(e.Connection.ID);
            Debug.WriteLine("DETECTED {0}", id);

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { txtCardId.Text = id; });


            //byte[] data;

            //switch (e.NfcTagType)
            //{
            //    case NfcTagType.MifareClassic1k:

            //        NfcMifareTagConnection mifareConn = (NfcMifareTagConnection)e.Connection;
            //        await mifareConn.Authenticate(MifareKeyAuth.KeyA, 0x08, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            //        await mifareConn.Read(0x08);

            //        data = new byte[16];
            //        for (byte i = 0; i < data.Length; i++)
            //            data[i] = i;

            //        await mifareConn.Write(0x08, data);

            //        await mifareConn.Read(0x08);
            //        break;

            //    case NfcTagType.MifareUltralight:

            //        NfcMifareUlTagConnection mifareUlConn = (NfcMifareUlTagConnection)e.Connection;

            //        for (byte i = 0; i < 16; i++)
            //        {
            //            byte[] read = await mifareUlConn.Read(i);
            //        }

            //        await mifareUlConn.Read(0x08);

            //        data = new byte[4];
            //        for (byte i = 0; i < data.Length; i++)
            //            data[i] = i;

            //        await mifareUlConn.Write(0x08, data);

            //        await mifareUlConn.Read(0x08);
            //        break;

            //    default:
            //        break;
            //}
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            nfc?.Close();
            Application.Current.Exit();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            CreateNfcReader();
        }
    }
}
