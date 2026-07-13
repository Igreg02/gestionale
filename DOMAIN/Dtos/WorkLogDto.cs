using System.ComponentModel.DataAnnotations;

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
            public Guid id { get; set; }

            public string Description { get; set; }

            public float HoursCounter { get; set; }

            public DateOnly Date { get; set; }

            public Guid IdProject { get; set; }

            public string ProjectName {get; set; }

            public Guid IdType { get; set; }

            public string TypeName {get; set; }

            public Guid IdStatus { get; set; }

            public string StatusName {get; set; }

        }

            public class Delete
            {
            public Guid Oid { get; set; }
            public Guid IdProject { get; set; }

            public string NameProject { get; set; }
        }

        /// <summary>
        /// Client -> Server
        /// </summary>
            public class Create
            {
            [Required(ErrorMessage = "Devi descrivere cosa è stato fatto")]

            public string Description { get; set; }

            [Required(ErrorMessage = "Devi indicare il numero di ore del lavoro svolto")]

            public float HoursCounter { get; set; }

            [Required(ErrorMessage = "Devi indicare il giorno in cui è stato svolto il lavoro")]

            public DateOnly Date { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID di quale progetto si fa riferimento")]

            public Guid IdProject { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID del tipo di lavoro che è stato svolto")]
            public Guid IdType { get; set; }
            [Required(ErrorMessage = "Devi indicare l'ID dello stato del lavoro")]
            public Guid IdStatus { get; set; }
        }


            public class Update
            {
            [Required(ErrorMessage = "Devi descrivere cosa è stato fatto")]

            public string Description { get; set; }

            [Required(ErrorMessage = "Devi indicare il numero di ore del lavoro svolto")]

            public float HoursCounter { get; set; }

            [Required(ErrorMessage = "Devi indicare il giorno in cui è stato svolto il lavoro")]

            public DateOnly Date { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID di quale progetto si fa riferimento")]

            public Guid IdProject { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID del tipo di lavoto che hai svolto")]

            public Guid IdType { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID dello stato del lavoro")]

            public Guid IdStatus { get; set; }
        }



    }
}
