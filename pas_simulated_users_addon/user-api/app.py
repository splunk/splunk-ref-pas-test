#!/bin/python

# Add lib directory to include paths
import os.path
import sys
sys.path.append(os.path.join(os.path.dirname(__file__), 'lib'))

# Includes
from flask import Flask, jsonify, abort, make_response

app = Flask(__name__)

users = [
    {
        'UserName': 'rblack',
        'UserFullName': u'Richard Black',
        'UserEmail': u'rblack@splunk.com', 
        'UserPhone': u'512-999-999', 
        'CompanyName': u'Splunk, Inc.', 
        'UserRole': u'Sr Architect', 
        'CompanyAddress': u'123 Main St, Austin, TX - 78737', 
        'CompanyPhone': u'512-808-0808',
        'UserImage': u'https://s3.amazonaws.com/uifaces/faces/twitter/brad_frost/73.jpg',
        'done': False,
        'AccountStatus': 'Account Unlocked'
    },
    {
        'UserName': 'jbell',
        'UserFullName': u'John Bell',
        'UserEmail': u'jbell@ibm.us.com', 
        'UserPhone': u'308-777-999', 
        'CompanyName': u'IBM US, Inc.', 
        'UserRole': u'Systems Engineer', 
        'CompanyAddress': u'16 Elm Street, Charlotee, NC- 87699', 
        'CompanyPhone': u'876-808-0808',
        'UserImage': 'https://s3.amazonaws.com/uifaces/faces/twitter/peterme/73.jpg',
        'done': False,
        'AccountStatus': 'Account Unlocked'
    },
    {
        'UserName': 'bberry',
        'UserFullName': u'Ben Berry',
        'UserEmail': u'bberry@gmail.com', 
        'UserPhone': u'208-999-999', 
        'CompanyName': u'Splunk, Inc.', 
        'UserRole': u'Developer', 
        'CompanyAddress': u'61615 5th street, Austin, TX - 78737', 
        'CompanyPhone': u'512-677-9999',
        'UserImage': 'https://s3.amazonaws.com/uifaces/faces/twitter/dustinlamont/73.jpg',
        'done': False,
        'AccountStatus': 'Account Unlocked'
    }
]

@app.route('/user_list/api/v1.0/users', methods=['GET'])
def get_users():
    return jsonify({'users': users})

@app.route('/user_list/api/v1.0/users/<string:user_name>', methods=['GET'])
def get_user(user_name):
    user = filter(lambda t: t['UserName'] == user_name, users)
    if len(user) == 0:
        abort(404)
    return jsonify({'user': user[0]})

@app.route('/user_list/api/v1.0/users/lock/<string:user_name>', methods=['POST'])
def lock_user_account(user_name):
    try:
        user = filter(lambda t: t['UserName'] == user_name, users)
        user[0]["AccountStatus"] = 'Account Locked'

        if len(user) == 0:
            abort(404)
        return jsonify({'user': user[0]["AccountStatus"]})
    except Exception, e:
        print >> sys.stderr, "ERROR Error sending message: %s" % e
        return jsonify({'Error': "Account lock attempt failed!"})

@app.route('/user_list/api/v1.0/users/unlock/<string:user_name>', methods=['POST'])
def unlock_user_account(user_name):
    try:
        user = filter(lambda t: t['UserName'] == user_name, users)
        user[0]["AccountStatus"] = 'Account Unlocked'

        if len(user) == 0:
            abort(404)
        return jsonify({'user': user[0]["AccountStatus"]})
    except Exception, e:
        print >> sys.stderr, "ERROR Error sending message: %s" % e
        return jsonify({'Error': "Account lock attempt failed!"})

@app.errorhandler(404)
def not_found(error):
    return make_response(jsonify({'error': 'Not found'}), 404)

if __name__ == '__main__':
    app.run(host='0.0.0.0', debug=True)
