

using System;
using System.Net.WebSockets;

using Microsoft.Extensions.Logging;

namespace C8yServices;

public static partial class LogMessageDefinitions
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

  [LoggerMessage(Level = LogLevel.Error, Message = "Failed to extract payload from Message = {Message}.", EventId = 6, SkipEnabledCheck = false)]
  public static partial void LogErrorFailedToExtractPayload(this ILogger logger, string message);

  [LoggerMessage(Level = LogLevel.Error, Message = "Failed to extract raw message from Message = {Message}.", EventId = 7, SkipEnabledCheck = false)]
  public static partial void LogErrorFailedToExtractRawMessage(this ILogger logger, string message);

  [LoggerMessage(Level = LogLevel.Information, Message = "Starting realtime websocket connection to URL = {Url}.", EventId = 8, SkipEnabledCheck = false)]
  public static partial void LogInformationStartWebsocketConnection(this ILogger logger, string url);
}