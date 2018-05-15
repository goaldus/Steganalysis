using System.IO;
using System.Drawing;
using Steganalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stegoanalysis
{
    class Program
    {       
        static int Main(string[] args)
        {
            double threshold;

            //ARGUMENTS CHECK
            if (args.Length < 1 || args.Length > 4)
                setError("Nespravny pocet argumentu, navstivte -help");

            if (args.Length == 1 && args[0] == "-help")
                printHelp();
            else if (args[0] == "analyze")
            {
                if (args.Length == 2)      // default analysis
                {
                    if (Directory.Exists(args[1]))
                        runAnalysis(args[1], SearchOption.TopDirectoryOnly);
                    else
                        setError("Zadana slozka neexistuje!");
                }
                else if (args.Length == 3)
                {
                    if (!Directory.Exists(args[1]))
                        setError("Zadana slozka neexistuje!");

                    if (isDoubleAndInRange(args[2], out threshold))                 // double in range of 0 - 1
                        runAnalysis(args[1], SearchOption.TopDirectoryOnly, threshold);
                    else if (args[2] == "-r")
                        runAnalysis(args[1], SearchOption.AllDirectories);
                    else
                        setError("Neplatny prikaz nebo prah neni v rozmezi 0 - 1, navstivte -help");
                }
                else if (args.Length == 4)
                {
                    if (!Directory.Exists(args[1]))
                        setError("Zadana slozka neexistuje!");

                    if (isDoubleAndInRange(args[2], out threshold))        // analyze DirName 0.5 -r
                    {
                        if (args[3] == "-r")
                            runAnalysis(args[1], SearchOption.AllDirectories, threshold);
                        else
                            setError("Neplatny prikaz, navstivte -help");
                    }
                    else if (args[2] == "-r")
                    {
                        if (isDoubleAndInRange(args[3], out threshold))
                            runAnalysis(args[1], SearchOption.AllDirectories, threshold);
                        else
                            setError("Neplatny prikaz nebo prah neni v rozmezi 0 - 1");
                    }
                    else
                        setError("Neplatny prikaz nebo prah neni v rozmezi 0 - 1, navstivte -help");
                }
                else
                    setError("Nespravny pocet argumentu, navstivte -help");
            }
            else if (args[0] == "visualattack" && args.Length == 2)
            {
                if (File.Exists(args[1]))
                {
                    var extension = Path.GetExtension(args[1]).ToLower();
                    if (extension == ".png" || extension == ".bmp")
                        runVisualAttack(args[1]);
                    else
                        setError("Aplikace s formatem tohoto souboru nedokaze pracovat, navstivte -help");
                }
                else
                    setError("Zadany soubor neexistuje!");
            }
            else
                setError("Neplatny prikaz, navstivte -help");

            return 0;
        }

        /// <summary>
        /// Creates image with LSB enhancement, then user can open this image and evaluate on his own
        /// </summary>
        static void runVisualAttack(string file)
        {
            using (Stream BitmapStream = System.IO.File.Open(file, System.IO.FileMode.Open))
            {
                Image picture = Image.FromStream(BitmapStream);
                var mBitmap = new Bitmap(picture);

                var newBitmap = LSBEnhancement.createLSBEnhancementImage(mBitmap);

                var extension = Path.GetExtension(file);
                var filename = Path.GetFileName(file).Replace(extension, "");
                newBitmap.Save(filename + "LSBenhanced" + extension);
            }

            Console.WriteLine("Soubor byl ulozen ve slozce: " + Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
         }

        /// <summary>
        /// Runs 3 detectors and computing mean value. When the value is above the threshold then the user is alerted.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="option"></param>
        /// <param name="threshold"></param>
        static void runAnalysis(string directory, SearchOption option, double threshold = 0.2)
        {
            var extension = new List<string> { ".bmp", ".png" };
            var files = Directory.GetFiles(directory, "*.*", option)
                .Select(s => s.ToLowerInvariant())
                .Where(s => extension.Contains(Path.GetExtension(s)));

            foreach (var file in files)
            {
                using (Stream BitmapStream = System.IO.File.Open(file, System.IO.FileMode.Open))
                {
                    Image picture = Image.FromStream(BitmapStream);
                    var mBitmap = new Bitmap(picture);

                    double avg = 0;

                    var ChiSquareAnalysis = new ChiSquare(picture.Width, picture.Height, mBitmap);
                    var cs = ChiSquareAnalysis.analyze();

                    var SamplePairsAnalysis = new SamplePairs(picture.Width, picture.Height, mBitmap);
                    var sp = SamplePairsAnalysis.analyze();

                    var RSAnalysis = new RSAnalysis(picture.Width, picture.Height, mBitmap, 2, 2);
                    var rs = RSAnalysis.analyze();

                    avg = (cs + sp + rs) / 3;

                    if (avg > threshold)
                    {
                        Console.WriteLine("Obrazek " + Path.GetFileName(file) + " je podezrely, priblizna delka skryte zpravy: "
                            + RSAnalysis.estimatedHiddenMessageLength + " B\n");
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether argument is double and is in range of 0 - 1
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="threshold"></param>
        /// <returns>true when argument is double and is in range of 0 - 1</returns>
        static bool isDoubleAndInRange(string arg, out double threshold)
        {
            if (double.TryParse(arg.Replace('.', ','), out threshold))
                if (threshold <= 1 && threshold >= 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Exits with error message
        /// </summary>
        /// <param name="message"></param>
        static void setError(string message)
        {
            System.Console.WriteLine(message);
            Environment.Exit(-1);
        }

        /// <summary>
        /// Function for printing help
        /// </summary>
        static void printHelp()
        {
            Console.WriteLine("\nSTEGANALYSIS");
            Console.WriteLine("==============");
            Console.WriteLine("Napoveda: tato aplikace dokaze detekovat skrytou informaci v obraze. Muzete ji detekovat vizualne, tim ze si nechate " +
                "vytvorit stejny obrazek se zvyraznenim LSB (least significant bits) nebo pomoci statistickych utoku, ktere vypisuji v pripade " +
                "podezreleho obrazku i odhadovanou delku skryte zpravy. Nutno podotknout, ze aplikace dokaze pracovat pouze s formaty .bmp a .png.\n");
            Console.WriteLine("Pouziti:\t-visualattack imageFileName\t\t- vytvori stejny obrazek, ale se zvyraznenim LSB");
            Console.WriteLine("\t\t-analyze directoryName [threshold] [-r]\t- analyzuje obrazky ve slozce. Pri vlozeni hodnoty v rozmezi od\n" +
                    "\t\t\t\t\t\t\t  0 - 1 s desetinnou carkou je mozno nastavit prah. V pripade\n" +
                    "\t\t\t\t\t\t\t  prekroceni prahu, je obrazek povazovan za cover medium, ktere\n" +
                    "\t\t\t\t\t\t\t  prenasi skrytou informaci. Vychozi prah je nastaven na 0.2.\n" +
                    "\t\t\t\t\t\t\t  Pro analyzu obrazku i v podslozkach, pouzijte prepinac -r.\n");
            Console.WriteLine(@"Priklad: steganalysis analyze C:\Users\pnovak\Pictures\ 0.3 -r");
            Console.WriteLine("\t steganalysis visualattack sun.png");
        }
    }
}
