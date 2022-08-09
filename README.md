# EpicorBridge
.NET Core Web API Facade to present layer for external applications to consume Epicor API. This could be combined with other services in addition to Epicor for a unified application interface connecting an app to Epicor. This app is intended to be run as a web service behind a firewall or application gateway and acts as a proxy between the client application and Epicor services. 

## Disclaimer

This example application relies on the calling application to pass an ```api_key``` (separate from an Epicor API Key) in the query parameter of each call to authenticate the calling app against a known value, which is configured in the app settings. One should take a layered approach to security and not rely on this alone. This is intended to demonstate one way to limit client application access to the API. 

## Configuration

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


```EpiUtils.cs``` is responsible for exposing methods to log into Epicor and grab a ```SessionID``` passed in subsequent calls. This is typically done upon app start or after a period of inactivity. 

```EpiSessionSvc``` runs as a hosted background service upon app start to login to Epicor and periodically refresh its session. Note; this sometimes does not gracefully shut down if the application is put into a dormant state. 

```EpiFunctions``` holds a static definiton list of which Function Libraries and Functions are consumed. This was done to allow changes to a particular Epicor Function name without changing the place where it is consumed in this application.

```EpiBAQs``` holds a static definition list of which BAQs are consumed. 

```EpiAPIConnect``` is the main service that is injected into each controller and allows the controller to call a Function or BAQ service. It contains two methods; ExecuteBAQ or InvokeFunction.

```ApiKeyAuthAttribute``` checks all requests for the presence of a matching ```api_key``` defined in the configuration file. 

All ```GET``` calls have OData ```$top``` and ```$filter``` parameters available.

There is one controller shown with several examples of presenting endpoints and calling the EpiAPIConnect within them. 
Of note; BAQ call responses are trimmed down to their ```value``` node and exlude any other metadata. Function calls are of the response type defined in the Function out parameters. 

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


