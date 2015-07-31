Instructions!
=============
1. Unpack JMeter to the location of your choosing.
2. Run JMeter and open the "HTTPInputTest.jmx" project file.
3. Select the "HTTP Input Users" node and set the desired number of threads and loop iterations.
4. Select the "HTTP Request Defaults" node and input your server name and port number.
    Example:
        Server: my.splunk.instance
        Port: 8088
5. Select the "HTTP Header Manager" node and paste in your HTTP Input Token value.
    IMPORTANT:
        Be sure that the "Authorization" value starts with "Splunk ".  Note the space!
        Your HTTP Input Token value should be pasted after the space.
6. Click the big green "Play" button.
7. Click the "View Results Tree" node.  The results of each test will be displayed here.