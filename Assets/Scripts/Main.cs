using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Map;
using UnityEngine;
using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;
using Screen = UnityEngine.Screen;

public class Main : MonoBehaviour
{
    // All textures for blocks
    public TxtList[] Textures = new TxtList[512];

    // White texture
    public Texture2D White;
    public Texture2D Noir;

    private bool contextMenuOpen;
    private bool contextBlockMenuOpen;
    private bool contextGeneralMenuOpen;
    private Vector2 fixedMousePosition;
    private GameObject contextObject;
    private Generator Generator;

    public static float MaxCameraUp = 10.18f;
    public static float MaxCameraDown = 3.18f;
    public static bool GameStarted = false;
    public static string CurrentModule;

    private Camera _camera;

    public static string GetPath => Path.GetDirectoryName(Application.dataPath);

    public void Start()
    {
        _camera = Camera.main;

        // Set camera axis for 3D
        _camera.transform.position = new Vector3(0, 3.38f, MaxCameraUp);
        _camera.transform.Rotate(new Vector3(-198.449f, 0, 0), Space.Self);
    }

    public void Update()
    {
        // Camera up and down
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (_camera.transform.position.z > MaxCameraDown)
                _camera.transform.Translate(new Vector3(0, 0, 250 * Time.deltaTime));
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (_camera.transform.position.z < MaxCameraUp)
                _camera.transform.Translate(new Vector3(0, 0, -250 * Time.deltaTime));
        }

