using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
        [Persistent("worklog")]
    public class WorkLog : XPObject
    {
        public WorkLog(Session session) : base(session) { }
        private string _description;
        [Persistent("description")]
        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }

        private int _hoursCounter;
        [Persistent("hoursCounter")]

        public int HoursCounter
        {
            get => _hoursCounter;
            set => SetPropertyValue(nameof(HoursCounter), ref _hoursCounter, value);
        }

        private DateTime _date;
        [Persistent("date")]

        public DateTime Date
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









        // Le relazioni si definiscono con l'attributo Association
        private Project _project;
        [Association("project-WorkLog")]
        [Persistent("idProject")]

        public Project Project
        {
            get => _project;
            set => SetPropertyValue(nameof(Project), ref _project, value);
        }

        private Type _type;
        [Association("type-WorkLog")]
        [Persistent("idType")]

        public Type Type
        {
            get => _type;
            set => SetPropertyValue(nameof(Type), ref _type, value);
        }

        private Status _status;
        
        [Association("status-WorkLog")]
        [Persistent("idStatus")]

        public Status Status
        {
            get => _status;
            set => SetPropertyValue(nameof(Status), ref _status, value);
        }

        // Relazione N-1: un WorkLog appartiene a un Employee
        private Employee _employee;
        [Association("Employee-WorkLogs")]
        [Persistent("idEmployee")]
        public Employee Employee
        {
            get => _employee;
            set => SetPropertyValue(nameof(Employee), ref _employee, value);
        }
    }
}