pasgetuserinfo - custom Splunk search command to get user information from a REST service.

This app also contain a sample REST service to test the command.

Steps to start the REST service
*******************************
1. On command line go to user-api directory
   $ cd $SPLUNK_HOME/etc/apps/pas_simulated_users_addon/user-api

2. Run the web server
   $ python app.py

REST server would run on localhost ip at 5000 port. 

Use below URL to get user information for user "rblack"
http://localhost:5000/user_list/api/v1.0/users/rblack

Currently service is configured to work with three users "rblack", "jbell", and "bberry".

Syntax for the pasgetuserinfo command
***************************************
| pasgetuserinfo user=rblack


Simulating User Account Lock/Unlock States
******************************************
Two REST endpoints are exposed to simulate account locking and unlocking and
should be called via a standard POST interaction.
The three accounts listed above are available for use in simulations.

Calling the endpoints with other accounts will result in a 404 response with
and the following JSON: "{'error': 'Not found'}".

1. Account Lock Endpoint:
http://localhost:5000/user_list/api/v1.0/users/lock/<username>

Successful Response:
{
  "user": "Account Locked"
}

2. Account Unlock Endpoint:
http://localhost:5000/user_list/api/v1.0/users/unlock/<username>

Successful Response:
{
  "user": "Account Unlocked"
}

