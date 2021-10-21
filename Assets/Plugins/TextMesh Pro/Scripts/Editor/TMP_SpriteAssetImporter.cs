using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using TMPro.SpriteAssetUtilities;
using Texture = UnityEngine.Texture;
using Texture2D = UnityEngine.Texture2D;
using TextureImporter = UnityEditor.TextureImporter;

namespace TMPro
{
    public class TMP_SpriteAssetImporter : EditorWindow
    {



        // Create Sprite Asset Editor Window
        [MenuItem("Window/TextMeshPro/Sprite Importer", false, 2026)]
        public static void ShowFontAtlasCreatorWindow()
        {
            var window = GetWindow<TMP_SpriteAssetImporter>();
            window.titleContent = new GUIContent("Sprite Importer");
            window.Focus();
        }

        Texture2D m_SpriteAtlas;
        SpriteAssetImportFormats m_SpriteDataFormat = SpriteAssetImportFormats.UnityPacker;
        TextAsset m_JsonFile;

        string m_CreationFeedback;
        int m_Fps = 5;
        TMP_SpriteAsset m_SpriteAsset;
        List<TMP_Sprite> m_SpriteInfoList = new List<TMP_Sprite>();
        Dictionary<string, int> m_NameToCount = new Dictionary<string, int>();
        List<Texture2D> m_UnityPackageTexs = new List<Texture2D>();
        Vector2 scrollPos = Vector2.zero;

        int[] atlasSizes = new[] { 64, 128, 256, 512, 1024, 2048 };
        string[] atlasSizesStr = new[] { "64", "128", "256", "512", "1024", "2048" };
        int atlasSize = 512;

        int[] atlasPaddings = new[] { 2, 4, 8, };
        string[] atlasPaddingsStr = new[] { "2", "4", "8", };
        int atlasPadding = 2;
        //padding

        void OnEnable()
        {
            // Set Editor Window Size
            SetEditorWindowSize();
            OnSelectionChange();
        }

        public void OnGUI()
        {
            DrawEditorPanel();
        }


        void DrawEditorPanel()
        {
            // label
            GUILayout.Label("Import Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            m_SpriteDataFormat = (SpriteAssetImportFormats)EditorGUILayout.EnumPopup("Import Format", m_SpriteDataFormat);

            if (m_SpriteDataFormat == SpriteAssetImportFormats.TexturePacker || m_SpriteDataFormat == SpriteAssetImportFormats.TexturePackerEx)
            {
                DoDrawTexturePacker();
            }
            else if (m_SpriteDataFormat == SpriteAssetImportFormats.UnityPacker)
            {
                DoDrawUnityPacker();
            }
        }


        void DoDrawTexturePacker()
        {
            //fps
            if (m_SpriteDataFormat == SpriteAssetImportFormats.TexturePackerEx || m_SpriteDataFormat == SpriteAssetImportFormats.UnityPacker)
                m_Fps = EditorGUILayout.IntField("Sprite Fps", m_Fps);

            // Sprite Texture Selection
            m_JsonFile = EditorGUILayout.ObjectField("Sprite Data Source", m_JsonFile, typeof(TextAsset), false) as TextAsset;

            // Sprite Texture Selection
            m_SpriteAtlas = EditorGUILayout.ObjectField("Sprite Texture Atlas", m_SpriteAtlas, typeof(Texture2D), false) as Texture2D;


            if (EditorGUI.EndChangeCheck())
            {
                m_CreationFeedback = string.Empty;
            }

            GUILayout.Space(10);

            GUI.enabled = m_JsonFile != null && m_SpriteAtlas != null;

            // Create Sprite Asset
            if (GUILayout.Button("Create Sprite Asset"))
            {
                m_CreationFeedback = string.Empty;

                // Read json data file
                if (m_JsonFile != null)
                {
                    Texture2D ntex = new Texture2D(m_SpriteAtlas.width, m_SpriteAtlas.height, m_SpriteAtlas.format, false);
                    ntex.LoadRawTextureData(m_SpriteAtlas.GetRawTextureData());
                    ntex.Apply();
                    m_SpriteAtlas = ntex;

                    TexturePacker.SpriteDataObject sprites = JsonUtility.FromJson<TexturePacker.SpriteDataObject>(m_JsonFile.text);

                    if (sprites != null && sprites.frames != null && sprites.frames.Count > 0)
                    {
                        int spriteCount = sprites.frames.Count;

                        // Update import results
                        m_CreationFeedback = "<b>Import Results</b>\n--------------------\n";
                        m_CreationFeedback += "<color=#C0ffff><b>" + spriteCount + "</b></color> Sprites were imported from file.";

                        // Create sprite info list
                        if (m_SpriteDataFormat == SpriteAssetImportFormats.TexturePackerEx)
                            m_SpriteInfoList = CreateSpriteInfoListEx(sprites);
                        else
                            m_SpriteInfoList = CreateSpriteInfoList(sprites);
                    }
                }

            }

            GUI.enabled = true;

            // Creation Feedback
            GUILayout.Space(5);
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(60));
            {
                EditorGUILayout.LabelField(m_CreationFeedback, TMP_UIStyleManager.label);
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);
            GUI.enabled = m_JsonFile != null && m_SpriteAtlas && m_SpriteInfoList != null && m_SpriteInfoList.Count > 0;    // Enable Save Button if font_Atlas is not Null.
            if (GUILayout.Button("Save Sprite Asset") && m_JsonFile != null)
            {
                string filePath = EditorUtility.SaveFilePanel("Save Sprite Asset File", new FileInfo(AssetDatabase.GetAssetPath(m_JsonFile)).DirectoryName, m_JsonFile.name, "asset");

                if (filePath.Length == 0)
                    return;

                SaveSpriteAsset(filePath);
            }
            GUI.enabled = true;
        }


