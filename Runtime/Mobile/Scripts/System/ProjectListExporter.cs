using System.IO;
using UnityEditor;
using UnityEngine;

namespace Twinny.System
{

    public static class ProjectListExporter
    {
        public static void Export(ProjectList projectList, string savePath)
        {
            foreach (var project in projectList.projects)
            {
                project.thumbnailURL = project.thumbnail ? AssetDatabase.GetAssetPath(project.thumbnail) : "";
                project.imageGaleryURL = new string[project.galery.Length];

                for (int i = 0; i < project.galery.Length; i++)
                {
                    project.imageGaleryURL[i] = project.galery[i] ? AssetDatabase.GetAssetPath(project.galery[i]) : "";
                }
            }

            var json = JsonUtility.ToJson(projectList, true);
            File.WriteAllText(savePath, json);
        }
    }

}