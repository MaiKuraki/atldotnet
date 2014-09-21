using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace ATL.PlaylistReaders.BinaryLogic
{
    /// <summary>
    /// XSPF (spiff) playlist reader
    /// </summary>
    public class XSPFReader : PlaylistReader
    {

        public override void GetFiles(FileStream fs, ref IList<String> result)
        {
            Uri uri;
            XmlTextReader source = new XmlTextReader(fs);
            bool inPlaylist = false;
            bool inTracklist = false;
            bool inTrack = false;
            bool inLocation = false;

            while (source.Read())
            {
                switch (source.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (source.Name.Equals("playlist", StringComparison.OrdinalIgnoreCase))
                        {
                            inPlaylist = true;
                        }
                        else if (inPlaylist && source.Name.Equals("tracklist", StringComparison.OrdinalIgnoreCase))
                        {
                            inTracklist = true;
                        }
                        else if (inTracklist && source.Name.Equals("track", StringComparison.OrdinalIgnoreCase))
                        {
                            inTrack = true;
                        }
                        else if (inTrack && source.Name.Equals("location", StringComparison.OrdinalIgnoreCase))
                        {
                            inLocation = true;
                        }
                        break;

                    case XmlNodeType.Text: //Display the text in each element.
                        if (inLocation)
                        {
                            uri = new Uri(source.Value);
                            if (uri.IsFile)
                            {
                                result.Add(System.IO.Path.GetFullPath(uri.LocalPath));
                            }
                            else // other protocols (e.g. HTTP)
                            {
                                //TODO
                            }
                        }
                        break;

                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (source.Name.Equals("playlist", StringComparison.OrdinalIgnoreCase))
                        {
                            inPlaylist = false;
                        }
                        else if (source.Name.Equals("tracklist", StringComparison.OrdinalIgnoreCase))
                        {
                            inTracklist = false;
                        }
                        else if (source.Name.Equals("track", StringComparison.OrdinalIgnoreCase))
                        {
                            inTrack = false;
                        }
                        else if (source.Name.Equals("location", StringComparison.OrdinalIgnoreCase))
                        {
                            inLocation = false;
                        }
                        break;
                }
            }

            source.Close();
        }
    }
}
