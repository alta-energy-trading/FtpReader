using Reader.Core;
using System.Collections.Generic;

namespace FtpReader
{
    public class FtpConsumerArgs : ConsumerArgs
    {
        public string Host { get; set; }
        public string LocalPath { get; set; }
        public string RemotePath { get; set; }
        public IEnumerable<string> Filenames { get; set; }
        public string Filter { get; set; }
        public bool GetLatest { get; set; }
        public ProtocolEnum Protocol { get; set; }
    }
}
