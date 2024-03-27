
using System;
using System.Threading;
using System.Threading.Tasks;

namespace C8yServices.Bootstrapping;

public interface ICumulocityCoreLibrayFactory : IDisposable
{
  Task InitOrRefresh(CancellationToken token = default);
}