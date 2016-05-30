using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace HomeWorld.Tracker.App.DAL.Model
{
    public interface IEntity
    {
        int Id { get; set; }
    }

    public abstract class EntityBase : IEntity
    {
        public EntityBase()
        {
        }

        [PrimaryKey, AutoIncrement]
        public virtual int Id { get; set; }
    }

    public class Person : EntityBase
    {
        public Person()
        {
        }

        [PrimaryKey]
        public override int Id { get; set; }

        public string Name { get; set; }
        public byte[] Image { get; set; }
        public bool InLocation { get; set; }
        public string CardUid { get; set; }
    }

    public class Movement : EntityBase
    {
        public Movement()
        {
        }
        
        public string CardId { get; set; }
        public string SwipeTime { get; set; }
        public bool InLocation { get; set; }
    }
}
