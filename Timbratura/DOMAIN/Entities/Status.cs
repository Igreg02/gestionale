using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("status")]
    public class Status : XPObject
    {
        public Status(Session session) : base(session) { }

        private string _name;
        [Size(255)]
        [Persistent("name")]
        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        // Relazione 1-N: Una Status ha molti 
        [Association("status-WorkLog")]
        public XPCollection<WorkLog> WorkLog => GetCollection<WorkLog>(nameof(WorkLog));
    }
}