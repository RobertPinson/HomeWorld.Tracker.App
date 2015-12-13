using HomeWorld.Tracker.App.Model;
using HomeWorld.Tracker.App.Service;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using uPLibrary.Nfc;
using uPLibrary.Hardware.Nfc;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HomeWorld.Tracker.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string UartBridgeName = "USB to UART Bridge";
        private readonly IPersonService _personService;
        private INfcReader _nfcReader;

        private int _pobCount;
        public ObservableCollection<PobItem> PeopleOnBoard { get; set; }

        private readonly CoreDispatcher _dispatcher;

        public MainPage()
        {
            this.InitializeComponent();

            //Connect with NFC reader
            CreateNfcREader();

            _pobCount = 0;
            txtCount.Text = _pobCount.ToString();
            PeopleOnBoard = PobViewSource.GetPobList();
            DataContext = this;
            _personService = new PersonService();
            _dispatcher = Window.Current.Dispatcher;
        }

        private void CreateNfcREader()
        {
            SetNfcStatus("Connecting to RFID Reader through UART Bridge ...");

            _nfcReader?.Close();

            Pn532CommunicationHsu.CreateSerialPort(UartBridgeName).ContinueWith(t =>
            {
                if (t.IsFaulted || t.Result == null)
                {
                    SetNfcStatus("Reader port configuration failed");

                    return;
                }

                _nfcReader = new NfcPN532Reader(t.Result);
                _nfcReader.TagDetected += nfc_TagDetected;
                _nfcReader.TagLost += nfc_TagLost;

                try
                {
                    var openResult = _nfcReader.Open(NfcTagType.MifareUltralight).Wait(5000);
                    SetNfcStatus(openResult ? "Reader ready" : "Reader open failed");
                }
                catch (Exception)
                {
                    SetNfcStatus("Reader open failed");
                }
            });
        }

        private async void SetNfcStatus(string value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { TxtStatus.Text = value; });
        }

        private async void nfc_TagLost(object sender, NfcTagEventArgs e)
        {
            Debug.WriteLine("LOST " + BitConverter.ToString(e.Connection.ID));

            //perform actions on card removed from NFC field

        }

        private async void nfc_TagDetected(object sender, NfcTagEventArgs e)
        {
            var id = BitConverter.ToString(e.Connection.ID);
            Debug.WriteLine("DETECTED {0}", id);

            //Call service to get associated person details
            var person = await _personService.GetPerson(id);

            //TODO store locally

            //notify user UI
            await _dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () =>
                {
                    PeopleOnBoard.Add(new PobItem { Id = person.Id.ToString(), Name = $"{person.FirstName} {person.LastName}" });
                    _pobCount++;
                    txtCount.Text = _pobCount.ToString();
                });
        }

        private void btnHello_Click(object sender, RoutedEventArgs e)
        {
            _pobCount++;
            txtCount.Text = _pobCount.ToString();
        }

        void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var SelectedItem = (PobItem)lvPobList.SelectedItem;

        }
    }
}