        // Camera movement
        var currentZ = _camera.transform.position.z;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            _camera.transform.Translate(new Vector3(0, -10 * Time.deltaTime, 0), Space.World);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            _camera.transform.Translate(new Vector3(0, 10 * Time.deltaTime, 0), Space.World);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _camera.transform.Translate(new Vector3(-10 * Time.deltaTime, 0, 0), Space.World);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            _camera.transform.Translate(new Vector3(10 * Time.deltaTime, 0, 0), Space.World);
        }


        // Let's see if mouse cover a block
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray);
        if (hits.Length > 0)
        {
            var hit = hits.First();
            if (Input.GetMouseButtonDown(1))
            {
                var closeAll = false;
                if (hit.collider != null)
                {
                    // Overlay
                    Debug.Log("Opening overlay");
                    if (contextMenuOpen == false)
                    {
                        contextObject = hit.collider.gameObject;
                        fixedMousePosition = Input.mousePosition;
                        contextMenuOpen = true;
                    }
                    else closeAll = true;
                }
                else closeAll = true;

                if (!closeAll) return;

                if (
                    contextBlockMenuOpen ||
                    contextGeneralMenuOpen
                )
                {
                    contextObject = hit.collider.gameObject;
                    fixedMousePosition = Input.mousePosition;
                    contextMenuOpen = true;
                }
                else
                    contextMenuOpen = false;

                contextBlockMenuOpen = false;
                contextGeneralMenuOpen = false;
            }
        }
    }

    private string input = "Default";

    public void OnGUI()
    {
        var style = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            font = Resources.GetBuiltinResource<Font>("Arial.ttf")
        };
        style.normal.textColor = Color.black;
        style.normal.background = Texture2D.whiteTexture;

        var styleInput = new GUIStyle(style);
        styleInput.normal.background = Texture2D.grayTexture;

        if (!GameStarted)
        {
            if (!Directory.Exists($@"{GetPath}\Modules"))
                throw new Exception("Directory Modules is required");

            // Search module
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.blackTexture);
            input = GUI.TextField(
                new Rect(Screen.width / 2 - 100, Screen.height / 2 - 55, 200, 40),
                input,
                styleInput
            );
            if (
                GUI.Button(
                    new Rect(Screen.width / 2 - 100, Screen.height / 2 + 15, 200, 40),
                    "Find module",
                    style
                )
            )
            {
                var pathModule = $@"{GetPath}\Modules\{input}\";
                var pathModuleBlocks = $@"{GetPath}\Modules\{input}\Blocks";
                if (
                    Directory.Exists(pathModule) &&
                    Directory.Exists(pathModuleBlocks)
                )
                {
                    // Adding external textures to the blocks list
                    var allBlocks = Directory.GetFiles(pathModuleBlocks);
                    var index = 0;
                    foreach (var file in allBlocks)
                    {
                        var parse = Path.GetFileNameWithoutExtension(file);
                        var txt = new Texture2D(Generator.BlockSize * 2, Generator.BlockSize * 2);
                        txt.LoadImage(File.ReadAllBytes(file));
                        var block = new TxtList
                        {
                            texture = txt,
                            name = parse
                        };
                        Textures[index] = block;
                        index++;
                    }
                    Textures = Textures.Where(txt => txt != null).ToArray();

                    CurrentModule = input;
                    GameStarted = true;

                    // Start the game once
                    StartGame();
                }
                else
                {
                    Tools.MessageBoxInfo($"Module {input} isn't complete or not exists.", "Error");
                }
            }

            return;
        }

        // First context menu
        if (
            contextMenuOpen
            && !contextBlockMenuOpen
            && !contextGeneralMenuOpen
        )
        {
            GUI.DrawTexture(new Rect(
                fixedMousePosition.x,
                Screen.height - fixedMousePosition.y,
                330, 145
            ), Noir);

            if (GUI.Button(new Rect(
                fixedMousePosition.x + 15,
                Screen.height - fixedMousePosition.y + 15,
                300, 50
            ), "Change block texture", style))
            {
                contextMenuOpen = false;
                contextBlockMenuOpen = true;
            }
            else if (GUI.Button(new Rect(
                fixedMousePosition.x + 15,
                Screen.height - fixedMousePosition.y + 80,
                300, 50
            ), "General options", style))
            {
                contextMenuOpen = false;
                contextGeneralMenuOpen = true;
            }
        }

        // Menu context for blocks
        if (!contextMenuOpen && contextBlockMenuOpen)
        {
            var line = 0;
            var column = 0;
            var list = new Rect[164];
            for (var iteration = 0; iteration < Textures.Length; iteration++) 
            {
                list[iteration] = new Rect(
                    fixedMousePosition.x + 15 + (50 * column) + (15 * column),
                    (Screen.height - fixedMousePosition.y) + 15 + (50 * line) + (15 * line),
                    50, 50
                );
                column++;
                if (column % 5 != 0) continue;
                line++;
                column = 0;
            }

            GUI.DrawTexture(new Rect(
                fixedMousePosition.x,
                Screen.height - fixedMousePosition.y,
                15 + (50 * 5) + (15 * 5),
                15 + (50 * (line + 1)) + (15 * (line + 1))
            ), Texture2D.whiteTexture);

            var index = 0;
            foreach (var txtObj in Textures)
            {
                if (GUI.Button(list[index], txtObj.texture, style))
                {
                    contextObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                        txtObj.texture,
                        new Rect(0, 0, Generator.BlockSize * 2, Generator.BlockSize * 2),
                        new Vector2(1, 1), Generator.BlockSize * 2
                    );
                    var uniq = contextObject.GetComponent<Uniq>();
                    ((string[]) ((object[]) Generator.Levels[0])[2 + uniq.line])[uniq.column] = txtObj.name;
                }

                index++;
            }
        }

        // Menu context for general options
        if (!contextMenuOpen && contextGeneralMenuOpen)
        {
            GUI.DrawTexture(new Rect(
                fixedMousePosition.x,
                Screen.height - fixedMousePosition.y,
                330, 145
            ), Noir);

            if (GUI.Button(new Rect(
                fixedMousePosition.x + 15,
                Screen.height - fixedMousePosition.y + 15,
                300, 50
            ), "Save map", style))
            {
                SaveMap();
            }
            else if (GUI.Button(new Rect(
                fixedMousePosition.x + 15,
                Screen.height - fixedMousePosition.y + 80,
                300, 50
            ), "Load map", style))
            {
                LoadMap();
            }
        }
    }

    private void StartGame()
    {
        // Generate the map
        Generator = new Generator(Textures);
        
        // Set all blocks by the first texture found
        var level = (object[]) Generator.Levels[0];
        for (var index = 2; index < level.Length; index++)
        {
            var type = (string[]) level[index];
            for (var iteration = 0; iteration < type.Length; iteration++)
            {
                try
                {
                    if (type[iteration] != null)
                        ((string[]) ((object[]) Generator.Levels[0])[index])[iteration] = Generator.Textures[0].name;
                }
                catch
                {
                    throw new Exception($"You must have at leat one default texture. ({Generator.Textures.Length})");
                }
            }
        }
        Generator.GenerateMap("Level0");
    }

    // Save all blocks array to a csv file
    public void SaveMap()
    {
        var map = (object[]) Generator.Levels.First();
        var oldMapName = map[0].ToString();
        var mapSize = map[1].ToString();
        var textToSave = $"{oldMapName}\r\n{mapSize}\r\n";

        for (var iteration = 2; iteration < map.Length; iteration++)
        {
            var mapLine = (string[]) map[iteration];
            textToSave = mapLine.Aggregate(textToSave, (current, number) => current + $"{number},");

            if (map.Length < iteration + 1) continue;

            textToSave = textToSave.Substring(0, textToSave.Length - 1);
            textToSave += "\r\n";
        }

        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");

        var fileNumber = 0;
        while (File.Exists($@"{GetPath}\Saves\Level{fileNumber}.abel"))
            fileNumber++;

        var fileName = $"Level{fileNumber}.abel";
        var savePath = $@"{GetPath}\Saves\{fileName}";
        var stream = File.CreateText(savePath);
        stream.Write(textToSave);
        stream.Close();
        stream.Dispose();

        Tools.RenameSaveInputBox("Enter name of the save", "Utility", fileName);
        Tools.MessageBoxInfo($"Level successfuly saved.", "Information");
    }

    // Load a map from a cvs file
    public void LoadMap()
    {
    }
}