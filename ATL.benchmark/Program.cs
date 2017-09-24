﻿using BenchmarkDotNet.Running;
using System;
using System.IO;

namespace ATL.benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<BufferSizes>();

            //BenchmarkRunner.Run<ATLOld_ATLNew>();

            //BenchmarkRunner.Run<ATL_TagLib>();

            //BenchmarkRunner.Run<Misc>();

            //readAt(@"E:\Dev\Source\Repos\atldotnet\ATL.test\Resources\OGG\ogg.ogg");

            //readAt(@"E:\Dev\Source\Repos\atldotnet\ATL.test\Resources\OGG\ogg.ogg");

            //readAt(@"E:\Dev\Source\Repos\atldotnet\ATL.test\Resources\MP3\id3v2.4_UTF8.mp3");

            //readAt(@"E:\Dev\Source\Repos\atldotnet\ATL.test\Resources\MP3\id3v2.4_UTF8.mp3");


            //readAt(@"E:\temp\ogg\lastHeaderInvalidTypeFlag.OGG");

            //compareInfo(@"E:\Music\VGM");
        }

        static private void readAt(string filePath, bool useTagLib = false)
        {
            FileFinder ff = new FileFinder();

//            Console.WriteLine(filePath);

            if (File.Exists(filePath))
            {
                Track t = new Track(filePath);
                //t.GetEmbeddedPicture(useOldImplementation, false);

//                Console.WriteLine(t.Title);
            }
            else if (Directory.Exists(filePath))
            {
                ff.FF_BrowseATLAudioFiles(filePath, false, true, true);
                if (useTagLib)
                {
                    Console.WriteLine("________________________________________________________");
                    ff.FF_BrowseTagLibAudioFiles(filePath, true, true);
                }
            }

//            Console.WriteLine("end");
//            Console.ReadLine();
        }
        
    }
}