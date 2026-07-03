using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    /// 
    public class Status
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
            [Required(ErrorMessage = "Lo stato deve avere un nome")]
            public string Name { get; set; }

        }



        public class Update
        {
            [Required(ErrorMessage = "Lo stato del lavoro deve avere un nome")]
            public string Name { get; set; }

        }
    }
}