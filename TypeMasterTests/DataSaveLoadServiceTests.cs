using System.Diagnostics;

namespace TypeMasterTests;

public class DataSaveLoadServiceTests
{
    DataSaveLoadService _dataSaveLoadService;
    [SetUp]
    public void Setup()
    {
        _dataSaveLoadService = new DataSaveLoadService(new CryptographyService());
    }

    [Test]
    public async Task GetOneByOneTest()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await foreach(var result in _dataSaveLoadService.GetDataCollectionAsync<WikipediaPageInfo>())
        {
            if(result != null)
                Console.WriteLine(result.Title);
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }
        Assert.Pass();
    }
}
