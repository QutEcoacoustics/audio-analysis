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
        open Acoustics.Shared
        open Acoustics.Tools
        open Acoustics.Tools.Audio
        open Acoustics.Tools.Wav
        open AudioAnalysisTools
        open Microsoft.FSharp.Core
        open Microsoft.FSharp.Math.SI
        open FELT
        open FELT.Classifiers
        open MQUTeR.FSharp.Shared
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

        open Bound

        type SpectrogramBound = Bound<float<s>, float<Hz>>
    
        module Point =
            let x p = p.x
            let y p = p.y
            let toTuple p = p.x, p.y


        let centroid (ae: Rectangle<float<_>,float<_>>) =
            {x = left ae + (ae.Width / 2.0) ; y = top ae  + (ae.Height / 2.0)}

        let inline centerToEdges center width =
            let h = LanguagePrimitives.DivideByInt width 2
            center - h, center + h

        let inline centroidToRect point width height=
            cornersToRect2 (centerToEdges point.x width) (centerToEdges point.y height)
    
        type EventRect = Rectangle<float<s>,float<Hz>>
        type Event = {
            AudioReadingId : Guid
            Bounds : EventRect
        }    

        type SearchConfig =
            {
                WorkingDirectory : string;
                ResultsDirectory : string;
                TestAudio : DirectoryInfo;
                TrainingData:  FileInfo;
                TrainingAudio : DirectoryInfo;
                AudioSnippetCache : DirectoryInfo;
            }

        let getNoiseProfile startOffset endOffset recordingID =
        
            raise <| new NotImplementedException()

        let cutSnippet (cacheDir: DirectoryInfo) =
            let mau = new MasterAudioUtility();
            let inline round' (x:float<'a>) = 
                x |> fromU |> round |> int |> LanguagePrimitives.Int32WithMeasure<'a>
            let sampleRate = 22050<Hz>
            let inline print x = if Option.isSome x then x.Value |> fromUI |> string else "--"

            (fun (sourceFile:FileInfo) (center:TimeSpan) (duration:TimeSpan) (lowBand:Option<Hertz>) (highBand:Option<Hertz>) ->       

                let left, right = 
                    let h = duration.TotalMilliseconds / 2.0 
                    let c = center.TotalMilliseconds
                    in round' <| c - h , c + h |> round'
                let low, high = Option.applyifSome round' lowBand, Option.applyifSome round' highBand
             
                // check cache
                let outFileName = sourceFile.Name + (sprintf "_%i-%i_%sHz-%sHz." left right (print low)  (print high))
                let outputFile = new FileInfo(Path.Combine(cacheDir.FullName, outFileName)) 

                if not outputFile.Exists then
                    let request = 
                        new AudioUtilityRequest(
                            OffsetStart = ( left |> TimeSpan.FromMilliseconds |> N), 
                            OffsetEnd = (right |> TimeSpan.FromMilliseconds |> N),
                            BandpassLow = (Option.mapToNullable float low),
                            BandpassHigh = (Option.mapToNullable float high),
                            SampleRate = (sampleRate |> fromUI |> N)
                        )  
                    //! warning: io mutation
                    mau.Modify(sourceFile, MediaTypes.MediaTypeWav, outputFile, MediaTypes.MediaTypeWav, request)

                // returns a wav
                let ar = new AudioRecording(outputFile.FullName)
                ar
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

        let extractFeatures snippet : Data =

            raise <| new NotImplementedException()



        let getTemplates (fip:FileInfo) (*workflow*) =
            
            if fip.Exists then
            
                use stream = fip.Open FileMode.Open
                let data : Data = Serialization.deserializeBinaryStream stream

                Infof "Loaded serialised data file from %A" fip

                data
            else
                raise <| FileNotFoundException("The data file was not found: " + fip.FullName, fip.FullName)



        let remapBoundsOfAnEvent bounds event =
            let centerAndAlign bound =
                // to do: sense checking
                Some <| centroidToRect event (width bound) (height bound)
            Array.map (centerAndAlign) bounds
            |> Array.choose (id)


        let compareEvents eventA eventB =
            // some sort of classification

            3.0

        let classifier : ClassifierBase = upcast new EuclideanClassifier(true)

        let compareTemplatesToEvent (templateData:Data) (event:SpectrogramPoint) =
            // import boundaries            
            let bounds = 
                let getBound headers = 
                    let dKey, sfKey, efKey = "duration", "startFreq", "endFreq" 
                    let g2 vs = 
                        let get k = Array.findIndex ((=) k) headers |> Array.get vs |> DataHelpers.getNumber
                        get dKey |> tou<s>, get sfKey |> tou<Hz>, get efKey |> tou<Hz>
                    (fun (values:Value[]) ->
                        let dVal, sfVal, efVal = g2 values
                        Bound<_,_>.create dVal sfVal efVal
                    )
                templateData.Instances |> Map.scanAll |> fun (h,v) -> Seq.map (getBound h) v |> Seq.toArray
        
            // create copies of the "event" with different bounds, all centered on one POI
            let overlays = remapBoundsOfAnEvent bounds event

            // for each overlay, extract stats
            let possibleEvents = extractFeatures overlays

            Diagnostics.Debug.Assert( possibleEvents.DataSet = DataSet.Test)

            // now cross-join training samples with all the possible overlays
            let distancesFunc =
                classifier.Classify templateData possibleEvents

            
            // ! order the results from highest match to lowest
            //Array.sort ...

            // run some filtering?


            distancesFunc

        let search templateData audioCutter (testAudioFile: FileInfo) =
            Infof "Started analysis on file: %s" testAudioFile.FullName

            
            if  not testAudioFile.Exists then
                failwithf "Tried to open file %s, it does not exist"  testAudioFile.FullName

            //!+ Purposely broken!
            let audioRecording = audioCutter testAudioFile TimeSpan.Zero TimeSpan.Zero None None

            // run aed 
            let aedEvents = [||]

            // for each aed event, match it to each training sample
            let analysedEvents = Array.map (compareTemplatesToEvent templateData) aedEvents
            ()

        let main (config:SearchConfig) =
        
            //let workflow = FELT.Workflows.Analyses.["???"]

        
            // misc: partially apply the cutoff function
            let cutSnippet = (cutSnippet config.AudioSnippetCache)

            // trained templates
            let templateData = getTemplates config.TrainingData

            // for each audio fule
            let files = config.TestAudio.GetFiles "*.mp3|*.wav"
            Infof "%i files found in %A" files.Length config.TestAudio
            let resultsForEachFile = Array.map (search templateData cutSnippet) files

            ()
