using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using WindowsAzure.Table.Attributes;

namespace Nimbus.Web.Utils
{
    public static class NotificationTableEntity
    {
        //http://blog.liamcavanagh.com/2011/11/how-to-sort-azure-table-store-results-chronologically/
        /// <summary>
        /// Gera PartitionKey a partir de userid e data
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GeneratePartitionKey(int userId, DateTime date)
        {
            //PERIGO: o ano máximo é 9999 e o mês máximo é 99 (para garantir caso aumente o número de meses do ano)
            return string.Format("{0:D10}:{1:D4}{2:D2}", userId, 9999 - date.Year, 99 - date.Month);
        }

        public static string GenerateMaxPartitionKey(int userId)
        {
            return string.Format("{0:D10}:{1:D4}{2:D2}", userId, 9999, 99);
        }


        /// <summary>
        /// Gera RowKey a partir de data
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GenerateRowKey(DateTime date)
        {
            long howMuchTimeTillTheEnd = long.MaxValue - date.ToFileTimeUtc();
            return string.Format("{0:D16}", howMuchTimeTillTheEnd / 10000); //em milissegundos
        }
    }
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
            this.PartitionKey = NotificationTableEntity.GeneratePartitionKey(userId, date);
            this.RowKey = NotificationTableEntity.GenerateRowKey(date);
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