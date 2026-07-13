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
            public Guid id { get; set; }
            public string Username { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
        
                public class Cashier
        {
            public int Oid { get; set; }

            public string Username { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }


        /// <summary>
        /// Client -> Server
        /// </summary>



        public class Update
        {   
            [Required(ErrorMessage = "Devi inserire un username")]
            [MaxLength(255)]
            public string Username { get; set; }

            [MaxLength(255)]
            [Required(ErrorMessage = "Devi inserire il nome dell'utente")]
            public string FirstName { get; set; }

            [MaxLength(255)]
            [Required(ErrorMessage = "Devi inserire il Cognome dell'utente")]

            public string LastName { get; set; }
        }

        
    }
}