using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    /// 
    public class Type
    {
        public class Response
        {
            public int Oid { get; set; }

            public string Name { get; set; }


        }

        public class Delete
        {
            public int Oid { get; set; }

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