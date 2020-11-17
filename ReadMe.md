<h1 align="center">Common Data Service (current environment) mock</h1>
<h2 align="center">PowerAutomateMockUp</h2>
<h3 align="center">Battery included mock to mock Power Automate CDS connector. Using PowerAutomateMockUp as skeleton. Using togehter with XrmMockup.</h3>
<p align="center">
Bagdes
</p>

## Usage
This is both a full featured Mock of the Power Automate connector, Common Data Service (current environment) and in the same time, it is a full featured demo on how one can exploit Power Automate Mockup (PAMU) to it fullest.

This will support all the actions provided in the connector, integrated as an extension into Delegate's XrmMockup*.


* I have modified XrmMockup to enable extension to Mockups core and I have build my own package to avoid messing with the real deal. 


## Repository specifications
This is the repository for Power Automate MockUp (PAMU) which have a CI/CD pipeline. The version, and triggering of release, is determined from the commits. Semantic-Release is used in the pipeline and it analyzes the commits using (Conventional Commits)[https://www.conventionalcommits.org/en/v1.0.0/].


## OrganizationResponse
It does not seem like XrmMockup populates the expected data in the responses, modifying XrmMockup will take too long.

Instead we'll use something else.


## Nicesness

### DI - Dependency Injection

Since PAMU is build using DI, we get the operation to take the full advantage of DI and create some amazing structure. E.g. we don't have to worry about OrganizationService, UserRefs or similar stuff get passed down through where it is needed. Let's say we in or CreaterecordAction need to execute a create request? Then we'll need an OrganizationService, just add it in the constructor of the Action implementation, add DI make sure you get it.

