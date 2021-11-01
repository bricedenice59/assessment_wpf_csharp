# The Objective

The objective of this test is to create the back-end code for a small email application. Each requirement is there for a very specific reason, and will test your skills as a developer on Mailbird. If you have any questions before you start the test, please don't hesitate to ask. Also, you're welcome to upgrade the .NET Framework version, add any dependencies you desire or make other changes, as long as the requirements below are met.

Note that the hiring decision will primarily be based on your performance on this test. The final result should show:

- Your ability to learn and work with the email component we use, Mail.dll.
- Your attention to detail when reading instructions and your ability to work alone.
- Your ability to understand the principles of MVVM and use WPF data binding.
- Your ability to create and work with a multi threaded application.
- Your ability to structure your code and avoid code duplication and other code smells.

# Application Requirements

- Connect, using Mail.dll, to a mail server of the type specified in the server type combo box. The options should be 'IMAP' and 'POP3'. The encryption options should be 'Unencrypted', 'SSL/TLS' and 'STARTTLS'. Those must be the exact names in the UI.
- Once the start button is clicked, the app should immediately begin downloading message headers for all emails in the inbox, and display that data in the data grid on the left as they download. The data to display / columns should at least include 'From', 'Subject' and 'Date', but you're welcome to add more.
- While the headers are being downloaded, the app should concurrently download the bodies (the actual HTML/Text) of the same messages in separate threads/tasks, so they're ready to be viewed when a message is selected in the view.
- To perform all these automatic download tasks, 5 connections to the server should be created when the start button is pressed, and these 5 connections should be used and reused (without disconnecting) during the course of the run of downloading everything in the inbox. When all headers and bodies have been downloaded, the connections should be disconnected properly (not just disposed).
- Clicking on a message in the data grid should select it, and show the message body HTML/Text in the text box on the right side. The body should be downloaded from the server on demand, if not already downloaded, or if already downloaded just shown immediately. Downloading on demand should use a new, separate, connection that exists outside of the 5 other connections. This has the intentional effect of introducing a race condition between the automatic download of message bodies and the on demand download, so make sure to handle this.
- The application should be completely thread safe, and no header or message body should ever (even in theory) be able to download more than once. To accomplish this, some locking will likely be required.
- The application architecture should be designed so it's testable, but creating the tests are not required.
- Code for both IMAP and POP3 should be implemented, but since most POP3 servers dislike 5 connections, you don't need to test your POP3 implementation, and it will not be tested during the review of your solution.

# Mail.dll Reference

The following are direct links to the appropriate samples from the Mail.dll samples page, to help you with the implementation.

## IMAP

- [Connect to server](http://www.limilabs.com/blog/use-ssl-with-imap)
- [Download headers](http://www.limilabs.com/blog/get-email-information-from-imap-fast)
- [Download bodies](http://www.limilabs.com/blog/download-parts-of-email-message)

## POP3

- [Connect to server](http://www.limilabs.com/blog/use-ssl-with-pop3)
- [Download headers](http://www.limilabs.com/blog/get-email-headers-using-pop3-top-command) (you can assume that the TOP command is supported) 
- [Download bodies](http://www.limilabs.com/blog/get-common-email-fields-subject-text-with-pop3)

Note that the NuGet included evaluation version of Mail.dll changes the subject of some emails to "Please purchase a license" message and shows "Please purchase a license" dialogs.

# What we look for in particular

- Embrace the KISS and SOLID design principles, being careful not to over architecture the solution. It should, above all, be easily understandable / debuggable.
- If for whatever reason you're unable to complete all of the requirements, make a note of what you didn't complete, how you would have done it, and make sure that what IS completed is working perfectly and the code is nice and clean.
- Code duplication is kept to a minimum by using inheritance and otherwise unifying code.
- Magic strings and other code smells are kept to a minimum / is non existent. The new C# language features will help here.
- The view (MainWindow.xaml) uses MVVM and data binding extensively/exclusively.
- The solution directories and files are nicely structured.
- The app is built with a focus on speed, using multiple connections (within reason) to download headers and bodies.
- The app is thread safe and uses proper synchronization.
- The UI is never blocking and all expensive operations are run on other threads.
- Variables have meaningful names, and naming in general, including properties and methods, is consistent with: https://msdn.microsoft.com/en-us/library/ms229002(v=vs.110).aspx
