using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class SpriteFontMesh : MonoBehaviour
{
    [System.Serializable]
    public class InputSprite
    {
        public SpriteRenderer renderer;
        public int charPosIndex;
    }

    public enum Alignment
    {
        Centre,
        CentreLeft,
        CentreRight,
        Left,
        Right
    }

    [Header("Text")]
    public string textToDisplay;
    private string currentText;
    public Alignment alignment;
    public bool centreLines;
    [Range(0.0f, 1f)]
    public float percent;
    public List<Color> characterColors = new List<Color>();
    public float alpha = 1;

    static List<string> characterMaps = new List<string>() 
    { 
        @"abcdefghijklmnopqrstuvwxyz0123456789?!.:()<>^_-%&",
        @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'""(!?)+-*/=\_[]%&<>^",
        @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'""(!?)+-*/=\_[]{}|#$%&<>^@~",
        @"0123456789'"""
    };

    static Dictionary<char, float> charWidths = new Dictionary<char, float>
    {
        { 'i', 7f }, { 'j', 10f }, {'l', 11f}, { '1', 9f }, { '!', 7f }, { '.', 6f }, {':', 6f}, {'(', 7f}, {')', 7f}, {'<', 8f}, {'>', 8f },
        {'^', 10f }, {'_', 10f }, {'-', 8f }, {' ', 6f}
    };

    [Header("Font Variables")]
    public int fontType;
    public Texture2D[] fontTextures;
    public Material[] fontMaterials;
    public int charsPerRow;
    public int charWidth = 8, charHeight;
    public float spacing = 1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

    private Color[] colors;

    public int actualCharCount = 0;

    [Header("Wave Offset")]
    public float waveAmplitude;
    public float waveFrequency;
    public float waveScrollSpeed;
    private float waveTime;

    [Header("Input Sprites")]
    public InputSprite[] inputSprites;

    private void Awake()
    {
        ValidateColors();
        if (currentText == null)
        {
            GenerateText(textToDisplay, percent);
        }
    }

    private void Update()
    {
        if (waveScrollSpeed != 0f)
        {
            waveTime += Time.deltaTime * waveScrollSpeed;
            GenerateText(currentText, percent);
        }
    }

    public void GenerateText(string text, float t = 1)
    {
        transform.name = text.Replace("\\n", " ");
        currentText = text;

        GetComponent<MeshRenderer>().material = fontMaterials[fontType];

        if (mesh == null)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }

        switch (fontType)
        {
            case 0: // Small font
                text = text.ToLower();
                charsPerRow = 7;
                charWidth = 12;
                charHeight = 12;
                break;

            case 1: // Gameplay font
                charsPerRow = 11;
                charWidth = 8;
                charHeight = 12;
                break;

            case 2: // Dev font
                charsPerRow = 10;
                charWidth = 8;
                charHeight = 8;
                break;
            case 3: // Stats font
                charsPerRow = 3;
                charWidth = 16;
                charHeight = 16;
                break;
        }

        text = text.Replace("\\n", "\n");
        string[] lines = Regex.Replace(text, @"\\c\d+", "").Split('\n'); // Split text into lines
        actualCharCount = Regex.Replace(text, @"(\\c\d+|\n)", "").Length; // Ignore newlines and colour codes for mesh allocation

        vertices = new Vector3[actualCharCount * 4];
        uvs = new Vector2[actualCharCount * 4];
        triangles = new int[actualCharCount * 6];
        colors = new Color[actualCharCount * 4];

        float maxVisibleWidth = 0; // Width of the currently visible text
        float totalVisibleHeight = -charHeight; // Height for visible lines
        int countedChars = 0;

        // First pass: Calculate maxVisibleWidth & totalVisibleHeight
        for (int lineIndex = 0; lineIndex < lines.Length && countedChars < actualCharCount; lineIndex++)
        {
            float lineWidth = 0;
            string line = lines[lineIndex];

            for (int i = 0; i < line.Length && countedChars < actualCharCount; i++)
            {
                char c = line[i];

                countedChars++;

                if (fontType == 0) lineWidth += (charWidths.ContainsKey(c) ? charWidths[c] : 12f) * spacing;
                else lineWidth += charWidth * spacing;
            }

            if (lineWidth > maxVisibleWidth)
                maxVisibleWidth = lineWidth;

            totalVisibleHeight += charHeight * spacing;
        }

        // Center offset only based on the visible portion
        float xOffset = centreLines ? 0 : Mathf.CeilToInt(-maxVisibleWidth * 0.5f);
        float yOffset = Mathf.CeilToInt(totalVisibleHeight * 0.5f);
        switch (alignment)
        {
            case Alignment.CentreLeft:
                xOffset = 0;
                break;

            case Alignment.CentreRight:
                xOffset = -maxVisibleWidth + 1;
                break;

            case Alignment.Left:
                xOffset = 0;
                yOffset = 0;
                break;

            case Alignment.Right:
                xOffset = -maxVisibleWidth + 1;
                yOffset = 0;
                break;
        }
        yOffset -= charHeight * 0.5f;

        // Reset loop variables
        float currentY = 0;
        int charIndex = 0;
        int spaceIndex = 0;
        bool exitLoops = false;

        for (int lineIndex = 0; lineIndex < lines.Length && !exitLoops; lineIndex++)
        {
            float currentX = 0;
            string line = lines[lineIndex];

            // Calculate the width of this line
            float lineWidth = 0;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (fontType == 0)
                    lineWidth += (charWidths.ContainsKey(c) ? charWidths[c] : 12f) * spacing;
                else
                    lineWidth += charWidth * spacing;
            }

            // If centreLines is enabled, adjust the X offset for this line
            float lineXOffset = centreLines ? -lineWidth * 0.5f: 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (charIndex + spaceIndex >= Mathf.Floor(t * actualCharCount)) // Stop genering quads for text if total characters created is greater than percent of all characters
                {
                    exitLoops = true;
                    break;
                }

                char c = line[i];

                if (c == ' ')
                {
                    if (fontType == 0)
                        currentX += 6f * spacing;
                    else
                        currentX += charWidth * spacing;
                    spaceIndex++;
                    continue;
                }

                if (!TryGetCharacterUV(c, out Vector2 uvMin, out Vector2 uvMax))
                    continue;

                int vertIndex = charIndex * 4;

                // Apply individual line centering if enabled
                CreateQuad(vertIndex, currentX + xOffset + lineXOffset, -currentY + yOffset, uvMin, uvMax);

                if (fontType == 0)
                    currentX += (charWidths.ContainsKey(c) ? charWidths[c] : 12f) * spacing;
                else
                    currentX += charWidth * spacing;

                charIndex++;
            }

            currentY += charHeight * spacing; // Move down for the next line
        }

        foreach (var inputSprite in inputSprites) // Display input bind sprites
        {
            if (inputSprite.charPosIndex < Mathf.Floor(t * actualCharCount))
            {
                inputSprite.renderer.color = Color.white * alpha;
            }
            else
            {
                inputSprite.renderer.color = Color.clear;
            }
        }

        UpdateMesh();
        UpdateMeshColors();
    }

    public void UpdateTextPercent(float t)
    {
        GenerateText(currentText, t);
    }

    public void ValidateColors()
    {
        if (characterColors.Count < 1) characterColors = new List<Color>() { Color.white };
    }

    public void SetColor(int colorIndex, Color newColor)
    {
        characterColors[colorIndex] = newColor;
        UpdateMeshColors();
    }

    public void UpdateMeshColors()
    {
        if (actualCharCount == 0) return; // Prevent errors

        // Create our colors array based on the number of visible characters
        colors = new Color[actualCharCount * 4];

        // Start with a default color, e.g., the first in the list.
        Color currentColor = characterColors[0] * new Color(1, 1, 1, alpha);

        int meshCharIndex = 0;

        string text = Regex.Replace(currentText, @"(\\n| )", "");

        // Iterate over the original text which contains inline control codes.
        for (int i = 0; i < text.Length; i++)
        {
            // Look for a control code: check if we have "\c" followed by a digit.
            if (text[i] == '\\' && i + 2 < text.Length && text[i + 1] == 'c' && char.IsDigit(text[i + 2]))
            {
                // Get the digit, convert to int and ensure it's within bounds.
                int colorIndex = text[i + 2] - '0';
                if (colorIndex >= 0 && colorIndex < characterColors.Count)
                {
                    currentColor = characterColors[colorIndex] * new Color(1, 1, 1, alpha);
                }
                // Skip the control code characters.
                i += 2;
                continue;
            }

            // For a visible character, assign the current color to its four vertices.
            int vertIndex = meshCharIndex * 4;
            colors[vertIndex] = currentColor;
            colors[vertIndex + 1] = currentColor;
            colors[vertIndex + 2] = currentColor;
            colors[vertIndex + 3] = currentColor;

            meshCharIndex++;
        }

        // Finally, assign the colors array to the mesh.
        mesh.colors = colors;
    }

    private bool TryGetCharacterUV(char c, out Vector2 uvMin, out Vector2 uvMax)
    {
        int index = characterMaps[fontType].IndexOf(c);
        if (index == -1)
        {
            uvMin = uvMax = Vector2.zero;
            return false;
        }

        int row = index / charsPerRow;
        int col = index % charsPerRow;

        float uvX = col * (charWidth / (float)fontTextures[fontType].width);
        float uvY = 1f - ((row + 1) * (charHeight / (float)fontTextures[fontType].height));
        float uvW = charWidth / (float)fontTextures[fontType].width;
        float uvH = charHeight / (float)fontTextures[fontType].height;

        uvMin = new Vector2(uvX, uvY);
        uvMax = new Vector2(uvX + uvW, uvY + uvH);
        return true;
    }

    private void CreateQuad(int vertIndex, float xPos, float yPos, Vector2 uvMin, Vector2 uvMax)
    {
        float waveYOffset = 0f;
        if (waveAmplitude != 0f) // Sine wave vertical offset
        {
            waveYOffset = Mathf.Sin((xPos * waveFrequency + waveTime) * Mathf.PI * 2f) * waveAmplitude;
        }
        yPos += waveYOffset;

        int triIndex = (vertIndex / 4) * 6;

        vertices[vertIndex] = new Vector3(xPos, yPos, 0);
        vertices[vertIndex + 1] = new Vector3(xPos + charWidth, yPos, 0);
        vertices[vertIndex + 2] = new Vector3(xPos, yPos + charHeight, 0);
        vertices[vertIndex + 3] = new Vector3(xPos + charWidth, yPos + charHeight, 0);

        uvs[vertIndex] = new Vector2(uvMin.x, uvMin.y);
        uvs[vertIndex + 1] = new Vector2(uvMax.x, uvMin.y);
        uvs[vertIndex + 2] = new Vector2(uvMin.x, uvMax.y);
        uvs[vertIndex + 3] = new Vector2(uvMax.x, uvMax.y);

        triangles[triIndex] = vertIndex;
        triangles[triIndex + 1] = vertIndex + 2;
        triangles[triIndex + 2] = vertIndex + 1;
        triangles[triIndex + 3] = vertIndex + 1;
        triangles[triIndex + 4] = vertIndex + 2;
        triangles[triIndex + 5] = vertIndex + 3;
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        for (int i = 0; i < vertices.Length; i++) // Scale down to 32 pixels per unit
        {
            vertices[i] /= 32f;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }
}
