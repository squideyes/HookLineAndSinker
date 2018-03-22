HookLineAndSinker is a simple WebHook sink that forwards JSON payloads to a query-string-specified email address.

To test the function locally, be sure to add EmailFrom and SendGridApiKey settings to the "Values" node of your local.settings.json file.

When publishing to Azure, be sure to to set the EmailFrom and SendGridApiKey Application Settings to appropriate values; most conveniently through the "Function App Settings" link on the Publish dialog.

To use the HookLineAndSinker.PayloadEmailer service "POST" a JSON payload to a URL similar to the following:

[http://myfuncs.azurewebsites.net/api/PayLoadEmailer?email=somedude@someco.com&subject=My Custom Subject](http://myfuncs.azurewebsites.net/api/PayLoadEmailer?email=somedude@someco.com&subject=My%20Custom%20Subject)

An email address is required but a (custom!) subject is optional.  Also, the PayloadEmailer function expected a JSON payload to be included in the body of your request.  If the query-string parameter(s) or body are invalid a BadRequest (400) response will be generated.

If you find HookLineAndSinker to be helpful, or even (*heaven forefend*!) you hate it, please consider submitting a Pull Request to improve it.