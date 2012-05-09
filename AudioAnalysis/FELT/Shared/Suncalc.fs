namespace MQUTeR.FSharp.Shared
    
    (*

    https://github.com/mourner/suncalc

    Copyright (c) 2011, Vladimir Agafonkin
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are
    permitted provided that the following conditions are met:

       1. Redistributions of source code must retain the above copyright notice, this list of
          conditions and the following disclaimer.

       2. Redistributions in binary form must reproduce the above copyright notice, this list
          of conditions and the following disclaimer in the documentation and/or other materials
          provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
    EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
    COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
    EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
    SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
    HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
    TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
    SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

    *)
    (*

        Modified by Anthony Truskinger
    *)
    
    module SunCalc = 

        open System
        open System.Diagnostics
        open Microsoft.FSharp.Math
        open Microsoft.FSharp.Math.SI
        open Microsoft.FSharp.Collections
        open System.Collections.Generic
        type Degree = float
        type DayPhase = string
        type DayPoint = string


        ////        type Twilights =
//            {
//                astronomical : Interval<DateTimeOffset>;
//                nautical : Interval<DateTimeOffset>;
//                civil : Interval<DateTimeOffset>;
//            }

        type SunPhases = Map<DayPhase, Interval<DateTimeOffset>>
//            {
//                dawn : DateTimeOffset;
//                sunrise : Interval<DateTimeOffset>;
//                    
//                transit : DateTimeOffset;
//                sunset : Interval<DateTimeOffset>;
//                    
//                dusk : DateTimeOffset;
//
//                morningTwilight : Twilights option;
//                eveningTwilight : Twilights option;
//            }
            //with
//                member this.IsDetailed =
//                    match this.morningTwilight.IsSome , this.eveningTwilight.IsSome with
//                        | true, true -> true
//                        | true , false | false, true -> raise (System.NotSupportedException("Cannot have some detailed members... need all of them"))
//                        | _ -> false
//                   
//                member this.InOrder =
//                    if this.IsDetailed then
//                        [|  |]
//                    else 
//                        [| |]
////                interface IEnumerable<string * Interval<DateTimeOffset>> with
////                    member this.GetEnumerator() =
//                        
//                member this.PhaseForTime (dto:DateTimeOffset) =
//                    dto



        //"Dawn" ; "Morning Twilight" ; "Sunrise" ; "Morning" ; "Daylight" ; "Evening" ; "Sunset" ; "Evening Twilight" ; "Dusk" ; "Night"
       // let simplePhases : DayPhase array = [|"dawn";  "morning"; "afternoon"; "dusk"; "night" |]

        // day phases coupled with the beginning of each phase in degrees of sun elevation. end of each 
        let DawnAstronomicalTwilight             ="dawn astronomical twilight"  
        let DawnNauticalTwilight                 ="dawn nautical twilight"  
        let DawnCivilTwilight                    ="dawn civil twilight"  
        let Sunrise                              ="sunrise"  
        let MorningGoldenHour                    ="morning golden hour"  
        // "daylight"
        let Morning                              ="morning"  
        // "daylight"
        let Afternoon                            ="afternoon"  
        let AfternoonGoldenHour                  ="afternoon golden hour"  
        let Sunset                               ="sunset"  
        let EveningCivilTwilight                 ="evening civil twilight"  
        let EveningNauticalTwilight              ="evening nautical twilight"  
        let EveningAstronomicalTwilight          ="evening astronomical twilight"  
        let Night                                ="night"   
        let SolarNoon (sunPhases:SunPhases)      = 
            let n1 = sunPhases.[Morning].Upper
            Debug.Assert (n1.Equals(sunPhases.[Afternoon].Lower))
            n1 
        let Dawn (sunPhases:SunPhases)      = 
            let d1 = sunPhases.[DawnNauticalTwilight].Upper
            Debug.Assert (d1.Equals(sunPhases.[DawnCivilTwilight].Lower))
            d1 
        let Dusk (sunPhases:SunPhases)      = 
            let d1 = sunPhases.[EveningCivilTwilight].Upper
            Debug.Assert (d1.Equals(sunPhases.[EveningNauticalTwilight].Lower))
            d1
        let NightParts (sunPhases:SunPhases)      =
            sunPhases.[DawnAstronomicalTwilight].Lower,
            sunPhases.[EveningAstronomicalTwilight].Upper


        let detailedPhases : DayPhase array = 
            [| 
            DawnAstronomicalTwilight           
            DawnNauticalTwilight               
            DawnCivilTwilight                  
            Sunrise                            
            MorningGoldenHour                  
            Morning                            
            Afternoon                          
            AfternoonGoldenHour                
            Sunset                             
            EveningCivilTwilight               
            EveningNauticalTwilight            
            EveningAstronomicalTwilight        
            Night                              
            |]
            
        // time configuration (angle, morning name (beginning), evening name (ending))
        let phaseGroups : (Degree * DayPhase * DayPhase) array = 
            [|
            -18.0, DawnAstronomicalTwilight , EveningAstronomicalTwilight ;
            -12.0, DawnNauticalTwilight , EveningNauticalTwilight ;
            -6.0, DawnCivilTwilight , EveningCivilTwilight ;
            -0.83, Sunrise , Sunset ;
            -0.3, MorningGoldenHour , AfternoonGoldenHour ;
            6.0, Morning , Afternoon ;
            //Double.NaN, , ;
            |]


            
