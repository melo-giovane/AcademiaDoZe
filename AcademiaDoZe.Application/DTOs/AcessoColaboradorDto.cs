namespace AcademiaDoZe.Application.DTOs;//Giovane Melo

public class AcessoColaboradorDto
{
    public int Id { get; set; }
    public required ColaboradorDto ColaboradorAcesso { get; set; }
    public required DateTime DataHora { get; set; }
}
