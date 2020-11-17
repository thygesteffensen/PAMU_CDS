﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Parser.ExpressionParser;

namespace PAMU_CDS.Auxiliary
{
    public static class EntityExtension
    {
        public static Entity CreateEntityFromParameters(this Entity entity,
            Dictionary<string, ValueContainer> parameters)
        {
            entity.LogicalName = parameters["entityName"].GetValue<string>();

            foreach (KeyValuePair<string, ValueContainer> keyValuePair in parameters.Where(kv =>
                kv.Key.StartsWith("item")))
            {
                // TODO: Figure out how to determine which value is expected.
                entity.Attributes[keyValuePair.Key.Replace("item/", "")] = keyValuePair.Value.GetValue<string>();
            }

            return entity;
        }

        public static ValueContainer ToValueContainer(this Entity entity)
        {
            var triggerOutputs = new Dictionary<string, ValueContainer>();
            foreach (var keyValuePair in entity.Attributes)
            {
                // TODO: Verify this works!
                // TODO: Look into the processed fields, starting with '_'
                triggerOutputs[keyValuePair.Key] = new ValueContainer(keyValuePair.Value.ToString(), true);
            }

            return new ValueContainer(triggerOutputs);
        }
    }
}