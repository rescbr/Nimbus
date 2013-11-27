using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Nimbus.Web.Notifications;
using Nimbus.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Nimbus.Web.API.Controllers
{
    ///Abreviações de tipo
    using MessageNotificationQuery = TableQuery<NotificationTableEntity<Notifications.MessageNotificationModel>>;


    [NimbusAuthorize]
    public class NotificationController : NimbusApiController
    {
        [HttpGet]
        public List<MessageNotificationModel> GetMessages()
        {
            DateTime now = DateTime.UtcNow;
            string nowPartitionKey = NotificationTableEntity.GeneratePartitionKey(NimbusUser.UserId, now);
            string endOfTimesPK = NotificationTableEntity.GenerateMaxPartitionKey(NimbusUser.UserId);
            //string nowRowKey = NotificationTableEntity.GenerateRowKey(now);

            CloudStorageAccount _storageAccount = CloudStorageAccount.Parse(NimbusConfig.StorageAccount);
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Const.Azure.MessageNotificationsTable);

            //MessageNotificationQuery query = new MessageNotificationQuery().Where(
            //    TableQuery.CombineFilters(
            //    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userPartitionKey),
            //    TableOperators.And,
            //    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, nowRowKey)
            //    )).Take(15);

            MessageNotificationQuery query = new MessageNotificationQuery().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, nowPartitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, endOfTimesPK)
                )).Take(15);


            var results = table.ExecuteQuery(query).ToList();

            return results.Select(r => r.DataObject).ToList();

        }
    }
}