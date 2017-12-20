﻿using DEM.Net.Lib;
using DEM.Net.Lib.Services;
using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Net;
using System.IO.Compression;
using System.Xml;
using System.Diagnostics;
using System.Windows.Media;

namespace SampleApp
{
    class Program
    {
        const string DATA_DIR = @"C:\Repos\DEM.Net\Data";

        [STAThread]
        static void Main(string[] args)
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            IGeoTiffService geoTiffService = new GeoTiffService(GeoTiffReaderType.LibTiff, DATA_DIR);
            ElevationService elevationService = new ElevationService(geoTiffService);

            //geoTiffService.GenerateDirectoryMetadata(DEMDataSet.AW3D30, false, true);
            // geoTiffService.GenerateFileMetadata(@"C:\Users\xfischer\AppData\Roaming\DEM.Net\ETOPO1\ETOPO1_Ice_g_geotiff.tif", false, false);

            //SpatialTrace_GeometryWithDEMGrid(elevationService, geoTiffService, WKT_TEST, DEMDataSet.AW3D30);

            LineDEMBenchmark(elevationService, DEMDataSet.AW3D30, 512);
            //LineDEMTest(elevationService, DEMDataSet.AW3D30, WKT_BREST_NICE, 100);



            //GeoTiffBenchmark();

            //Test_GetMetadataFromVRT(elevationService, DEMDataSet.AW3D30);

            //elevationService.DownloadMissingFiles(DEMDataSet.AW3D30, GetBoundingBox(WKT_AIX_BAYONNE_EST_OUEST));
            ////elevationService.DownloadMissingFiles(DEMDataSet.SRTM_GL3_srtm, GetBoundingBox(WKT_GRAND_TRAJET_MARSEILLE_ALPES_MULTIPLE_TILES));

            ////GenerateDownloadReports(geoTiffService);

            //geoTiffService.GenerateDirectoryMetadata(DEMDataSet.AW3D30, false, false);

            //Spatial trace of line +segments + interpolated point + dem grid
            //SpatialTrace_GeometryWithDEMGrid(elevationService, geoTiffService, WKT_TEST, DEMDataSet.AW3D30);

            Console.Write("Press any key to exit...");
            Console.ReadLine();

        }

        private static void GeoTiffTests(IGeoTiffService geoTiffService, string tiffPath)
        {
            FileMetadata metaData = geoTiffService.ParseMetadata(tiffPath);
            float elevation;
            using (GeoTiff tiff = new GeoTiff(tiffPath))
            {
                elevation = tiff.ParseGeoDataAtPoint(metaData, 122, 122);
            }

        }

