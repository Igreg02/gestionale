using System.ComponentModel.DataAnnotations;
namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    /// 
    public class Project
    {
        public class Response
        {
            public int Oid { get; set; }
            public string Name { get; set; }

            public int IdCompany { get; set; }

            public string CompanyName { get; set; }

        }

        public class Delete
        {
            public int Oid { get; set; }

            public string Name { get; set; }

            public int IdCompany { get; set; }

            public string CompanyName { get; set; }

        }






        /// <summary>
        /// Client -> Server
        /// </summary>

        public class Create
        {
            
            [Required(ErrorMessage = "Il nome del progetto è obbligatorio.")]
            [MaxLength(255)]
            public string Name { get; set; }

            [Required(ErrorMessage = "L'azienda di appartenenza è obbligatoria.")]
            public int IdCompany { get; set; }
        }

        public class Update
        {
            [Required(ErrorMessage = "Il nome del progetto è obbligatorio.")]
            [MaxLength(255)]
            public string Name { get; set; }

            [Required(ErrorMessage = "L'azienda di appartenenza è obbligatoria.")]
            public int IdCompany { get; set; }
        }
    }
}