import sys
import datetime
import time
from datetime import date, timedelta

if len(sys.argv) !=2:
    print "provide testdata file path (e.g. 'C:\\Program Files\\Splunk\\etc\\apps\\ri_pas_sample_data\\data\\eventgenerated.log'"
    exit(1)

filename=sys.argv[1]  #'C:\\Program Files\\Splunk\\etc\\apps\\ri_pas_sample_data\\data\\eventgenerated.log'

# get all unique timestamps
timestamps=[]
lines=open(filename)
for line in lines:
    #timestamp=11/19/2014
    try:
        timestamp=line.split(' ')[0]
        if "=" in timestamp:
            timestamp=timestamp.split('=')[1]

        if timestamp not in timestamps:
            timestamps.append(timestamp)
    except:
        print "error to process "+line
        exit(1)

lines.close()

# update the timestamps so that the latest timestamp will be today
timestamps.sort(reverse=True)
totalDays=len(timestamps)
if timestamps[0]!=date.today().strftime('%m/%d/%Y'):
    dic={}
    dateDelta=0
    for i in range (0,totalDays):        
        today=date.today()-timedelta(dateDelta)
        dic[timestamps[i]]=today.strftime('%m/%d/%Y')
        dateDelta=dateDelta+1        

    #replacing old timestamp with new timestamp
    f=open(filename);
    text=f.read()
    for timestamp in timestamps:
        text=text.replace(timestamp,dic[timestamp])
    f.close()

    f=open(filename,"w")
    f.write(text)
    f.close()

print "Finish updating timestamps in testdata file"



    