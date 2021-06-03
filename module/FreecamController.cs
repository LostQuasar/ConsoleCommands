using Comfort.Common;
using EFT;
using EFT.UI;
using UnityEngine;

namespace Spaceman.ConsoleCommands
{
    public class FreecamController : MonoBehaviour
    {
        private GameObject mainCamera;
        private Freecam freeCamScript;

        private BattleUIScreen playerUi;
        private bool uiHidden;
        public bool UIHiddenStatus { get => uiHidden; }

        // TODO:
        // Menu for adjusting settings
        // Config file for default settings
        // Configurable keybinds

        // TODO MAYBE:
        // Hide player weapon
        // Hide version number UI element
        // FreeCam controller support (camera could be smoother with an analog stick, apparently)
        // Adjusting speed/sensitivity without a menu (Ctrl+ScrollWheel for example)

        /// <summary>
        /// Toggles the Freecam mode
        /// </summary>
        public void ToggleCamera()
        {
            // Get our own Player instance. Null means we're not in a raid
            var localPlayer = GetLocalPlayerFromWorld();
            if (localPlayer == null)
            {
                return;
            }

            // If we don't have the main camera object cached, go look for it in the scene
            if (mainCamera == null)
            {
                // Finding a GameObject by name directly is apparantly better than searching for objects of a type.
                // 'FPS Camera' is our main camera instance - so we just grab that
                mainCamera = GameObject.Find("FPS Camera");
                if (mainCamera == null)
                {
                    Debug.LogError("Terkoiz.Freecam: ERROR: Failed to locate main camera");
                    PreloaderUI.Instance.Console.AddLog("ERROR: Failed to locate main camera", "FreeCam");
                    return;
                }
            }

            // Create the Freecam script and attach it to the main camera
            if (freeCamScript == null)
            {
                freeCamScript = mainCamera.AddComponent<Freecam>();
            }

            // We disable the player's GameObject, which prevents the player from moving and interacting with the world while we're in Freecam mode
            // Also, since the camera position keeps getting updated to where the player is, disabling the player's GameObject also stops this behaviour
            if (!freeCamScript.IsActive)
            {
                localPlayer.gameObject.SetActive(false);
                freeCamScript.IsActive = true;
            }
            else
            {
                freeCamScript.IsActive = false;
                localPlayer.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// When triggered during Freecam mode, teleports the player to where the camera was and exits Freecam mode
        /// </summary>
        public void MovePlayerToCamera()
        {
            var localPlayer = GetLocalPlayerFromWorld();
            if (localPlayer == null)
            {
                return;
            }

            // If we don't have the main camera cached, it means we have yet to enter Freecam mode and there is nowhere to teleport to
            if (mainCamera == null)
            {
                return;
            }

            // We basically do what ToggleCamera() does, but with extra code inbetween
            if (freeCamScript.IsActive)
            {
                freeCamScript.IsActive = false;
                // We grab the camera's position, but we subtract a bit off the Y axis, because the players coordinate origin is at the feet
                var position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - 1.8f, mainCamera.transform.position.z);
                localPlayer.gameObject.transform.SetPositionAndRotation(position, mainCamera.transform.rotation);
                localPlayer.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the main UI (health, stamina, stance, hotbar, etc.)
        /// </summary>
        public void ToggleUi()
        {
            // Check if we're currently in a raid
            if (GetLocalPlayerFromWorld() == null)
            {
                return;
            }

            // If we don't have the UI Component cached, go look for it in the scene
            if (playerUi == null)
            {
                playerUi = GameObject.Find("BattleUIScreen").GetComponent<BattleUIScreen>();

                if (playerUi == null)
                {
                    PreloaderUI.Instance.Console.AddLog("ERROR: Failed to locate player UI", "FreeCam");
                    return;
                }
            }

            playerUi.gameObject.SetActive(uiHidden);
            uiHidden = !uiHidden;
        }

        /// <summary>
        /// Gets the current <see cref="Player"/> instance if it's available
        /// </summary>
        /// <returns>Local <see cref="Player"/> instance; returns null if the game is not in raid</returns>
        public static Player GetLocalPlayerFromWorld()
        {
            // If the GameWorld instance is null or has no RegisteredPlayers, it most likely means we're not in a raid
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.RegisteredPlayers == null)
            {
                return null;
            }

            // One of the RegisterdPlayers will have the IsYourPlayer flag set, which will be our own Player instance
            return gameWorld.RegisteredPlayers.Find(p => p.IsYourPlayer);
        }
    }
}