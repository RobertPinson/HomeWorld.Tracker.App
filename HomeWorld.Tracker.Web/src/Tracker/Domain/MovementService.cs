using System;
using System.Linq;
using HomeWorld.Tracker.Web.Domain.Model;
using HomeWorld.Tracker.Web.Dtos;
using HomeWorld.Tracker.Web.Models;
using Microsoft.AspNet.Mvc;

namespace HomeWorld.Tracker.Web.Domain
{
    public class MovementService : IMovementService
    {
        private readonly TrackerDbContext _context;

        public MovementService(TrackerDbContext context)
        {
            _context = context;
        }
        
        public MovementResult Save(MovementDto movement)
        {
            var result = new MovementResult();

            try
            {
                //check card is valid
                var card =
                    _context.Card.FirstOrDefault(
                        c => string.Equals(c.Uid, movement.Uid, StringComparison.OrdinalIgnoreCase));

                if (card == null)
                {
                    result.IsError = true;
                    result.ErrorMessage = $"Invalid Card Id: {movement.Uid}";
                    return result;
                }

                //record movement
                var location = (from d in _context.Device
                                join l in _context.Location on d.LocationId equals l.Id
                                where d.Id.Equals(movement.DeviceId)
                                select l).FirstOrDefault();

                if (location != null)
                {
                    //Check ingress or egress
                    var latestMovement =
                        _context.Movement
                            .OrderByDescending(m => m.SwipeTime)
                            .FirstOrDefault(m => m.CardId == movement.Uid);

                    if (latestMovement == null)
                    {
                        //ingress
                        //first recorded movement
                        result.Ingress = true;
                    }
                    else
                    {
                        //If latest movement this location egress  
                        result.Ingress = latestMovement.LocationId != location.Id;
                    }

                    //Location 1 is off site
                    var locationId = result.Ingress ? location.Id : 1;

                    _context.Movement.Add(new Movement
                    {
                        CardId = movement.Uid,
                        DeviceId = movement.DeviceId,
                        LocationId = locationId,
                        SwipeTime = DateTime.Now
                    });

                    _context.SaveChanges();
                }
                else
                {
                    result.IsError = true;
                    result.ErrorMessage = $"Device location not found deviceId: {movement.DeviceId}";
                    return result;
                }

                var person = (from c in _context.Card
                              join pc in _context.PersonCard on c.Id equals pc.CardId
                              join p in _context.Person on pc.PersonId equals p.Id
                              where c.Uid.Equals(movement.Uid)
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