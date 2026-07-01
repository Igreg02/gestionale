using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("Company")]
    public class Company : XPObject
    {
        public Company(Session session) : base(session) { }

        private string _name;
        [Size(255)] // Corrisponde al varchar del database
        [Persistent("name")]
        public string Name
        {
            get => _name;
            set => SetPropertyValue(nameof(Name), ref _name, value);
        }

        private string _email;
        [Size(255)]
        [Persistent("email")]
        public string Email
        {
            get => _email;
            set => SetPropertyValue(nameof(Email), ref _email, value);
        }

        // Relazione 1-N: Una Company ha molti Projects
        [Association("Company-Projects")]
        public XPCollection<Project> Projects => GetCollection<Project>(nameof(Projects));
    }
}