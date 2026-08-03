namespace GestionaleRendicontazione.Client.Models
{
    /// <summary>
    /// Rappresentazione client dei worklog restituiti da GET /api/worklog. È un superset dei campi
    /// esposti sia dalla vista Admin che dalla vista User (vedi backend, Domain.Dtos.WorkLogDto):
    /// con un token User la risposta non contiene IdEmployee/EmployeeName, che restano quindi ai
    /// valori di default alla deserializzazione — System.Text.Json ignora le proprietà mancanti.
    /// </summary>
    public sealed class WorkLogResponseDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public float HoursCounter { get; set; }
        public DateOnly Date { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public Guid IdProject { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public Guid? IdEmployee { get; set; }
        public string? EmployeeName { get; set; }

        public Guid IdType { get; set; }
        public string TypeName { get; set; } = string.Empty;

        public Guid IdStatus { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}
