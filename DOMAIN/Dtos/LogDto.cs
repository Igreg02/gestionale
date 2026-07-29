using System;

namespace GestionaleRendicontazione.Domain.Dtos
{
    public class LogDto
    {
        public class Response
        {
            public int Id { get; set; }
            public DateTime Data { get; set; }
            public string Livello { get; set; } = string.Empty;
            public string Messaggio { get; set; } = string.Empty;
            public string Metodo { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }
    }
}
