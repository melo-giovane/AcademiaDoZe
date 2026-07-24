using System;
using System.Collections.Generic;
using System.Text;

/* Cadastro da matricula
O sistema deve permitir o registro da matricula:
• *aluno, *plano {mensal, trimestral, semestral ou anual}, *data de inicio, *data final, *objetivo, restriGöes {ex:
diabetes, pressäo alta, labirintite, alergias, problemas respiratörios, uso de remédios continuos, etc.},
observaqöes sobre as restriqöes, laudo médico. */

namespace AcademiaDoZe.Domain.Entities;

private class Matricula : Entity
{
    public Aluno Aluno { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    // public Objetivo Objetivo { get; private set; } 


}
