# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.1] - 22-02-2021
### Updated
- Package dependency's
- Live data service authorization logging
- Live data authorization flow

### Removed
- Live data sensor links
- Networking dabase integration in the live data service

## [1.4.0] - 16-02-2021
### Added
- Routing cache
- Internal queue metrics

### Updated
- Command subscription QoS levels (from 1 to 3)
- Load tests for new caches

### Removed
- Ingress projects
- API projects (except the network API)
- Common projects (caching)

## [1.3.0] - 06-02-2021
### Updated
- Measurement query result JSON deserialization
- MeasurementQueryResult model annotations

### Removed
- Unused configuration files
- Unused deployment configuration

## [1.2.2] - 05-02-2021
### Updated
- Hardcode swagger and/or open API scheme's

### Added
- Set HTTP as possible scheme
- Set HTTPS as possible scheme

## [1.2.1] - 05-02-2021
### Added
- HTTPS swagger support

### Updated
- Network API swagger
- Auth API swagger
- Data API swagger
- Auth API swagger
- Dashboard API swagger

## [1.2.0] - 05-02-2021
### Updated
- Encoding storage conversion
- Sensor creation flow: publish sensor keys on the MQTT broker

### Added
- Ingress router request metrics
- Egress router request metrics

## [1.1.2] - 26-01-2021
### Updated
- Script directory name
- MQTT service project files
- CI/CD pipelines

### Added
- Security policy

### Removed
- CAKE build system
- Unused API code

## [1.1.1] - 26-01-2021
### Updated
- Update the versioning schema:
  - Update version of the MQTT service
  - Update the version of the core API's

## [1.1.0] - 26-01-2021
### Added
- Network:
  - Network project setup
  - Contracts project
  - Solution folders for:
    - Database project
    - General files
  - Message router
  - Network API + Gateway
  - Networking database:
    - Trigger administration
    - Sensor link administration
  - Live data service

- Platform:
  - Moved MQTT ingress service
  - Forward MQTT ingress to the HTTP gateway
  
- API:
  - Refactor SensateService into SensateIoT.API
  - Upgrade API's to .NET 5
  - Improve swagger documentation

### Removed
- Trigger administration (moved to network project)
- Network API (moved to network project)
- Trigger service (moved to network project)
- Live data service (moved to network project)

## [1.0.0] - 09-10-2020
### Added
- API's:
  - Network API
  - Data API
  - Authorization API
  - Blob API
  - Dashboard API
  
- Ingress services
- Data processing
