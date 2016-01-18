using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeWorld.Tracker.Web.Dtos;

namespace HomeWorld.Tracker.Web.Domain
{
    public interface IPeopleService
    {
        IEnumerable<PersonDto> GetInLocation(int id);
    }
}
