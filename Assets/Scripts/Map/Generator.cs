using System;
using System.Collections.Generic;
using System.Linq;
using Gui;
using UnityEngine;

namespace Map
{
    public class Generator
    {
        // Set size of blocks
        public const int BlockSize = 128;

        // Set texture
        public TxtList[] Textures;

        // All blocks to draw on the map
        public static GameObject[] Blocks = new GameObject[16384];

        // Chuncks of each level
        public List<object> Levels = new List<object>
        {
            new object[]
            {
                "Level0",
                "18x11",
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"},
                new[] {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"}
            }
        };

        public Generator(TxtList[] textures)
        {
            Textures = textures;
        }

        public int blocksLength = 0;
        public void GenerateMap(string levelName)
        {
            object[] foundLevel = null;
            var mapWidth = 0;
            var mapHeight = 0;
            var timeout = 0;

            // Loop on levels and find it
            Levels.ForEach(level =>
            {
                var converted = (object[]) level;
                if (converted[0].ToString() != levelName) return;

                foundLevel = converted;
                var splitSize = converted[1].ToString().Split('x');
                mapWidth = int.Parse(splitSize[0]);
                mapHeight = int.Parse(splitSize[1]);
                Debug.Log($"Map width : {mapWidth}");
                Debug.Log($"Map height : {mapHeight}");
            });

            // Generate blocks
            if (foundLevel == null) return;

            for (var indexHeight = 0; indexHeight < mapHeight; indexHeight++)
            {
                timeout++;
                var currentLine = (string[]) foundLevel[indexHeight + 2];

                for (var indexWidth = 0; indexWidth < mapWidth; indexWidth++)
                {
                    timeout++;
                    var currentColumn = currentLine[indexWidth];
                    var block = new Block
                    {
                        Sprite = Sprite.Create(
                            GetTexture(currentColumn),
                            new Rect(0, 0, BlockSize*2, BlockSize*2),
                            new Vector2(1, 1), BlockSize*2
                        )
                    };

                    var newGameObject = new GameObject(
                        $"BlockGeneratedId{timeout}"
                    );
                    var spriteRenderer = newGameObject.AddComponent<SpriteRenderer>();
                    var uniq = newGameObject.AddComponent<Uniq>();
                    uniq.line = indexHeight;
                    uniq.column = indexWidth;
                    spriteRenderer.color = new Color(1, 1, 1, 1);
                    spriteRenderer.sprite = block.Sprite;
                    newGameObject.transform.position = new Vector3(
                        (-7.9f) + (BlockSize * indexWidth / 128f), 5f + -(BlockSize * indexHeight / 128f),0
                    );
                    if (newGameObject != null)
                    {
                        newGameObject.AddComponent<MapObjectCollider>();
                        newGameObject.AddComponent<BoxCollider>();
                        Blocks[blocksLength++] = newGameObject;
                    }

                    if (KillNecessary(timeout)) return;
                }

                if (KillNecessary(timeout)) return;
            }
        }

        private Texture2D GetTexture(string currentColumn)
        {
            foreach (var txtObj in Textures)
            {
                if (txtObj.name == currentColumn)
                {
                    return txtObj.texture;
                }
            }
            
            var @default = new Texture2D(BlockSize * 2, BlockSize * 2);
            for (var index = 0; index < BlockSize * 2; index++)
            {
                for (var iteration = 0; iteration < BlockSize * 2; iteration++)
                {
                    @default.SetPixel(index, iteration, Color.red);
                }
            }
            return @default;
        }

        private static bool KillNecessary(int timeout)
        {
            return timeout > 128 * 128;
        }
    }

    public class Block
    {
        public Sprite Sprite;
    }

    public class Uniq : MonoBehaviour
    {
        public int column;
        public int line;
    }

    public class TxtList
    {
        public Texture2D texture;
        public string name;
    }
}