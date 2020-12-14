<h1 align="center">Common Data Service (current environment) mock</h1>
<h2 align="center">Power Automate MockUp</h2>
<h3 align="center">Battery included mock to mock Power Automate CDS connector. Using <a href="https://github.com/thygesteffensen/PowerAutomateMockUp">Power Automate MockUp</a> as skeleton and <a href="http://github.com/delegateas/XrmMockup">XrmMockup</a> as Dynamics Mock.</h3>
<p align="center">
        <img alt="Build status" src="https://img.shields.io/github/workflow/status/thygesteffensen/PAMU_CDS/Build">
    <a href="https://www.nuget.org/packages/PAMU_CDSce/">
        <img alt="Nuget downloads" src="https://img.shields.io/nuget/dt/PAMU_CDSce">
    </a>
    <a href="https://www.nuget.org/packages/PAMU_CDSce/">
        <img alt="Nuget version" src="https://img.shields.io/nuget/v/PAMU_CDSce">
    </a>
    <!--<a href="https://www.nuget.org/packages/PAMU_CDSce/">
        <img alt="Nuget prerelease version" src="https://img.shields.io/nuget/vpre/PowerAutomateMockUp">-->
    </a>
</p>

This is a full featured mock for the [Common Date Service (current environment)](https://docs.microsoft.com/en-us/connectors/commondataserviceforapps/) connector for Power Automate.

The mock is build using [Power Automate Mockup](https://github.com/thygesteffensen/PowerAutomateMockup) as the flow engine and [XrmMockup](https://github.com/delegateas/XrmMockup) to mock the underlying Dynamics 365.

## How to use

### Getting Started

First of all, replace your XrmMockup dependency with the [development version](https://www.nuget.org/packages/bd1fe5ca33fd455dafb99d34768b8de4/) developed to this project. The development version is build on the latest version of XrmMockup.

When configuring XrmMockup, add the following to the `XrmMockupSettings` when configuring your `XrmMockup365` instance:
```c#
MockUpExtensions = new List<IMockUpExtension> {_pamuCds}
```

Somewhere before the XrmMockup configure step, do the following:
```c#
CommonDataServiceCurrentEnvironment _pamuCds;

// ...

var flowFolderPath = new Uri("<Path to folder containing flows>");
_pamuCds = new CommonDataServiceCurrentEnvironment(flowFolderPath);
```

That's all. Now you can run your unit tests and the action executed in Power Automate flow will also be executed now, against your mock instance.

### Adding handlers to other actions 
Coming soon

### Asserting Actions
Coming soon

## Actions

The focus right now is create a MVP to use in my bachelor project, this meaning not all functions will be implemented at the moment. I will later create a description of how to contribute to this project, but not before the assignment have been handed in.

### General
Every call against CDS returns a JSON object with headers and body. Headers will not be generated, as the MVP does not support the use cae.

The body will almost be as the real deal, but with minor deviations. They are described below.

#### Symbol meaning:

- ✔ Action is intended to work 100% as the real thing, bugs might appear
- ❗ Limited functionality. Action will work, but some logic is not implemented, yet
- ❌ Action is not implemented

### Create a new record ✔

### Delete a record ✔ 

### Executes a changeset request ❗

This action could be implemented using the [ExecuteTransactionRequest](https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.messages.executetransactionrequest?view=dynamics-general-ce-9). However, XrmMockup does support the request and no issue is currently actively tracking this.

This action is not implemented as it works in Power Automate.

CDS actions inside the change set action will be executed.
If one of the CDS actions fails, the change set action will fail as well, piggy backing the error from the failing CDS action. Actions following the failing action inside the change set action will not be executed.
Changes made in D365 will still exists, meaning it is not a transaction.

This is basically the saem as a scope. The `ScopeActionExecutor` is used, to mock the behaviour.

### Get a record ❗

This action supports 4 parameters
1. Entity name - Table name
2. Entity id - row id
3. $select 
4. $expand

#### 3. $select
The select query is rather easy, since it is just are list of strings delimited by a comma, which can easily be converted to a ColumnSet.

#### 4. $expand
The expand query is a bit more complex. It's documentation can be seen [here](https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/webapi/retrieve-entity-using-web-api#retrieve-related-entities-for-an-entity-by-expanding-navigation-properties). 

Since the input of expand can have different forms, interpreting in can be a bit difficult. You cannot create a simple split string by comma, since this will not divide the odata filters corrext. Instead I have created a parser, similar to ExpressionParser in [PAMU](https://github/com/thygesteffensen/PowerAutomateMockUp), but more simple.
At the moment, the Parser does not cover all cases, but it's sufficient enough to cover the base case in the MVP.

The grammer for the parser is:
```
$expand=something($select=prop1,prop2),something2($select=prop1,prop2;$orderby=prop func;$filter=endswith(subhect,'1'))
```

````xml
<values>        ::= <value> *(,<value>)                 <!-- something($select=prop1,prop2),something2($select=prop1,prop2;$filter=prop func) -->

<value>         ::= <string> *(<parameters>)            <!-- something2($select=prop1,prop2;orderby=prop func) -->

<parameters>    ::= '('<parameter> *(;<paramters>)')'   <!-- ($select=prop1,prop2;orderby=prop func) -->
<paramter>      ::= $<string>=(<properties>|<function>) <!-- $select=prop1,prop2 -->

<properties>    ::= <string> *(,<string>)               <!-- prop1,prop2 -->
<function>      ::= <string> '('+(<string>)')'          <!-- endswith(subhect,'1') --> 
````

### Get file or image content ❌
Action is not implemented and will not be implemented in the near future.

### List records ❌
Actions is not yet implemented, but it will be soon.

### Perform a bound action ❌
XrmMockup does not support [actions](https://docs.microsoft.com/en-us/dynamics365/customerengagement/on-premises/customize/actions). If you want support check [support custom action plugins #65](https://github.com/delegateas/XrmMockup/issues/65).

### Perform an unbound action ❌
XrmMockup does not support [actions](https://docs.microsoft.com/en-us/dynamics365/customerengagement/on-premises/customize/actions). If you want support check [support custom action plugins #65](https://github.com/delegateas/XrmMockup/issues/65).


### Predict ❌
Action is not implemented and will not be implemented in the near future.

### Relate records ✔

### Unrelate records ✔

### Update record ✔

### Upload file or image content ❌
Action is not implemented and will not be implemented in the near future.


## Code style
The code is written using [Riders](https://www.jetbrains.com/help/rider/Settings_Code_Style_CSHARP.html) default C# code style.

Commits are written in [conventional commit](https://www.conventionalcommits.org/en/v1.0.0/) style, the commit messages are used to determine the version and when to release a new version. The pipeline is hosted on Github and [Semantic Release](https://github.com/semantic-release/semantic-release) is used.

## Installation

Currently the project is still in alpha. To find the packages at nuget.com, you have to check 'Prerelease', before the nuget appears.

You also need a modified version of [XrmMockup](https://github.com/delegateas/XrmMockup), you can find that [here]()

## Tests

Tests are located in the **Tests** project and they are written using Nunit as test framework.

## Contribute

This is my bachelor project and I'm currently not accepting contributions until it have been handed in. Anyway, fell free to drop an issue with a suggestion or improvement.

## Credits
Delegate A/S and the team behind XrmMockup.


## License

© Thyge Skødt Steffensen
