using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Contenitore unico dei DTO relativi a WorkLog.
    /// - <see cref="Admin"/>: vista/operazioni complete, usate da chi ha ruolo "Admin" (accede a tutti i dipendenti).
    /// - <see cref="User"/>: vista/operazioni ridotte, usate dal dipendente autenticato sui propri worklog
    ///   (l'IdEmployee non è mai esposto/richiesto: viene dedotto dal token).
    /// </summary>
    public class WorkLogDto
    {
        public class Admin
        {
            /// <summary>
            /// Server -> Client
            /// </summary>
            public class Response
            {
                public Guid Id { get; set; }

                public string Description { get; set; } = string.Empty;

                public float HoursCounter { get; set; }

                public DateOnly Date { get; set; }

                public DateTime CreateAt { get; set; }

                public DateTime UpdateAt { get; set; }

                public Guid IdProject { get; set; }
                public string ProjectName { get; set; } = string.Empty;

                public Guid IdEmployee { get; set; }
                public string EmployeeName { get; set; } = string.Empty;

                public Guid IdType { get; set; }
                public string TypeName { get; set; } = string.Empty;

                public Guid IdStatus { get; set; }
                public string StatusName { get; set; } = string.Empty;
            }

            /// <summary>
            /// Client -> Server
            /// </summary>
            public class Create
            {
                [Required(ErrorMessage = "Devi descrivere cosa è stato fatto")]
                public string Description { get; set; } = string.Empty;

                [Required(ErrorMessage = "Devi indicare il numero di ore del lavoro svolto")]
                [Range(1, 24, ErrorMessage = "Le ore devono essere comprese tra 1 e 24.")]
                public float HoursCounter { get; set; }

                [Required(ErrorMessage = "Devi indicare il giorno in cui è stato svolto il lavoro")]
                public DateOnly Date { get; set; }

                // CreateAt / UpdateAt sono impostati dal server, il client non li manda.

                [Required(ErrorMessage = "Devi indicare l'ID di quale progetto si fa riferimento")]
                public Guid IdProject { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID del dipendente che ha svolto il lavoro")]
                public Guid IdEmployee { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID del tipo di lavoro che è stato svolto")]
                public Guid IdType { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID dello stato del lavoro")]
                public Guid IdStatus { get; set; }
            }

            public class Update
            {
                [Required(ErrorMessage = "Devi descrivere cosa è stato fatto")]
                public string Description { get; set; } = string.Empty;

                [Required(ErrorMessage = "Devi indicare il numero di ore del lavoro svolto")]
                [Range(1, 24, ErrorMessage = "Le ore devono essere comprese tra 1 e 24.")]
                public float HoursCounter { get; set; }

                [Required(ErrorMessage = "Devi indicare il giorno in cui è stato svolto il lavoro")]
                public DateOnly Date { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID di quale progetto si fa riferimento")]
                public Guid IdProject { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID del dipendente che ha svolto il lavoro")]
                public Guid IdEmployee { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID del tipo di lavoro che hai svolto")]
                public Guid IdType { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID dello stato del lavoro")]
                public Guid IdStatus { get; set; }
            }
        }

        public class User
        {
            /// <summary>
            /// Server -> Client
            /// </summary>
            public class Response
            {
                public Guid Id { get; set; }

                public string Description { get; set; } = string.Empty;

                public float HoursCounter { get; set; }

                public DateOnly Date { get; set; }

                public Guid IdProject { get; set; }
                public string ProjectName { get; set; } = string.Empty;

                public Guid IdType { get; set; }
                public string TypeName { get; set; } = string.Empty;

                public Guid IdStatus { get; set; }
                public string StatusName { get; set; } = string.Empty;
            }


            /// <summary>
            /// Client -> Server
            /// </summary>
            public class Create
            {
                [Required(ErrorMessage = "Devi descrivere cosa è stato fatto")]
                public string Description { get; set; } = string.Empty;

                [Required(ErrorMessage = "Devi indicare il numero di ore del lavoro svolto")]
                [Range(1, 24, ErrorMessage = "Le ore devono essere comprese tra 1 e 24.")]
                
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
                public string Description { get; set; } = string.Empty;

                [Required(ErrorMessage = "Devi indicare il numero di ore del lavoro svolto")]
                [Range(1, 24, ErrorMessage = "Le ore devono essere comprese tra 1 e 24.")]
                public float HoursCounter { get; set; }

                [Required(ErrorMessage = "Devi indicare il giorno in cui è stato svolto il lavoro")]
                public DateOnly Date { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID di quale progetto si fa riferimento")]
                public Guid IdProject { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID del tipo di lavoro che hai svolto")]
                public Guid IdType { get; set; }

                [Required(ErrorMessage = "Devi indicare l'ID dello stato del lavoro")]
                public Guid IdStatus { get; set; }
            }
        }
    }
}
