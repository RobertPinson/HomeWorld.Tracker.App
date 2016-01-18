using System;
using System.Threading.Tasks;

namespace HomeWorld.Tracker.App.Service
{
    interface IPersonService
    {
        Task<MovementResponseDto> PostMovement(string cardId);
    }
}
