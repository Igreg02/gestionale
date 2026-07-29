using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    public class CompanyDto
    {
        public class Response
        {
            public Guid Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

        }


        public class Create
        {
            [Required(ErrorMessage = "Devi inserire il nome dell'azienda")]
            [MaxLength(255)]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Devi inserire l'email dell'azienda")]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public class Update
        {
            [Required(ErrorMessage = "Devi inserire il nome dell'azienda")]
            [MaxLength(255)]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Devi inserire l'email dell'azienda")]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }
    }
}