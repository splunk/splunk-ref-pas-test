warumgetuserinfo - custom Splunk search command to get user information from a REST service.

This app also contain sample REST service to test the command.

Steps to start REST service ( works on MAC and UNIX/LINUX flavor machines)
****************************************************************
On command line go to user-api directory
cd $SPLUNK_HOME/etc/apps/warum_getuserinfo/user-api

Install easy_install
sudo apt-get install python-setuptools

Install python virtualenv
sudo apt-get install python-virtualenv

virtualenv flask
	New python executable in flask/bin/python
	Installing setuptools............................done.
	Installing pip...................done.
	
flask/bin/pip install flask

Run app.py
./app.py

REST server would run on localhost ip at 5000 port. 

Use below URL to get user information for user "rblack"
http://localhost:5000/user_list/api/v1.0/users/rblack

Currently service is configured to work with three users "rblack", "jbell" and "bberry".

Sytax for the warumgetuserinfo command
************************************
| warumgetuserinfo user=rblack