//            6.0, , ;
//            -0.3, , ;
//            -0.83, , ;
//            -6.0, , ;
//            -12.0, , ;
//            -18.0


            
        let J1970 = 2440588.0
        let G1970 = new DateTimeOffset(1970,1,1,0,0,0, TimeSpan.Zero)
        let J2000 = 2451545.0
        let deg2rad = Math.PI / 180.0
        let M0 = 357.5291 * deg2rad
        let M1 = 0.98560028 * deg2rad
        let J0 = 0.0009
        let J1 = 0.0053
        let J2 = -0.0069
        let C1   = deg2rad * 1.9148
        let C2   = deg2rad * 0.0200
        let C3   = deg2rad * 0.0003
        let P    = deg2rad * 102.9372
        let e    = deg2rad * 23.45
        let th0  = deg2rad * 280.1600
        let th1  = deg2rad * 360.9856235


        let h0 = -0.83 * deg2rad //sunset angle
        let d0 = 0.53 * deg2rad //sun diameter
        let h1 = -6.0 * deg2rad //nautical twilight angle
        let h2 = -12.0 * deg2rad //astronomical twilight angle
        let h3 = -18.0 * deg2rad //darkness angle
        
        //let msInDay = 1000 * 60 * 60 * 24 |> float

        let ic a b = Interval.create a b
        

        let dateToJulianDate (date: DateTimeOffset) =
            let diff = date - G1970
            diff.TotalDays - 0.5 + J1970 
        
        //[<System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)>]
        let julianDateToDate j timeZoneOffset =
            let offset = TimeSpan.FromDays(j - J1970 + 0.5)
            (G1970 + offset).ToOffset(timeZoneOffset)
        
    
        // General sun calculations

        let getJulianCycle J lw = round(J - J2000 - J0 - lw/(2.0 * Math.PI)) 
        let getSolarMeanAnomaly Js = M0 + M1 * (Js - J2000) 
        let getEquationOfCenter M = C1 * sin(M) + C2 * sin(2.0 * M) + C3 * sin(3.0 * M) 
        let getEclipticLongitude M C = M + P + C + Math.PI 
        let getSunDeclination lsun = asin(sin(lsun) * sin(e)) 

        // Calculations for sun times

        let getApproxSolarTransit Ht lw n = J2000 + J0 + (Ht + lw)/(2.0 * Math.PI) + n 
        let getSolarTransit js M lsun = js + (J1 * sin(M)) + (J2 * sin(2.0 * lsun)) 
        let getHourAngle h phi d = acos((sin(h) - sin(phi) * sin(d)) / (cos(phi) * cos(d))) 
    
        // Calculations for sun position
        let getRightAscension lsun = atan2 (sin(lsun) * cos(e)) (cos(lsun))
        let getSiderealTime J lw = th0 + th1 * (J - J2000) - lw
        let getAzimuth H phi d = atan2 (sin(H)) (cos(H) * sin(phi) - tan(d) * cos(phi))
        let getAltitude H phi d = asin(sin(phi) * sin(d) + cos(phi) * cos(d) * cos(H))
        
    
        
        
    
        let getSunsetJulianDate w0 M lsun lw n =  
            getSolarTransit (getApproxSolarTransit w0 lw n) M lsun
        
    
        let getSunriseJulianDate jtransit jset = 
            jtransit - (jset - jtransit) 
        
    
        type SunPosition =
            {
                azimuth : float
                altitude : float
            }

        let sunPosition date lat lng =
            let jd = dateToJulianDate(date)
            let lw = (-lng * deg2rad) 
            let phi = (lat * deg2rad)
            let M = getSolarMeanAnomaly jd
            let C = getEquationOfCenter M
            let lsun = getEclipticLongitude M C
            let d = getSunDeclination lsun
            let a = getRightAscension lsun
            let th = getSiderealTime jd lw
            let H = th - a
            {
                azimuth = getAzimuth H phi d ;
                altitude = getAltitude H phi d 
            }
        
    
         
        let getDayInfo date lat lng offset = 
            let julianDateToDate x = julianDateToDate x offset

            let lw = -lng * deg2rad
            let phi = lat * deg2rad
            let J = dateToJulianDate(date)
                
            let n = getJulianCycle J lw
            let Js = getApproxSolarTransit 0.0 lw n
            let M = getSolarMeanAnomaly Js
            let C = getEquationOfCenter M
            let lsun = getEclipticLongitude M C
            let d = getSunDeclination lsun

            let JNoon = getSolarTransit Js M lsun

            let getSetJ h =
                let w = getHourAngle h phi d
                let a = getApproxSolarTransit w lw n
                getSolarTransit a M lsun

            let noon = julianDateToDate JNoon

            let getPair arg =
                let angle, mdayPhase, edayPhase = arg
                let jset = getSetJ (angle * deg2rad)
                let jrise = JNoon - (jset - JNoon)
                (mdayPhase, julianDateToDate(jrise)), (edayPhase, julianDateToDate(jset))

            let times = Array.map getPair phaseGroups
            let preNoon, postNoon = Array.unzip times

            
            // convert to intervals
            let empty = Map.empty<DayPhase, Interval<DateTimeOffset>>
            let _, preNoon'   = Array.foldBack (fun (phase, thisTime) (lastTime, map) -> thisTime, (Map.add phase (ic thisTime lastTime) map)) preNoon  (noon, empty) 
            let _, postNoon'  = Array.foldBack (fun (phase, thisTime) (lastTime, map) -> thisTime, (Map.add phase (ic lastTime thisTime) map)) postNoon (noon, empty) 

            let result = Map.merge preNoon' postNoon'

            result

