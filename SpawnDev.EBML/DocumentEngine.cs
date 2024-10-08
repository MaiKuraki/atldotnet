﻿using System.Linq;

namespace SpawnDev.EBML
{
    /// <summary>
    /// Base class for EBML document engine
    /// </summary>
    public abstract class DocumentEngine
    {
        /// <summary>
        /// DocTypes this engine supports
        /// </summary>
        public abstract string[] DocTypes { get; }
        /// <summary>
        /// Returns true if this engine supports the current DocType
        /// </summary>
        public bool DocTypeSupported => DocTypes.Contains(Document.DocType);
        /// <summary>
        /// The EBML document this engine is attached to
        /// </summary>
        public Document Document { get; private set; }
        /// <summary>
        /// Required EBMLDocumentEngine constructor
        /// </summary>
        /// <param name="document"></param>
        public DocumentEngine(Document document)
        {
            Document = document;
        }
    }
}
