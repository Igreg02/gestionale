namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    /// 
    public class Project
    {
        public class Response
        {
            public int Oid { get; set; }

            public string Name { get; set; }

            public int IdCompany { get; set; }

            public string CompanyName { get; set; }

        }

        public class Delete
        {
            public int Oid { get; set; }

            public string Name { get; set; }

            public int IdCompany { get; set; }

            public string CompanyName { get; set; }

        }






        /// <summary>
        /// Client -> Server
        /// </summary>

        public class Create
        {

            public int Oid { get; set; }

            public string Name { get; set; }

            public int IdCompany { get; set; }

            public string CompanyName { get; set; }
        }



        public class Update
        {
            public string Name { get; set; }

            public string CompanyName { get; set; }
        }
    }
}