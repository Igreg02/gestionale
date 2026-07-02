namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Contenitore dei DTO relativi a WorkLog.
    /// </summary>
    public class WorkLogDto
    {
        /// <summary>
        /// Server -> Client (visibile solo agli Admin)
        /// </summary>
        public class Response
        {
            public int Oid { get; set; }

            public string Description { get; set; }

            public float HoursCounter { get; set; }

            public DateOnly Date { get; set; }

            public DateTime CreateAt { get; set; }

            public int IdProject { get; set; }

            public int IdEmployee { get; set; }

            public int IdType { get; set; }

            public int IdStatus { get; set; }
        }

        /// <summary>
        /// Client -> Server (usato dai lavoratori per creare/aggiornare un WorkLog)
        /// </summary>
        public class Create
        {
            public string Description { get; set; }

            public float HoursCounter { get; set; }

            public DateOnly Date { get; set; }

            public int IdProject { get; set; }

            public int IdType { get; set; }

            public int IdStatus { get; set; }
        }

            public class CreateAsAdmin
        {
            public string Description { get; set; }

            public float HoursCounter { get; set; }

            public DateOnly Date { get; set; }

            public int IdProject { get; set; }

            public int IdType { get; set; }

            public int IdStatus { get; set; }
        }
    }
}