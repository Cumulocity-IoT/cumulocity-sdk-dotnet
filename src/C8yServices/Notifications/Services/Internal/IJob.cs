namespace C8yServices.Notifications.Services.Internal;

internal interface IJob
{
  Task Execute(CancellationToken token = default);
}