using Pinecone;
using Pinecone.Grpc;
using Xunit;

namespace PineconeTests;

public class IndexTests
{
    [Fact]
    public async Task Create_index()
    {
        const string indexName = "test-index-1";

        using var pinecone = new PineconeClient(UserSecrets.Read("PineconeApiKey"), "gcp-starter");

        foreach (var existingIndexName in await pinecone.ListIndexes())
        {
            await pinecone.DeleteIndex(existingIndexName);
        }
        
        await pinecone.CreateIndex(indexName, 8, Metric.Euclidean);

        Index<GrpcTransport> index;
        do
        {
            await Task.Delay(10);
            index = await pinecone.GetIndex(indexName);
        } while (!index.Status.IsReady);
        
        
        
        await pinecone.DeleteIndex(indexName);
        // var status = index.Status;
        // Assert.True(status.IsReady);
    }
}