# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
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