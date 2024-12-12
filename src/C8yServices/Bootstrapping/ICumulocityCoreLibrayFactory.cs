namespace C8yServices.Bootstrapping;

public interface ICumulocityCoreLibrayFactory : IDisposable
{
  Task InitOrRefresh(CancellationToken token = default);
}