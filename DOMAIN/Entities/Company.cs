using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("company")]
    public class Company : XPCustomObject
    {
        public Company(Session session) : base(session) { }

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

        private string _email = string.Empty;
        [Size(255)]
        [Persistent("email")]
        public string Email
        {
            get => _email;
            set => SetPropertyValue(nameof(Email), ref _email, value);
        }

        [Association("company-Project")]
        public XPCollection<Project> Project => GetCollection<Project>(nameof(Project));
    }
}