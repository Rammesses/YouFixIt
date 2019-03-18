# RESTful Services - Best Practices

We create a lot of APIs within our codebase - Web APIs at the presentation 
layer (front end), Service APIs in the middle tier (back end),
and elsewhere for supporting services.

This document outlines our best practice guidelines for writing these services.

## Principles of REST

In general, when building services that expose CRUD or similar object-based APIs, we should build our services according to RESTful guidelines

REST stands for *RE*presentational *S*tate *T*ransfer, and is based on the following principles:

1. REST is a client-server architecture. 

   The client and the server both have a different set of concerns. The server stores and/or manipulates information and makes it available to the user in an efficient manner. The client takes that information and displays it to the user and/or uses it to perform subsequent requests for information. This separation of concerns allows both the client and the server to evolve independently as it only requires that the interface stays the same.

2. REST is stateless. 

   That means the communication between the client and the server always contains all the information needed to perform the request. There is no session state in the server, it is kept entirely on the client's side. If access to a resource requires authentication, then the client needs to authenticate itself with every request.

3. REST is cacheable. 

   The client, the server and any intermediary components can all cache resources in order to improve performance.

4. REST provides a uniform interface between components. 

   This simplifies the architecture, as all components follow the same rules to speak to one another. It also makes it easier to understand the interactions between the different components of the system. A number of constraints are required to achieve this. They are covered in the rest of the chapter.

5. REST is a layered system. 

   Individual components cannot see beyond the immediate layer with which they are interacting. This means that a client connecting to an intermediate component, like a proxy, has no knowledge of what lies beyond. This allows components to be independent and thus easily replaceable or extendable.

6. REST optionally provides code on demand. 

   "Code" may be downloaded to extend client functionality. This is optional however because the client may not be able to download or run this code, and so a REST component cannot rely on it being executed.

