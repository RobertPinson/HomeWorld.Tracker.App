using HomeWorld.Tracker.App.Model;
using HomeWorld.Tracker.App.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using uPLibrary.Nfc;
using uPLibrary.Hardware.Nfc;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using HomeWorld.Tracker.App.Core;
using HomeWorld.Tracker.App.DAL;
using HomeWorld.Tracker.App.DAL.Model;
using HomeWorld.Tracker.App.Domain;
using HomeWorld.Tracker.Dto;

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
        private PersonManager _personManager;
        private MovementManager _movementManager;

        private int _pobCount;
        public ObservableCollection<PobItem> PeopleOnBoard { get; set; }

        private readonly CoreDispatcher _dispatcher;

        public MainPage()
        {
            InitializeComponent();

            //TODO get config from 
            _movementManager = new MovementManager(2, 4);

            _personManager = new PersonManager();
            _personManager.PeopleReceivedEventHandler += new EventHandler<List<Person>>(PeopleReceived);
            _pobCount = 0;
            txtCount.Text = _pobCount.ToString();

            PeopleOnBoard = new SortedObservableCollection<PobItem>();
           
            DataContext = this;
            _personService = new PersonService();
            _dispatcher = Window.Current.Dispatcher;

            //start background task to connect to card Reader.
            var task = new Task(async () => await CreateNfcReader());
            task.Start();

            //Mqtt Service Connect
            MqttService.Connect();
            MqttService.MessageReceived += new EventHandler<Movement>(MovementMessage);
        }

        private async void MovementMessage(object sender, Movement e)
        {
            //Get Person for local cache
            var person = DataService.GetPersonByCardId(e.CardId);
            if (person != null)
            {
                person.InLocation = e.InLocation;
                DataService.AddUpdatePerson(person);

                await UpdateUi(person);
            }
            else
            {
                //TODO get from server
            }
        }

        private async void PeopleReceived(object sender, List<Person> e)
        {
            //People received from server and to local cache so need to update UI
            foreach (var person in e)
            {
                await UpdateUi(person);
            }
        }

        private async Task CreateNfcReader()
        {
            while (true)
            {
                if (_nfcReader == null || _nfcReader != null && !_nfcReader.IsRunning)
                {
                    SetNfcStatus("Connecting Reader...");

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
            var uId = BitConverter.ToString(e.Connection.ID);
            Debug.WriteLine("DETECTED {0}", uId);

            //1. Get Person
            var person = DataService.GetPersonByCardId(uId);

            if (person == null)
            {
                //Call service to get associated person details
                var movementResponseDto = await _personService.PostMovement(uId);

                if (movementResponseDto == null)
                {
                    return;
                }

                person = new Person
                {
                    CardUid = movementResponseDto.CardUid,
                    Id = movementResponseDto.Id,
                    Name = movementResponseDto.Name,
                    InLocation = movementResponseDto.Ingress,
                    Image = movementResponseDto.Image
                };
            }

            //2. Update movements
            var movement = new Movement
            {
                CardId = uId,
                InLocation = !person.InLocation,
                SwipeTime = DateTime.UtcNow.ToString("s")
            };
            DataService.AddMovement(movement);

            //3. Update Person record
            person.InLocation = !person.InLocation;
            DataService.AddUpdatePerson(person);

            //4. Update UI
            await UpdateUi(person);
        }

        private async Task UpdateUi(Person person)
        {
            //notify user UI
            await _dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () =>
                {
                    var pobItem = new PobItem
                    {
                        Id = person.Id.ToString(),
                        Name = person.Name,
                        Image = await ToBitmapImage(person.Image)
                    };

                    if (person.InLocation)
                    {
                        //Add or update
                        var item = PeopleOnBoard.FirstOrDefault(i => i.Id == pobItem.Id);

                        if (item == null)
                        {
                            PeopleOnBoard.Add(pobItem);
                            _pobCount++;
                            txtCount.Text = _pobCount.ToString();
                        }
                        else
                        {
                            //item.Name = pobItem.Name;
                            //item.Image = pobItem.Image;

                            PeopleOnBoard.Remove(item);
                            PeopleOnBoard.Add(pobItem);
                        }
                    }
                    else
                    {
                        var item = PeopleOnBoard.FirstOrDefault(i => i.Id == pobItem.Id);

                        if (item != null)
                        {
                            PeopleOnBoard.Remove(item);
                            _pobCount--;
                            txtCount.Text = _pobCount.ToString();
                        }
                    }
                });
        }

        private async Task<BitmapImage> ToBitmapImage(byte[] bytes)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(bytes);
                    await writer.StoreAsync();
                }
                var image = new BitmapImage();
                await image.SetSourceAsync(stream);

                return image;
            }

            //var bmp = new WriteableBitmap(100, 100);
            //using (var stream = bmp.PixelBuffer.AsStream())
            //{
            //    stream.Write(bytes, 0, bytes.Length);
            //}

            //return bmp;
        }

        void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var SelectedItem = (PobItem)LvPobList.SelectedItem;

        }
    }
}
