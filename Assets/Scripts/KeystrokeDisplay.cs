using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class KeystrokeDisplay : MonoBehaviour
{
    [Header("UI Setup")]
    public TMP_Text keyText;

    void Update()
    {
        if (keyText == null) return;

        List<string> pressedKeys = new List<string>();

        // --- KEYS FÜR BEWEGUNG ---
        if (Input.GetKey(KeyCode.W)) pressedKeys.Add("W");
        if (Input.GetKey(KeyCode.A)) pressedKeys.Add("A");
        if (Input.GetKey(KeyCode.S)) pressedKeys.Add("S");
        if (Input.GetKey(KeyCode.D)) pressedKeys.Add("D");
        if (Input.GetKey(KeyCode.Space)) pressedKeys.Add("SPACE");
        if (Input.GetKey(KeyCode.LeftControl)) pressedKeys.Add("L CTRL");

        // --- KEYS FÜR LASER UND SCHUSS STEUERUNGEN ---
        if (Input.GetKey(KeyCode.F)) pressedKeys.Add("F(Laser)");
        if (Input.GetKey(KeyCode.R)) pressedKeys.Add("R");
        if (Input.GetKey(KeyCode.Alpha1)) pressedKeys.Add("1");
        if (Input.GetKey(KeyCode.Alpha2)) pressedKeys.Add("2");

        // --- MAUS FÜR SCHUSS UND GREIFHAKEN ---
        // 0 = Linksklick, 1 = Rechtsklick
        if (Input.GetMouseButton(0)) pressedKeys.Add("LMB");
        if (Input.GetMouseButton(1)) pressedKeys.Add("RMB");

        // Mausrad für die Verlängerung/Verkürzung des Seils
        if (Input.mouseScrollDelta.y > 0) pressedKeys.Add("Scroll Up");
        if (Input.mouseScrollDelta.y < 0) pressedKeys.Add("Scroll Down");

        // --- TEXT AKTUALISIEREN ---
        if (pressedKeys.Count > 0)
        {
            // Verbindet alle Einträge der Liste mit einem " + "
            keyText.text = string.Join(" + ", pressedKeys);
        }
        else
        {
            // Wenn nichts gedrückt wird, leere den Text
            keyText.text = "";
        }
    }
}