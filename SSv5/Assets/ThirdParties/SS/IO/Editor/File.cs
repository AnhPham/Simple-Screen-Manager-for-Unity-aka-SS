/**
 * @author Anh Pham (Zenga)
 * @email anhpt.csit@gmail.com, anhpt@zenga.com.vn
 * @date 2024/03/29
 */

using UnityEngine;

namespace SS.IO
{
    public class File
    {
        public static string Copy(string sourceFileName, string targetRelativePath, bool replaceExistFile = true)
        {
            string targetFullPath = System.IO.Path.Combine(Application.dataPath, targetRelativePath);

            string directoryPath = System.IO.Path.GetDirectoryName(targetFullPath);
            string templatePath = SS.IO.Searcher.SearchFileInProject(sourceFileName);

            if (templatePath == null)
            {
                return null;
            }

            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }

            if (System.IO.File.Exists(targetFullPath))
            {
                if (replaceExistFile)
                {
                    System.IO.File.Delete(targetFullPath);
                }
            }

            if (!System.IO.File.Exists(targetFullPath))
            {
                UnityEditor.FileUtil.CopyFileOrDirectory(templatePath, targetFullPath);
            }

            return targetFullPath;
        }

        public static void ReplaceFileContent(string fullPath, string oldString, string newString)
        {
            string fileContents = System.IO.File.ReadAllText(fullPath);
            fileContents = fileContents.Replace(oldString, newString);
            System.IO.File.WriteAllText(fullPath, fileContents);
        }
    }
}
