#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Twinny.System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon;
using Amazon.Runtime;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;

namespace Twinny.Editor
{


    [CustomEditor(typeof(ProjectList))]
    public class ProjectListEditor : UnityEditor.Editor
    {
        private ProjectList _projectList;

        private enum UploadTarget { SFTP, AWSS3 }
        private UploadTarget _uploadTarget = UploadTarget.SFTP;

        // SFTP defaults
        private string _host = "195.35.41.141";
        private int _port = 65002;
        private string _user = "u125564719";
        private string _password = "";

        // AWS defaults
        private string _awsAccessKey = "";
        private string _awsSecretKey = "";
        private string _awsRegion = "us-east-1";
        private string _bucketName = "";
        private IAmazonS3 _s3Client;

        // Subdiretório para upload
        private string _subDirectory = "project_list";

        private void OnEnable()
        {
            _projectList = (ProjectList)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Upload Settings", EditorStyles.boldLabel);

            _uploadTarget = (UploadTarget)EditorGUILayout.EnumPopup("Upload Target", _uploadTarget);

            if (_uploadTarget == UploadTarget.SFTP)
            {
                EditorGUILayout.LabelField("SFTP Config", EditorStyles.boldLabel);
                _host = EditorGUILayout.TextField("Host/IP", _host);
                _port = EditorGUILayout.IntField("Port", _port);
                _user = EditorGUILayout.TextField("User", _user);
                _password = EditorGUILayout.PasswordField("Password", _password);
            }
            else if (_uploadTarget == UploadTarget.AWSS3)
            {
                EditorGUILayout.LabelField("AWS S3 Config", EditorStyles.boldLabel);
                _awsAccessKey = EditorGUILayout.TextField("Access Key", _awsAccessKey);
                _awsSecretKey = EditorGUILayout.PasswordField("Secret Key", _awsSecretKey);
                _awsRegion = EditorGUILayout.TextField("Region", _awsRegion);
                _bucketName = EditorGUILayout.TextField("Bucket Name", _bucketName);
            }

            _subDirectory = EditorGUILayout.TextField("Subdirectory", _subDirectory);

            EditorGUILayout.Space();
            if (GUILayout.Button("Upload ProjectList"))
            {
                if(ValidateFields())
                UploadProjectListAsync();
            }
        }


        private bool ValidateFields()
        {
            // Validação de target
            if (_uploadTarget == UploadTarget.SFTP)
            {
                if (string.IsNullOrWhiteSpace(_host) ||
                    _port <= 0 ||
                    string.IsNullOrWhiteSpace(_user) ||
                    string.IsNullOrWhiteSpace(_password))
                {
                    EditorUtility.DisplayDialog("Validation Error",
                        "Please fill all SFTP fields (Host, Port, User, Password).", "OK");
                    return false;
                }
            }
            else if (_uploadTarget == UploadTarget.AWSS3)
            {
                if (string.IsNullOrWhiteSpace(_awsAccessKey) ||
                    string.IsNullOrWhiteSpace(_awsSecretKey) ||
                    string.IsNullOrWhiteSpace(_awsRegion) ||
                    string.IsNullOrWhiteSpace(_bucketName))
                {
                    EditorUtility.DisplayDialog("Validation Error",
                        "Please fill all AWS S3 fields (Access Key, Secret Key, Region, Bucket Name).", "OK");
                    return false;
                }
            }

            // Validação do subdiretório
            if (string.IsNullOrWhiteSpace(_subDirectory))
            {
                EditorUtility.DisplayDialog("Validation Error",
                    "Please specify a Subdirectory for upload.", "OK");
                return false;
            }

            return true; // Tudo ok
        }



        private async void UploadProjectListAsync()
        {
            string remoteDir = $"public_html/{_subDirectory.Trim('/')}";

            string[] localFiles;

            // ===== Exporta arquivos na thread principal =====
            try
            {
                localFiles = ExportProjectListFiles(); // exporta texturas e JSON
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao exportar arquivos: {ex}");
                return;
            }

            // ===== Upload em background =====
            try
            {
                if (_uploadTarget == UploadTarget.SFTP)
                {
                    var uploader = new Twinny.Editor.SmartUpload(_host, _port, _user, _password);

                    // Rodando o upload em background pra não travar a Unity
                    await Task.Run(() => uploader.UploadFilesAsync(localFiles, remoteDir).GetAwaiter().GetResult());
                }
                else if (_uploadTarget == UploadTarget.AWSS3)
                {
                    _s3Client = new AmazonS3Client(
                        _awsAccessKey,
                        _awsSecretKey,
                        new Amazon.S3.AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_awsRegion) });

                    var uploader = new Twinny.Editor.SmartUpload(_s3Client, _bucketName);

                    await Task.Run(() => uploader.UploadFilesAsync(localFiles, remoteDir).GetAwaiter().GetResult());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro no upload: {ex}");
                return;
            }

            // ===== Revela a pasta temporária =====
            EditorUtility.RevealInFinder(Path.Combine(Application.dataPath, "../Temp/ProjectListUpload"));
            Debug.Log("[SmartUpload] Upload finalizado!");
        }



        private string[] ExportProjectListFiles()
        {
            List<string> files = new List<string>();
            string tempDir = Path.Combine(Application.dataPath, "../Temp/ProjectListUpload");

            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            // Export each project's thumbnail
            foreach (var project in _projectList.projects)
            {
                if (project.thumbnail != null)
                {
                    string thumbPath = Path.Combine(tempDir, $"{project.addressableKey}_thumbnail.png");
                    SaveTextureToFile(project.thumbnail, thumbPath);
                    files.Add(thumbPath);
                }

                if (project.galery != null)
                {
                    for (int i = 0; i < project.galery.Length; i++)
                    {
                        if (project.galery[i] != null)
                        {
                            string galPath = Path.Combine(tempDir, $"{project.addressableKey}_galery_{i}.png");
                            SaveTextureToFile(project.galery[i], galPath);
                            files.Add(galPath);
                        }
                    }
                }
            }

            // Export ProjectList JSON
            string jsonPath = Path.Combine(tempDir, "ProjectList.json");
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(_projectList, Formatting.Indented));
            files.Add(jsonPath);

            return files.ToArray();
        }

        private void SaveTextureToFile(Texture2D tex, string path)
        {
            Texture2D readableTex = GetReadableTexture(tex);

            byte[] bytes;
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".jpg" || ext == ".jpeg")
                bytes = readableTex.EncodeToJPG();
            else
                bytes = readableTex.EncodeToPNG();

            if (bytes == null)
                throw new Exception($"Failed to encode texture {tex.name} to {ext}");

            File.WriteAllBytes(path, bytes);
        }

        private Texture2D GetReadableTexture(Texture2D tex)
        {
            // Se já for legível e não comprimida
            if (tex.isReadable && tex.format != TextureFormat.DXT1 && tex.format != TextureFormat.DXT5 &&
                tex.format != TextureFormat.PVRTC_RGB2 && tex.format != TextureFormat.PVRTC_RGB4 &&
                tex.format != TextureFormat.PVRTC_RGBA2 && tex.format != TextureFormat.PVRTC_RGBA4 &&
                tex.format != TextureFormat.ETC_RGB4 && tex.format != TextureFormat.ETC2_RGBA8 &&
                !tex.format.ToString().StartsWith("ASTC"))
            {
                return tex;
            }

            // Cria uma render texture temporária
            RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(tex, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readableTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTex.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            return readableTex;
        }




    }
#endif

}