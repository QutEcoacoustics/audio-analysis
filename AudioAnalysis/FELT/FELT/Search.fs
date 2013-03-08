namespace FELT
    module Search =


        (*
            New module
            Objective: search for events

            Three stages:
            1) template construction
            2) point of interest detection
            3) POI classification (as a template)


            1) make use of work already done
                - ensure at least 4 or more features are used
                    - duration, startFreq, endFreq used for preprocessing

            2) based on AED... in principal easy enough

            3) is where the real work will needs to begin
                - // this is a classifier
                - load templates

                - load aed events

                - prep work
                    - for each aed event
                            - get the nosie profile for the minute surrounding the event
                            - calculate centroid of aed event
            
                    - for each template        
                        - calculate centroid
                
                - for each template (t)
                
                    - for each aed event (ae)
                            - get the snippet of audio (sn) for those bounds. 
                                - from ae.centroid, cut out t.width, aligned by t.centroid
                                - add padding?
                        
                            - use freq bounds on template to apply a bandpass to sn
                                - padding?
                                - roll off response?

                            - run snippet through feature extraction sn => sn.features <- values
                        
                            - calculate classification metric (e.g. distance)
                                - from sn.features
                                - to t.features

                            - return tuple
                                (t.ID, ae.ID,distance)
                        - POSSIBLE IMPROVEMENT
                            - for every n (n=?) pixels P in ae.bounds
                                - where template does not go over bounds
                                - slide template across all available positions
                                - minimum movement is n pixels (x / y) 


                - summarise results
                    - for each result (r)
                        - return a list of likely ?species/calls?

        *)
        open System
        open System.Extensions
        open System.Diagnostics
        open System.IO
        open QutSensors.AudioAnalysis.AED.Util
        open QutSensors.AudioAnalysis.AED
        open Acoustics.Shared
        open Acoustics.Tools
        open Acoustics.Tools.Audio
        open Acoustics.Tools.Wav
        open AudioAnalysisTools
        open Microsoft.FSharp.Core
        open Microsoft.FSharp.Math
        open Microsoft.FSharp.Math.SI
        open FELT
        open FELT.Classifiers
        open MQUTeR.FSharp.Shared
        open System.IO
        open Microsoft.FSharp.Collections
        open TowseyLib

        type Point<'a, 'b> = { x : 'a; y: 'b}
        type SpectrogramPoint = Point<float<s>, float<Hz>>

        type Bound<'a, 'b> = {
            duration : 'a;
            startFrequency :'b;
            endFrequency: 'b;
            }
            with
                static member create d sf ef = {duration = d; startFrequency = sf; endFrequency = ef}

        module Bound =
            let inline width b = b.duration
            let inline height b = b.endFrequency - b.startFrequency
            let inline create d sf ef = {duration = d; startFrequency = sf; endFrequency = ef}

        open Bound

        type SpectrogramBound = Bound<float<s>, float<Hz>>
    
        module Point =
            let x p = p.x
            let y p = p.y
            let toTuple p = p.x, p.y
            let create x y = {x = x; y = y}

        let centroid (ae: Rectangle<float<_>,float<_>>) =
            {x = left ae + ((Util.width ae) / 2.0) ; y = top ae  + ((Util.height ae) / 2.0)}

        let inline centerToEdges center width =
            let h = LanguagePrimitives.DivideByInt width 2
            center - h, center + h

        let inline centroidToRect point width height=
            cornersToRect2 (centerToEdges point.x width) (centerToEdges point.y height)
    
        type EventRect = Rectangle<float<s>,float<Hz>>
        type Event = {
            AudioReadingId : Guid
            Rectangle : EventRect
        }    

        type AedConfig =  {
            SmallAreaThreshold : int<px ^ 2>;
            IntensityThreshold : float<dB>;

        }

        type SearchConfig =
            {
                WorkingDirectory : string;
                ResultsDirectory : DirectoryInfo;
                ResultsFile : FileInfo;
                TestAudio : DirectoryInfo;
                TrainingData:  FileInfo;
                TrainingAudio : DirectoryInfo;
                AudioSnippetCache : DirectoryInfo;
                AedConfig : AedConfig;
            }

        let getNoiseProfile startOffset endOffset recordingID =
        
            raise <| new NotImplementedException()

        type AudioCutterClosure = FileInfo -> TimeSpan -> TimeSpan -> Option<Hertz> -> Option<Hertz> -> AudioRecording
        let cutSnippet (cacheDir: DirectoryInfo) : AudioCutterClosure =
            let mau = new MasterAudioUtility();
            let inline round' (x:float<'a>) = 
                x |> fromU |> round |> int |> LanguagePrimitives.Int32WithMeasure<'a>
            let sampleRate = 22050<Hz>
            let inline print x = if Option.isSome x then x.Value |> fromUI |> string |> sprintf "_%sHz" else String.Empty

            let f  (sourceFile:FileInfo) (center:TimeSpan) (duration:TimeSpan) lowBand highBand =

                let left, right = 
                    let h = duration.TotalMilliseconds / 2.0 
                    let c = center.TotalMilliseconds
                    in round' <| c - h , c + h |> round'
                let low, high = Option.applyifSome round' lowBand, Option.applyifSome round' highBand
             
                // check cache
                let outFileName = Path.GetFileNameWithoutExtension(sourceFile.Name) + (sprintf "_%i-%i" left right) + (print low) + (print high) + sourceFile.Extension
                let outputFile = new FileInfo(Path.Combine(cacheDir.FullName, outFileName)) 

                //? possibly should add cache age check
                if not outputFile.Exists then
                    let request = 
                        new AudioUtilityRequest(
                            OffsetStart = ( left |> TimeSpan.FromMilliseconds |> N), 
                            OffsetEnd = (right |> TimeSpan.FromMilliseconds |> N),
                            BandpassLow = (Option.mapToNullable float low),
                            BandpassHigh = (Option.mapToNullable float high)
                            //SampleRate = (sampleRate |> fromUI |> N)
                        )  
                    //! warning: io mutation
                    mau.Modify(sourceFile, MediaTypes.MediaTypeWav, outputFile, MediaTypes.MediaTypeWav, request)

                // returns a wav
                let ar = new AudioRecording(outputFile.FullName)
                ar
            // return closure
            f

        let getSnippetInfo =
            let mau = new MasterAudioUtility();

            (fun (sourceFile:FileInfo) ->
                mau.Info(sourceFile)    
            )
        
    
        let snippetToSpectrogram (wavSource:AudioRecording) =
            // can enable noise reduction here
            let config = new SonogramConfig( NoiseReductionType = NoiseReductionType.NONE )

            let sp = new SpectralSonogram(config, wavSource.GetWavReader());
            sp

        let spectrogramToMatrix (sonogram:SpectralSonogram) =
            Math.Matrix.ofArray2D sonogram.Data |> mTranspose

        let spectrogramBandpass (sonogram:SpectralSonogram) (low:int<px>) (high:int<px>) =
            let spm = spectrogramToMatrix sonogram
        
            let l, h = int low, int high

            //? unsure if this is correct
            spm.[l..h,*]

        type FeatureAction = 
            | Spectral of (AudioRecording -> SpectralSonogram -> Value)
            | Sample of (AudioRecording -> Value)
            | None of  (unit -> Value)
        /// extractFeatures ->  for every event, and then every feature selected, extract those features
        let extractFeatures (snippets:EventRect array) featureList audioCutter sourceFile : Data =

            // three types of features
            //  - statistical (need no prior claculation)
            //  - time-domain (needs raw pcm signals) -> needs cut files
            //  - spectral (needs spectrogram) -> needs cut spectrograms (of cut files)

            /// a mapping of feature names to functions and datatypes
            let routeAction feature =
                let (action:FeatureAction), headerName, dataType =
                    match feature with
                        | EqualsOut "bullshit" a -> None(fun () -> upcast( new Number(3.0))), a, DataType.Number;
                        | _ -> raise <| new NotImplementedException()
                action, (headerName, dataType)

            // prep: the set of operations to apply to each event
            let actions = List.map routeAction featureList
                
            // Prep: create the Instances data structure
            let eventCount = Array.length snippets
            let instances = List.fold (fun state (_, (name, _)) -> Map.add name (Array.zeroCreateUnchecked eventCount) state) Map.empty<ColumnHeader, Value[]> actions
            let classes = Array.zeroCreateUnchecked eventCount

            //+ execution
            let applyActionsToEvent (e:Index) ((instanceMap:Map<ColumnHeader, Value[]>), (classLabels:Class array)) (event: EventRect) =
                // pre-pare audio - we only want to cut this once, and reuse it for each feature
                let getAudio =
                    (fun () ->
                        // TODO: BROKEN
                        new AudioRecording([||])
                    )
                // pre-pare spectrogram - we only want to calculate this once, and reuse it for each feature
                let getSpectrogram =
                    (fun () ->
                        // TODO: BROKEN()
                        new SpectralSonogram("", null)
                    )

                let runAction (action, (headerName, (dataType:DataType))) =
                    let v =     
                        match action with 
                            | None f -> raise <| new InvalidOperationException()
                            | Spectral fftf -> fftf (getAudio()) (getSpectrogram())
                            | Sample samplef -> samplef (getAudio())   
                    instanceMap.[headerName].[e] <- v

                List.iter (runAction) actions

                
                // lastly mutate the values in the storage mechanism
                classLabels.[e] <- "Unknown_" + e.ToString("000000")
                // return state
                instanceMap, classLabels

            // each event will remap to one "row" in the dataset
            //! warning mutation of value and classes arrays is occuring
            let instances, classes = Array.foldi applyActionsToEvent (instances, classes) snippets
            
            {DataSet = DataSet.Test; Headers =  actions |> List.unzip |> snd |> Map.ofList; Instances = instances; ClassHeader = "Tag"; Classes = classes  }



        let getTemplates (fip:FileInfo) (*workflow*) =
            
            if fip.Exists then
            
                use stream = fip.Open FileMode.Open
                let data : MQUTeR.FSharp.Shared.CacheFormat = Serialization.deserializeBinaryStream stream

                Infof "Loaded serialised data file from %A" fip

                data
            else
                raise <| FileNotFoundException("The data file was not found: " + fip.FullName, fip.FullName)



        let nyquist = 11025.0<Hz>
        let pixelMax = 256.0<px>
        let rHertz = Interval.rescale (Interval.create 0G nyquist) (Interval.create 0.0<px> pixelMax)
        let rSeconds duration horizPixels = Interval.rescale (Interval.create 0G duration) (Interval.create 0.0<px> horizPixels)
        let rToHertz = Interval.rescale   (Interval.create 0.0<px> pixelMax) (Interval.create 0.0<Hz> nyquist)
        let rToSeconds duration horizPixels = Interval.rescale (Interval.create 0.0<px> horizPixels) (Interval.create 0.0<s> duration)

        let convertToPixels duration horizPixels (bound: Bound<float<s>, float<Hz>>) =
            let u = rHertz bound.endFrequency
            let l = rHertz bound.startFrequency
            let d = rSeconds duration horizPixels bound.duration 
            Bound.create d l u

        let convertToDomainUnits duration horizPixels  bound = 
            Bound.create (rToSeconds duration horizPixels bound.duration) (rToHertz bound.startFrequency) (rToHertz bound.endFrequency)
        let convertRectToDomainUnits duration horizPixels rect =
            let r = rToSeconds duration horizPixels
            cornersToRect (r <| left rect) (r <| right rect)  (rToHertz <| top rect) (rToHertz <| bottom rect)

        let inline remapBoundsOfAnEvent bounds (event: EventRect) =
            let centerAndAlign bound =
                let midpointOfPoi = centroid event
                // to do: sense checking
                centroidToRect midpointOfPoi (width bound) (height bound)
            Array.map (centerAndAlign) bounds


        let classifier : ClassifierBase = upcast new EuclideanClassifier(true)

        let compareTemplatesToEvent (templateData:Data)  testAudioRecording (testSpectrogram: SpectralSonogram) (event: Rectangle2<float<px>>) =
            // import boundaries            
            let bounds = 
                let getBound headers = 
                    let dKey, sfKey, efKey = "TagDuration", "StartFrequency", "EndFrequency" 
                    let g2 vs = 
                        let get k = Array.findIndex ((=) k) headers |> Array.get vs |> DataHelpers.getNumber
                        get dKey |> tou<s>, get sfKey |> tou<Hz>, get efKey |> tou<Hz>
                    (fun (values:Value[]) ->
                        let dVal, sfVal, efVal = g2 values
                        Bound<_,_>.create dVal sfVal efVal
                    )
                templateData.Instances |> Map.scanAll |> fun (h,v) -> Seq.map (getBound h) v |> Seq.toArray
            
            // create copies of the "event" with different bounds, all centered on one POI
            //! make sure params to conversion are correct
            let event' = convertRectToDomainUnits (testSpectrogram.FrameDuration * (tou2 testSpectrogram.FrameCount)) (testSpectrogram.FrameCount |> tou2) event
            let overlays = remapBoundsOfAnEvent bounds event'

            // for each overlay, extract stats
            let possibleEvents = extractFeatures overlays

            //Diagnostics.Debug.Assert( possibleEvents.DataSet = DataSet.Test)

            // now cross-join training samples with all the possible overlays
            let distancesFunc =
                classifier.Classify templateData templateData //WRONG!@!!!!!!!!!!!!!!!!!!!!!!! possibleEvents

            
            // ! order the results from highest match to lowest
            //Array.sort ...

            // run some filtering?


            distancesFunc

        let summariseAnalysisOfPois resultDepth result =
            match result with
                | Function lr ->
                    //!+ wtf this is not going to work
                    let i = 3 //!+ 3?? seriously 3? anthony has no idea what his own code does
                    // returns (Distance * TrainingIndex)[]
                    lr i
                | _ -> raise <| new NotImplementedException("Cannot parse other types of results yet")

        let search templateData (audioCutter: AudioCutterClosure) (aedConfig: AedConfig)  testAudioFile =
            Infof "Started analysis on file: %s"  <| IO.fullName testAudioFile
            
            if  not testAudioFile.Exists then
                failwithf "Tried to open file %s, it does not exist"  testAudioFile.FullName

            // check that the file is not too big
            let info = getSnippetInfo testAudioFile
            if info.Duration.Value > 10.0.Minutes then
                Warnf "Current test file (%A) is over 10 Minutes long (%f m) - this may take a while!"  testAudioFile info.Duration.Value.TotalMinutes

            let audioRecording = audioCutter testAudioFile (info.Duration.Value.DivideBy 2L) info.Duration.Value Option.None Option.None
            let spectrogram = snippetToSpectrogram audioRecording

            // run aed 
            Diagnostics.Debug.Assert( spectrogram.NyquistFrequency = 11025)

            Infof "Starting AED analysis of test file (using config: IntensityThreshold=%A, SmallAreaThreshold=%A)" aedConfig.IntensityThreshold aedConfig.SmallAreaThreshold

            let aedEvents = 
                AcousticEventDetection.detectEventsMinor (float aedConfig.IntensityThreshold) (int aedConfig.SmallAreaThreshold) (0.0, float spectrogram.NyquistFrequency) spectrogram.Data
                |> Seq.map (toFloatRect >> addDimensions px px)

            let lengthOfPois = Seq.length aedEvents 
            Infof "Aed analysis finished of %s, %i events created" testAudioFile.Name lengthOfPois

            // for each aed event, match it to each training sample, note: lazy execution
            Info "Starting analysis of aed events"
            let analysedEvents = Seq.map (compareTemplatesToEvent templateData audioRecording spectrogram) aedEvents

            // summarise the results, for each POI event
            // the lazy result executer needs to know how many results to calculate, this is equal to the number of POIs
            //! results actually executed here
            let summaries = Seq.map (summariseAnalysisOfPois lengthOfPois) analysedEvents
            summaries, testAudioFile

        let main (config:SearchConfig) =
        
            //let workflow = FELT.Workflows.Analyses.["???"]

        
            // misc: partially apply the cutoff function
            let cutSnippet = (cutSnippet config.AudioSnippetCache)

            // trained templates
            let templateData = getTemplates config.TrainingData

            

            // for each audio fule
            let files = config.TestAudio.GetFiles "*.wav"
            Infof "%i files found in %A" files.Length config.TestAudio
            let resultsForEachFile = Array.map (search templateData.CachedData cutSnippet config.AedConfig ) files

            // save the results some how
            Serialization.serializeJsonToFile resultsForEachFile (config.ResultsFile.FullName)
            ()
