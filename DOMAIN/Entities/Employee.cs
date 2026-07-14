using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    // PermissionPolicyUser estende BaseObject (XPBaseObject) —Possiede Oid Guid, persistenza, session.
    // In più fornisce UserName, Email, IsActive, Roles collection, password hashing, lockout.
    [Persistent("Employee")]
    [DefaultClassOptions]
    public class Employee : PermissionPolicyUser
    {
        public Employee(Session session) : base(session) { }

        private string _firstName = string.Empty;
        [Persistent("firstName")]
        public string FirstName
        {
            get => _firstName;
            set => SetPropertyValue(nameof(FirstName), ref _firstName, value);
        }

        private string _lastName = string.Empty;
        [Persistent("lastName")]
        public string LastName
        {
            get => _lastName;
            set => SetPropertyValue(nameof(LastName), ref _lastName, value);
        }

        private string _passwordHash = string.Empty;
        [Persistent("passwordHash")]
        [Size(1024)]
        public string PasswordHash
        {
            get => _passwordHash;
            set => SetPropertyValue(nameof(PasswordHash), ref _passwordHash, value);
        }

        [Association("Employee-WorkLogs")]
        public XPCollection<WorkLog> WorkLogs => GetCollection<WorkLog>(nameof(WorkLogs));
    }
}