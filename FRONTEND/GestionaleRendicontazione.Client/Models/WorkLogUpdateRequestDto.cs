using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Client.Models
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
}
