module BackEnd.Tests

open Xunit
open BackEnd

[<Fact>]
let ``When calling initialize the data store is initialized`` () =
    let dataStore = new BackEnd.DataStore()
    let sut = new BackEndService(dataStore)

    sut <>! null
