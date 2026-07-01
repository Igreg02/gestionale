using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("Type")]
    public class Type : XPObject
    {
        public Type(Session session) : base(session) { }

        private string _name;
        [Size(255)]
        [Persistent("name")]
        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        // Relazione 1-N: Una Type ha molti
        [Association("Type-WorkLog")]

        public XPCollection<WorkLog> WorkLog => GetCollection<WorkLog>(nameof(WorkLog));
    }
}