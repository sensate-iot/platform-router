# Sensate IoT - Network

![header1] ![header2] ![header3]

This is the core network solution for the Sensate IoT data platform. This
solution contains all network infrastructure services:

- Message Router
- Gateway + configuration API
- Trigger service
- Storage service
- Live data service
- Database definition

## Router

The message router is at the core of Sensate IoT and responsible for routing
messages between various systems. The router uses the MQTT protocol to route
messages to:

- Trigger services;
- Storage services;
- Live data services;
- Public MQTT broker.

The router routes both SO (Sensor Originating, or measurements) and ST (Sensor
Terminating, or actuator) messages.

## Gateway

The gateway is the entry point to the platform. All other ingress services forward
data to this gateway internally. The gateway performs message authentication. The
authorization of messages is done by the router.

## Services

This solution contains serveral services that add value to a message or measurement:

- automation via the trigger service;
- persistance via the storage service;
- real-time updates via the live data service.

[header1]: https://github.com/sensate-iot/platform-network/workflows/Docker/badge.svg "Docker Build"
[header2]: https://github.com/sensate-iot/platform-network/workflows/Format%20check/badge.svg ".NET format"
[header3]: https://img.shields.io/badge/version-v1.6.2-informational "Sensate IoT version"
