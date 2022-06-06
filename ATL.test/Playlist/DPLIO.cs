﻿using ATL.Playlist;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace ATL.test.IO.Playlist
{
    [TestClass]
    public class DPLIO
    {
        [TestMethod]
        public void PLIO_R_DPL()
        {
            string testFileLocation = TestUtils.CopyFileAndReplace(TestUtils.GetResourceLocationRoot() + "_Playlists/playlist.dpl", "$PATH", TestUtils.GetResourceLocationRoot(false));

            try
            {
                IPlaylistIO pls = PlaylistIOFactory.GetInstance().GetPlaylistIO(testFileLocation);

                Assert.IsNotInstanceOfType(pls, typeof(ATL.Playlist.IO.DummyIO));
                Assert.AreEqual(4, pls.FilePaths.Count);
                foreach (string s in pls.FilePaths) Assert.IsTrue(File.Exists(s));
                foreach (Track t in pls.Tracks) Assert.IsTrue(t.Duration > 0); // Ensures the track has been parsed
            }
            finally
            {
                if (Settings.DeleteAfterSuccess) File.Delete(testFileLocation);
            }
        }

        [TestMethod]
        public void PLIO_W_DPL()
        {
            IList<string> pathsToWrite = new List<string>();
            pathsToWrite.Add("aaa.mp3");
            pathsToWrite.Add("bbb.mp3");

            IList<Track> tracksToWrite = new List<Track>();
            tracksToWrite.Add(new Track(Path.Combine(TestUtils.GetResourceLocationRoot() + "MP3", "empty.mp3")));
            tracksToWrite.Add(new Track(Path.Combine(TestUtils.GetResourceLocationRoot() + "MOD", "mod.mod")));


            string testFileLocation = TestUtils.CreateTempTestFile("test.dpl");
            try
            {
                IPlaylistIO pls = PlaylistIOFactory.GetInstance().GetPlaylistIO(testFileLocation);

                // Test Path writing
                pls.FilePaths = pathsToWrite;

                using (FileStream fs = new FileStream(testFileLocation, FileMode.Open))
                {
                    // Test if the default UTF-8 BOM has been written at the beginning of the file
                    byte[] bom = new byte[3];
                    fs.Read(bom, 0, 3);
                    Assert.IsTrue(StreamUtils.ArrEqualsArr(bom, PlaylistIO.BOM_UTF8));
                    fs.Seek(0, SeekOrigin.Begin);

                    using (StreamReader sr = new StreamReader(fs))
                    {
                        Assert.AreEqual("DAUMPLAYLIST", sr.ReadLine());
                        Assert.AreEqual("topindex=0", sr.ReadLine());
                        Assert.AreEqual("saveplaypos=0", sr.ReadLine());
                        Assert.AreEqual("playtime=0", sr.ReadLine());
                        Assert.AreEqual("1*file*aaa.mp3", sr.ReadLine());
                        Assert.AreEqual("2*file*bbb.mp3", sr.ReadLine());
                        Assert.IsTrue(sr.EndOfStream);
                    }
                }
                IList<string> filePaths = pls.FilePaths;
                Assert.AreEqual(2, filePaths.Count);
                Assert.IsTrue(filePaths[0].EndsWith(pathsToWrite[0]));
                Assert.IsTrue(filePaths[1].EndsWith(pathsToWrite[1]));


                // Test Track writing
                pls.Tracks = tracksToWrite;

                using (FileStream fs = new FileStream(testFileLocation, FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                {
                    Assert.AreEqual("DAUMPLAYLIST", sr.ReadLine());
                    Assert.AreEqual("topindex=0", sr.ReadLine());
                    Assert.AreEqual("saveplaypos=0", sr.ReadLine());
                    Assert.AreEqual("playtime=334106", sr.ReadLine());
                    int counter = 1;
                    foreach (Track t in tracksToWrite)
                    {
                        Assert.AreEqual(counter + "*file*" + t.Path, sr.ReadLine());
                        Assert.AreEqual(counter + "*title*" + t.Title, sr.ReadLine());
                        Assert.AreEqual(counter + "*duration2*" + (long)t.DurationMs, sr.ReadLine());
                        counter++;
                    }
                    sr.ReadLine();
                    Assert.IsTrue(sr.EndOfStream);
                }

                IList<Track> tracks = pls.Tracks;
                Assert.AreEqual(2, tracks.Count);
                Assert.AreEqual(tracksToWrite[0].Path, tracks[0].Path);
                Assert.AreEqual(tracksToWrite[1].Path, tracks[1].Path);
            }
            finally
            {
                if (Settings.DeleteAfterSuccess) File.Delete(testFileLocation);
            }
        }
    }
}