using CommunityToolkit.Mvvm.Messaging.Messages;//Giovane Melo
namespace AcademiaDoZe.Presentation.AppMaui.Message;

public sealed class BancoPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value)
{
}
// ValueChangedMessage<T> é uma classe base do toolkit para mensagens que carregam um valor.