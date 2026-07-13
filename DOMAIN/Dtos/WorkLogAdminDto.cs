using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Contenitore dei DTO relativi a WorkLogAdmin
    /// </summary>
    public class WorkLogAdminDto
    {
        /// <summary>
        /// Server -> Client
        /// </summary>
        public class Response
        {
            public Guid Id { get; set; }

            public string Description { get; set; }

            public float HoursCounter { get; set; }

            public DateOnly Date { get; set; }

            public DateTime CreateAt {get;set;}

            public DateTime UpdateAt {get;set;}


            public Guid IdProject { get; set; }
            public string ProjectName {get; set;}

            public Guid IdEmploy { get; set; }

            public string EmployeeName {get; set;}

            public Guid IdType { get; set; }
            public string TypeName {get; set;}

            public Guid IdStatus { get; set; }

            public string StatusName {get; set;}

        }

            public class Delete
            {
            public Guid Id { get; set; }
            public Guid IdEmployee { get; set; }
            public string EmployeeName {get; set;}
            public Guid IdProject { get; set; }
            public string ProjectName {get; set;}
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

            // CreateAt / UpdateAt sono impostati dal server, il client non li manda.

            [Required(ErrorMessage = "Devi indicare l'ID di quale progetto si fa riferimento")]
            public Guid IdProject { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID del dipendente che svolto il lavoro")]
            public Guid IdEmployee { get; set; }

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
