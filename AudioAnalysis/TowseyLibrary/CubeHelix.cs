using ColorMine.ColorSpaces;
using System;


namespace TowseyLibrary
{


public class CubeHelix
        {
            
            const double Radians = Math.PI / 180;

            private readonly Hsl colorA;
            private readonly Hsl colorB;
            private readonly double aHue;
            private readonly double bHue;
            private readonly double aSaturation;
            private readonly double bSaturation;
            private readonly double aLuminosity;
            private readonly double bLuminosity;

            public CubeHelix(Hsl colorARgb, Hsl colorBRgb, double gamma = 1.0)
            {
                this.Gamma = gamma;
                this.colorA = colorARgb.To<Hsl>();
                this.colorB = colorBRgb.To<Hsl>();

                this.aHue = (this.colorA.H + 120) * Radians;
                this.bHue = ((this.colorB.H + 120) * Radians) - this.aHue;
                this.aSaturation = this.colorA.S;
                this.bSaturation = this.colorB.S - this.aSaturation;
                this.aLuminosity = this.colorA.L;
                this.bLuminosity = this.colorB.L - this.aLuminosity;

                if (double.IsNaN(this.bSaturation))
                {
                    this.bSaturation = 0;

                    if (double.IsNaN(this.aSaturation))
                    {
                        this.aSaturation = this.colorB.S;
                    }
                }

                if (double.IsNaN(this.bHue))
                {
                    this.bHue = 0;

                    if (double.IsNaN(this.aHue))
                    {
                        this.aHue = this.colorB.H;
                    }
                }
            }

            public double Gamma { get; private set; }

            public Rgb GetColor(double unitValue)
            {
                var hue = this.aHue + (this.bHue * unitValue);
                var luminosity = Math.Pow(this.aLuminosity + (this.bLuminosity * unitValue), this.Gamma);
                var amplitude = (this.aSaturation + (this.bSaturation * unitValue)) * luminosity * (1 - luminosity);
                var cosh = Math.Cos(hue);
                var sinh = Math.Sin(hue);

                return new Rgb()
                           {
                               R = (luminosity + (amplitude * ((-0.14861 * cosh) + (1.78277 * sinh)))) * 255,
                               G = (luminosity + (amplitude * ((-0.29227 * cosh) - (0.90649 * sinh)))) * 255,
                               B = (luminosity + (amplitude * (+1.97294 * cosh))) * 255
                           };
                
            }

    }

        }
