# Sensate IoT

![header1] ![header2] ![header3]

Sensate is an opensource Internet of Things data broker. It enables
simplified management of devices, data and data analysis through a
well documented RESTful API. The backend scales extremely well, both
vertically and horizontally. High availability and performance is
ensured through clever use of caches and document stores.

[header1]: https://github.com/sensate-iot/SensateService/workflows/Docker/badge.svg "Docker Build"
[header2]: https://github.com/sensate-iot/SensateService/workflows/Format%20check/badge.svg ".NET format"
[header3]: https://img.shields.io/badge/version-v1.0.0-informational "Sensate IoT version"

## Sensor communication

Sensors can communicate in one of two ways with the sensate servers:

* MQTT (with or without TLS)
* HTTP
	* REST call or
	* Websockets

In both cases, a pseudo model in JSON format is expected by the Sensate backend:

```
{
  "longitude":4.79691,
  "latitude":51.58701,
  "timestamp": "2020-10-08T06:30:34.1115Z",
  "sensorId":"5efdaa9f282963000157823f",
  "secret":"$23c00474e37e82468518699c27411831214a818653d70fe2ee9bd80eea59f418==",
  "data":{
    "x":{
      "value":9.79635,
      "unit":"m/s2",
      "precision":0.01,
      "accuracy":0.5
    },
    "y":{
      "value":12.67006,
      "unit":"m/s2",
      "precision":0.01,
      "accuracy":0.5
    },
    "z":{
      "value":65.89981,
      "unit":"m/s2",
      "precision":0.01,
      "accuracy":0.5
    }
  }
}
```

On the root object the follow property's are required:

* sensorId;
* secret;
* data.

If the sensor doesn't provide a value for `timestamp`, the platform will assign
the current timestamp to the measurment.

The data property is an object of data points. At least one datapoint is required
and the `value` is the only required datapoint property. Please note that the array
length of ```data``` isn't limited to three. A measurement can contain anywhere
between 1 and 25 data points.

## Contributing

Contributing to the Sensate IoT project is very much appriciated. Please note that
writing code isn't the only way to contribute:

* You can submit bug reports on the issue system;
* You can submit feature requests on the same issue system;
* You can pick up existing issue's and implement them;
* You can help design features by replying on existing issue's.

When you pick up an existing issue in order to resolve it, *please let us know*. This will
prevent two people working on the same issue. If you are creating a new issue
please use one of the supplied github templates. Sections that are not applicable should
be removed. More information about contributing to Sensate IoT can be found in CONTRIBUTING.md.
