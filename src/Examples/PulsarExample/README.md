# Pulsar Example

This interactive console application demonstrates how to work with Cumulocity IoT's Pulsar/MQTT integration.

## Overview

The example provides an interactive menu to:
- **Create consumers** to receive messages from devices (`from-device` topic)
- **Create producers** to send messages to devices (`to-device` topic)
- **Remove consumers** ⚠️ IMPORTANT - consumers persist on server side!
- **Remove producers**
- **Send messages** to devices via producers
- **List active resources** (consumers, producers, tenants)

## Important: Consumer Lifecycle

**Pulsar consumers persist on the server side!** This means:
- When you create a consumer with a subscription name, it's registered on the Pulsar broker
- Messages for that subscription will accumulate on the server even when your app is not running
- You **must explicitly remove/unsubscribe** consumers to prevent message buildup
- This example shows how to properly clean up resources

## Architecture

- **Interactive Console**: Menu-driven interface for all operations
- **PulsarConsoleService**: Manages consumer/producer lifecycle and message handling
- **Multi-tenant Support**: Automatically detects and works with all subscribed tenants

## Cumulocity Pulsar Topics

Cumulocity provides two MQTT topics per tenant:

### From Device Topic
- **Format**: `persistent://<tenantID>/mqtt/from-device`
- **Purpose**: Consume messages sent by devices via MQTT
- **Usage**: Subscribe to this topic to receive device data, measurements, events, etc.

### To Device Topic
- **Format**: `persistent://<tenantID>/mqtt/to-device`
- **Purpose**: Send messages to devices via MQTT
- **Usage**: Publish to this topic to send commands, configurations, or acknowledgments to devices

## Configuration

The application expects the following environment variables (automatically provided by Cumulocity platform):

```
C8Y_BASEURL=https://your-tenant.cumulocity.com
C8Y_BASEURL_PULSAR=pulsar://your-tenant.cumulocity.com:6650
C8Y_BOOTSTRAP_TENANT=management
C8Y_BOOTSTRAP_USER=servicebootstrap_myapp
C8Y_BOOTSTRAP_PASSWORD=...
```

## How to Use

1. **Configure Environment** - Set up the required environment variables (see Configuration section)
2. **Run the Application** - Start the console app
3. **Wait for Subscriptions** - The app will initialize and wait for tenant subscriptions
4. **Create a Consumer** - Use menu option 1 to create a consumer for a tenant
   - Enter a unique subscription name (e.g., "my-app-subscription")
   - Consumer will start receiving messages from the `from-device` topic
5. **Create a Producer** - Use menu option 2 to create a producer for a tenant
6. **Send Messages** - Use menu option 5 to send test messages to devices
7. **Remove Consumers** - Use menu option 3 to properly clean up consumers ⚠️ IMPORTANT
8. **Exit Cleanly** - Use menu option 7 to dispose all resources before exiting

## Example Session

```
=== Pulsar Operations Menu ===
1. Create Consumer (from-device)
2. Create Producer (to-device)
3. Remove Consumer ⚠️  IMPORTANT
4. Remove Producer
5. Send Message via Producer
6. List Active Resources
7. Cleanup and Exit

Choice: 1

--- Create Consumer ---
Using tenant: t12345
Enter subscription name (e.g., 'my-subscription'): test-consumer
✓ Consumer created: t12345:test-consumer
  Topic: persistent://t12345/mqtt/from-device
  Subscription: test-consumer

⚠️  Remember: This consumer now persists on the Pulsar server!
    Messages will accumulate if not consumed or removed.
```

## Configuration

The application expects the following environment variables (automatically provided by Cumulocity platform):

```bash
C8Y_BASEURL=https://your-tenant.cumulocity.com
C8Y_BASEURL_PULSAR=pulsar://your-tenant.cumulocity.com:6650
C8Y_BOOTSTRAP_TENANT=management
C8Y_BOOTSTRAP_USER=servicebootstrap_myapp
C8Y_BOOTSTRAP_PASSWORD=...
```

## Usage

1. Deploy as microservice to Cumulocity platform
2. Subscribe tenants to your application
3. The service will automatically:
   - Detect new tenant subscriptions
   - Create Pulsar consumers for each tenant's `from-device` topic
   - Start processing messages from devices
   - Optionally send responses back via the `to-device` topic

## Customization

To adapt this for your use case:

1. Modify the message processing logic in `ProcessMessage()` method
2. Implement device-specific business logic (data storage, alerting, etc.)
3. Use `SendResponseToDevice()` to send commands or acknowledgments back to devices
4. Add error handling and retry logic as needed
5. Create additional services for different message types or processing workflows

## SDK Convenience Features

The SDK provides convenient topic properties:
```csharp
var pulsarService = pulsarServiceProvider.GetForTenant("myTenant");

// Topic names are automatically constructed:
var fromTopic = pulsarService.FromDeviceTopic; // persistent://myTenant/mqtt/from-device
var toTopic = pulsarService.ToDeviceTopic;     // persistent://myTenant/mqtt/to-device

// Use them to create consumers and producers:
var consumer = pulsarService.CreateConsumer("my-subscription", fromTopic);
var producer = pulsarService.CreateProducer(toTopic);
```

## Message Flow

```
Devices (MQTT)
    ↓
from-device topic → Consumer → Process Message → Business Logic
                                                      ↓
                                            to-device topic → Producer
                                                      ↓
                                                  Devices (MQTT)
```

## Topics

Cumulocity exposes exactly two MQTT topics via Pulsar:
- `persistent://<tenantID>/mqtt/from-device` - Messages FROM devices
- `persistent://<tenantID>/mqtt/to-device` - Messages TO devices

## Notes

- Each tenant subscription creates a separate consumer
- Messages are acknowledged after successful processing
- Failed messages can be negatively acknowledged for retry
- The service checks for new tenants every 30 seconds
