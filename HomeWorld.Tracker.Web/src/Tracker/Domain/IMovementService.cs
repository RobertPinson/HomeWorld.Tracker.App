using System.Collections.Generic;
using System.Threading.Tasks;
using HomeWorld.Tracker.Web.Domain.Model;
using HomeWorld.Tracker.Web.Dtos;

namespace HomeWorld.Tracker.Web.Domain
{
    public interface IMovementService
    {
        MovementResult Save(MovementDto movement);
    }
}
