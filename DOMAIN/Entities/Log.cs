using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Entities
{
    [Persistent("LogApplicativo")]
    public class LogApplicativo : XPCustomObject // <--- Cambiato qui
    {
        public LogApplicativo(Session session) : base(session) { }

        // Definizione esplicita della chiave primaria autoincrementante
        private int _id;
        [Key(true)] 
        public int Id
        {
            get => _id;
            set => SetPropertyValue(nameof(Id), ref _id, value);
        }

        private DateTime _data;
        public DateTime Data
        {
            get => _data;
            set => SetPropertyValue(nameof(Data), ref _data, value);
        }

        private string _livello;
        [Size(50)]
        public string Livello
        {
            get => _livello;
            set => SetPropertyValue(nameof(Livello), ref _livello, value);
        }

        private string _messaggio;
        [Size(SizeAttribute.Unlimited)]
        public string Messaggio
        {
            get => _messaggio;
            set => SetPropertyValue(nameof(Messaggio), ref _messaggio, value);
        }

        private string _metodo;
        [Size(10)]
        public string Metodo
        {
            get => _metodo;
            set => SetPropertyValue(nameof(Metodo), ref _metodo, value);
        }

        private string _path;
        [Size(255)]
        public string Path
        {
            get => _path;
            set => SetPropertyValue(nameof(Path), ref _path, value);
        }

        private string _stackTrace;
        [Size(SizeAttribute.Unlimited)]
        public string StackTrace
        {
            get => _stackTrace;
            set => SetPropertyValue(nameof(StackTrace), ref _stackTrace, value);
        }

        private string _userId;
        public string UserId
        {
            get => _userId;
            set => SetPropertyValue(nameof(UserId), ref _userId, value);
        }
        
    }
}