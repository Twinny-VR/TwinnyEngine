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
using UnityEditor.SearchService;
using System.Linq;
using Concept.Editor;

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

        [Tooltip("All remote data will deleted. Don't use it if you want subscribe files.")]
        private bool _cleanUpCache;

        private MobileRuntime m_mobileRuntime;

        private void OnEnable()
        {
            _projectList = (ProjectList)target;
            m_mobileRuntime = Resources.Load<MobileRuntime>("MobileRuntimePreset");
        }

        public override void OnInspectorGUI()
        {



            DrawDefaultInspector();

            if(m_mobileRuntime == null)
            {


            EditorGUILayout.Space();

            // cria a box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // estilo customizado pro texto
            var yellowCentered = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.yellow }
            };

            // texto amarelo centralizado
            EditorGUILayout.LabelField("⚠ Nenhum RuntimePreset definido! Deseja criar um?", yellowCentered);

            // espaço antes do botão
            GUILayout.Space(5);

            // botão centralizado
            if (GUILayout.Button("Criar Preset", GUILayout.Height(25)))
            {
                if (AssetDatabase.IsValidFolder("Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                string fileName = "MobileRuntimePreset";
                string assetPath = "Assets/Resources/" + fileName + ".asset";

                var preset = ScriptableObject.CreateInstance<MobileRuntime>();
                AssetDatabase.CreateAsset(preset, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Novo preset 'MobileRuntimePreset' criado e salvo em: " + assetPath);

                m_mobileRuntime = Resources.Load<MobileRuntime>("MobileRuntimePreset");
            }

            EditorGUILayout.EndVertical();
            return;
            }

            EditorGUILayout.LabelField("Local Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            try
            {
                m_mobileRuntime.localProjectListTempDirectory = EditorGUILayout.TextField("Local Temp Folder", m_mobileRuntime.localProjectListTempDirectory);

                if (GUILayout.Button("Select...", GUILayout.MaxWidth(80)))
                {
                    string initialDir = Path.Combine(Application.dataPath, m_mobileRuntime.localProjectListTempDirectory);
                    initialDir = Path.GetFullPath(initialDir);

                    if (!Directory.Exists(initialDir))
                    {
                        string tempDir = Path.Combine(Application.dataPath, "../Temp");
                        tempDir = Path.GetFullPath(tempDir);

                        if (!Directory.Exists(tempDir))
                            Directory.CreateDirectory(tempDir);

                        initialDir = tempDir;
                    }

                    string selected = EditorUtility.OpenFolderPanel("Select Local Temp Folder", initialDir, "");
                    if (!string.IsNullOrEmpty(selected))
                    {
                        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

                        string fullSelected = Path.GetFullPath(selected);

                        if (fullSelected.StartsWith(projectRoot))
                        {
                            // gera caminho relativo à raiz
                            string relative = Path.GetRelativePath(projectRoot, fullSelected).Replace("\\", "/");

                            // adiciona "../" explicitamente, já que está fora da Assets
                            if (!relative.StartsWith("."))
                                relative = "../" + relative;

                            m_mobileRuntime.localProjectListTempDirectory = relative;
                        }
                        else
                            m_mobileRuntime.localProjectListTempDirectory = fullSelected;
                    }
                }
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Remote Settings", EditorStyles.boldLabel);
            m_mobileRuntime.remoteProjectListDirectory = EditorGUILayout.TextField("Host Subdirectory", m_mobileRuntime.remoteProjectListDirectory);
            EditorGUILayout.BeginHorizontal();

            // Desenha só o prefixo do label
            var controlRect = EditorGUILayout.GetControlRect();
            controlRect = EditorGUI.PrefixLabel(controlRect, new GUIContent("JSON Host File Name"));

            // Campo de texto ocupa o espaço sobrando (menos o do .json)
            m_mobileRuntime.projectListFileName = EditorGUI.TextField(
                new Rect(controlRect.x, controlRect.y, controlRect.width - 36, controlRect.height),
                m_mobileRuntime.projectListFileName
            );

            // ".json" alinhado à direita
            var rightHelpBox = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(controlRect.x + controlRect.width - 32, controlRect.y, 32, controlRect.height), ".json", rightHelpBox);

            EditorGUILayout.EndHorizontal();
            _cleanUpCache = EditorGUILayout.ToggleLeft("Clean Up Remote Data", _cleanUpCache);


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




            EditorGUILayout.Space();
            if (GUILayout.Button("Upload ProjectList"))
            {
                if (ValidateFields())
                    UploadProjectListAsync();
            }

            EditorGUILayout.EndVertical();
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
            if (string.IsNullOrWhiteSpace(m_mobileRuntime.remoteProjectListDirectory))
            {
                EditorUtility.DisplayDialog("Validation Error",
                    "Please specify a Subdirectory for upload.", "OK");
                return false;
            }

            return true; // Tudo ok
        }



        private async void UploadProjectListAsync()
        {
            string remoteDir = $"public_html/{m_mobileRuntime.remoteProjectListDirectory.Trim('/')}";
            string relativeDir = Path.Combine(Application.dataPath, m_mobileRuntime.localProjectListTempDirectory);
            List<(string, string)> localFiles;

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
                    var uploader = new SmartUploader(_host, _port, _user, _password);
                    uploader.OnStatusChanged += (status) => { Debug.Log(status); };

                    // Rodando o upload em background pra não travar a Unity
                    await Task.Run(() => uploader.UploadFilesAsync(localFiles, remoteDir, _cleanUpCache).GetAwaiter().GetResult());
                }
                else if (_uploadTarget == UploadTarget.AWSS3)
                {
                    _s3Client = new AmazonS3Client(
                        _awsAccessKey,
                        _awsSecretKey,
                        new Amazon.S3.AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_awsRegion) });

                    var uploader = new SmartUploader(_s3Client, _bucketName);
                    uploader.OnStatusChanged += (status) => { Debug.Log(status); };
                    await Task.Run(() => uploader.UploadFilesAsync(localFiles, remoteDir, _cleanUpCache).GetAwaiter().GetResult());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro no upload: {ex}");
                return;
            }
        }



        private List<(string localPath, string remotePath)> ExportProjectListFiles()
        {
            var files = new List<(string, string)>();
            string tempDir = Path.GetFullPath(Path.Combine(Application.dataPath, m_mobileRuntime.localProjectListTempDirectory));

            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            // Caminho da raiz do ProjectList.asset
            string projectListPath = AssetDatabase.GetAssetPath(_projectList);
            string rootDir = Path.GetFullPath(Path.GetDirectoryName(projectListPath)); // Assets/Projetos

            // Exporta JSON na raiz temporária
            foreach (var project in _projectList.projects)
            {
                if (!project.uploadThis) continue;
                // Thumbnail
                if (project.thumbnail != null)
                {
                    var thumbPath = SaveAssetFile(project.thumbnail, rootDir, tempDir);
                    project.thumbnailURL = thumbPath.Item2;
                    files.Add(thumbPath);
                }

                if (project.video != null)
                {
                    var videoPath = SaveAssetFile(project.video, rootDir, tempDir);
                    project.videoURL = videoPath.Item2;
                    files.Add(videoPath);
                }

                // Galeria
                if (project.galery != null && project.galery.Length > 0)
                {
                    var galPaths = new List<string>();
                    foreach (var tex in project.galery)
                    {
                        if (tex != null)
                        {
                            var galPath = SaveAssetFile(tex, rootDir, tempDir);
                            files.Add(galPath);
                        }
                    }
                    project.imageGaleryURL = files.Select(f => f.Item2).ToArray();
                }
            }

            // Exporta o JSON final
            string jsonPath = Path.Combine(tempDir, m_mobileRuntime.projectListFileName+".json");
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(_projectList, Formatting.Indented));
            files.Add((jsonPath, m_mobileRuntime.projectListFileName+".json")); // no remoto só o nome do JSON

            return files;
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


        private (string, string) SaveAssetFile(UnityEngine.Object asset, string rootDir, string targetDir)
        {
            string path = SaveRelativeAssetFile(asset, rootDir, targetDir);
            path = Path.GetFullPath(path);
            string remotePath = Path.GetRelativePath(targetDir, path).Replace("\\", "/");
            return (path, remotePath);
        }

        private string SaveRelativeAssetFile(UnityEngine.Object asset, string rootDir, string targetDir)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
                throw new Exception($"Asset {asset.name} não tem caminho físico no projeto");

            string absSrcPath = Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath);

            // caminho relativo a partir da pasta do ProjectList.asset
            // string relativePath = assetPath.Substring(rootDir.Length + 1).Replace("\\", "/");
            string relativePath = Path.GetRelativePath(rootDir, assetPath).Replace("\\", "/");

            string dstPath = Path.Combine(targetDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(dstPath));
            File.Copy(absSrcPath, dstPath, true);

            return dstPath;
        }

        // Retorna caminho relativo de um arquivo em relação a uma pasta base
        private string GetRelativePath(string baseDir, string fullPath)
        {
            Uri pathUri = new Uri(fullPath);
            if (!baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                baseDir += Path.DirectorySeparatorChar;
            Uri dirUri = new Uri(baseDir);
            return Uri.UnescapeDataString(dirUri.MakeRelativeUri(pathUri).ToString());
        }


    }
#endif

}