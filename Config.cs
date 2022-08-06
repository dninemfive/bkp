using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bkp
{
    public class Config
    {
        public string Name { get; private set; }
        public List<string> SourceFolders = new();
        public string DestinationFolder;
        public Config(string name, List<string> sources, string destination)
        {
            Name = name;
            SourceFolders = sources;
            DestinationFolder = destination;
        }
        private long? _size = null;
        public long Size
        {
            get
            {
                if (_size is null)
                {
                    _size = 0;
                    foreach (string folder in SourceFolders) _size += Utils.CalculateSizeOf(folder);
                }
                return _size.Value;
            }
        }
    }
}
