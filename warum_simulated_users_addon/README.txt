warumgetuserinfo - custom Splunk search command to get user information from a REST service.

This app also contain a sample REST service to test the command.

Steps to start the REST service
*******************************
1. On command line go to user-api directory
   $ cd $SPLUNK_HOME/etc/apps/warum_getuserinfo/user-api

2. Run the web server
   $ python app.py

REST server would run on localhost ip at 5000 port. 

Use below URL to get user information for user "rblack"
http://localhost:5000/user_list/api/v1.0/users/rblack

Currently service is configured to work with three users "rblack", "jbell", and "bberry".

Syntax for the warumgetuserinfo command
***************************************
| warumgetuserinfo user=rblack
