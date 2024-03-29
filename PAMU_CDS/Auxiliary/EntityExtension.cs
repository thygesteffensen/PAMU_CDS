﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Parser.ExpressionParser;

namespace PAMU_CDS.Auxiliary
{
    public static class EntityExtension
    {
        public static Entity CreateEntityFromParameters(this Entity entity,
            ValueContainer parameters)
        {
            // Dynamics API uses plural names for entites/tables, which isn't the name used as logical names...
            var entityName = parameters["entityName"].GetValue<string>();
            entity.LogicalName = entityName.Substring(0, entityName.Length - 1);

            var parametersDict = parameters.GetValue<Dictionary<string, ValueContainer>>();

            if (parametersDict.TryGetValue("recordId", out var recordId))
            {
                entity.Id = new Guid(recordId.GetValue<string>());
            }

            if (parametersDict.TryGetValue("item", out var items))
            {
                foreach (KeyValuePair<string, ValueContainer> keyValuePair in items
                    .GetValue<Dictionary<string, ValueContainer>>())
                {
                    if (keyValuePair.Key.Contains("@odata.bind"))
                    {
                        entity.AddLookupAttribute(keyValuePair.Key, keyValuePair.Value);
                        continue;
                    }

                    if (keyValuePair.Key == "statecode" || keyValuePair.Key == "statuscode")
                    {
                        var value = keyValuePair.Value.GetValue<int>();
                        entity.Attributes[keyValuePair.Key] = new OptionSetValue(value);
                        continue;
                    }

                    // TODO: Figure out how to determine which value is expected.
                    entity.Attributes[keyValuePair.Key] = keyValuePair.Value.GetValue<object>();
                }
            }

            return entity;
        }

        private static void AddLookupAttribute(
            this Entity entity, 
            string key, 
            ValueContainer valueContainer)
        {
            var attribute = key.Split('_').FirstOrDefault();
            if (attribute == null) throw new Exception($"Unable to parse attribute from key {key}");

            var valueRegex = Regex.Match(valueContainer.GetValue<string>(), "([^(]*)\\(([^)]*)\\)");
            if (!valueRegex.Success) throw new Exception("Invalid format for lookup, expected something like accounts(030cae85-afa4-4cd2-96a8-a18b6301a6fa)");
            var logicalPlural = valueRegex.Groups[1].Value;
            if (!Guid.TryParse(valueRegex.Groups[2].Value, out Guid id)) throw new Exception($"Unable to parse id from {valueRegex.Groups[2].Value}");

            entity.Attributes.Add(
                attribute, 
                new EntityReference(
                    logicalPlural.Substring(0,logicalPlural.Length-1),
                    id
                )
            );
        }

        public static ValueContainer ToValueContainer(this Entity entity)
        {
            var triggerOutputs = new Dictionary<string, ValueContainer>();
            foreach (var keyValuePair in entity.Attributes)
            {
                AddObjectToValueContainer(triggerOutputs, keyValuePair);

                triggerOutputs["@odata.type"] = new ValueContainer($"#Microsoft.Dynamics.CRM.{entity.LogicalName}");
            }

            return new ValueContainer(new Dictionary<string, ValueContainer>
                {{"body", new ValueContainer(triggerOutputs)}});
        }

        private static void AddObjectToValueContainer(this IDictionary<string, ValueContainer> dict,
            KeyValuePair<string, object> kvp)
        {
            switch (kvp.Value)
            {
                case EntityReference entityReference:
                    // This is the primary column of the related entity's value
                    // dict[$"_{kvp.Key}_value@OData.Community.Display.V1.FormattedValue"] = new ValueContainer(entityReference.Id.ToString());
                    dict[$"_{kvp.Key}_value@Microsoft.Dynamics.CRM.associatednavigationproperty"] =
                        new ValueContainer(entityReference.Id.ToString());
                    dict[$"_{kvp.Key}_value@Microsoft.Dynamics.CRM.lookuplogicalname"] =
                        new ValueContainer(entityReference.LogicalName);
                    dict[$"_{kvp.Key}_value@odata.type"] = new ValueContainer("#Guid");
                    dict[$"_{kvp.Key}_value"] = new ValueContainer(entityReference.Id.ToString());
                    break;
                case string s:
                    dict[kvp.Key] = new ValueContainer(s);
                    break;
                case int i:
                    dict[kvp.Key] = new ValueContainer(i);
                    break;
                case double d:
                    dict[kvp.Key] = new ValueContainer((float) d);
                    break;
                case decimal d:
                    dict[kvp.Key] = new ValueContainer((float) d);
                    break;
                case Guid guid:
                    dict[$"{kvp.Key}@odata.type"] = new ValueContainer("#Guid");
                    dict[kvp.Key] = new ValueContainer(guid.ToString());
                    break;
                case OptionSetValue optionSetValue:
                    dict[$"{kvp.Key}@OData.Community.Display.V1.FormattedValue"] =
                        new ValueContainer(
                            "<label not yet present>"); // The options set value. We'll need the context...
                    dict[kvp.Key] = new ValueContainer(optionSetValue.Value);
                    break;
                case null:
                    break;
                case bool b:
                    dict[kvp.Key] = new ValueContainer(b);
                    break;
                // TODO: Figure out how Mockup handles Date and Datetime offset? see birthdate and modifiedon for reference
                case DateTime dateTime:
                    dict[$"{kvp.Key}"] =
                        dict[$"{kvp.Key}@odata.type"] = new ValueContainer("#Date");
                    dict[kvp.Key] = new ValueContainer(dateTime.ToString());
                    break;
                case Money money:
                    dict[kvp.Key] = new ValueContainer((float) money.Value);
                    break;
                default:
                    throw new NotImplementedException();
                // throw new ArgumentOutOfRangeException();
            }
        }
    }
}