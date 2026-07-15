using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
        [Persistent("worklog")]
    public class WorkLog : XPCustomObject
    {
        public WorkLog(Session session) : base(session) { }

        private Guid _id;
        [Key(AutoGenerate = true)]
        [Persistent("id")]
        public Guid Id
        {
            get => _id;
            set => SetPropertyValue(nameof(Id), ref _id, value);
        }
        private string _description = string.Empty;
        [Persistent("description")]
        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }

        private float _hoursCounter;
        [Persistent("hoursCounter")]

        public float HoursCounter
        {
            get => _hoursCounter;
            set => SetPropertyValue(nameof(HoursCounter), ref _hoursCounter, value);
        }

        private DateOnly _date;
        [Persistent("date")]

        public DateOnly Date
        {
            get => _date;
            set => SetPropertyValue(nameof(Date), ref _date, value);
        }

        private DateTime _createAt;
        [Persistent("createAt")]

        public DateTime CreateAt
        {
            get => _createAt;
            set => SetPropertyValue(nameof(CreateAt), ref _createAt, value);
        }

        private DateTime _updateAt;
        [Persistent("updateAt")]

        public DateTime UpdateAt
        {
            get => _updateAt;
            set => SetPropertyValue(nameof(UpdateAt), ref _updateAt, value);
        }

        private bool _isDeleted;
        [Persistent("isDeleted")]
        public bool IsWorkLogDeleted
        {
            get => _isDeleted;
            set => SetPropertyValue(nameof(IsWorkLogDeleted), ref _isDeleted, value);
        }

        private Project _project = null!;
        [Association("project-WorkLog")]
        [Persistent("idProject")]

        public Project Project
        {
            get => _project;
            set => SetPropertyValue(nameof(Project), ref _project, value);
        }

        private Type _type = null!;
        [Association("type-WorkLog")]
        [Persistent("idType")]

        public Type Type
        {
            get => _type;
            set => SetPropertyValue(nameof(Type), ref _type, value);
        }

        private Status _status = null!;
        
        [Association("status-WorkLog")]
        [Persistent("idStatus")]

        public Status Status
        {
            get => _status;
            set => SetPropertyValue(nameof(Status), ref _status, value);
        }

        private Employee _employee = null!;
        [Association("Employee-WorkLogs")]
        [Persistent("idEmployee")]
        public Employee Employee
        {
            get => _employee;
            set => SetPropertyValue(nameof(Employee), ref _employee, value);
        }
    }
}