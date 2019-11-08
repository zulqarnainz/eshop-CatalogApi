using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CatalogAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace CatalogAPI.Helpers
{
    public class StorageAccountHelper
    {
        private CloudStorageAccount storageAccount;

        private CloudStorageAccount tableStorageAccount;

        // used for connecting to blob storage service
        private CloudBlobClient blobClient;

        public CloudTableClient tableClient;


        private string storageConnectionStrings;

        public string StorageConnectionStrings
        {
            get { return this.storageConnectionStrings; }
            set {

                this.storageConnectionStrings = value;
                this.storageAccount = CloudStorageAccount.Parse(this.storageConnectionStrings);
            }
        }

        private string tableConnectionStrings;

        public string TableConnectionStrings
        {
            get { return this.tableConnectionStrings; }
            set
            {

                this.tableConnectionStrings = value;
                this.tableStorageAccount = CloudStorageAccount.Parse(this.tableConnectionStrings);
            }
        }


        // methods

        public async Task<string> UploadFileToBlobAsync(string filePath, string ContainerName)
        {
            blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(ContainerName);

            await container.CreateIfNotExistsAsync();

            BlobContainerPermissions permissions = new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Container
            };
            await container.SetPermissionsAsync(permissions);

            

            var fileName = Path.GetFileName(filePath);
            var blob = container.GetBlockBlobReference(fileName);

            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(filePath);

            return blob.Uri.AbsoluteUri;

        }

        public async Task<CatalogEntity> SaveTotTableAsync(CatalogItem item)
        {
            CatalogEntity catalogEntity = new CatalogEntity(item.Name, item.Id)
            {
                ImageUrl = item.ImageUrl,
                ReorderLevel = item.ReorderLevel,
                Quantity = item.Quantity,
                Price = item.Price,
                ManufacturingDate = item.ManufacturingDate
            };

            //tableClient = storageAccount.CreateCloudTableClient();
            tableClient = tableStorageAccount.CreateCloudTableClient();
            var catalogTable = tableClient.GetTableReference("catalog");
            await catalogTable.CreateIfNotExistsAsync();

            TableOperation operation = TableOperation.InsertOrMerge(catalogEntity);

            var tableResult = await catalogTable.ExecuteAsync(operation);

            return tableResult.Result as CatalogEntity;
        }







    }
}
