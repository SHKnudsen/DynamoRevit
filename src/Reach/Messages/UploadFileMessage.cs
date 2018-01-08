﻿using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Reach.Execution;


namespace Reach.Messages
{
    /// <summary>
    /// This message allows to open the file received as a byte array 
    /// or to open a file from <see cref="Path"/> if it is specified
    /// </summary>
    /// <example>
    /// This sample shows Json structure
    /// </example>
    /// <code>
    /// {
    ///     "type":"UploadFileMessage",
    ///     "Path":"c:\\Home.dyn"
    /// }
    /// </code>
    [DataContract]
    public class UploadFileMessage : Message
    {
        public string FileName { get; private set; }
        public bool IsCustomNode { get; private set; }
        public byte[] FileContent { get; private set; }
        
        /// <summary>
        /// Path to the specified .dyn or .dyf file
        /// </summary>
        [DataMember]
        public string Path
        {
            get { return path; }
            set
            {
                if (IsValidDynamoFilePath(value))
                {
                    path = value;
                    IsCustomNode = value.EndsWith(".dyf");
                }
            }
        }

        private string path;

        public static bool IsValidDynamoFilePath(string toCheck)
        {
            try
            {
                var extension = System.IO.Path.GetExtension(toCheck);
                switch (extension.ToLower())
                {
                    case ".dyn":
                    case ".dyf":
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        
        public UploadFileMessage(byte[] content)
        {
            FileContent = content;
            FileName = GetFileName();
        }

        public UploadFileMessage() { }
        
        /// <summary>
        /// Gets the file name from the file content
        /// </summary>
        /// <returns>The file name with correct extension</returns>
        private string GetFileName()
        {
            var xmlDoc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(FileContent))
            {
                xmlDoc.Load(ms);
            }

            string name = null;
            
            var topNode = xmlDoc.GetElementsByTagName("Workspace");

            // legacy support
            if (topNode.Count == 0)
            {
                topNode = xmlDoc.GetElementsByTagName("dynWorkspace");
            }

            bool hasID = false;
            // find workspace name
            foreach (XmlNode node in topNode)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (att.Name.Equals("Name"))
                        name = att.Value;
                    else if (att.Name.Equals("ID"))
                        hasID = !string.IsNullOrEmpty(att.Value);
                }
            }

            IsCustomNode = hasID || name != "Home";
            return IsCustomNode ? name + ".dyf" : "Home.dyn";
        }

        internal override void Execute(MessageHandler handler)
        {
            handler.UploadFile(this);
        }
    }
}
