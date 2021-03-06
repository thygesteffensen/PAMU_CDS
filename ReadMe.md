<h1 align="center">Common Data Service (current environment) mock</h1>
<h2 align="center"><a href="https://github.com/thygesteffensen/PowerAutomateMockUp">Power Automate Mock-Up</a></h2>
<h3 align="center">Battery included mock for Power Automate CDS connector. Using <a href="https://github.com/thygesteffensen/PowerAutomateMockUp">Power Automate Mock-Up</a> as the flow engine and <a href="http://github.com/delegateas/XrmMockup">XrmMockup</a> as Dynamics engine.</h3>
<p align="center">
        <img alt="Build status" src="https://img.shields.io/github/workflow/status/thygesteffensen/PAMU_CDS/Release/dev">
    <a href="https://www.nuget.org/packages/PAMU_CDSce/">
        <img alt="Nuget downloads" src="https://img.shields.io/nuget/dt/PAMU_CDSce">
    </a>
    <a href="https://www.nuget.org/packages/PAMU_CDSce/">
        <img alt="Nuget version" src="https://img.shields.io/nuget/v/PAMU_CDSce">
    </a>
    <a href="https://www.nuget.org/packages/PAMU_CDSce/">
        <img alt="Nuget prerelease version" src="https://img.shields.io/nuget/vpre/PAMU_CDSce">
    </a>
</p>

