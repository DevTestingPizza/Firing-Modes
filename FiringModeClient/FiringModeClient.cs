using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FiringModeClient
{
    public class FiringModeClient : BaseScript
    {
        #region General variables
        private bool weaponSafety = false;
        private int firemode = 0;
        List<string> automaticWeapons = new List<string>{
            "WEAPON_MICROSMG",
            "WEAPON_MACHINEPISTOL",
            "WEAPON_MINISMG",
            "WEAPON_SMG",
            "WEAPON_SMG_MK2",
            "WEAPON_ASSAULTSMG",
            "WEAPON_ASSAULTSMG",
            "WEAPON_COMBATPDW",
            "WEAPON_MG",
            "WEAPON_COMBATMG",
            "WEAPON_COMBATMG_MK2",
            "WEAPON_GUSENBERG",
            "WEAPON_ASSAULTRIFLE",
            "WEAPON_ASSAULTRIFLE_MK2",
            "WEAPON_CARBINERIFLE",
            "WEAPON_CARBINERIFLE_MK2",
            "WEAPON_ADVANCEDRIFLE",
            "WEAPON_SPECIALCARBINE",
            "WEAPON_BULLPUPRIFLE",
            "WEAPON_COMPACTRIFLE",
        };
        #endregion


        /// <summary>
        /// Constructor.
        /// </summary>
        public FiringModeClient()
        {
            Tick += OnTick;
            Tick += ShowCurrentMode;
        }

        /// <summary>
        /// OnTick async Task.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            // Load the texture dictionary.
            if (!HasStreamedTextureDictLoaded("mpweaponsgang0"))
            {
                RequestStreamedTextureDict("mpweaponsgang0", true);
                while (!HasStreamedTextureDictLoaded("mpweaponsgang0"))
                {
                    await Delay(0);
                }
            }

            // Only run the rest of the code when the player is holding an automatic weapon.
            if (IsAutomaticWeapon(GetSelectedPedWeapon(PlayerPedId())))
            {

                // If the weapon safety feature is turned on, disable the weapon from firing.
                if (weaponSafety)
                {
                    // Disable shooting.
                    DisablePlayerFiring(PlayerId(), true);

                    // If the user tries to shoot while the safety is enabled, notify them.
                    if (IsDisabledControlJustPressed(0, 24))
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification("~r~Weapon safety mode is enabled!~n~~w~Press ~y~K ~w~to switch it off.", true);
                        PlaySoundFrontend(-1, "Place_Prop_Fail", "DLC_Dmod_Prop_Editor_Sounds", false);
                    }
                }

                // If the player pressed L (7/Slowmotion Cinematic Camera Button) ON KEYBOARD ONLY(!) then switch to the next firing mode.
                if (IsInputDisabled(2) && IsControlJustPressed(0, 7))
                {
                    // Switch case for the firemode, setting it to the different options and notifying the user via a subtitle.
                    switch (firemode) {

                        // If it's currently 0, make it 1 and notify the user.
                        case 0:
                            firemode = 1;
                            CitizenFX.Core.UI.Screen.ShowSubtitle("Weapon firing mode switched to ~b~burst fire~w~.", 3000);
                            PlaySoundFrontend(-1, "Place_Prop_Success", "DLC_Dmod_Prop_Editor_Sounds", false);
                            break;

                        // If it's currently 1, make it 2 and notify the user.
                        case 1:
                            firemode = 2;
                            CitizenFX.Core.UI.Screen.ShowSubtitle("Weapon firing mode switched to ~b~single shot~w~.", 3000);
                            PlaySoundFrontend(-1, "Place_Prop_Success", "DLC_Dmod_Prop_Editor_Sounds", false);
                            break;
                        
                        // If it's currently 2 or somehow anything else, make it 0 and notify the user.
                        case 2:
                        default:
                            firemode = 0;
                            CitizenFX.Core.UI.Screen.ShowSubtitle("Weapon firing mode switched to ~b~full auto~w~.", 3000);
                            PlaySoundFrontend(-1, "Place_Prop_Success", "DLC_Dmod_Prop_Editor_Sounds", false);
                            break;
                    }
                }

                // If the player pressed K (311/Rockstar Editor Keyframe Help display button) ON KEYBOARD ONLY(!) then toggle the safety mode.
                if (IsInputDisabled(2) && IsControlJustPressed(0, 311))
                {
                    weaponSafety = !weaponSafety;
                    CitizenFX.Core.UI.Screen.ShowSubtitle("~y~Weapon safety mode ~g~" + (weaponSafety ? "~g~enabled" : "~r~disabled") + "~y~.", 3000);
                    PlaySoundFrontend(-1, "Place_Prop_Success", "DLC_Dmod_Prop_Editor_Sounds", false);
                }

                // Now on to the handling of the different firing modes.
                // (1) Burst shot firing mode
                if (firemode == 1)
                {
                    // If the player starts shooting... 
                    if (IsControlJustPressed(0, 24))
                    {
                        // ...wait 300ms(for most guns this allows about 3 bullets to be shot when holding down the trigger)
                        await Delay(300);

                        // After that, if the user is still pulling the trigger, disable shooting for the player while still allowing them to aim.
                        // As soon as the user lets go of the trigger, this while loop will be stopped and the user can pull the trigger again.
                        while (IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24))
                        {
                            DisablePlayerFiring(PlayerId(), true);
                            
                            // Because we're now in a while loop, we need to add a delay to prevent the game from freezing up/crashing.
                            await Delay(0);
                        }
                    }
                }
                // (2) Single shot firing mode
                else if (firemode == 2)
                {
                    // If the player starts shooting...
                    if (IsControlJustPressed(0, 24))
                    {
                        // ...disable the weapon after the first shot and keep it disabled as long as the trigger is being pulled.
                        // once the player lets go of the trigger, the loop will stop and they can pull it again.
                        while (IsControlPressed(0, 24) || IsDisabledControlPressed(0, 24))
                        {
                            DisablePlayerFiring(PlayerId(), true);
                            
                            // Because we're now in a while loop, we need to add a delay to prevent the game from freezing up/crashing.
                            await Delay(0);
                        }
                    }
                }
                // We don't need to have a function that handles firing mode 0, since that's full auto mode and that's enabled by default anyway.
            }
        }

        /// <summary>
        /// Checks if the given weapon hash (int) is present in the weapons list defined at the top of this file.
        /// This also returns false regardless of the weapon the player has equipped, if the player is in a vehicle.
        /// Meaning the fire mode in a vehicle will always be full auto for all weapons.
        /// </summary>
        /// <param name="weaponHash"></param>
        /// <returns>true/false (bool), true if the weapon is found in the list, false if it's not.</returns>
        private bool IsAutomaticWeapon(int weaponHash)
        {
            foreach (string weapon in automaticWeapons)
            {
                if (GetHashKey(weapon) == weaponHash)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Used to draw text ont the screen on the specified x,y
        /// </summary>
        /// <param name="text"></param>
        /// <param name="posx"></param>
        /// <param name="posy"></param>
        private void ShowText(string text, float posx, float posy)
        {
            SetTextFont(4);
            SetTextScale(0.0f, 0.31f);
            SetTextJustification(1);
            SetTextColour(250, 250, 120, 255);
            SetTextDropshadow(1, 255, 255, 255, 255);
            SetTextEdge(1, 0, 0, 0, 205);
            BeginTextCommandDisplayText("STRING");
            AddTextComponentSubstringPlayerName(text);
            EndTextCommandDisplayText(posx, posy);
        }

        /// <summary>
        /// Show the current firing mode visually just below the ammo count.
        /// Called every frame.
        /// </summary>
        private async Task ShowCurrentMode()
        {
            // Just add a wait in here when it's not being displayed, to remove the async warnings. 
            if (!IsAutomaticWeapon(GetSelectedPedWeapon(PlayerPedId())))
            {
                await Delay(0);
            }
            // If the weapon is a valid weapon that has different firing modes, then this will be shown.
            else
            {
                if (weaponSafety)
                {
                    ShowText(" ~r~X", 0.975f, 0.065f);
                }
                else
                {
                    switch (firemode)
                    {
                        case 1:
                            ShowText("||", 0.975f, 0.065f);
                            break;
                        case 2:
                            ShowText("|", 0.975f, 0.065f);
                            break;
                        case 0:
                        default:
                            ShowText("|||", 0.975f, 0.065f);
                            break;
                    }
                }
                DrawSprite("mpweaponsgang0", "w_ar_carbinerifle_mag1", 0.975f, 0.06f, 0.099f, 0.099f, 0.0f, 200, 200, 200, 255);
            }
        }
    }
}
