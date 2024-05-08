using System.ComponentModel.DataAnnotations;

namespace Kolokwium1.DTOs;

public class Perscription
{
    [Required]
    public int IdPerscription { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public DateTime DueDate { get; set; }
    [Required]
    public int IdPatient { get; set; }
    [Required]
    public int IdDoctor { get; set; }
}