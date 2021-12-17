# Sensate IoT - Router 

![header1] ![header2] ![header3]

The message router is at the core of Sensate IoT and responsible for routing
messages between various systems. The router uses the MQTT protocol to route
messages to:

- Trigger services;
- Storage services;
- Live data services;
- Public MQTT broker.

The router routes both SO (Sensor Originating, or measurements) and ST (Sensor
Terminating, or actuator) messages.

## Configuration

The message router needs various settings in order to function correctly:

- Database settings:
  - MongoDB
  - PostgreSQL

- Serilog configuration
- Routing config:
  - Trigger topic
  - Storage topic
  - Network event topic
  - Actuator topic
  - Live data topic

- MQTT configuration
- Metrics server

## Metrics

The following metrics are collected from the routing service:

- Message routed counts:
  - Ingress count;
  - Egress count;
- Request duration;
- Routing duration (per message).

[header1]: https://github.com/sensate-iot/platform-router/workflows/Docker/badge.svg "Docker Build"
[header2]: https://github.com/sensate-iot/platform-router/workflows/Format%20check/badge.svg ".NET format"
[header3]: https://img.shields.io/badge/version-v1.8.0-informational "Sensate IoT Router version"
