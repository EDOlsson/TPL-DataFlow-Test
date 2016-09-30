module BackEnd.Tests

open BackEnd
open DataFlowDemo
open Xunit
open FsUnit.Xunit

type ServiceSpy() =
    let mutable _lastPostedSessionId : SessionIdentifier option = None
    let mutable _lastFetchedSessionId : SessionIdentifier option = None

    interface IFakeSpectraService with
        member this.PostFetalData(id : SessionIdentifier, d : Data) =
            _lastPostedSessionId <- Some id

        member this.FetchAnalytics(id : SessionIdentifier) =
            _lastFetchedSessionId <- Some id
            new System.Object()
        
    member this.GetIdOfLastPostedData () =
        _lastPostedSessionId

    member this.GetIdOfLastFetchedAnalytics () =
        _lastFetchedSessionId

[<Fact>]
let ``When calling initialize the data store is initialized`` () =
    let dataStore = new DataStore()
    let serviceSpy = new ServiceSpy()

    let sut = new BackEndService(dataStore, serviceSpy)

    let createdId = sut.InitializeAsync(42).Result

    dataStore.TheData.Keys |> should contain createdId

[<Fact>]
let ``When initializing the service is not contacted`` () =
    let service = new ServiceSpy()
    let sut = new BackEndService(new DataStore(), service)

    sut.InitializeAsync(42).Wait()

    service.GetIdOfLastPostedData() |> should equal None
    service.GetIdOfLastFetchedAnalytics() |> should equal None

[<Fact>]
let ``When posting fetal data to uninitialized session the service is not contacted`` () =
    let service = new ServiceSpy()
    let sut = new BackEndService(new DataStore(), service)

    sut.PostDataAsync(new SessionIdentifier(), new Data("123456")) |> ignore

    service.GetIdOfLastPostedData() |> should equal None

[<Fact>]
let ``Given an initialized session when posting fetal data the service is contacted`` () =
    let service = new ServiceSpy()
    let sut = new BackEndService(new DataStore(), service)

    let id = sut.InitializeAsync(42).Result

    sut.PostDataAsync(id, new Data("don't care")).Wait()

    service.GetIdOfLastPostedData() |> should equal (Some id)