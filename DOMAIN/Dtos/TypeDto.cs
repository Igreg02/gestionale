using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    /// 
    public class TypeDto
    {
        public class Response
        {
            public Guid Id { get; set; }

            public string Name { get; set; }


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
            [Required(ErrorMessage = "La tipologia deve avere un nome")]

            public string Name { get; set; }

        }

        public class Update
        {
            [Required(ErrorMessage = "La tipologia deve avere un nome")]
            public string Name { get; set; }

        }
    }
}