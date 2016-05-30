using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeWorld.Tracker.App.DAL.Model;
using HomeWorld.Tracker.Dto;

namespace HomeWorld.Tracker.App.Service
{
    interface IPersonService
    {
        Task<MovementResponseDto> PostMovement(string cardId);
        Task<IEnumerable<Person>> GetPeople(string excludeIds, int deviceId);
    }
}
