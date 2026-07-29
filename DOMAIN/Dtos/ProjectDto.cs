using System.ComponentModel.DataAnnotations;
namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Contenitore dei DTO relativi a Project.
    /// Server -> Client
    /// </summary>
    public class ProjectDto
    {
        public class Response
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;

            public Guid IdCompany { get; set; }

            public string CompanyName { get; set; } = string.Empty;
        }


        /// <summary>
        /// Client -> Server
        /// </summary>
        public class Create
        {
            [Required(ErrorMessage = "Il nome del progetto è obbligatorio.")]
            [MaxLength(255)]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "L'azienda di appartenenza è obbligatoria.")]
            public Guid IdCompany { get; set; }
        }

        public class Update
        {
            [Required(ErrorMessage = "Il nome del progetto è obbligatorio.")]
            [MaxLength(255)]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "L'azienda di appartenenza è obbligatoria.")]
            public Guid IdCompany { get; set; }
        }
    }
}
