# Sensate IoT ![logo]

![header]

Sensate is an opensource Internet of Things data broker. It enables simplified management 
of devices, data and data analysis through a well documented RESTful API. The backend scales extremely 
well, both vertically and horizontally. High availability and performance is ensured through 
clever use of caches and document stores.

[header]: https://sensate.bietje.net/assets/images/sensate-usage.png "Sensate IoT"
[logo]: sensate.png

## Sensor communication

Sensors can communicate in one of two ways with the sensate servers:

* MQTT (with or without TLS)
* Websockets

In both cases, a pseudo model in JSON format is expected by the Sensate backend:

    {
        "Data": 1.1234,
        "Longitude": 50.0
        "Latitude": 50.0
        "CreatedById": "123ACFD",
        "CreatedBySecret": "Super-secret",
        "CreatedAt": "DateTime
    }

All but the `CreatedAt` attribute are required. The format of the `CreatedAt` attribute is:

    %d.%M.%Y %H:%m:%s

## Contributing

Contributing to the Sensate project is verry much appriciated. Please note that writing code isn't the
only way to contribute:

* You can submit bug reports on the issue system
* You can submit feature requests on the same issue system
* You can pick up existing issue's and implement them
* You can help design features by replying on existing issue's

When you pick up an existing issue in order to resolve it, *please let us know*. This will
prevent two people working on the same issue.

## Installation & support

The installation guide can be found in INSTALL.md. Please check this document
before attempting to seek support. If INSTALL.md doesn't resolve your problems, please
don't hesitate to email us at sensate@mail.bietje.net.