        void DoDrawUnityPacker()
        {
            //图集大小
            atlasSize = EditorGUILayout.IntPopup("AtlasSize: ", atlasSize, atlasSizesStr, atlasSizes);

            //图集大小
            atlasPadding = EditorGUILayout.IntPopup("Padding: ", atlasPadding, atlasPaddingsStr, atlasPaddings);

            //fps
            if (m_SpriteDataFormat == SpriteAssetImportFormats.TexturePackerEx || m_SpriteDataFormat == SpriteAssetImportFormats.UnityPacker)
                m_Fps = EditorGUILayout.IntField("Sprite Fps", m_Fps);

            EditorGUILayout.LabelField("Images: " + m_UnityPackageTexs.Count);

            if (EditorGUI.EndChangeCheck())
            {
                m_CreationFeedback = string.Empty;
            }

            GUILayout.Space(10);


            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
            if (m_UnityPackageTexs.Count > 0)
            {
                int count = 0;
                int rowCount = (int)position.width / 45;
                while (count < m_UnityPackageTexs.Count)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int i = count; i < m_UnityPackageTexs.Count; i++)
                    {
                        ++count;
                        EditorGUILayout.ObjectField(m_UnityPackageTexs[i], typeof(Texture), false, GUILayout.Width(40), GUILayout.Height(40));
                        if (count % rowCount == 0)
                            break;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.EndVertical();
                }

            }
            EditorGUILayout.EndScrollView();

            GUI.enabled = m_UnityPackageTexs.Count > 0;

            if (m_SpriteAtlas != null)
            {
                EditorGUILayout.ObjectField(m_SpriteAtlas, typeof(Texture2D), false, GUILayout.Width(256), GUILayout.Height(256));
            }

