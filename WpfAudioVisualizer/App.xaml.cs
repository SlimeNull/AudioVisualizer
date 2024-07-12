using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using CommandLine.Text;

namespace WpfAudioVisualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public class AppOptions
        {
            [Option('f', "full-screen")]
            public bool FullScreen { get; set; }


            [Option(longName: "enable-curve", HelpText = "Enable curve rendering")] 
            public bool EnableCurve { get; set; }

            [Option(longName: "enable-strips", HelpText = "Enable strips rendering")]
            public bool EnableStrips { get; set; }
            
            [Option(longName: "enable-border", HelpText = "Enable border rendering")] 
            public bool EnableBorder { get; set; }
            
            [Option(longName: "enable-circle-strips", HelpText = "Enable circle strips rendering")] 
            public bool EnableCircleStripsRendering { get; set; }


            [Option(longName: "disable-curve", HelpText = "Disable curve rendering")] 
            public bool DisableCurve { get; set; }
            [Option(longName: "disable-strips", HelpText = "Disable strips rendering")] 
            public bool DisableStrips { get; set; }
            [Option(longName: "disable-border", HelpText = "Disable border rendering")] 
            public bool DisableBorder { get; set; }
            [Option(longName: "disable-circle-strips", HelpText = "Disable circle strips rendering")] 
            public bool DisableCircleStripsRendering { get; set; }


            [Option(longName: "spectrum-size", HelpText = "Set the spectrum size")] 
            public int? SpectrumSize { get; set; }
            [Option(longName: "spectrum-sample-rate", HelpText = "Set the spectrum sampling rate")] 
            public int? SpectrumSampleRate { get; set; }
            [Option(longName: "spectrum-blurry", HelpText = "Set the spectrum blurry")] 
            public int? SpectrumBlurry { get; set; }
            [Option(longName: "spectrum-factor", HelpText = "Set the spectrum factor (scale)")] 
            public float? SpectrumFactor { get; set; }


            [Option(longName: "strip-count", HelpText = "Set the strip count")] 
            public int? StripCount { get; set; }
            [Option(longName: "strip-spacing", HelpText = "Set the strip spacing")] 
            public float? StripSpacing { get; set; }
            [Option(longName: "circle-strip-count", HelpText = "Set the circle strip count")] 
            public int? CircleStripCount { get; set; }
            [Option(longName: "circle-strip-spacing", HelpText = "Set the circle strip spacing")] 
            public float? CircleStripSpacing { get; set; }
            [Option(longName: "circle-strip-rotation-speed", HelpText = "Set the circle strip rotation speed")] 
            public double? CircleStripRotationSpeed { get; set; }

            [Option('o', longName: "opacity", Default = 0.5)]
            public double Opacity { get; set; } = 0.5;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var parserResult = Parser.Default.ParseArguments<AppOptions>(e.Args);


            parserResult
                .WithParsed(o =>
                {
                    new MainWindow(o).Show();
                })
                .WithNotParsed(errs =>
                {
                    if (!ConsoleUtils.HasConsole)
                    {
                        ConsoleUtils.AllocConsole();
                        ConsoleUtils.InvalidateOutAndError();
                    }

                    Console.WriteLine(HelpText.AutoBuild(parserResult));
                });

            base.OnStartup(e);
        }
    }
}

