# Sensate IoT

![header1] ![header2] ![header3]

Sensate is an opensource Internet of Things data broker. It enables
simplified management of devices, data and data analysis through a
well documented RESTful API. The backend scales extremely well, both
vertically and horizontally. High availability and performance is
ensured through clever use of caches and document stores.

[header1]: https://github.com/sensate-iot/SensateService/workflows/Docker/badge.svg "Docker Build"
[header2]: https://github.com/sensate-iot/SensateService/workflows/Format%20check/badge.svg ".NET format"
[header3]: https://img.shields.io/badge/version-v0.5.1-informational "Sensate IoT version"

## Sensor communication

Sensors can communicate in one of two ways with the sensate servers:

* MQTT (with or without TLS)
* HTTP
	* REST call or
	* Websockets

In both cases, a pseudo model in JSON format is expected by the Sensate backend:

```
{
	"Longitude":4.796483396238929,
	"Latitude":51.59792154363324,
	"CreatedAt": "2020-04-02T00:00:00.000Z"
	"CreatedById":"5aa29b95c8640c2a3865e6fd",
	"CreatedBySecret":"$6e8b7b65ec31587babd07fc095fd1236a2dd76d03cfdea9c9864306e3b1c1342==",
	"Data":{
		"x":{
			"Value":8.753648712522201,
			"Precision": 0.125,
			"Accuracy": 0.998,
			"Unit":"m/s2"

		},
		"y":{
			"Value":11.267976634923137,
			"Unit":"m/s2",
			"Precision": 0.1,
			"Accuracy": 0.98884

		},
		"z":{
			"Value":16.580878999366355,
			"Precision": 0.225,
			"Accuracy": 0.754,
			"Unit":"m/s2"
		}
	}
}
```

On the root object the follow property's are required:

* CreatedById;
* CreatedBySecret;
* Data.

If the sensor doesn't provide a value for `CreatedAt`, the platform will assign
the current timestamp to the measurment.

The data property is an object of data points. At least one datapoint is required
and the `Value` is the only required datapoint property. Please note that the array
length of ```Data``` isn't limited to three. A measurement can contain anywhere
between 1 and 25 data points.

## Contributing

Contributing to the Sensate IoT project is verry much appriciated. Please note that
writing code isn't the only way to contribute:

* You can submit bug reports on the issue system
* You can submit feature requests on the same issue system
* You can pick up existing issue's and implement them
* You can help design features by replying on existing issue's

When you pick up an existing issue in order to resolve it, *please let us know*. This will
prevent two people working on the same issue. If you are creating a new issue
please use one of the supplied github templates. Section that are not applicable should
be removed.
