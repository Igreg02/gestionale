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
            public int Oid { get; set; }

            public string Description { get; set; }

            public float HoursCounter { get; set; }

            public DateOnly Date { get; set; }

            public DateTime CreateAt { get; set; }

            public int IdProject { get; set; }
            public string ProjectName {get; set;}

            public int IdEmployee { get; set; }

            public string EmployeeName {get; set;}

            public int IdType { get; set; }
            public string TypeName {get; set;}

            public int IdStatus { get; set; }

            public string StatusName {get; set;}

        }

            public class Delete
        {
            public int Oid { get; set; }
            public int IdEmployee { get; set; }
            public string EmployeeName {get; set;}
            public int IdProject { get; set; }
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

            [Required(ErrorMessage = "Devi indicare l'ID di quale progetto si fa riferimento")]
            public int IdProject { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID del dipendente che svolto il lavoro")]
            public int IdEmployee { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID del tipo di lavoro che è stato svolto")]
            public int IdType { get; set; }
            
            [Required(ErrorMessage = "Devi indicare l'ID dello stato del lavoro")]
            public int IdStatus { get; set; }
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
            public int IdProject { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID del tipo di lavoto che hai svolto")]
            public int IdType { get; set; }

            [Required(ErrorMessage = "Devi indicare l'ID dello stato del lavoro")]
            public int IdStatus { get; set; }
        }

        
    }
}