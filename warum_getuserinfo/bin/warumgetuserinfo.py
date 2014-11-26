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
		url = 'http://localhost:5000/user_list/api/v1.0/users/' + self.user
		data = requests.get(url).json()
		if 'user' in data:
			# Known user.
			row = {}
			for k, v in data['user'].iteritems():
				row[str(k)] = str(v)
			yield row
		else:
			# Unknown user. Return no data.
			pass
dispatch(WarumGetUserInfoCommand, sys.argv, sys.stdin, sys.stdout, __name__)
