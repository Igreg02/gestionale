using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("company")]
    public class Company : XPObject
    {
        public Company(Session session) : base(session) { }

        private string _name;
        [Size(255)]
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

        // Relazione 1-N: Una Company ha molti
        [Association("company-Project")]
        public XPCollection<Project> Project => GetCollection<Project>(nameof(Project));
    }
}