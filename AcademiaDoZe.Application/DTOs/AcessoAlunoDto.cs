namespace AcademiaDoZe.Application.DTOs;//Giovane Melo

public class AcessoAlunoDto
{
    public int Id { get; set; }
    public required AlunoDto AlunoAcesso { get; set; }
    public required DateTime DataHora { get; set; }
}
