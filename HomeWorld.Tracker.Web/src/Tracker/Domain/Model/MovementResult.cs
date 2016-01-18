using HomeWorld.Tracker.Web.Models;

namespace HomeWorld.Tracker.Web.Domain.Model
{
    public class MovementResult
    {
        public Person Person { get; set; }
        public bool Ingress { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}