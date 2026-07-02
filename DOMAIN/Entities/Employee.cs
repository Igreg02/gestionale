using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    // PermissionPolicyUser estende BaseObject (XPBaseObject) — ha già Oid Guid, persistenza, session.
    // In più fornisce UserName, Email, IsActive, Roles collection, password hashing, lockout.
    // Non si può ereditare anche da XPObject direttamente (no multi-inheritance in C#),
    // ma funzionalmente PermissionPolicyUser È già un BaseObject con Oid Guid.
    [Persistent("Employee")]
    [DefaultClassOptions]
    public class Employee : PermissionPolicyUser
    {
        public Employee(Session session) : base(session) { }

        private string _firstName;
        [Persistent("firstName")]
        public string FirstName
        {
            get => _firstName;
            set => SetPropertyValue(nameof(FirstName), ref _firstName, value);
        }

        private string _lastName;
        [Persistent("lastName")]
        public string LastName
        {
            get => _lastName;
            set => SetPropertyValue(nameof(LastName), ref _lastName, value);
        }

        // Data assunzione
        private DateTime _hireDate;
        [Persistent("hireDate")]
        public DateTime HireDate
        {
            get => _hireDate;
            set => SetPropertyValue(nameof(HireDate), ref _hireDate, value);
        }









        [Association("Employee-WorkLogs")]
        public XPCollection<WorkLog> WorkLogs => GetCollection<WorkLog>(nameof(WorkLogs));
    }
}