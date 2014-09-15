﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage.Blob;
using Web.Storage.Connection;
using Web.Storage.Manager;

namespace Web.Storage.Topic
{
    [BlobStorage("topic-thumbnails")]
    public class TopicThumbnailManager:AzureBlobManagerBase
    {
        public TopicThumbnailManager(BlobStorageConnection connection) : base(connection)
        {
        }

        public async Task<GetTopicImageResult> getTopicImage(string topicImage)
        {
            CloudBlockBlob blobRef = Container.GetBlockBlobReference(topicImage);
            using (MemoryStream ms=new MemoryStream())
            {
                await blobRef.DownloadToStreamAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return new GetTopicImageResult(blobRef.Properties.ContentType, ms);
            }
        }

        public class GetTopicImageResult
        {
            public GetTopicImageResult(string contentType, Stream contentBody)
            {
                ContentType = contentType;
                ContentBody = contentBody;
            }

            public string ContentType { get; set; }

            public Stream ContentBody { get; set; }
        }
    }


}