            // Create Sprite Asset
            if (GUILayout.Button("Create Sprite Asset"))
            {
                m_CreationFeedback = string.Empty;

                m_SpriteAtlas = new Texture2D(2, 2, TextureFormat.ARGB32, false);

                foreach (var tex in m_UnityPackageTexs)
                {
                    string assetPath = AssetDatabase.GetAssetPath(tex);
                    TextureImporter import = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    bool isImport = false;
                    if (import.mipmapEnabled)
                    {
                        import.mipmapEnabled = false;
                        isImport = true;
                    }
                    else if (!import.isReadable)
                    {
                        import.isReadable = true;
                        isImport = true;
                    }
                    else if (import.npotScale != TextureImporterNPOTScale.None)
                    {
                        import.npotScale = TextureImporterNPOTScale.None;
                        isImport = true;
                    }

                    if (isImport)
                        import.SaveAndReimport();
                }


                Rect[] rects = m_SpriteAtlas.PackTextures(m_UnityPackageTexs.ToArray(), atlasPadding, atlasSize);
                TexturePacker.SpriteDataObject sprites = new TexturePacker.SpriteDataObject();
                sprites.frames = new List<TexturePacker.SpriteData>();
                for (int i = 0; i < rects.Length; i++)
                {
                    Rect rect = rects[i];
                    TexturePacker.SpriteData sdata = new TexturePacker.SpriteData();
                    string assetPath = AssetDatabase.GetAssetPath(m_UnityPackageTexs[i]);
                    sdata.filename = FileUtil.UnityGetFileName(assetPath);
                    sdata.pivot = Vector2.zero;
                    rect.x *= m_SpriteAtlas.width;
                    rect.y = m_SpriteAtlas.height - (rect.y + rect.height) * m_SpriteAtlas.height;
                    rect.width *= m_SpriteAtlas.width;
                    rect.height *= m_SpriteAtlas.height;
                    sdata.frame = new TexturePacker.SpriteFrame { x = rect.x, y = rect.y, w = rect.width, h = rect.height };
                    sprites.frames.Add(sdata);
                }

                if (sprites != null && sprites.frames != null && sprites.frames.Count > 0)
                {
                    m_SpriteInfoList = CreateSpriteInfoListEx(sprites);
                }

            }

            GUI.enabled = true;


            GUILayout.Space(5);
            GUI.enabled = m_SpriteAtlas && m_SpriteInfoList != null && m_SpriteInfoList.Count > 0 && m_UnityPackageTexs.Count > 0;    // Enable Save Button if font_Atlas is not Null.
            if (GUILayout.Button("Save Sprite Asset"))
            {
                string filePath = EditorUtility.SaveFilePanel("Save Sprite Asset File", new FileInfo(AssetDatabase.GetAssetPath(m_UnityPackageTexs[0])).DirectoryName, "", "asset");

                if (filePath.Length == 0)
                    return;
                SaveSpriteAtlas(filePath);
                SaveSpriteAsset(filePath);
            }
            GUI.enabled = true;
        }

