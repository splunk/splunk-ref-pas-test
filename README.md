# Splunk Reference App - PAS - Test Repo 
### Version 1.0.1

The Splunk Reference App - PAS teaches you how to develop apps for Splunk. Here, you can explore the evolution of the reference app along with some additional engineering artifacts, like tests, deployment considerations, and tradeoff discussions.

The accompanying Splunk Developer Guide for Building Apps presents a documentary of how the team went about building and testing this reference app. The guide is currently available as an early preview at <http://dev.splunk.com/devguide>. We welcome your feedback on both the app and the guide.

### What Does This Repo Contain?
* **pas_simulated_users_addon** - a simulated identity provider so that you do not need to install your own LDAP or AD for the demo purposes. Follow instructions in the README.txt inside the folder on how to start it.
* **tests/PythonTest** - Selenium tests to drive the app through the browser.
* **tests/unit-test** - alternative Selenium tests in C#.
* **tests/pas_sample_data** - a sample data set used by the automated tests above. You can also use it to light the app up if you don't want to enable eventgen. 
_Note_: The top 2 panels in the Summary dashboard require data from the past 24 hrs, which this dataset doesn't contain. You will either need to load your own data or run eventgen for them to show visualizations.

### Accompanying Code Repo
<https://github.com/splunk/splunk-ref-pas-code>

### Community and Feedback
Questions, comments, suggestions? To provide feedback about this release, to get help with any problems, or to stay connected with other developers building on Splunk please visit the <http://answers.splunk.com> community site. 

File any issues on [GitHub](https://github.com/splunk/splunk-ref-pas-test/issues).

Community contributions via pull requests are welcomed! Go to the 
[Open Source](http://dev.splunk.com/view/opensource/SP-CAAAEDM) page for more information. 

* Email: devinfo@splunk.com
* Blog: <http://blogs.splunk.com/dev>
* Twitter: [@splunkdev](http://twitter.com/splunkdev)

### License

The Splunk Reference App - PAS is licensed under the Apache License 2.0. Details can be found in the LICENSE file.
