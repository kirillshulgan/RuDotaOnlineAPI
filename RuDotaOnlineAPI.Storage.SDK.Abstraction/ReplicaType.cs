namespace RuDotaOnlineAPI.Storage.SDK.Abstraction;

public enum ReplicaType
{
    Leader,     // Запись и критичные чтения
    SyncRead,   // Обычные операции чтения
    AsyncRead   // Аналитика, отчёты, справочники
}