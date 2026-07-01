using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    public class WorkLog : XPObject
    {
        public WorkLog(Session session) : base(session) { }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }

        private int _hoursCounter;
        public int HoursCounter
        {
            get => _hoursCounter;
            set => SetPropertyValue(nameof(HoursCounter), ref _hoursCounter, value);
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => SetPropertyValue(nameof(Date), ref _date, value);
        }

        private DateTime _createAt;
        public DateTime CreateAt
        {
            get => _createAt;
            set => SetPropertyValue(nameof(CreateAt), ref _createAt, value);
        }

        private DateTime _updateAt;
        public DateTime updateAt
        {
            get => _updateAt;
            set => SetPropertyValue(nameof(updateAt), ref _updateAt, value);
        }









        // Le relazioni si definiscono con l'attributo Association
        private Project _project;
        [Association("Project-WorkLog")]
        public Project Project
        {
            get => _project;
            set => SetPropertyValue(nameof(Project), ref _project, value);
        }

        private Type _type;
        [Association("Type-WorkLog")]
        public Type Type
        {
            get => _type;
            set => SetPropertyValue(nameof(Type), ref _type, value);
        }

        private Status _status;
        [Association("Status-WorkLog")]
        public Status Status
        {
            get => _status;
            set => SetPropertyValue(nameof(Status), ref _status, value);
        }

/*
        private Employee _employee;
        [Association("Employee-WorkLogs")]
        public Employee _employee
        {
            get => __employee;
            set => SetPropertyValue(nameof(Employee), ref _employee, value);
        }

*/
    }
}