using HomeWorld.Tracker.App.Model;
using HomeWorld.Tracker.App.Service;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
            InitializeComponent();

            _pobCount = 0;
            txtCount.Text = _pobCount.ToString();
            PeopleOnBoard = PobViewSource.GetPobList();
            DataContext = this;
            _personService = new PersonService();
            _dispatcher = Window.Current.Dispatcher;

            //start background task to connect to card Reader.
            var task = new Task(async () => await CreateNfcReader());
            task.Start();
        }

        private async Task CreateNfcReader()
        {
            while (true)
            {
                if (_nfcReader == null || _nfcReader != null && !_nfcReader.IsRunning)
                {
                    SetNfcStatus("Connecting to RFID Reader through UART Bridge ...");

                    if (_nfcReader != null)
                    {
                        await Task.Delay(2000);
                        _nfcReader.Close();
                    }

                    try
                    {
                        var portCreated = await Pn532CommunicationHsu.CreateSerialPort(UartBridgeName);

                        _nfcReader = new NfcPn532Reader(portCreated);
                        _nfcReader.TagDetected += nfc_TagDetected;
                        _nfcReader.TagLost += nfc_TagLost;

                        await ReaderOpen();
                    }
                    catch (Exception)
                    {
                        SetNfcStatus("Reader port configuration failed");
                    }
                }
            }
        }

        private async Task ReaderOpen()
        {
            try
            {
                await _nfcReader.Open(NfcTagType.MifareUltralight);
                SetNfcStatus("Reader ready");
            }
            catch (Exception)
            {
                SetNfcStatus("Reader open failed");
            }
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
