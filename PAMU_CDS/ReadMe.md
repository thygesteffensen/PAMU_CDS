# ReadMe

## odata stuff

```
$expand=something($select=prop1,prop2),something2($select=prop1,prop2;$filter=prop func)
```

````xml
<intput>        ::= $<string>= <values>                 <!-- $expand=something($select=prop1,prop2) -->
<values>        ::= <value> *(,<value>)                 <!-- something($select=prop1,prop2),something2($select=prop1,prop2;$filter=prop func) -->

<value>         ::= <string> *(<parameters>)            <!-- something2($select=prop1,prop2;$filter=prop func) -->

<parameters>    ::= '('<parameter> *(;<paramters>)')'   <!-- ($select=prop1,prop2;$filter=prop func) -->
<paramter>      ::= $<string>=<properties>              <!-- $select=prop1,prop2 -->

<properties>    ::= <string> *(,<string>)               <!-- prop1,prop2 --> 
````