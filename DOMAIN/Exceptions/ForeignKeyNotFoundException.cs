using System;

namespace GestionaleRendicontazione.Domain.Exceptions
{
    // FK referenziata dal client non esiste: è un errore di validazione dell'input,
    // non un conflitto di stato server-side. Va mappata a 422, non a 409 (Cf. ProblemDetailsExtensions).
    public class ForeignKeyNotFoundException : Exception
    {
        public ForeignKeyNotFoundException(string message) : base(message) { }
    }
}
