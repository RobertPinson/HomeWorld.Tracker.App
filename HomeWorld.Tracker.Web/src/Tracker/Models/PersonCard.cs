namespace HomeWorld.Tracker.Web.Models
{
    public class PersonCard
    {
        public int PersonId { get; set; }
        public int CardId { get; set; }
        public Person Person { get; set; }
        public Card Card { get; set; }
    }
}
