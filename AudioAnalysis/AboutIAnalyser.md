#IAnalyser
---

#Introduction
`IAnalyser` is a C# interface designed to allow for the generic execution of analysis code.

## Implementers
Implementers of `IAnalyser` must implement the methods of the interface. 

The `Analyse` method in particular must support analysing a piece of an input piece of audio (not necessarilly a one minute chunk).

### `Analyse` results
The result object returned by the `Analyse` method supports returning:
 
 - An `IList<T>` of events
	 - events are _acoustic events_ from the audio
	 - each event is one object instance with properties
 - An `Ilist<T>` of summary indices
	 - _summary indices_ are a set of features that describe an audio segment
	 - each set of summary indices are one object instance, with properties that represent the various indexes/features extracted by then analysis
 - An `IList<T>` of spectral indices
	 - a _spectral index_  returns an array of values for each typeof index
	 - each spectral index is an object instance and each _spectrum_ is an array of values stored in a property on the object
 - A `Dictionary<string,object>`
	 - allows returning unstructured results stored by key
	 - currently these results are not used 


where the `Count` of objects in `SummaryIndices` and `SpectralIndices` is determined by the _??index resolution??_ and should be equal.

### Converting events to indices
The `IAnalyser` interface has a method that allows events to be recoded as indices so that the events are easily visualised with index visualisers.

## Consumers
The `IAnalyser` interface is most often used by `AnalyseLongRecording`. 

`AnalyseLongRecording` is a subprogram that will apply an `IAnalyser` analysis to every minute of an input audio file. It utilises intra-program parallelisation (`Environment.CpuCount` minutes are analysed concurrently), is often used concurrently as a common operation for inter-program parallelisation, and represents hardened, well-debugged code.

In addition to reading configuration, cutting audio, and manging the process, `AanalyseLongRecording` will collate and process the results of each `IAnalyser` and then pass them back to `IAnalyser` for summarisation