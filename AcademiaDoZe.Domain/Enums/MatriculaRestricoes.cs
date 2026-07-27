using System;//Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Enums;

[Flags]
public enum MatriculaRestricoes
{
    none = 0,
    Diabetes = 1,
    PressaoAlta = 2,
    Labirintite = 4,
    Alergias = 8,
    ProblemasRespiratorios = 16,
    RemedioContinuo = 32
}
