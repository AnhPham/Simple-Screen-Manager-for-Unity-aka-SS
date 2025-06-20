/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SS.IO
{
    public class Searcher
    {
        public enum PathType
        {
            Absolute,
            Relative
        }

        private static List<FileInfo> SearchFile(DirectoryInfo dir, string fileName)
        {
            List<FileInfo> foundItems = dir.GetFiles(fileName).ToList();
            DirectoryInfo[] dis = dir.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                foundItems.AddRange(SearchFile(di, fileName));
            }

            return foundItems;
        }

        public static string SearchFileInProject(string fileName, PathType pathType = PathType.Absolute)
        {
            DirectoryInfo di = new DirectoryInfo(Application.dataPath);
            List<FileInfo> fis = SearchFile(di, fileName);

            if (fis.Count >= 1)
            {
                switch (pathType)
                {
                    case PathType.Absolute:
                        return fis[0].FullName;

                    case PathType.Relative:
                        var fullPath = fis[0].FullName;
                        var assetIndex = fullPath.LastIndexOf("Assets");

                        if (assetIndex >= 0)
                        {
                            return fullPath.Substring(assetIndex);
                        }
                        return fullPath;
                }
            }

            return null;
        }
    }
}
