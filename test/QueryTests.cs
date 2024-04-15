using Pinecone;
using Pinecone.Grpc;
using Xunit;

namespace PineconeTests;

public class QueryTests(QueryTests.QueryFixture fixture) : IClassFixture<QueryTests.QueryFixture>
{
    private const string IndexName = "test-index-1";

    private QueryFixture Fixture { get; } = fixture;

    [Fact]
    public async Task Create_index()
    {
        var vectors = Enumerable.Range(1, 10).Select(i => new Vector
        {
            Id = Guid.NewGuid().ToString(),
            Values = [i * 0.1f, i * 0.2f, i * 0.3f, i * 0.4f, i * 0.5f, i * 0.6f, i * 0.7f, i * 0.8f],
        });
        
        await Fixture.Index.Upsert(vectors);
        
        var x = 0.314f;
        var results =
            await Fixture.Index.Query(
                [x * 0.1f, x * 0.2f, x * 0.3f, x * 0.4f, x * 0.5f, x * 0.6f, x * 0.7f, x * 0.8f],
                1);
        
        Assert.Equal(8, results.Length);
    }

    public class QueryFixture : IAsyncLifetime
    {
        public PineconeClient Pinecone { get; private set; } = null!;
        public Index<GrpcTransport> Index { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Pinecone = new PineconeClient(UserSecrets.Read("PineconeApiKey"), "gcp-starter");

            await ClearIndexes();
            await CreateAndWaitForIndex();
        }

        private async Task CreateAndWaitForIndex()
        {
            await Pinecone.CreateIndex(IndexName, 8, Metric.Euclidean);
            do
            {
                await Task.Delay(10);
                Index = await Pinecone.GetIndex(IndexName);
            } while (!Index.Status.IsReady);
        }

        public async Task DisposeAsync()
        {
            await ClearIndexes();
            Pinecone.Dispose();
        }

        private async Task ClearIndexes()
        {
            foreach (var existingIndexName in await Pinecone.ListIndexes())
            {
                await Pinecone.DeleteIndex(existingIndexName);
            }
        }
    }
}