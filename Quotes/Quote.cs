using System;
using System.Collections.Generic;
using Demo.QuoteRequestWorkflow.Master;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Demo.QuoteRequestWorkflow.Quotes
{
    public class Quote : TableEntity
    {
        [IgnoreProperty]
        public string Id
        {
            get => RowKey;
            set
            {
                RowKey = value;
                ConfigureKeys();
            }
        }

        private void ConfigureKeys()
        {
            if (string.IsNullOrWhiteSpace(RowKey))
            {
                throw new Exception($"A Quote required an Id.");
            }
            PartitionKey = RowKey.Substring(0, 1).ToUpperInvariant();
        }

        public string Description { get; set; }

        [EntityEnumPropertyConverter]
        public Status Status { get; set; }
        public DateTime DueDate { get; set; }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            EntityEnumPropertyConverter.Serialize(this, results);
            return results;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            EntityEnumPropertyConverter.Deserialize(this, properties);
        }
    }
}
