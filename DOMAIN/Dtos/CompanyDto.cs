namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    /// 
    public class Company
    {
        public class Response
        {
            public int Oid { get; set; }

            public string Name { get; set; }

            public string email { get; set; }

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


            public string Name { get; set; }

            public string email { get; set; }

        }



        public class Update
        {
            public string Name { get; set; }

            public string email { get; set; }
        }
    }
}