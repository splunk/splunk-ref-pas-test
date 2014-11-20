#!/usr/bin/env python

import requests
import json
import sys, time
from splunklib.searchcommands import \
    dispatch, GeneratingCommand, Configuration, Option, validators

@Configuration()
class WarumGetUserInfoCommand(GeneratingCommand):
	user = Option(require=True)

	def generate(self):
		url = 'http://10.10.10.10:5000/user_list/api/v1.0/users/' + self.user
		response  = requests.get(url)
		data = response.json()
		data_list = data['user'].items()
		row = {}
		#for s in data_list:
			#row = row +' '+ str(s[0])+'="'+str(s[1])+'"'
		#yield {'_time': time.time(), 'sourcetype': 'RESTAPI', '_raw': row }
		for k,v in data_list:
			row[str(k)] = str(v)
		yield row
dispatch(WarumGetUserInfoCommand, sys.argv, sys.stdin, sys.stdout, __name__)