(from https://ninenines.eu/docs/en/cowboy/2.6/guide/rest_principles/)

In real-terms this means

* Use sensible, descriptive API routes (URLs)
* Deal with Resources (Cases, Surveyors, etc)
* Define operations based on the HTTP method used
* Be consistent

## API routes

RESTful principles extend to making the HTTP routes your API exposes self-descriptive (and consistent).

**Rule 1:** Use plural nouns for both retrieving lists of resources, and when modifying individual resources, _except_ when a sub-resource can only ever be singular (e.g. the booking on a case).

**Rule 2:** Use HTTP methods to indicate the intention of the action

* `GET` to retrieve resources
* `PUT` to create resources
* `POST` to update resources
* `DELETE` to delete resources

**Rule 3:** Use query parameters _only_ to define sort, filter and paging within `GET` requests.

#### Sample `GET` methods

* `GET /api/v1/surveyors/` - Gets the list of all surveyors
* `GET /api/v1/surveyors/1234` - Gets the surveyor details for Surveyor #1234 
* `GET /api/v1/surveyors/1234/shifts` - Gets all the shifts for Surveyor #1234
* `GET /api/v1/surveyors/1234/shifts?date=2019-01-14` - Gets the shift for Surveyor #1234 on 14th Jan 2019

#### Sample `POST` methods

* `POST /api/v1/surveyors` - Creates a new surveyor **and returns it**
* `POST /api/v1/cases/QU123456/booking` - Creates a new booking on the existing Case QU123456 **and returns the booking**

#### Sample `PUT` methods

* `PUT /api/v1/surveyors/1234` - Updates a surveyor record **and returns it**
* `PUT /api/v1/cases/QU123456/booking` - Updated the existing booking on the Case QU123456 **and returns the booking**

#### Sample `DELETE` methods

* `DELETE /api/v1/surveyors/1234` - Deletes a surveyor record
* `DELETE /api/v1/cases/QU123456/booking` - Deletes the existing booking on the Case QU123456

## HTTP Status Codes

We _must_ return sensible status codes from our APIs

* `200 ok` - the request was successful (whether or not data is returned)
* `404 Not Found` - the requested resource was not found
* `400 Bad Request` - the request was invalid (validation or some other reason)

We should **never** see `500 Server Error` responses unless something is seriously wrong. Catching and handling a `500 Server Error` response in a client is **always** incorrect.

## Returning errors

Our services need to be consistent and logical in their response patterns - otherwise, debugging errors becomes difficult, and sensibly passing error information between layers becomes impossible.

Consider when an API call is made with an invalid request - without any error handling, the service would return a `500 Server Error` in response to say a `ValidationException`. Instead, code the service defensively to return a `400 Bad Request` response.

Additionally, you should use the RFC7807 `problem+json` pattern for returning additional data with our errors. Our validation error response then looks like this:

```
   HTTP/1.1 400 Bad Request
   Content-Type: application/problem+json
   Content-Language: en

   {
   "type": "https://schemas.landmark.co.uk/errors/validation-error",
   "title": "Your request parameters didn't validate.",
   "invalid-params": [ {
                         "name": "age",
                         "reason": "must be a positive integer"
                       },
                       {
                         "name": "color",
                         "reason": "must be 'green', 'red' or 'blue'"}
                     ]
   }
```

If the calling client doesn't understand the extended data being returned, it's still getting the `400 Bad Request` status code. Your strongly typed .Net client, however, can use this data to translate the `400 Bad Request` response back into a `ValidationErrorsException` that then propagates up the stack.

**NOTE:** Your API should **never** generate `500 Server Error` responses in normal operation, and you should **never** pass exception data out of your service using `problem+json`.

## HATEOS

HATEOS (*H*ypermedia *a*s *t*he *E*ngine *o*f *A*pplication *S*tate) is an extension to RESTful services that addresses the final RESTful principle - that of delivering "code" to extend the service. Specifically, it provides a mechanism to decorate the JSON representation of a resource with metadata that describes other available endpoints.

So, for example, getting a `Case` might include links to available activitie APIs on that resource:

```
GET /api/v1/cases/QU123456 HTTP/1.1
Accept: application/hal+json

HTTP/1.1 200 OK
Content-Type: application/hal+json

{
    "_links": {
        "self": { "href": "/api/v1/cases/QU123456" },
        "book": { "href": "/api/v1/cases/QU123456/book" },
        "cancelbooking": { "href": "/api/v1/cases/QU123456/cancelbooking" },
        "rebook": { "href": "/api/v1/cases/QU123456/rebook" },
        ...
    },

    "SysRef": "QU123456",
    ...
}
```

Alternatively, getting a `Surveyor` might include links to retrieve the Surveyor's `Shifts`:

```
GET /api/v1/surveyors/1234 HTTP/1.1
Accept: application/hal+json

HTTP/1.1 200 OK
Content-Type: application/hal+json

{
    "_links": {
        "self": { "href": "/api/v1/surveyors/1234" },
        "find": { "href": "/api/v1/surveyors/{id}", "templated": true }
        "shifts": { "href": "/api/v1/surveyors/1234/shifts" },
        "shiftstoday": { "href": "/api/v1/surveyors/1234/shifts?date=today" },
        "shiftsonday": { "href": "/api/v1/surveyors/1234/shifts{?date}", "templated": true },
        "privateappointments": { "href": "/api/v1/surveyors/1234/privateappointments" },
        ...
    },

    "SurveyorId": "12345",
    ...
}
```

This is particularly useful when returning paged result sets, as the `_links` node can return the correct navigation URLs - removing the need to calculate them in the client.

```
GET /api/v1/cases?status=E&slaEndDate=2019-01-14&page=2 HTTP/1.1
Accept: application/hal+json

HTTP/1.1 200 OK
Content-Type: application/hal+json

{
    "_links": {
        "self": { "href": "/api/v1/cases?status=E&slaEndDate=2019-01-14&page=2" },
        "next": { "href": "/api/v1/cases?status=E&slaEndDate=2019-01-14&page=1" },
        "prev": { "href": "/api/v1/cases?status=E&slaEndDate=2019-01-14&page=3" },
        "find": { "href": "/api/v1/cases/{id}", "templated": true }
        ...
    },

    "cases": [
        { 
            "SysRef": "QU123456",
            ...
        },
        ...
    ]
    ...
}
```


## Best Practice

* **DO** use the avalable HTTP methods to differentiate actons on a resource
  * **DO NOT** use `GET` when the resource will be modified
  * **DO NOT** use `PATCH` - use a different endpoint and a custom DTO

* **DO** return sensible HTTP status codes

* **DO** return additional information when a failure resoponse is returned
   (see https://tools.ietf.org/html/rfc7807)

* **DO** use HATEOS / HAL meta-data to describe related actions available in the API

* **DO NOT** catch exceptions in your service APIs *unless* you are converting them to sensible error responses (with `problem+json`)

* **DO NOT** catch and handle `500 Server Error` responses in your client