using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    public class EmployeeDto
    {
        public class Response
        {
            public Guid Oid { get; set; }
            public string Username { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Client -> Server
        /// </summary>
        public class Update
        {
            [Required(ErrorMessage = "Devi inserire un username")]
            [MaxLength(255)]
            public string Username { get; set; } = string.Empty;

            [MaxLength(255)]
            [Required(ErrorMessage = "Devi inserire il nome dell'utente")]
            public string FirstName { get; set; } = string.Empty;

            [MaxLength(255)]
            [Required(ErrorMessage = "Devi inserire il Cognome dell'utente")]
            public string LastName { get; set; } = string.Empty;
        }
    }
}
