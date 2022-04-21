using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bkp
{
    public static class Index
    {
        public static Dictionary<byte[], FileAlias> Aliases { get; private set; }
        public static void Index(string folderPath)
        {

        }
        public static void Add(string path)
        {
            FileHash fileHash = new(path);
            if(Aliases.ContainsKey(fileHash.Hash))
            {
                Aliases[fileHash.Hash].Add(fileHash);
            } else
            {
                Aliases[fileHash.Hash] = new FileAlias(fileHash);
            }
        }
    }
}
