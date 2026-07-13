using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("status")]
    public class Status : XPCustomObject
    {
        public Status(Session session) : base(session) { }

        private Guid _id;
        [Key(AutoGenerate = true)]
        [Persistent("id")]
        public Guid Id
        {
            get => _id;
            set => SetPropertyValue(nameof(Id), ref _id, value);
        }
        private string _name;
        [Size(255)]
        [Persistent("name")]
        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        [Association("status-WorkLog")]
        public XPCollection<WorkLog> WorkLog => GetCollection<WorkLog>(nameof(WorkLog));
    }
}