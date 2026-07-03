using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("project")]
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
        [Association("company-Project")]
        [Persistent("idCompany")]
        public Company Company
        {
            get => _company;
            set => SetPropertyValue(nameof(Company), ref _company, value);
        }


        [Association("project-WorkLog")]

        public XPCollection<WorkLog> WorkLog => GetCollection<WorkLog>(nameof(WorkLog));
    }
}