namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Contenitore dei DTO relativi a WorkLog
    /// </summary>
    public class WorkLogDto
    {
        /// <summary>
        /// Server -> Client
        /// </summary>
        public class Response
        {
            public int Oid { get; set; }

            public string Description { get; set; }

            public float HoursCounter { get; set; }

            public DateOnly Date { get; set; }

            public int IdProject { get; set; }

            public string ProjectName {get; set; }

            public int IdType { get; set; }

            public string TypeName {get; set; }

            public int IdStatus { get; set; }

            public string StatusName {get; set; }

        }

            public class Delete
        {
            public int Oid { get; set; }
            public int IdProject { get; set; }

            public string NameProject { get; set; }
        }
        
        /// <summary>
        /// Client -> Server
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


            public class Update
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