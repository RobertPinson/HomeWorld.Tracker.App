using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Xaml;
using HomeWorld.Tracker.App.DAL;
using HomeWorld.Tracker.App.DAL.Model;
using HomeWorld.Tracker.App.Service;
using HomeWorld.Tracker.Dto;
using Newtonsoft.Json.Linq;


namespace HomeWorld.Tracker.App.Domain
{
    public class MovementManager
    {
        private readonly string _topic;
        private readonly int _deviceId;
        private readonly DispatcherTimer _timer;

        public MovementManager(int locationId, int deviceId)
        {
            //Configure
            _topic = $"location/{locationId}/movement";
            _deviceId = deviceId;

            //Start timer
            _timer = new DispatcherTimer();
            _timer.Tick += TimerOnTick;
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
            TimerOnTick(this, null);
            _timer.Start();
        }

        private void TimerOnTick(object sender, object e)
        {
            try
            {
                var movements = DataService.GetMovements();

                foreach (var movement in movements)
                {
                    try
                    {
                        dynamic movementDto = new JObject();
                        movementDto.CardId = movement.CardId;
                        movementDto.DeviceId = _deviceId;
                        movementDto.InLocation = movement.InLocation ? 1 : 0;
                        movementDto.SwipeTime = movement.SwipeTime;

                        MqttService.Publish(_topic, movementDto);
                        Debug.WriteLine("[MovementManager] Movement published!!");
                        DataService.DeleteMovement(movement.Id);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[MovementManager] TimerTick ERROR: {0}", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[MovementManager] TimerTick ERROR: {0}", ex.Message);
            }
        }

        // {
        //  "CardId": "fd-a6-4a-95",
        //	"DeviceId": 6,
        //	"InLocation": 1,
        //	"SwipeTime": "2016-05-22T18:18:54Z"
        //}

        //var topic = $"location/{locationId}/movement";     
    }

    public class PersonManager
    {
        private readonly DispatcherTimer _timer;
        private readonly IPersonService _personService;

        public EventHandler<List<Person>> PeopleReceivedEventHandler;

        public PersonManager()
        {
            _personService = new PersonService();

            _timer = new DispatcherTimer();
            _timer.Tick += TimerOnTick;
            _timer.Interval = new TimeSpan(0, 0, 1, 0);
            TimerOnTick(this, null);
            _timer.Start();
        }

        public async Task<IEnumerable<Person>> GetPeopleServerRefresh()
        {
            //Get People from server
            return await GetPeople();
        }

        private async void TimerOnTick(object sender, object o)
        {
            //Get People from server
            var peopleReceived = await GetPeople();

            if (peopleReceived == null)
            {
                return;
            }

            //update local DB
            var persons = peopleReceived as IList<Person> ?? peopleReceived.ToList();
            foreach (var person in persons)
            {
                DataService.AddUpdatePerson(person);
            }

            //Notify people received
            OnPeopleReceived(persons);
        }

        private void OnPeopleReceived(IEnumerable<Person> peopleReceived)
        {
            PeopleReceivedEventHandler?.Invoke(this, peopleReceived.ToList());
        }

        private async Task<IEnumerable<Person>> GetPeople()
        {
            //Get people
            var people = await _personService.GetPeople(string.Empty, 4);

            return people;
        }
    }
}
