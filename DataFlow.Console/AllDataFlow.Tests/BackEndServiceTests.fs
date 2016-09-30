module BackEndServiceTests

open System
open DataFlowDemo
open TestDoubles
open Xunit
open FsUnit.Xunit
open BackEnd

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

    service.IdOfLastPostedData |> should equal None
    service.IdOfLastFetchedAnalytics |> should equal None

[<Fact>]
let ``When posting fetal data to uninitialized session the service is not contacted`` () =
    let service = new ServiceSpy()
    let sut = new BackEndService(new DataStore(), service)

    sut.PostDataAsync(new SessionIdentifier(), new Data("123456")) |> ignore

    service.IdOfLastPostedData |> should equal None

[<Fact>]
let ``Given an initialized session When posting fetal data Then the service is contacted`` () =
    let idLookup = new DataStore()
    let service = new ServiceSpy()
    let sut = new BackEndService(idLookup, service)

    let id = sut.InitializeAsync(42).Result

    sut.PostDataAsync(id, new Data("don't care")).Wait()

    service.IdOfLastPostedData |> should equal (Some (idLookup.TheData.[id]))

[<Fact>]
let ``Given an initialized session When posting fetal data Then the service receives the data`` () =
    let service = new ServiceSpy()
    let sut = new BackEndService(new DataStore(), service)

    let id = sut.InitializeAsync(39).Result

    let expected = new Data("this is test fetal data")
    sut.PostDataAsync(id, expected).Wait()

    service.LastPostedFetalData |> should equal (Some expected)

[<Fact>]
let ``Given an initialized session When fetching analytics Then the correct analytics are returned`` () =
    let service = new ServiceSpy()
    let sut = new BackEndService(new DataStore(), service)

    let id = sut.InitializeAsync(38).Result

    let expected = { Name = "Accel"; Timestamp = DateTimeOffset.Now; PeakValue = byte 127 }
    service.SetAnalyticsToReturn(expected)

    let actual = sut.FetchAnalyticsAsync(id).Result

    actual |> should equal expected

[<Fact>]
let ``Given an initialized session When fetching analytics Then the service is passed the id`` () =
    let service = new ServiceSpy()
    let idLookup = new DataStore()
    let sut = new BackEndService(idLookup, service)

    let id = sut.InitializeAsync(38).Result

    let actual = sut.FetchAnalyticsAsync(id).Result

    service.IdOfLastFetchedAnalytics |> should equal (Some (idLookup.TheData.[id]))

[<Fact>]
let ``Given no initialized sessions When deleting an unknown session Then xxx`` () =
    let idLookup = new DataStore()
    let service = new ServiceSpy()
    let sut = new BackEndService(idLookup, service)

    sut.DeleteSessionAsync(new SessionIdentifier()).Wait()

[<Fact>]
let ``Given an initialized session When deleting that session Then that session is removed`` () =
    ()
