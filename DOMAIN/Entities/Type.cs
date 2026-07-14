using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("type")]
    public class Type : XPCustomObject
    {
        public Type(Session session) : base(session) { }
        
        private Guid _id;
        [Key(AutoGenerate = true)]
        [Persistent("id")]
        public Guid Id
        {
            get => _id;
            set => SetPropertyValue(nameof(Id), ref _id, value);
        }
        private string _name = string.Empty;
        [Size(255)]
        [Persistent("name")]
        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        [Association("type-WorkLog")]

        public XPCollection<WorkLog> WorkLog => GetCollection<WorkLog>(nameof(WorkLog));
    }
}