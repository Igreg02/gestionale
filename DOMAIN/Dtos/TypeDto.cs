using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    public class TypeDto
    {
        public class Response
        {
            public Guid Id { get; set; }

            public string Name { get; set; } = string.Empty;

        }


        public class Create
        {
            [Required(ErrorMessage = "La tipologia deve avere un nome")]

            public string Name { get; set; } = string.Empty;

        }

        public class Update
        {
            [Required(ErrorMessage = "La tipologia deve avere un nome")]
            public string Name { get; set; } = string.Empty;

        }
    }
}