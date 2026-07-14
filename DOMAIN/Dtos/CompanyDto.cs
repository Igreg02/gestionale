using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    /// 
    public class CompanyDto
    {
        public class Response
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public string Email { get; set; }

        }

        public class Delete
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

        }






        /// <summary>
        /// Client -> Server
        /// </summary>

        public class Create
        {

            [Required(ErrorMessage = "Devi inserire il nome dell'azienda")]
            [MaxLength(255)]
            public string Name { get; set; }

            [Required(ErrorMessage = "Devi inserire il l'email dell'azienda")]
            [EmailAddress]
            public string email { get; set; }

        }



        public class Update
        {
            [Required(ErrorMessage = "Devi inserire il nome dell'azienda")]

            [MaxLength(255)]
            public string Name { get; set; }

            [Required(ErrorMessage = "Devi inserire il l'email dell'azienda")]
            [EmailAddress]
            public string email { get; set; }
        }
    }
}