        private static void GeoTiffBenchmark()
        {
            DEMDataSet dataSet = DEMDataSet.AW3D30;
            ElevationService elevationServiceLibTiff = new ElevationService(new GeoTiffService(GeoTiffReaderType.LibTiff, DATA_DIR));

            string wkt = WKT_BREST_NICE;
            elevationServiceLibTiff.DownloadMissingFiles(dataSet, GetBoundingBox(wkt));

            const int NUM_ITERATIONS = 10;

            Stopwatch swCoreTiff = new Stopwatch();
            Stopwatch swGeoTiff = new Stopwatch();
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < NUM_ITERATIONS; i++)
                {


                    swGeoTiff.Start();
                    var lineElevationData = elevationServiceLibTiff.GetLineGeometryElevation(wkt, dataSet, InterpolationMode.Hyperbolic);
                    swGeoTiff.Stop();
                }
            }


            long geoTiffMs = swGeoTiff.ElapsedMilliseconds;

            long codeTiffMs = swCoreTiff.ElapsedMilliseconds;

            Console.WriteLine($"GeoTiff : {geoTiffMs} ms, Native : {codeTiffMs}");
        }

        private static void Test_GetMetadataFromVRT(ElevationService elevationService, DEMDataSet dataSet)
        {
            GDALVRTFileService gdalService = new GDALVRTFileService(elevationService.GetDEMLocalPath(dataSet), dataSet);

            gdalService.Setup(false);

            GDALSource source = gdalService.Sources().FirstOrDefault(s => s.SourceFileName.EndsWith("N043E006_AVE_DSM.tif"));



        }


        static void LineDEMBenchmark(ElevationService elevationService, DEMDataSet dataSet, int numSamples)
        {

            Dictionary<string, string> dicWktByName = new Dictionary<string, string>();
            //dicWktByName.Add(nameof(WKT_EXAMPLE_GOOGLE), WKT_EXAMPLE_GOOGLE);

            // Before GeoTiff window optim : 90s
            // After GeoTiff optim : 77s / release : 60s;


            dicWktByName.Add(nameof(WKT_BREST_NICE), WKT_BREST_NICE);
            dicWktByName.Add(nameof(WKT_HORIZONTAL_DEM_EDGE), WKT_HORIZONTAL_DEM_EDGE);
            dicWktByName.Add(nameof(WKT_VERTICAL_DEM_EDGE), WKT_VERTICAL_DEM_EDGE);
            dicWktByName.Add(nameof(WKT_MONACO), WKT_MONACO);
            dicWktByName.Add(nameof(WKT_TEST), WKT_TEST);
            dicWktByName.Add(nameof(WKT_NO_DEM), WKT_NO_DEM);
            dicWktByName.Add(nameof(WKT_ZERO), WKT_ZERO);
            dicWktByName.Add(nameof(WKT_NEG100), WKT_NEG100);
            dicWktByName.Add(nameof(WKT_BREST_SPAIN_OCEAN), WKT_BREST_SPAIN_OCEAN);
            dicWktByName.Add(nameof(WKT_EXAMPLE_GOOGLE), WKT_EXAMPLE_GOOGLE);
            dicWktByName.Add(nameof(WKT_PARIS_AIX), WKT_PARIS_AIX);
            dicWktByName.Add(nameof(WKT_PETITE_BOUCLE), WKT_PETITE_BOUCLE);
            dicWktByName.Add(nameof(WKT_GRAND_TRAJET), WKT_GRAND_TRAJET);
            dicWktByName.Add(nameof(WKT_GRAND_TRAJET_MARSEILLE_ALPES_MULTIPLE_TILES), WKT_GRAND_TRAJET_MARSEILLE_ALPES_MULTIPLE_TILES);
            dicWktByName.Add(nameof(WKT_BAYONNE_AIX_OUEST_EST), WKT_BAYONNE_AIX_OUEST_EST);
            dicWktByName.Add(nameof(WKT_AIX_BAYONNE_EST_OUEST), WKT_AIX_BAYONNE_EST_OUEST);
            dicWktByName.Add(nameof(WKT_BAYONNE_NICE_DIRECT), WKT_BAYONNE_NICE_DIRECT);
            dicWktByName.Add(nameof(WKT_DEM_INTERPOLATION_BUG), WKT_DEM_INTERPOLATION_BUG);

            Stopwatch sw = Stopwatch.StartNew();

            InterpolationMode[] modes = { InterpolationMode.Bilinear, InterpolationMode.Hyperbolic };
            for (int i = 0; i < 5; i++)
            {
                foreach (var wkt in dicWktByName)
                {
                    elevationService.DownloadMissingFiles(dataSet, GetBoundingBox(wkt.Value));

                    foreach (InterpolationMode mode in modes)
                    {
                        var lineElevationData = elevationService.GetLineGeometryElevation(wkt.Value, dataSet, mode);
                        lineElevationData = GeometryService.ComputePointsDistances(lineElevationData);
                        //var sampledLineElevationData = ReduceList(lineElevationData, numSamples).ToList();
                        //File.WriteAllText($"ElevationData_{wkt.Key}_{mode}.txt", elevationService.ExportElevationTable(lineElevationData));
                        //File.WriteAllText($"ElevationData_{wkt.Key}_{mode}_{numSamples}samples.txt", elevationService.ExportElevationTable(sampledLineElevationData));
                    }
                }
            }

            sw.Stop();
            Console.WriteLine($"LineDEMTests performed in {sw.Elapsed:g}.");
        }

        static void LineDEMTest(ElevationService elevationService, DEMDataSet dataSet, string wkt, int numSamples)
        {

            Stopwatch sw = Stopwatch.StartNew();

            elevationService.DownloadMissingFiles(dataSet, GetBoundingBox(wkt));


            var lineElevationData = elevationService.GetLineGeometryElevation(wkt, dataSet, InterpolationMode.Hyperbolic);
            lineElevationData = GeometryService.ComputePointsDistances(lineElevationData);
            //var sampledLineElevationData = ReduceList(lineElevationData, numSamples).ToList();
            File.WriteAllText($"ElevationData_LineDEMTesttest.txt", elevationService.ExportElevationTable(lineElevationData));
            //File.WriteAllText($"ElevationData_{wkt.Key}_{mode}_{numSamples}samples.txt", elevationService.ExportElevationTable(sampledLineElevationData));


            sw.Stop();
            Console.WriteLine($"LineDEMTest performed in {sw.Elapsed:g}.");
        }

        private static List<GeoPoint> ReduceList(List<GeoPoint> lineElevationData, int numSamples)
        {
            // Aim is to use DouglasPeucker to reduce points without getting rid of peaks and other steep changes
            // Let's create a geometry with x = distnace from origin and y = elevation
            // and run douglas peucker on it

            //SqlGeometryBuilder gb = new SqlGeometryBuilder();
            //gb.SetSrid(0); // custom SRID
            //gb.BeginGeometry(OpenGisGeometryType.LineString);

            //gb.BeginFigure(lineElevationData[0].DistanceFromOriginMeters, lineElevationData[0].Altitude.GetValueOrDefault(0));
            //for (int i = 1; i < lineElevationData.Count; i++)
            //{
            //    gb.AddLine(lineElevationData[i].DistanceFromOriginMeters, lineElevationData[i].Altitude.GetValueOrDefault(0));
            //}
            //gb.EndFigure();
            //gb.EndGeometry();
            //SqlGeometry geom = gb.ConstructedGeometry;

            float maxElevation = lineElevationData.Max(pt => pt.Altitude.GetValueOrDefault(0));

            double reduceFactor = 10 * ((double)numSamples / (double)(maxElevation == 0 ? float.MaxValue : maxElevation));
            //SqlGeometry geomReduced = geom.Reduce(reduceFactor);
            //SpatialTrace.Enable();
            //SpatialTrace.Clear();
            //SpatialTrace.TraceGeometry(geom, "Geom");
            //SpatialTrace.TraceGeometry(geomReduced, "Geom reduced");
            //SpatialTrace.ShowDialog();

            // List<GeoPoint> reducedList = new DouglasPeucker(lineElevationData, reduceFactor * 1.8).Compress();
            int chunksize = lineElevationData.Count / numSamples;
            if (lineElevationData.Count <= numSamples)
            {
                return lineElevationData;
            }

            List<GeoPoint> result = GetNth(lineElevationData, chunksize).ToList();
            return result;
        }

        private static IEnumerable<T> GetNth<T>(List<T> list, int n)
        {
            for (int i = 0; i < list.Count; i += n)
                yield return list[i];

            if (list.Count % n != 0)
            {
                yield return list.Last();
            }
        }

        static DEM.Net.Lib.BoundingBox GetBoundingBox(string wkt, double buffer = 60)
        {
            SqlGeometry geom = GeometryService.ParseWKTAsGeometry(wkt);
            return geom.ToGeography().STBuffer(60).GetBoundingBox();
        }

        static void SpatialTrace_GeometryWithDEMGrid(ElevationService elevationService, IGeoTiffService geoTiffService, string wktBbox, DEMDataSet dataSet)
        {
            SpatialTrace.Enable();
            DEM.Net.Lib.BoundingBox bbox = null;
            if (wktBbox != null)
            {
                SqlGeometry geom = GeometryService.ParseWKTAsGeometry(wktBbox);
                SpatialTrace.TraceGeometry(geom, "Bbox");
                bbox = geom.ToGeography().STBuffer(60).GetBoundingBox();

                //SpatialTrace.Indent("Line Segments");
                //int i = 0;
                //foreach (var seg in geom.Segments())
                //{
                //	i++;
                //	Color color = (i % 2 == 0) ? Colors.Blue : Colors.Red;
                //	SpatialTrace.SetLineColor(color);
                //	SpatialTrace.TraceGeometry(seg, "Seg" + i, "Seg" + i);
                //}

                SpatialTrace.Unindent();
            }




            Dictionary<string, DemFileReport> tiles = geoTiffService.GenerateReport(dataSet, bbox);

            SpatialTrace.Indent("DEM tiles");
            SpatialTrace.SetLineColor(Colors.Black);
            foreach (var tile in tiles)
            {
                SpatialTrace.SetFillColor(tile.Value.IsExistingLocally ? Color.FromArgb(128, 0, 255, 0) : Color.FromArgb(128, 255, 0, 0));

                SqlGeometry tileBbox = tile.Value.Source.BBox.AsGeomety();
                SpatialTrace.TraceGeometry(tileBbox, $"{tile.ToString()}");
            }
            SpatialTrace.Unindent();

            // View spatial trace in bin\debug with spatial trace viewer
            SpatialTrace.ShowDialog();
            SpatialTrace.Disable();

        }

        #region Sample WKT
        const string WKT_HORIZONTAL_DEM_EDGE = "LINESTRING (6.1 43.99, 6.9 44.01)";
        const string WKT_VERTICAL_DEM_EDGE = "LINESTRING (6.001 43.1, 5.99 43.9)";
        const string WKT_MONACO = "LINESTRING (7.0806884765625 45.37916094640917, 7.48443603515625 43.77307711737606)";
        const string WKT_TEST = "LINESTRING (8.668212890625 40.58058466412761, 5.438232421875 43.389081939117496)";
        const string WKT_NO_DEM = "LINESTRING (7.492675781249999 41.582579601430346, 7.5146484375 39.791654835253425)";
        const string WKT_ZERO = "LINESTRING(-4.36763906478882	48.4062232971191, -4.35986089706421	48.4028930664063)";
        const string WKT_NEG100 = "LINESTRING(-4.04486131668091	48.2679634094238, -4.03826761245728	48.2651405334473)";
        const string WKT_BREST_SPAIN_OCEAN = "LINESTRING(-4.519500732421875 43.373509919227104,-4.47418212890625 48.39966209090939)";
        const string WKT_EXAMPLE_GOOGLE = "LINESTRING(-118.291994 36.578581,-116.83171 36.23998)";
        const string WKT_BREST_NICE = "LINESTRING(-4.482421875 48.45539196446375,6.943359375 43.5612374716474)";
        const string WKT_PARIS_AIX = "LINESTRING(2.340087890625 48.87047363512827,2.362060546875 48.71124007358497,2.581787109375 48.376663195419056,3.043212890625 48.091266595037794,3.31787109375 47.92220925866507,3.779296875 47.789516887184,4.010009765625 47.48600498307925,4.39453125 47.426577530514564,4.833984375 47.068601854632306,4.833984375 46.86617699946977,4.921875 46.35297057957134,4.735107421875 46.102161444290594,4.779052734375 45.80427288878466,4.81201171875 45.46627091868822,4.910888671875 44.9321165649521,4.7021484375 44.282939125313995,4.888916015625 44.01491649204199,5.1416015625 43.554893125282966,5.460205078125 43.53896711771029)";
        const string WKT_GRANDE_BOUCLE = "LINESTRING(5.4471588134765625 43.54239685275213,5.447598695755005 43.54232686010967,5.448499917984009 43.54240462970736,5.450270175933838 43.54189912552898,5.451364517211914 43.54155693567945,5.451589822769165 43.54143250252535,5.452630519866943 43.5414247254447,5.452491044998169 43.54078700141682,5.453864336013794 43.54060034920576,5.454164743423462 43.541385840026344,5.455001592636108 43.54261460712012,5.455162525177002 43.54268459942852,5.455119609832764 43.54283236070161,5.454883575439453 43.54277014547287,5.453628301620483 43.54329897287061,5.452373027801514 43.54342340217237,5.452415943145752 43.54387445623844,5.452297925949097 43.544457710803165,5.452029705047607 43.54497874677534,5.452061891555786 43.545134279028574,5.4524266719818115 43.545305364043664,5.452308654785156 43.54640185193005,5.45246958732605 43.54694620020958,5.452094078063965 43.54795711968697,5.452297925949097 43.54830704940204,5.451525449752808 43.54905355933778,5.451074838638306 43.549014678840194,5.450838804244995 43.54906133543429,5.4500555992126465 43.54995557984124,5.449733734130859 43.550849810980594,5.449916124343872 43.55092756958308,5.450259447097778 43.55180623481941,5.450087785720825 43.552140590792334,5.449733734130859 43.55249827187253,5.449519157409668 43.553019238343815,5.4480063915252686 43.55332248541017,5.447877645492554 43.5532680565619,5.4470837116241455 43.55270043850059,5.447072982788086 43.55255270141602,5.449637174606323 43.55090424201289,5.449744462966919 43.55084203511484,5.449948310852051 43.55034437761901,5.450077056884766 43.549940027877945,5.450377464294434 43.549574555585714,5.448800325393677 43.54892136554366,5.448875427246094 43.54864142478726,5.448735952377319 43.54834593035609,5.448349714279175 43.54784047599715,5.447491407394409 43.54718726716287,5.44721245765686 43.54707062198327,5.446654558181763 43.5473272410804,5.44622540473938 43.547412780536675,5.4457855224609375 43.54737389898068,5.445528030395508 43.547412780536675,5.445120334625244 43.54584972222492,5.445195436477661 43.54501762987626,5.445624589920044 43.54444215742132,5.445399284362793 43.542715707095056,5.445302724838257 43.5425912763326,5.445420742034912 43.54254461473046,5.445570945739746 43.54261460712012,5.4471588134765625 43.54239685275213)";
        const string WKT_PETITE_BOUCLE = "LINESTRING(5.44771671295166 43.54234241403722,5.450162887573242 43.54196911866801,5.452415943145752 43.541378062939685,5.452544689178467 43.54070922973245,5.453896522521973 43.54063145794773,5.454990863800049 43.542715707095056,5.453681945800781 43.543275642347915,5.4500555992126465 43.544177749316354,5.448317527770996 43.544768777596964,5.445957183837891 43.54703951656395,5.44546365737915 43.547506096168696,5.445120334625244 43.54526648112833,5.445678234100342 43.544177749316354,5.445399284362793 43.54256016860185,5.44771671295166 43.54234241403722)";
        const string WKT_GRAND_TRAJET = "LINESTRING(5.447738170623779 43.54234241403722,5.4471588134765625 43.54238907579587,5.4467082023620605 43.54249795309221,5.445442199707031 43.54263793789861,5.445570945739746 43.544457710803165,5.445120334625244 43.545313140623726,5.445120334625244 43.54605968763827,5.4454851150512695 43.54755275393054,5.445050597190857 43.54756830650979,5.444814562797546 43.54747499097408,5.44447660446167 43.547319464760186,5.4441118240356445 43.547319464760186,5.443317890167236 43.54747110282363,5.44297456741333 43.54743610945823,5.442625880241394 43.54730002395522,5.442357659339905 43.54709395103727,5.4420894384384155 43.54691898291237,5.441558361053467 43.54686066009121,5.441048741340637 43.54690731835265,5.440464019775391 43.54705895745292,5.440303087234497 43.547163938145005,5.440260171890259 43.54748665542395,5.4400938749313354 43.54761107608205,5.439755916595459 43.547661621901064,5.439584255218506 43.54812042047349,5.439535975456238 43.548551998993865,5.439498424530029 43.549636763791966,5.439112186431885 43.55005666750473,5.436794757843018 43.55179845907704,5.4358720779418945 43.55228055320695,5.435550212860107 43.552902604450466,5.435013771057129 43.553275832114664,5.434691905975342 43.553213627664476,5.434584617614746 43.55259157963129,5.434906482696533 43.552296104566295,5.434370040893555 43.55102087977367,5.433297157287598 43.550538775567375,5.431044101715088 43.54999445973189,5.429842472076416 43.549870043993536,5.428211688995361 43.55019663475889,5.427310466766357 43.55010332329225,5.426774024963379 43.54962121174641,5.426902770996094 43.548936917769765,5.427439212799072 43.5485014539215,5.4283833503723145 43.54730391211671,5.432267189025879 43.54119141255846,5.432717800140381 43.54008705264584,5.432631969451904 43.53862491121964,5.4320526123046875 43.53730273153031,5.426838397979736 43.52692650336303,5.426516532897949 43.52547960110387,5.426580905914307 43.52390819482177,5.427052974700928 43.522072242564995,5.428190231323242 43.51914705066751,5.428404808044434 43.5178400041918,5.428125858306885 43.51623727760756,5.427138805389404 43.51440109191845,5.4253363609313965 43.51287608167312,5.423233509063721 43.51200462994486,5.419907569885254 43.51124209936016,5.416581630706787 43.51010606653939,5.41301965713501 43.508300819714854,5.411109924316406 43.50694684916336,5.409865379333496 43.50582629884247,5.408470630645752 43.50398979639134,5.406968593597412 43.50089251739086,5.406453609466553 43.49846438857513,5.406582355499268 43.49513333506259,5.406625270843506 43.49427719349613,5.406560897827148 43.49329650733292,5.4062819480896 43.49320310782013,5.405981540679932 43.49331207390435,5.404994487762451 43.49480644607877,5.404157638549805 43.495569184288435,5.402441024780273 43.4985733451658,5.402162075042725 43.49805969094556,5.401604175567627 43.49712576298661,5.401303768157959 43.49588050323809,5.401153564453125 43.494977673862394,5.401260852813721 43.49385690137624,5.400681495666504 43.49317197461708,5.400488376617432 43.49187993253735,5.400831699371338 43.49077466978604,5.401153564453125 43.48904668326483,5.400295257568359 43.4879102327493,5.399415493011475 43.48706955696546,5.398836135864258 43.48649353161793,5.396690368652344 43.483021694960314,5.394952297210693 43.48095095299566,5.39379358291626 43.47917597482142,5.388686656951904 43.48093538323836,5.384116172790527 43.48292827955829,5.3827643394470215 43.484236082041896,5.38029670715332 43.482165381701805,5.37954568862915 43.481277916971784,5.377528667449951 43.479425097710845,5.376412868499756 43.47880228856128,5.375125408172607 43.47782135113263,5.373859405517578 43.47853759272874,5.372421741485596 43.47861544456456,5.371134281158447 43.478989131980235,5.369589328765869 43.47771235710197,5.368494987487793 43.47705838878934,5.368280410766602 43.47696496416688,5.367722511291504 43.476575693351926,5.368366241455078 43.474707158532745,5.368945598602295 43.47375729784957)";
        const string WKT_GRAND_TRAJET_MARSEILLE_ALPES_MULTIPLE_TILES = "LINESTRING(5.38330078125 43.27920492608278,5.36407470703125 43.305193797650546,5.3778076171875 43.32717570677798,5.350341796875 43.38508989465155,5.3778076171875 43.42699324866588,5.41900634765625 43.488797600050006,5.4327392578125 43.518680251604984,5.46844482421875 43.58039085560783,5.49591064453125 43.62215891380658,5.4876708984375 43.661911057260674,5.548095703125 43.655949912568225,5.6304931640625 43.65793702655821,5.6689453125 43.685749717616766,5.73760986328125 43.70163689691259,5.9051513671875 43.935483850319756,5.9820556640625 44.12308489306967,5.91064453125 44.315987905196906,5.9710693359375 44.422011314236634,6.053466796875 44.484749436619964,6.082305908203125 44.562098859191025,6.087799072265625 44.66083904265623,6.0150146484375 44.72575949075228,6.0047149658203125 44.76014269639826,6.040763854980469 44.77951995657652,6.079730987548828 44.791886317441076,6.115350723266602 44.81341451906245,6.139340400695801 44.825636646910226,6.173994541168213 44.81908450825152,6.222563982009888 44.822139982642284,6.260833740234375 44.82763029742813)";
        const string WKT_BAYONNE_AIX_OUEST_EST = "LINESTRING(-1.56005859375 43.444942955261254,-1.263427734375 43.548548110912854,-0.87890625 43.51668853502907,-0.428466796875 43.35713822211053,-0.10986328125 43.24520272203356,0.28564453125 43.17313537107136,0.41748046875 43.092960677116295,0.999755859375 43.12504316740127,1.1865234375 43.28520334369384,1.439208984375 43.620170616189895,1.571044921875 43.476840397778936,2.032470703125 43.229195113965,2.65869140625 43.15710884095329,2.999267578125 43.14909399920127,3.438720703125 43.36512572875844,3.97705078125 43.644025847699496,4.361572265625 43.81867485545321,4.658203125 43.65197548731187,5.130615234375 43.620170616189895,5.42724609375 43.532620426810105)";
        const string WKT_AIX_BAYONNE_EST_OUEST = "LINESTRING(5.44921875 43.532620426810105,5.152587890625 43.628123412124616,4.647216796875 43.67581809328341,4.306640625 43.84245116699039,3.93310546875 43.644025847699496,3.2080078125 43.30919109985686,2.96630859375 43.12504316740127,2.30712890625 43.189157696549216,1.669921875 43.40504748787035,1.439208984375 43.65197548731187,1.318359375 43.46089378008257,1.16455078125 43.27720532212024,0.933837890625 43.117024121350475,0.384521484375 43.08493742707592,0.142822265625 43.205175817237304,-0.3955078125 43.34116005412307,-1.07666015625 43.54058479482877,-1.494140625 43.48481212891604)";
        const string WKT_BAYONNE_NICE_DIRECT = "LINESTRING(-1.51611328125 43.50075243569041,7.261962890625 43.711564246658504)";
        const string WKT_DEM_INTERPOLATION_BUG = "LINESTRING(5.42859268188477 43.5304183959961, 5.42845249176025 43.5301399230957,5.4283127784729 43.5298614501953,5.42817258834839 43.5295829772949,5.42803287506104 43.5293045043945,5.42789268493652 43.5290260314941,5.4277548789978 43.528751373291,5.42761468887329 43.5284729003906)";

        const string WKT_POLY_FRANCE = "POLYGON ((-6.328125 41.21172151054787, 10.01953125 41.21172151054787, 10.01953125 51.37178037591737, -6.328125 51.37178037591737, -6.328125 41.21172151054787))";


        #endregion
    }
}