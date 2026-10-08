using System.ComponentModel.DataAnnotations;//Giovane Melo 
namespace AcademiaDoZe.Application.Enums;

public enum AppColaboradorVinculo
{
    [Display(Name = "CLT")]
    CLT = 0,
    [Display(Name = "Estagiário")]
    Estagio = 1
}