# ReadMe

## odata stuff

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

