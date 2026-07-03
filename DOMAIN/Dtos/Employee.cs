using System.Security.Cryptography;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>

    public class Employee
    {
        public class Response
        {
            public int Oid { get; set; }

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
            public string Username { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        
    }
}