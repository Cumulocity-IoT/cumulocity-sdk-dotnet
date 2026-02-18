using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Converter;
using Client.Com.Cumulocity.Client.Model;
using Client.Com.Cumulocity.Client.Supplementary;

using Moq;

namespace C8yServices.Inventory;

public class InventoryApiHelperTests
{
  private readonly Mock<IManagedObjectsApi> _managedObjectsApiMock;
  private readonly Mock<IChildOperationsApi> _childOperationsApiMock = new();
  private readonly Mock<ICumulocityCoreLibrary> _cumulocityCoreLibraryMock = new();

  public InventoryApiHelperTests()
  {

    var agentInfoCollection = new ManagedObjectCollection<TestManagedObject> { ManagedObjects = new() };
    for (var i = 0; i < 5; i++)
    {
      agentInfoCollection.ManagedObjects.Add(new TestManagedObject(i));
    }
    _managedObjectsApiMock = new Mock<IManagedObjectsApi>();
    _managedObjectsApiMock.Setup(GetManagedObjectsExpression<TestManagedObject>(TestManagedObject.ObjectType, 1, 5))
      .Returns(Task.FromResult<ManagedObjectCollection<TestManagedObject>?>(agentInfoCollection));
    _managedObjectsApiMock.Setup(GetManagedObjectsExpression<TestManagedObject>(TestManagedObject.ObjectType, 2, 5))
      .Returns(Task.FromResult<ManagedObjectCollection<TestManagedObject>?>(agentInfoCollection));
    _managedObjectsApiMock.Setup(GetManagedObjectsExpression<TestManagedObject>(TestManagedObject.ObjectType, 3, 5))
      .Returns(Task.FromResult<ManagedObjectCollection<TestManagedObject>?>(new ManagedObjectCollection<TestManagedObject> { ManagedObjects = new() }));
    _managedObjectsApiMock.Setup(GetManagedObjectsExpression<TestManagedObject>(null, 1, InventoryApiHelper.DefaultPageSize, "testQuery"))
      .Returns(Task.FromResult<ManagedObjectCollection<TestManagedObject>?>(new ManagedObjectCollection<TestManagedObject> { ManagedObjects = new List<TestManagedObject> { new() } }));
    _cumulocityCoreLibraryMock.Setup(x => x.Inventory.ManagedObjectsApi).Returns(_managedObjectsApiMock.Object);
    _cumulocityCoreLibraryMock.Setup(x => x.Inventory.ChildOperationsApi).Returns(_childOperationsApiMock.Object);
  }

  [Fact(DisplayName = "test 'RequestAllAsync' without request params")]
  public async Task RequestAllAsyncNoRequestParams()
  {
    var result = await InventoryApiHelper.RequestAllAsync<TestManagedObjectNoRequestParams>(_cumulocityCoreLibraryMock.Object);
    Assert.True(result.IsT1);
  }

  [Fact(DisplayName = "test 'RequestAllAsync' without managed objects API exception")]
  public async Task TestRequestAllAsyncApiException()
  {
    _managedObjectsApiMock.Reset();
    _managedObjectsApiMock.Setup(GetManagedObjectsExpression<ManagedObject>(type: TestManagedObject.ObjectType, currentPage: 1, pageSize: InventoryApiHelper.DefaultPageSize))
      .Throws<Exception>();

    var result = await InventoryApiHelper.RequestAllAsync<TestManagedObject>(_cumulocityCoreLibraryMock.Object);
    Assert.True(result.IsT1);
    _managedObjectsApiMock.Verify(GetManagedObjectsExpression<ManagedObject>(TestManagedObject.ObjectType, 1, InventoryApiHelper.DefaultPageSize), Times.Once);
  }

  [Fact(DisplayName = "test 'RequestAllAsync' with page size 5")]
  public async Task RequestAllAsync()
  {
    var result = await InventoryApiHelper.RequestAllAsync<TestManagedObject>(_cumulocityCoreLibraryMock.Object, 5);

    Assert.True(result.IsT0);
    Assert.Equal(10, result.AsT0.Count);
    _managedObjectsApiMock.Verify(GetManagedObjectsExpression<ManagedObject>(TestManagedObject.ObjectType, 1, 5), Times.Once);
    _managedObjectsApiMock.Verify(GetManagedObjectsExpression<ManagedObject>(TestManagedObject.ObjectType, 2, 5), Times.Once);
    _managedObjectsApiMock.Verify(GetManagedObjectsExpression<ManagedObject>(TestManagedObject.ObjectType, 3, 5), Times.Once);
  }

  [Fact(DisplayName = "Valid request single result")]
  public async Task RequestPageByQueryAsyncValidRequestSingleResult()
  {
    var result = await InventoryApiHelper.RequestPageByQueryAsync<TestManagedObject>(_cumulocityCoreLibraryMock.Object, "testQuery", 1);
    Assert.True(result.IsT0);
    Assert.Single(result.AsT0);
  }

  [Fact(DisplayName = "Valid request empty result")]
  public async Task RequestPageByQueryAsyncValidRequestUnknownQueryEmptyResult()
  {
    var result = await InventoryApiHelper.RequestPageByQueryAsync<TestManagedObject>(_cumulocityCoreLibraryMock.Object, "testQueryUnknown", 1);
    Assert.True(result.IsT0);
    Assert.Empty(result.AsT0);
  }

  [Fact(DisplayName = "Empty query request")]
  public async Task RequestPageByQueryAsyncEmptyQueryResult()
  {
    var result = await InventoryApiHelper.RequestPageByQueryAsync<TestManagedObject>(_cumulocityCoreLibraryMock.Object, "", 1);

    Assert.True(result.IsT1);
  }

  private static Expression<Func<IManagedObjectsApi, Task<ManagedObjectCollection<T>?>>> GetManagedObjectsExpression<T>(string? type = null, int? currentPage = null,
    int? pageSize = null, string? query = null, bool? withChildren = false, bool? withParents = false) where T : ManagedObject
  {
    return x => x.GetManagedObjects<T>(null, null, null, currentPage, null, null, null, null, pageSize, null, query, null, null, type, withChildren, null, null, withParents, null, null, null, It.IsAny<CancellationToken>());
  }

  [JsonConverter(typeof(ManagedObjectJsonConverter<TestManagedObjectNoRequestParams>))]
  public class TestManagedObjectNoRequestParams : QueryableManagedObject;

  [JsonConverter(typeof(ManagedObjectJsonConverter<TestManagedObject>))]
  public class TestManagedObject : QueryableManagedObject
  {
    public const string ObjectType = "testType";

    [JsonIgnore]
    public new static Dictionary<RequestParameterType, string> RequestParameters => new()
    {
      { RequestParameterType.Type, ObjectType }
    };

    // required for serialization
    public TestManagedObject() { }

    public TestManagedObject(int id)
    {
      Id = id.ToString(CultureInfo.InvariantCulture);
      Type = ObjectType;
    }
  }
}