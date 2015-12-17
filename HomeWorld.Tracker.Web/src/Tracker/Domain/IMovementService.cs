using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeWorld.Tracker.Web.Models;

namespace HomeWorld.Tracker.Web.Domain
{
    public interface IMovementService
    {
        MovementResult Save(string cardUid, int deviceId);
    }

    public class MovementResult
    {
        public Person Person { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class MovementService : IMovementService
    {
        private readonly TrackerDbContext _context;

        public MovementService(TrackerDbContext context)
        {
            _context = context;
        }

        public MovementResult Save(string cardUid, int deviceId)
        {
            var result = new MovementResult();

            try
            {
                //check card is valid
                var card =
                    _context.Card.FirstOrDefault(
                        c => string.Equals(c.Uid, cardUid, StringComparison.OrdinalIgnoreCase));

                if (card == null)
                {
                    result.IsError = true;
                    result.ErrorMessage = $"Invalid Card Id: {cardUid}";
                    return result;
                }

                //record movement
                var location = (from d in _context.Device
                                join l in _context.Location on d.LocationId equals l.Id
                                where d.Id.Equals(deviceId)
                                select l).FirstOrDefault();

                if (location != null)
                {
                    _context.Movement.Add(new Movement
                    {
                        CardId = cardUid,
                        DeviceId = deviceId,
                        LocationId = location.Id,
                        SwipeTime = DateTime.Now
                    });
                    _context.SaveChanges();
                }
                else
                {
                    result.IsError = true;
                    result.ErrorMessage = $"Device location not found deviceId: {deviceId}";
                    return result;
                }

                var person = (from c in _context.Card
                              join pc in _context.PersonCard on c.Id equals pc.CardId
                              join p in _context.Person on pc.PersonId equals p.Id
                              where c.Uid.Equals(cardUid)
                              select p).FirstOrDefault();

                result.Person = person;
            }
            catch (Exception ex)
            {
                //TODO log
            }

            return result;
        }
    }
}