This is a full featured mock for the [Common Date Service (current environment)](https://docs.microsoft.com/en-us/connectors/commondataserviceforapps/) connector for Power Automate.

The mock is built using [Power Automate Mock-Up](https://github.com/thygesteffensen/PowerAutomateMockup) as the flow engine and [XrmMockup](https://github.com/delegateas/XrmMockup) to mock the underlying Dynamics 365 instance.

## Getting Started

First of all, upgrade your XrmMockup with version [1.7.1](https://www.nuget.org/packages/XrmMockup365/) or higher.

When configuring XrmMockup, add the following to the `XrmMockupSettings` (Only supported in XrmMockup365):
```c#
MockUpExtensions = new List<IMockUpExtension> {_pamuCds}
```

Somewhere before the XrmMockup configure step, do the following to setup Power Automate Mock-Up and add PAMU_CDS to the service collection:
```c#
var services = new ServiceCollection();
services.AddFlowRunner();
services.AddPamuCds();

var sp = services.BuildServiceProvider();

_pamuCds = sp.GetRequiredService<XrmMockupCdsTrigger>();
_pamuCds.AddFlows(new Uri(System.IO.Path.GetFullPath(@"Workflows")));
```

That's all. The flows in the folder will be executed like they would on the server and the actions will be triggered from XrmMockup.

Now you can run your unit tests and the action executed in Power Automate flow will also be executed now, against your mock instance.

### Download flows
One way to get the flows, is to export the soltuion containing the flows, then unzip and extract the flows to the desired location.

If you are using [XrmFramework](https://github.com/delegateas/XrmFramework) or if you're using [Daxif](https://github.com/delegateas/Daxif), you can execute the `F#` script availible [here](https://github.com/thygesteffensen/FlowUnitTester/blob/main/DG/DG.FlowUnitTester/Tools/Daxif/DownloadWorkflows.fsx).

Depending on the location of the flows, they might have to be included in the `.csproj`. If the flows are placed in a directory inside the test project, in a folder named `flows`, simply add the following `ItemGroup`, to copy the flows when building:

```xml
<ItemGroup>
    <Content Include="flows\**\*.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

### Configuring
PAMU_CDS can be configured to behave in a certain way. PAMU can also be configured, see [here](https://github.com/thygesteffensen/PowerAutomateMockUp#configuration)

#### Do not execute flows
Add the name of the flow description file, to ignore the flow when triggering from flow from XrmMockup.

```cs
services.Configure<CdsFlowSettings>(x => 
    x.DontExecuteFlows = new[] {"<flow description file name>.json"});
```

## Actions

The focus right now is to create an MVP to use in my bachelor project, thus not all functions will be implemented for the moment. I will later create a description of how to contribute to this project, but not before the assignment have been handed in.

|    | **Action**|
|----|----------|
| ✔ | [Create a new record](#create-a-new-record-)
| ✔ | [Delete a record](#delete-a-record-)
| ❗ | [Execute a changeset request](#executes-a-changeset-request-)
| ❗ | [Get a record](#get-a-record-)
| ❌ | [Get file or image content](#get-file-or-image-content-)
| ❗ | [List records](#list-records-)
| ❌ | [Perform a bound action](#perform-a-bound-action-)
| ❌ | [Perform a unbound action](#perform-an-unbound-action-)
| ✔ | [Releate records](#relate-records-)
| ✔ | [Unrelate records](#unrelate-records-)
| ✔ | [Update a record](#update-record-)
| ❌ | [Upload file or image content](#upload-file-or-image-content-)

### Unsupported actions
As with [PAMU](https://github.com/thygesteffensen/PowerAutomateMockup), you can add actions using one of the three extension methods
```cs
services.AddFlowActionByName<GetMsnWeather>("Get_forecast_for_today_(Metric)");
services.AddFlowActionByApiIdAndOperationsName<Notification>(
    "/providers/Microsoft.PowerApps/apis/shared_flowpush", 
    new []{ "SendEmailNotification", "SendNotification" });
services.AddFlowActionByFlowType<IfActionExecutor>("If");
```

A more detialed guide is availible at [PAMU#Actions](https://github.com/thygesteffensen/PowerAutomateMockUp/tree/dev#adding-actions).

### General
Every call against CDS returns a JSON object with headers and body. Headers will not be generated, as the MVP does not support the use case.

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

### List records ❗
Currently the list records in converted to a QueryExpression, but it should be converted to a FetchXML instead, since FetchXML cover more of the features of Odata than QueryExpression.

|   | **Name**        | **Key**    | **Required** | **Type** | **Description**                                                                                                |
|---|-----------------|------------|--------------|----------|----------------------------------------------------------------------------------------------------------------|
| ✔ | Entity name     | entityName | True         | string   | Choose an option or add your own                                                                               |
| ✔ | Select Query    | $select    |              | string   | Limit the properties returned while retrieving data.                                                           |
| ❗ | Filter Query    | $filter    |              | string   | An ODATA filter query to restrict the entries returned (e.g. stringColumn eq 'string' OR numberColumn lt 123). |
| ❗ | Order By        | $orderby   |              | string   | An ODATA orderBy query for specifying the order of entries.                                                    |
| ❌ | Expand Query    | $expand    |              | string   | Related entries to include with requested entries (default = none).                                            |
| ✔ | Fetch Xml Query | fetchXml   |              | string   | Fetch Xml query                                                                                                |
| ✔ | Top Count       | $top       |              | integer  | Total number of entries to retrieve (default = all).                                                           |
| ❗ | Skip token      | $skiptoken |              | string   | The skip token.                                                                                                |

Skip token, as well as Odata.nextLink on response will not be implemented.

Fetch Xml Expression will simple be a `FetchExpression` instead of a QueryExpression. The correctness of `FetchExpression` will depend on XrmMockup.


#### Filter Query
The filter query is made in OData in Power Automate. I have written a small Odata parser, which parses the Odata query to a FilterExpression, but not to a full extend.

Every Condition Operator, i.e. eq, ne, lt, is supported for strings, integers, decimals, booleans and null.
The functions Startswith, Endswith and substringof is supported, the others are not supported, as they cannot be easily mapped to a FilterExpression. 

The CFG in EBNF for the parser is:
```xml
or ::= and ('or' or)+
and ::= stm ('and' and)+
stm ::= func | '(' or ')' | attr op val
val ::= func | const
op ::= eq ne ...
attr ::= string
func ::= string '(' params ')'
```

#### Order By
Order By is not working as in Power Automate, simply because the QueryExpression does not support ordering of linked entities. 

#### Expand Query
Is not supported at the moment.

#### Skip token
Not supported.

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

## Trigger

XrmMockupCdsTrigger is a class to trigger a set of flows, based on the Request made to Dynamics. XrmMockupCdsTrigger is built to be used togehter with XrmMockup.

The trigger will apply a filters from the trigger, but only with limited functionality for now.

## Code style
The code is written using [Riders](https://www.jetbrains.com/help/rider/Settings_Code_Style_CSHARP.html) default C# code style.

Commits are written in [conventional commit](https://www.conventionalcommits.org/en/v1.0.0/) style, the commit messages are used to determine the version and when to release a new version. The pipeline is hosted on Github and [Semantic Release](https://github.com/semantic-release/semantic-release) is used.

## Installation

Currently the project is still in alpha. To find the packages at nuget.com, you have to check 'Prerelease', before the nuget appears.

You also need [XrmMockup](https://github.com/delegateas/XrmMockup) to get the full functionality togehter with Dynamics 365 customizations. If you don't use XrmMockup or don't want to, you can provide a mock of IOrganizationService and add it to the service collection.

## Tests

Tests are located in the **Tests** project and they are written using Nunit as test framework.

## Contribute

This is my bachelor project and I'm currently not accepting contributions until it have been handed in. Anyway, fell free to drop an issue with a suggestion or improvement.

## Credits
Delegate A/S and the team behind XrmMockup.


## License

MIT