        void OnSelectionChange()
        {
            m_UnityPackageTexs.Clear();

            foreach (var guid in Selection.assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter import = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (import)
                {
                    m_UnityPackageTexs.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath));
                }
            }
            m_UnityPackageTexs.Sort((Texture2D v1, Texture2D v2) => { return v1.name.CompareTo(v2.name); });
            this.Repaint();
        }





        /// <summary>
        /// 
        /// </summary>
        List<TMP_Sprite> CreateSpriteInfoList(TexturePacker.SpriteDataObject spriteDataObject)
        {
            List<TexturePacker.SpriteData> importedSprites = spriteDataObject.frames;

            List<TMP_Sprite> spriteInfoList = new List<TMP_Sprite>();

            for (int i = 0; i < importedSprites.Count; i++)
            {
                TMP_Sprite sprite = new TMP_Sprite();

                sprite.id = i;
                sprite.name = Path.GetFileNameWithoutExtension(importedSprites[i].filename) ?? "";
                sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(sprite.name);

                // Attempt to extract Unicode value from name
                int unicode;
                int indexOfSeperator = sprite.name.IndexOf('-');
                if (indexOfSeperator != -1)
                    unicode = TMP_TextUtilities.StringHexToInt(sprite.name.Substring(indexOfSeperator + 1));
                else
                    unicode = TMP_TextUtilities.StringHexToInt(sprite.name);

                sprite.unicode = unicode;

                sprite.x = importedSprites[i].frame.x;
                sprite.y = m_SpriteAtlas.height - (importedSprites[i].frame.y + importedSprites[i].frame.h);
                sprite.width = importedSprites[i].frame.w;
                sprite.height = importedSprites[i].frame.h;

                //Calculate sprite pivot position
                sprite.pivot = importedSprites[i].pivot;

                // Properties the can be modified
                sprite.xAdvance = sprite.width;
                sprite.scale = 1.0f;
                sprite.xOffset = 0 - (sprite.width * sprite.pivot.x);
                sprite.yOffset = sprite.height - (sprite.height * sprite.pivot.y);

                spriteInfoList.Add(sprite);
            }

            return spriteInfoList;
        }

        List<TMP_Sprite> CreateSpriteInfoListEx(TexturePacker.SpriteDataObject spriteDataObject)
        {
            GenNameToFrameDic(spriteDataObject);
            List<TexturePacker.SpriteData> importedSprites = spriteDataObject.frames;

            List<TMP_Sprite> spriteInfoList = new List<TMP_Sprite>();



            for (int i = 0; i < importedSprites.Count; i++)
            {
                TMP_Sprite sprite = new TMP_Sprite();

                sprite.id = i;

                GenFrameCountToName(importedSprites[i].filename, out sprite.name, out sprite.frameCount);

                sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(sprite.name);

                // Attempt to extract Unicode value from name
                int unicode;
                int indexOfSeperator = sprite.name.IndexOf('-');
                if (indexOfSeperator != -1)
                    unicode = TMP_TextUtilities.StringHexToInt(sprite.name.Substring(indexOfSeperator + 1));
                else
                    unicode = TMP_TextUtilities.StringHexToInt(sprite.name);

                sprite.unicode = unicode;

                sprite.x = importedSprites[i].frame.x;
                sprite.y = m_SpriteAtlas.height - (importedSprites[i].frame.y + importedSprites[i].frame.h);
                sprite.width = importedSprites[i].frame.w;
                sprite.height = importedSprites[i].frame.h;

                //Calculate sprite pivot position
                sprite.pivot = importedSprites[i].pivot;

                // Properties the can be modified
                sprite.xAdvance = sprite.width;
                sprite.scale = 1.0f;
                sprite.xOffset = 0 - (sprite.width * sprite.pivot.x);
                sprite.yOffset = sprite.height - (sprite.height * sprite.pivot.y);

                sprite.fps = m_Fps;
                spriteInfoList.Add(sprite);
            }

            return spriteInfoList;
        }

        void GenNameToFrameDic(TexturePacker.SpriteDataObject spriteDataObject)
        {
            m_NameToCount.Clear();
            List<TexturePacker.SpriteData> importedSprites = spriteDataObject.frames;
            for (int i = 0; i < importedSprites.Count; i++)
            {
                string name = Path.GetFileNameWithoutExtension(importedSprites[i].filename) ?? "";
                string[] sp = name.Split('_');
                if (sp.Length == 2)
                {
                    string fname = sp[0];
                    if (!m_NameToCount.ContainsKey(fname))
                        m_NameToCount.Add(fname, -1);
                    m_NameToCount[fname]++;
                }
            }
        }

        void GenFrameCountToName(string kname, out string nname, out int count)
        {
            string name = Path.GetFileNameWithoutExtension(kname) ?? "";
            count = 0;
            nname = name;
            string[] sp = name.Split('_');
            if (sp.Length == 2)
            {
                string fname = sp[0];
                if (m_NameToCount.ContainsKey(fname))
                {
                    count = m_NameToCount[fname];
                    if (sp[1] == "1")
                        nname = fname;
                }
            }
        }


        void SaveSpriteAtlas(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.
            string dataPath = Application.dataPath;

            string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            string dirName = Path.GetDirectoryName(relativeAssetPath);
            string fileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            string pathNoExt = dirName + "/" + fileName;

            string projectPath = dataPath.Substring(0, dataPath.Length - 6);
            string assetPath = pathNoExt + "Atlas.png";
            string targetPath = Path.Combine(projectPath, assetPath);


            if (File.Exists(targetPath))
                File.Delete(targetPath);

            Texture2D ntex = new Texture2D(m_SpriteAtlas.width, m_SpriteAtlas.height, TextureFormat.RGBA32, false);
            TextureImporter import = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if(import)
            {
                import.isReadable = true;
                import.SaveAndReimport();
            }

            ntex.SetPixels32(m_SpriteAtlas.GetPixels32());
            ntex.Apply();
            File.WriteAllBytes(targetPath, ntex.EncodeToPNG());

            AssetDatabase.Refresh();
            m_SpriteAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if(!import)
                import = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            import.mipmapEnabled = false;
            import.isReadable = false;
            import.SaveAndReimport();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        void SaveSpriteAsset(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            string dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
                return;
            }

            string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            string dirName = Path.GetDirectoryName(relativeAssetPath);
            string fileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            string pathNoExt = dirName + "/" + fileName;


            // Create new Sprite Asset using this texture
            m_SpriteAsset = CreateInstance<TMP_SpriteAsset>();

            string projectPath = dataPath.Substring(0, dataPath.Length - 6);
            string targetPath = Path.Combine(projectPath, pathNoExt + ".asset");


            if (File.Exists(targetPath))
            {

                m_SpriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(pathNoExt + ".asset");
                //Texture2D otex = m_SpriteAsset.spriteSheet as Texture2D;
                ////otex.Resize()

                //DestroyImmediate(m_SpriteAsset.spriteSheet, true);
            }
            else
            {
                AssetDatabase.CreateAsset(m_SpriteAsset, pathNoExt + ".asset");
            }

            // Compute the hash code for the sprite asset.
            m_SpriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(m_SpriteAsset.name);

            // Assign new Sprite Sheet texture to the Sprite Asset.
            m_SpriteAsset.spriteSheet = m_SpriteAtlas;

            //用于重新做字体时把charCode存起来
            List<TMP_SpriteCharacter> spriteCharacterTable = m_SpriteAsset.spriteCharacterTable;
            if (spriteCharacterTable != null && spriteCharacterTable.Count > 0)
            {
                foreach (var itemSpriteCharacter in spriteCharacterTable)
                {
                    foreach (var spriteInfo in m_SpriteInfoList)
                    {
                        if (itemSpriteCharacter.name == spriteInfo.name)
                        {
                            spriteInfo.charCode = itemSpriteCharacter.charCode;
                            spriteInfo.unicode = (int)itemSpriteCharacter.unicode;
                        }
                    }
                }
            }
            m_SpriteAsset.spriteInfoList = m_SpriteInfoList;

            // Add new default material for sprite asset.
            AddDefaultMaterial(m_SpriteAsset);

            //重新更新数据
            m_SpriteAsset.EditorUpdate();
        }


        /// <summary>
        /// Create and add new default material to sprite asset.
        /// </summary>
        /// <param name="spriteAsset"></param>
        static void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
        {
            Shader shader = Shader.Find("TextMeshPro/Sprite");
            Material material = new Material(shader);
            material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

            spriteAsset.material = material;
            material.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(material, spriteAsset);

            //spriteAsset.spriteSheet.name = spriteAsset.name + " Atlas";
            //AssetDatabase.AddObjectToAsset(spriteAsset.spriteSheet, spriteAsset);
        }


        /// <summary>
        /// Limits the minimum size of the editor window.
        /// </summary>
        void SetEditorWindowSize()
        {
            EditorWindow editorWindow = this;

            Vector2 currentWindowSize = editorWindow.minSize;

            editorWindow.minSize = new Vector2(Mathf.Max(230, currentWindowSize.x), Mathf.Max(300, currentWindowSize.y));
        }

    }
}