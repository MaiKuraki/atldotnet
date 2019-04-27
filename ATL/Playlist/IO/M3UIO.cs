using System;
using System.IO;
using System.Collections;
using System.Text;
using ATL.Logging;
using System.Collections.Generic;

namespace ATL.Playlist.IO
{
    /// <summary>
    /// M3U/M3U8 playlist manager
    /// </summary>
    public class M3UIO : PlaylistIO
    {
        private Encoding getEncoding(FileStream fs)
        {
            if (System.IO.Path.GetExtension(FFileName).ToLower().Equals(".m3u8"))
            {
                return System.Text.Encoding.UTF8;
            }
            else
            {
                return StreamUtils.GetEncodingFromFileBOM(fs);
            }
        }

        protected override void getFiles(FileStream fs, IList<string> result)
        {
            Encoding encoding = getEncoding(fs);

            using (TextReader source = new StreamReader(fs, encoding))
            {
                string s = source.ReadLine();
                while (s != null)
                {
                    // If the read line isn't a metadata, it's a file path
                    if ((s.Length > 0) && (s[0] != '#'))
                    {
                        if (!System.IO.Path.IsPathRooted(s))
                        {
                            s = System.IO.Path.GetDirectoryName(FFileName) + System.IO.Path.DirectorySeparatorChar + s;
                        }
                        result.Add(System.IO.Path.GetFullPath(s));
                    }
                    s = source.ReadLine();
                }
            }
        }

        protected override void setTracks(FileStream fs, IList<Track> values)
        {
            Encoding encoding = getEncoding(fs);

            using (TextWriter w = new StreamWriter(fs, encoding))
            {
                if (Settings.M3U_useExtendedFormat) w.WriteLine("#EXTM3U");

                foreach (Track t in values)
                {
                    if (Settings.M3U_useExtendedFormat)
                    {
                        w.Write("#EXTINF:");
                        if (t.Duration > 0) w.Write(t.Duration); else w.Write(-1);
                        w.Write(",");
                        string label = "";
                        if (t.Artist != null && t.Artist.Length > 0) label = t.Artist + " - ";
                        if (t.Title != null && t.Title.Length > 0) label += t.Title;
                        if (0 == label.Length) label = System.IO.Path.GetFileNameWithoutExtension(t.Path);
                        w.WriteLine(label);
                        w.WriteLine(t.Path); // Can be rooted or not
                    } else
                    {
                        w.WriteLine(t.Path); // Can be rooted or not
                    }
                }
            }
        }
    }
}
