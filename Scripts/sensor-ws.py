#
# Dummy sensor using the websocket interface.
#
# @author Michel Megens
# @email  dev@bietje.net
#

import sys, os
import json
import argparse

from time import sleep
from random import random
from websocket import WebSocket

class WebSocketSensor(object):
	def __init__(self, conf):
		self.host = conf.host
		self.ws = WebSocket()
		self.ws.connect(self.host)
		self.sensor = conf.id
		self.secret = conf.secret

	def generate_measurement(self):
		measurement = {}
		measurement['Longitude'] = 4.7769
		measurement['Latitude']  =  51.58307
		measurement['CreatedById'] = self.sensor
		measurement['CreatedBySecret'] = self.secret
		measurement['Data'] = {}
		measurement['Data']['Volts'] = random() * 100
		return measurement

	def run(self):
		while True:
			m = self.generate_measurement()
			m = json.dumps(m)
			self.ws.send(m)
			print self.ws.recv()
			sleep(1)

class ArgumentParser(object):
	def __init__(self):
		self.host = None
		self.secret = None
		self.id = None

	def parse(self):
		parser = argparse.ArgumentParser(description='Websocket measurement generator tool')
		parser.add_argument('-H', '--host', metavar='HOST',
			help='websocket host (e.g. ws://localhost:5000)', required=True)
		parser.add_argument('-s', '--secret', metavar='SECRET',
			help='sensor secret set during sensor creation', required=True)
		parser.add_argument('-i', '--id', metavar='SENSOR',
			help='unique sensor identifier', required=True)
		parser.add_argument('-v', '--version', action='version', version='%(prog)s 0.1.0')
		parser.parse_args(namespace=self)

def main():
	p = ArgumentParser()
	p.parse()
	sensor = WebSocketSensor(p)
	sensor.run()

if __name__ == "__main__":
	main()
