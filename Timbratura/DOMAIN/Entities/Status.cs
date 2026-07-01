using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("Status")]
    public class Status : XPObject
    {
        public Status(Session session) : base(session) { }

        private string _name;
        [Size(255)] // Corrisponde al varchar del database
        [Persistent("name")]
        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        // Relazione 1-N: Una Status ha molti 
        [Association("Status-WorkLog")]
        public XPCollection<WorkLog> WorkLog => GetCollection<WorkLog>(nameof(WorkLog));
    }
}