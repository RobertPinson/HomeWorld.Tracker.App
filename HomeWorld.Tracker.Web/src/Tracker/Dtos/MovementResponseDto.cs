using HomeWorld.Tracker.Web.Models;

namespace HomeWorld.Tracker.Web.Dtos
{
    public class MovementResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public bool Ingress { get; set; }
    }
}