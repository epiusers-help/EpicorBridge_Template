# EpicorBridge
.NET Core Web API Facade to present layer for external applications to consume Epicor API. This could be combined with other services in addition to Epicor for a unified application interface connecting an app to Epicor. This app is intended to be run as a web service behind a firewall or application gateway and acts as a proxy between the client application and Epicor services. 

## Disclaimer

This example application relies on the calling application to pass an ```api_key``` (separate from an Epicor API Key) in the query parameter of each call to authenticate the calling app against a known value, which is configured in the app settings. One should take a layered approach to security and not rely on this alone. This is intended to demonstate one way to limit client application access to the API. 

# Configuration
## Connection Configuration
In the app settings, you will see an EpiSettings node. This is where the application is configured to connect to your Epicor instance. 
This example also utilizs a single company, but it could be extended to multi-company interaction. 

Config Setting  | Where From
------------- | -------------
Host  | Epicor Host Server Name
Company | Company ID
Instance | Epicor Instance Name
ApiKey | Epicor API Key generated upon API Key Creation
IntegrationUser | Epicor User Account ID for Integration
IntegrationPassword | Epicor User Account Password for Integration
LicenceTypeGuid | Licence Type Guid for this app to run under ( "00000003-9439-4B30-A6F4-6D2FD4B9FD0F" for WebService License Type)

To establish the known secret between client app and this app, create a client secret in the api_key node.
![image](https://user-images.githubusercontent.com/23319293/183756710-ff119f33-73fb-45cc-9cb9-b0239e8d35d3.png)

Other settings could be configured here as well.

## Application Organization
The application is organized into two main sections. The ```Utils``` folder holds classes and services responsible for client API Key Authentication, Epicor Session brokering, Epicor Connection settings, and static Function and BAQ definitions that are consumed in this app. 
![image](https://user-images.githubusercontent.com/23319293/183758153-fbfe6150-d15f-43d8-950c-6f66ee0535d5.png)


```EpiUtils``` is responsible for exposing methods to log into Epicor and grab a ```SessionID``` passed in subsequent calls. This is typically done upon app start or after a period of inactivity. This service is instantiated as a singleton upon app start. 

```EpiSessionSvc``` runs as a hosted background service upon app start to login to Epicor and periodically refresh its session. Note; this sometimes does not gracefully shut down if the application is put into a dormant state. 

```EpiFunctions``` holds a static definiton list of which Function Libraries and Functions are consumed. This was done to allow changes to a particular Epicor Function name without changing the place where it is consumed in this application.

```EpiBAQs``` holds a static definition list of which BAQs are consumed. 

```EpiAPIConnect``` is the main service that is injected into each controller and allows the controller to call a Function or BAQ service. It contains two methods; ExecuteBAQ or InvokeFunction.

```ApiKeyAuthAttribute``` checks all requests for the presence of a matching ```api_key``` defined in the configuration file. 

All ```GET``` calls have OData ```$top``` and ```$filter``` parameters available.

### Configuring BAQs
In this example, I provided 3 BAQs and examples of how to call them. Typically, one could organize your BAQ section by domain (i.e. Customer BAQs, Contact BAQs, etc.) in this list, however it is not a requirement and the BAQ name can be passed as a string instead. 

### Configuring Functions
In this example, I am showing how to call a custom Epicor Function Library and its functions, however since it is rather specific from company to company, this should be used to demonstrate structural functionality (no pun intended) rather than execution functionality. 

## EpiAPIConnect
This is a special service that is instantiated as a scoped service upon app start. 
In each controller responsible for extending endpoints that in turn call the Epicor API, this service should be made available by dependency injection. 
It follows a predicatble pattern; 1 method to call a BAQ, 1 method to invoke a function. 
Within both BAQ and Function calls, the `SessionID` and `ClaimedLicense` headers are added to the call to Epicor. 

In the `ExecuteBAQ` method, the call is passed a BAQ ID, an IQueryCollection of any forwarded query string, and finally an HTTP Verb. This will typically be GET or PATCH on a BAQ. Of note; BAQ call responses are trimmed down to their ```value``` node and exlude any other metadata. Function calls are of the response type defined in the Function out parameters. If metadata is required, one could simply return the entire response and not just the value node. 
![image](https://user-images.githubusercontent.com/23319293/183799815-aa6daab6-3767-4ae6-bba1-d8321fefec79.png)
![image](https://user-images.githubusercontent.com/23319293/183799872-e6787c65-8dc8-4f15-9c9d-7e6c3fbf6e4f.png)
![image](https://user-images.githubusercontent.com/23319293/183799733-7cc09f7b-f1d7-408f-afd1-0a8d31e78e24.png)



In the `InvokeFunction` method, the call is passed a Function Library name, the Function ID, and a collection of data (JSON) as a dynamic object to be forwarded on as the Function request parameters. The HTTP verb to invoke is always POST, so this is built into the call. 
When passing JSON into an endpoint that calls a Function, I use the `[FromBody]` tag to indicate the location of the data and is always of type `dynamic`. Classes could be used to define the JSON in true MVC style, but I've found the dynamic object gets the job done (with a little help from some well placed remarks in the source. 
![image](https://user-images.githubusercontent.com/23319293/183799579-2aa816d0-6729-4af8-8e8c-009b0f1d7875.png)


# Controllers
There is one controller shown with several examples of presenting endpoints and calling the EpiAPIConnect within them. 
Extending the solution to multiple domains is very easy; simply create your controller by domain, inject the `EpiAPIConnect` service, and build away. 
![image](https://user-images.githubusercontent.com/23319293/183799964-098a5cc6-6e01-454c-843b-ac02ea600419.png)


## Controller Routes
In this example I configured the controller routes to follow the pattern [Route("api/v{version:apiVersion}/[controller]/[action]")] as indicated in the route annotation in the controller. This could be changed if needed but provides a high level of predictability when extending the solution to many endpoints. 

## Starting and Consuming the app
The app is pre-configured to use port 44372 but this can be changed. Starting the app will spin up the services and grab an Epicor Session. The app Swagger UI page is available to test with. 
![image](https://user-images.githubusercontent.com/23319293/183768367-f5945fb7-bc7e-4d8a-9056-137535dc348c.png)

Prior to using the Swagger page, the ApiKey must be filled out with the client api_key variable. 
![image](https://user-images.githubusercontent.com/23319293/183768424-cbefcaf3-10c4-4ed7-9a30-561669abc3b7.png)

From there, the calls can be made. 

## Consuming the application
Once the app is deployed and hosted on a web server (I used IIS for mine), the API is available to call. 
Example:
`https://hostserver:port/api/v1/samplecustomer/getcustomerlist/?$top=10&api_key=apikeyvalue`

## Conclusions
Hopefully this gives a good starting point for building out a flexiable backend-for-frontend application layer. This allows one to decouple their front end technology and Epicor (or any other related services) into a unified API. 


