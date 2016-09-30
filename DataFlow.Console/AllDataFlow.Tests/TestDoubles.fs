module TestDoubles

open System
open BackEnd
open DataFlowDemo

type ServiceSpy() =
    let mutable _lastPostedSessionId : BedId option = None
    let mutable _lastFetchedSessionId : BedId option = None
    let mutable _lastPostedFetalData : Data option = None
    let mutable _analyticsResults = new Object()

    interface ISpectraService with
        member this.PostFetalData(id : BedId, d : Data) =
            _lastPostedSessionId <- Some id
            _lastPostedFetalData <- Some d

        member this.FetchAnalytics(id : BedId) =
            _lastFetchedSessionId <- Some id
            _analyticsResults
        
    member this.IdOfLastPostedData       with get() = _lastPostedSessionId

    member this.IdOfLastFetchedAnalytics with get() = _lastFetchedSessionId

    member this.LastPostedFetalData      with get() = _lastPostedFetalData

    member this.SetAnalyticsToReturn(a) = _analyticsResults <- a
        
type DummyAnalytics = { Name : string; Timestamp : DateTimeOffset; PeakValue : byte }
