using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("Project")]
    public class Project : XPObject
    {
        public Project(Session session) : base(session) { }

        private string _name;
        [Size(255)]
        [Persistent("name")]
        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        private Company _company;
        // Relazione N-1: Il progetto appartiene a una specifica Company
        [Association("Company-Project")]
        [Persistent("idCompany")] // Chiave esterna sul database
        public Company Company
        {
            get => _company;
            set => SetPropertyValue(nameof(Company), ref _company, value);
        }


        [Association("Project-WorkLog")]
        public XPCollection<WorkLog> WorkLog => GetCollection<WorkLog>(nameof(WorkLog));
    }
}