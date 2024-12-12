using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

using Microsoft.Extensions.Logging;

namespace C8yServices.Extensions.Notifications.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal static partial class LogMessageDefinitions
{
  [LoggerMessage(Level = LogLevel.Debug, Message = "Generic error in external MonitorHandler.", EventId = 1, SkipEnabledCheck = false)]
  public static partial void LogDebugGenericErrorInExternalMonitorHandler(this ILogger logger, Exception exception);

  [LoggerMessage(Level = LogLevel.Debug, Message = "Generic error in MonitorHandler.", EventId = 2, SkipEnabledCheck = false)]
  public static partial void LogDebugGenericErrorInMonitorHandler(this ILogger logger, Exception exception);

  [LoggerMessage(Level = LogLevel.Debug, Message = "Generic error in external ReceiveHandler.", EventId = 3, SkipEnabledCheck = false)]
  public static partial void LogDebugGenericErrorInExternalReceiveHandler(this ILogger logger, Exception exception);

  [LoggerMessage(Level = LogLevel.Debug, Message = "Generic error in ReceiveHandler.", EventId = 4, SkipEnabledCheck = false)]
  public static partial void LogDebugGenericErrorInReceiveHandler(this ILogger logger, Exception exception);

  [LoggerMessage(Level = LogLevel.Debug, Message = "ClientWebSocketState: {State}.", EventId = 5, SkipEnabledCheck = false)]
  public static partial void LogDebugWebSocketState(this ILogger logger, WebSocketState state);

  [LoggerMessage(Level = LogLevel.Information, Message = "Starting realtime websocket connection to URL = {Url}.", EventId = 8, SkipEnabledCheck = false)]
  public static partial void LogInformationStartWebsocketConnection(this ILogger logger, string url);

  [LoggerMessage(Level = LogLevel.Error, Message = "Generic exception has been thrown in {Method}.", EventId = 10, SkipEnabledCheck = false)]
  public static partial void LogErrorMethodGenericException(this ILogger logger, Exception exception, string method);

  [LoggerMessage(Level = LogLevel.Debug, Message = "Error during {Phase} {Message} {Transient}.", EventId = 11, SkipEnabledCheck = false)]
  public static partial void LogDebugErrorDuringPhase(this ILogger logger, string phase, string message, bool transient);
}