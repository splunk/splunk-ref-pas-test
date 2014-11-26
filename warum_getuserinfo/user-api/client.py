import requests
import json

url = 'http://10.10.10.10:5000/user_list/api/v1.0/users/rblack'
response  = requests.get(url)
data = response.json()
test1 = data['user'].items()
row = {}
for k,v in test1:
	row[str(k)] = str(v)
print row

