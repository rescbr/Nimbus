using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using WindowsAzure.Table.Attributes;

namespace Nimbus.Web.Utils
{
    public class NotificationTableEntity<T> : TableEntity
    {
        public NotificationTableEntity() { }

        public NotificationTableEntity(int userId)
        {
            var now = DateTime.UtcNow;
            SetEntityPartitionRowKey(userId, now);
        }

        public NotificationTableEntity(T obj, int userId)
        {
            var now = DateTime.UtcNow;
            SetEntityPartitionRowKey(userId, now);
            DataObject = obj;
        }

        public NotificationTableEntity(int userId, DateTime date)
        {
            SetEntityPartitionRowKey(userId, date);
        }

        public NotificationTableEntity(T obj, int userId, DateTime date)
        {
            SetEntityPartitionRowKey(userId, date);
            DataObject = obj;
        }

        internal void SetEntityPartitionRowKey(int userId, DateTime date)
        {
            long howMuchTimeTillTheEnd = long.MaxValue - date.ToFileTimeUtc();
            this.PartitionKey = string.Format("{0:D10}:{1:D4}{2:D2}", userId, date.Year, date.Month);
            this.RowKey = string.Format("{0:D16}", howMuchTimeTillTheEnd / 10000); //em milissegundos
        }

        [Ignore]
        public T DataObject
        {
            get
            {
                T deserialized = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(SerializedData);
                return deserialized;
            }
            set
            {
                string serializedData = ServiceStack.Text.JsonSerializer.SerializeToString<T>(value);
                SerializedData = serializedData;
            }
        }

        public string SerializedData { get; set; }
    }
}