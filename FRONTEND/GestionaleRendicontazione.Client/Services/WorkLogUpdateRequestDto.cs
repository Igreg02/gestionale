using System;
using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Client.Services
{
    public class WorkLogUpdateRequestDto
    {
        [Required(ErrorMessage = "Devi descrivere cosa è stato fatto")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Devi indicare il numero di ore del lavoro svolto")]
        [Range(1, 24, ErrorMessage = "Le ore devono essere comprese tra 1 e 24.")]
        public float HoursCounter { get; set; }

        [Required(ErrorMessage = "Devi indicare il giorno in cui è stato svolto il lavoro")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Devi indicare l'ID di quale progetto si fa riferimento")]
        public Guid IdProject { get; set; }

        [Required(ErrorMessage = "Devi indicare l'ID del dipendente che ha svolto il lavoro")]
        public Guid IdEmployee { get; set; }

        [Required(ErrorMessage = "Devi indicare l'ID del tipo di lavoro che hai svolto")]
        public Guid IdType { get; set; }

        [Required(ErrorMessage = "Devi indicare l'ID dello stato del lavoro")]
        public Guid IdStatus { get; set; }
    }

    public class ProjectResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid IdCompany { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }

    public class StatusResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TypeResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class EmployeeResponseDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
