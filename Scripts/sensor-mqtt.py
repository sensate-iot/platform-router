#
# Dummy sensor using the MQTT interface.
#
# @author Michel Megens
# @email  dev@bietje.net
#

import sys, os
import json
import argparse
import paho.mqtt.publish as mqtt

from time import sleep
from random import random

class MqttSensor(object):
	def __init__(self, conf):
		self.host = conf.host
		self.port = conf.port
		self.sensor = conf.id
		self.secret = conf.secret

		if conf.user is not None:
			self.auth = {}
			self.auth['username'] = conf.user
			self.auth['password'] = conf.pw
		else:
			self.auth = None

	def generate_measurement(self):
		measurement = {}
		measurement['Longitude'] = 4.7769
		measurement['Latitude']  =  51.58307
		measurement['CreatedById'] = self.sensor
		measurement['CreatedBySecret'] = self.secret
		measurement['Data'] = [
			{'Name' : 'x', 'Value' : random() * 10},
			{'Name' : 'y', 'Value' : random() * 100},
			{'Name' : 'z', 'Value' : random() * 20}
		]
		return measurement

	def run(self):
		while True:
			m = self.generate_measurement()
			m = json.dumps(m)

			if self.auth is not None:
				mqtt.single(
					'sensate/measurements', m, hostname=self.host, port=self.port,
					auth=self.auth
				)
			else:
				mqtt.single('sensate/measurements', m, hostname=self.host, port=self.port)
			sleep(1)

class ArgumentParser(object):
	def __init__(self):
		self.host = None
		self.secret = None
		self.id = None
		self.port = 1883
		self.user = None
		self.pw = None

	def parse(self):
		parser = argparse.ArgumentParser(description='Websocket measurement generator tool')
		parser.add_argument('-H', '--host', metavar='HOST',
			help='MQTT broker hostname', required=True)
		parser.add_argument('-p', '--port', metavar='PORT',
			help='MQTT broker port number')
		parser.add_argument('-s', '--secret', metavar='SECRET',
			help='sensor secret set during sensor creation', required=True)
		parser.add_argument('-P', '--pw', metavar='PASSWORD',
			help='MQTT broker password')
		parser.add_argument('-u', '--user', metavar='USERNAME',
			help='MQTT broker username')
		parser.add_argument('-i', '--id', metavar='SENSOR',
			help='unique sensor identifier', required=True)
		parser.add_argument('-v', '--version', action='version', version='%(prog)s 0.1.0')
		parser.parse_args(namespace=self)

def main():
	p = ArgumentParser()
	p.parse()
	sensor = MqttSensor(p)
	sensor.run()

if __name__ == "__main__":
	main()