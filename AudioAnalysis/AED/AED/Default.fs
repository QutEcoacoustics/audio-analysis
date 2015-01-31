namespace QutSensors.AudioAnalysis.AED

module Default =
    open Util

    let intensityThreshold = 9.0

    let smallAreaThreshold = 200

    //let freqMax = 11025.0

    //let bandPassMaxDefault = freqMax

    //let bandPassMinDefault = 0.0

    let bandPassFilter : (float * float) option = None

    let doNoiseRemoval = true

    type SeparateParameters = {
        AreaThreshold : int<px^2>
        MainThreshold : Percent
        OrthogonalThreshold : Percent
        ExtrapolateBridgeEvents : bool 
    }

    type SeparateStyle =
        | Horizontal of SeparateParameters
        | Vertical of SeparateParameters
        | Skip

    let largeAreaHorizontal = Horizontal {
            AreaThreshold = 3000<px * px>;
            MainThreshold = 20.0.percent;
            OrthogonalThreshold = 100.0.percent / 3.0;
            ExtrapolateBridgeEvents = true
        }

    let largeAreaVeritical = Skip

    let eprNormalisedMinScore = 4.0/15.0

open System.Runtime.InteropServices
type AedOptions (nyquistFrequency : float,
                 ?intensityThreshold,
                 ?smallAreaThreshold ,
                 ?bandPassFilter,
                 ?doNoiseRemoval,
                 ?largeAreaHorizontal,
                 ?largeAreaVeritical) =
    member val NyquistFrequency = nyquistFrequency        
    member val IntensityThreshold  = defaultArg intensityThreshold   Default.intensityThreshold   with get, set   
    member val SmallAreaThreshold  = defaultArg smallAreaThreshold   Default.smallAreaThreshold   with get, set   
    member val BandPassFilter      = defaultArg bandPassFilter       Default.bandPassFilter       with get, set   
    member val DoNoiseRemoval      = defaultArg doNoiseRemoval       Default.doNoiseRemoval       with get, set   
    member val LargeAreaHorizontal = defaultArg largeAreaHorizontal  Default.largeAreaHorizontal  with get, set
    member val LargeAreaVeritical = defaultArg largeAreaVeritical    Default.largeAreaVeritical   with get, set
    new(nyquistFrequency : float) = AedOptions(nyquistFrequency)
