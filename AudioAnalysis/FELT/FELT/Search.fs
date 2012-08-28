module Search


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
    open QutSensors.AudioAnalysis.AED.Util
    open Microsoft.FSharp.Core

    let centroid (ae: Rectangle<Pixel, Pixel>) =
        ae.Left + (ae.Width / 2.0) , ae.Top  + (ae.Height /2.0)


    let inline centerToEdges center width =
        let h = LanguagePrimitives.DivideByInt width 2
        center - h, center + h


    let getNoiseProfile startOffset endOffset recordingID =
        
        raise <| new NotImplementedException()

    let cutSnippet sourceFile (center:TimeSpan) (duration:TimeSpan) (lowBand:Hertz) (highBand:Hertz) =
        
        // returns a wav

        raise <| new NotImplementedException()
    
    let snippetToSpectrogram wavSource =
        
        raise <| new NotImplementedException()

    let extractFeatures snippet =

        raise <| new NotImplementedException()

    let getTemplates data workflow =

        raise <| new NotImplementedException()


