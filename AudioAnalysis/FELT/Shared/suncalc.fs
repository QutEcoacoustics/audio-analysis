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
    
    
    module SunCalc = 

        open System
        open Microsoft.FSharp.Math
        open Microsoft.FSharp.Math.SI

        type Twilights =
            {
                astronomical : Interval<DateTime>;
                nautical : Interval<DateTime>;
                civil : Interval<DateTime>;
            }

        type SunPhases =
            {
                dawn : DateTime;
                sunrise : Interval<DateTime>;
                    
                transit : DateTime;
                sunset : Interval<DateTime>;
                    
                dusk : DateTime;

                morningTwilight : Twilights option;
                eveningTwilight : Twilights option;
            }
            
        let J1970 = 2440588.0
        let G1970 = new DateTime(1970,1,1,0,0,0, DateTimeKind.Utc)
        let J2000 = 2451545.0
        let deg2rad = Math.PI / 180.0
        let M0 = 357.5291 * deg2rad
        let M1 = 0.98560028 * deg2rad
        let J0 = 0.0009
        let J1 = 0.0053
        let J2 = -0.0069
        let C1 = 1.9148 * deg2rad
        let C2 = 0.0200 * deg2rad
        let C3 = 0.0003 * deg2rad
        let P = 102.9372 * deg2rad
        let e = 23.45 * deg2rad
        let th0 = 280.1600 * deg2rad
        let th1 = 360.9856235 * deg2rad
        let h0 = -0.83 * deg2rad //sunset angle
        let d0 = 0.53 * deg2rad //sun diameter
        let h1 = -6.0 * deg2rad //nautical twilight angle
        let h2 = -12.0 * deg2rad //astronomical twilight angle
        let h3 = -18.0 * deg2rad //darkness angle
        let msInDay = 1000 * 60 * 60 * 24 |> float

        let ic  (a,b) = Interval.create a b

        let JulianCalender = new System.Globalization.JulianCalendar()

        let dateToJulianDate (date: DateTime) =
            let diff = date - G1970
            diff.TotalDays - 0.5 + J1970 
        
    
        let julianDateToDate j  =
            let offset = TimeSpan.FromDays(j + 0.5)

            G1970 + offset
            //new DateTime ((j + 0.5 - J1970) * msInDay) 
        
    
        let getJulianCycle  J lw =
            round(J - J2000 - J0 - lw/(2.0 * Math.PI)) 
        
    
        let getApproxSolarTransit Ht lw n =
            J2000 + J0 + (Ht + lw)/(2.0 * Math.PI) + n 
        
    
        let getSolarMeanAnomaly Js =
            M0 + M1 * (Js - J2000) 
        
    
        let getEquationOfCenter M =
            C1 * sin(M) + C2 * sin(2.0 * M) + C3 * sin(3.0 * M) 
        
    
        let getEclipticLongitude M C =
            M + P + C + Math.PI 
        
    
        let getSolarTransit js M lsun =  
            js + (J1 * sin(M)) + (J2 * sin(2.0 * lsun)) 
        
    
        let getSunDeclination lsun =
            asin(sin(lsun) * sin(e)) 
        
    
        let getRightAscension lsun =
            atan2  (sin(lsun) * cos(e)) (cos(lsun))
        
    
        let getSiderealTime J lw =
            th0 + th1 * (J - J2000) - lw
        
    
        let getAzimuth th a phi d = 
            let H = th - a
            atan2  (sin(H)) (cos(H) * sin(phi) - tan(d) * cos(phi))
        
    
        let getAltitude th a phi d = 
            let H = th - a
            asin(sin(phi) * sin(d) + cos(phi) * cos(d) * cos(H))
        
    
        let getHourAngle h phi d =  
            acos((sin(h) - sin(phi) * sin(d)) / (cos(phi) * cos(d))) 
        
    
        let getSunsetJulianDate w0 M lsun lw n =  
            getSolarTransit (getApproxSolarTransit w0 lw n) M lsun
        
    
        let getSunriseJulianDate jtransit jset = 
            jtransit - (jset - jtransit) 
        
    
        type SunPosition =
            {
                azimuth : float
                altitude : float
            }

        let getSunPosition J lw phi =
            let M = getSolarMeanAnomaly J
            let C = getEquationOfCenter M
            let lsun = getEclipticLongitude M C
            let d = getSunDeclination lsun
            let a = getRightAscension lsun
            let th = getSiderealTime J lw
            
            {
                azimuth = getAzimuth th a phi d ;
                altitude = getAltitude th a phi d 
            }
        
    
         
        let getDayInfo date lat lng detailed = 
            let lw = -lng * deg2rad
            let phi = lat * deg2rad
            let J = dateToJulianDate(date)
                
            let n = getJulianCycle J lw
            let Js = getApproxSolarTransit 0.0 lw n
            let M = getSolarMeanAnomaly Js
            let C = getEquationOfCenter M
            let lsun = getEclipticLongitude M C
            let d = getSunDeclination lsun
            let Jtransit = getSolarTransit Js M lsun
            let w0 = getHourAngle h0 phi d
            let w1 = getHourAngle (h0 + d0) phi d
            let Jset = getSunsetJulianDate w0 M lsun lw n
            let Jsetstart = getSunsetJulianDate w1 M lsun lw n
            let Jrise = getSunriseJulianDate Jtransit Jset
            let Jriseend = getSunriseJulianDate Jtransit Jsetstart
            let w2 = getHourAngle h1 phi d
            let Jnau = getSunsetJulianDate w2 M lsun lw n
            let Jciv2 = getSunriseJulianDate Jtransit Jnau
            
            let info = 
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



                
            
            info
            
        
        let sunPosition date lat lng =
            let jd = dateToJulianDate(date)
            getSunPosition jd (-lng * deg2rad) (lat * deg2rad)
            
        


    (* 
    let di = SunCalc.getDayInfo(data, lat, lng)
    let sunrisePos = SunCalc.getSunPosition(di.sunrise.start, lat, lng)
    let sunsetPos = SunCalc.getSunPosition(di.sunset.end, lat, lng)
    *)