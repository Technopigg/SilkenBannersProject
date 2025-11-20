using UnityEngine;

public class DevConsole : MonoBehaviour
{
    private string input = "";
    private bool showConsole = false;

    void Update()
    {
        // Toggle console with backquote (`) key
        if (Input.GetKeyDown(KeyCode.BackQuote))
            showConsole = !showConsole;
    }

    void OnGUI()
    {
        if (!showConsole) return;

        GUI.Box(new Rect(10, 10, 400, 100), "Dev Console");

        // Give the text field a control name so we can focus it
        GUI.SetNextControlName("ConsoleInput");
        input = GUI.TextField(new Rect(20, 40, 380, 20), input);

        // Auto-focus the text field when console is shown
        if (Event.current.type == EventType.Repaint)
        {
            GUI.FocusControl("ConsoleInput");
        }

        if (GUI.Button(new Rect(20, 70, 80, 20), "Run"))
        {
            HandleCommand(input);
            input = "";
        }
    }

    void HandleCommand(string cmd)
    {
        Debug.Log($"[Console] Command: {cmd}");

        if (cmd.StartsWith("zoom"))
        {
            float zoom = float.Parse(cmd.Split(' ')[1]);
            Camera.main.orthographicSize = zoom;
            Debug.Log($"[Console] Camera zoom set to {zoom}");
        }
        else if (cmd == "goto EnemySpawn")
        {
            var spawner = FindObjectOfType<SquadSpawner>();
            if (spawner != null && spawner.enemySpawnPoint != null)
            {
                Camera.main.transform.position = spawner.enemySpawnPoint.position + new Vector3(0, 20, -20);
                Debug.Log("[Console] Camera moved to Enemy spawn point");
            }
        }
        else if (cmd == "list squads")
        {
            var squads = FindObjectsOfType<Squad>();
            foreach (var s in squads)
                Debug.Log($"[Console] Squad {s.squadID} with {s.soldiers.Count} soldiers");
        }
    }
}