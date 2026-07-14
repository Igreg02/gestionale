using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    /// 
    public class StatusDto
    {
        public class Response
        {
            public Guid Id { get; set; }

            public string Name { get; set; } = string.Empty;


        }

        /// <summary>

        /// Client -> Server

        /// </summary>

        public class Create
        {
            [Required(ErrorMessage = "Lo stato deve avere un nome")]
            public string Name { get; set; } = string.Empty;

        }



        public class Update
        {
            [Required(ErrorMessage = "Lo stato del lavoro deve avere un nome")]
            public string Name { get; set; } = string.Empty;

        }
    }
}