//            let mta, eta = getPair (detailedPhases.["dawn astronomical twilight"])
//            let mtn, etn = getPair (detailedPhases.["dawn nautical twilight"])
//            let mtc, etc = getPair (detailedPhases.["dawn civil twilight"])
//            let sunrise, sunset = getPair (detailedPhases.["sunrise"])
//            let mgh, egh = getPair (detailedPhases.["morning golden hour"])
//            let mng, aft = getPair (detailedPhases.["morning"])
//
//
//            [| 
//            "dawn astronomical twilight", ic mta mtn;
//            "dawn nautical twilight", ic mtn mtc;
//            "dawn civil twilight", ic mtc sunrise;
//            "sunrise", ic sunrise mgh;
//            "morning golden hour", ic mgh mng;
//            "morning", ic mng noon; // "daylight"
//            "afternoon", ic noon aft; // "daylight"
//            "afternoon golden hour", ic aft egh;
//            "sunset", ic egh sunset;
//            "evening civil twilight", ic sunset etc;
//            "evening nautical twilight", ic etc etn;
//            "evening astronomical twilight", ic etn eta;
//            "night" , ic eta (mta.AddDays(1.0))
//            |] |> Map.ofArray


            (*
            let w0 = getHourAngle h0 phi d
            let w1 = getHourAngle (h0 + d0) phi d
            let Jset = getSunsetJulianDate w0 M lsun lw n
            let Jsetstart = getSunsetJulianDate w1 M lsun lw n
            let Jrise = getSunriseJulianDate Jtransit Jset
            let Jriseend = getSunriseJulianDate Jtransit Jsetstart
            let w2 = getHourAngle h1 phi d
            let Jnau = getSunsetJulianDate w2 M lsun lw n
            let Jciv2 = getSunriseJulianDate Jtransit Jnau
            
         
            let mt, et =
                if detailed then
                    let w3 = getHourAngle h2 phi d
                    let w4 = getHourAngle h3 phi d
                    let Jastro = getSunsetJulianDate w3 M lsun lw n
                    let Jdark = getSunsetJulianDate w4 M lsun lw n
                    let Jnau2 = getSunriseJulianDate Jtransit Jastro
                    let Jastro2 = getSunriseJulianDate Jtransit Jdark
                
                    let morningTwilight = 
                        { 
                            astronomical = Interval.create  <|| (julianDateToDate(Jastro2), julianDateToDate(Jnau2));
                            nautical     = Interval.create  <|| (julianDateToDate(Jnau2),   julianDateToDate(Jciv2));
                            civil        = Interval.create  <|| (julianDateToDate(Jciv2),   julianDateToDate(Jrise))
                        }
                    
                    let nightTwilight =  
                        {
                            civil        =Interval.create  <|| (julianDateToDate(Jset),   julianDateToDate(Jnau));
                            nautical     =Interval.create  <|| (julianDateToDate(Jnau),   julianDateToDate(Jastro));
                            astronomical =Interval.create  <|| (julianDateToDate(Jastro), julianDateToDate(Jdark))
                        }
                    Some(morningTwilight), Some(nightTwilight)
                else
                    None, None


                
            {
                dawn = julianDateToDate(Jciv2);
                sunrise = 
                    Interval.create <|| (julianDateToDate(Jrise), julianDateToDate(Jriseend));
                transit = julianDateToDate(Jtransit);
                sunset = 
                    Interval.create <|| (julianDateToDate(Jsetstart), julianDateToDate(Jset));
                dusk = julianDateToDate(Jnau);
                morningTwilight = mt;
                eveningTwilight = et
            }
            *)


                
            
            
            
        

            
        


    (* 
    let di = SunCalc.getDayInfo(data, lat, lng)
    let sunrisePos = SunCalc.getSunPosition(di.sunrise.start, lat, lng)
    let sunsetPos = SunCalc.getSunPosition(di.sunset.end, lat, lng)
    *)