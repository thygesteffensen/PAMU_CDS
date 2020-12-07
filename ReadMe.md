<h1 align="center">Common Data Service (current environment) mock</h1>
<h2 align="center">PowerAutomateMockUp</h2>
<h3 align="center">Battery included mock to mock Power Automate CDS connector. Using PowerAutomateMockUp as skeleton. Using togehter with XrmMockup.</h3>
<p align="center">
        <img alt="Build status" src="https://img.shields.io/github/workflow/status/thygesteffensen/PAMU_CDS/Build/dev">
    <a href="https://www.nuget.org/packages/PAMU_CDS/">
        <img alt="Nuget downloads" src="https://img.shields.io/nuget/dt/PowerAutomateMockUp">
    </a>
    <a href="https://www.nuget.org/packages/PAMU_CDS/">
        <img alt="Nuget version" src="https://img.shields.io/nuget/v/PowerAutomateMockUp">
    </a>
    <!--<a href="https://www.nuget.org/packages/PAMU_CDS/">
        <img alt="Nuget prerelease version" src="https://img.shields.io/nuget/vpre/PowerAutomateMockUp">-->
    </a>
</p>
<p align="center">
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/Index">Home</a>
    |
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/GettingStarted">Getting Started</a>
    |
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/Technical">Technical</a>
</p>

This is a full featured mock for the [Common Date Service (current environment)](https://docs.microsoft.com/en-us/connectors/commondataserviceforapps/) connector for Power Automate.

This is both a full featured mock and an example of how to use [Power Automate Mockup](https://github.com/thygesteffensen/PowerAutomateMockup),

This mock i build using [Power Automate Mockup](https://github.com/thygesteffensen/PowerAutomateMockup) as the flow engine and [XrmMockup](https://github.com/delegateas/XrmMockup) to mock the underlying Dynamics 365.

## How to use

### Introduction
This is a fully featured mock for the CDS ce connector and it works OOB if you're already using XrmMockup to test your Dynamics 365 plugins. If not, you can still use this, but you will also need to set up XrmMockup.

### Getting Started

First of all, replace your XrmMockup dependency with the development version developed to this project. The development version is build on the latest version of XrmMockup.

When configuring XrmMockup, add the following:
```c#
MockUpExtensions = new List<IMockUpExtension> {_pamuCds}
```

Somewhere before the XrmMockup configure step, do the following:
```c#
CommonDataServiceCurrentEnvironment _pamuCds;

// ...

var flowFolderPath = new Uri("<Path tp folder containing flows>");
_pamuCds = new CommonDataServiceCurrentEnvironment(flowFolderPath);
```

That's all. Now you can run your unit tests and the action executed in Power Automate flow will also be executed now, against your mock instance.

### Adding handles to other actions
Coming soon

### Asserting Actions
Coming soon

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

## Not supported

The focus right now is create a MVP to use in my bachelor project, this meaning not all functions will be implemented at the moment. I will later create a description of how to contribute to this project, but not before the assignment have been handed in.

### General
Every call against CDS returns a JSON object with headers and body. Headers will not be generated, as the MVP does not support the use cae.

The Body will be almost as the real deal, but with minor deviations. They are described below.


### Get a record

This action support 4 parameters
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

## License

© Thyge Skødt Steffensen
