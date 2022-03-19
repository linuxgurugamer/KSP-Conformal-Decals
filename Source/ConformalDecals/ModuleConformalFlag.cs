using UnityEngine;

namespace ConformalDecals {
    public class ModuleConformalFlag : ModuleConformalDecal {
        private const string DefaultFlag = "Squad/Flags/default";

        [KSPField(isPersistant = true)] public string flagUrl = DefaultFlag;

        [KSPField(isPersistant = true)] public bool useCustomFlag;

        public string MissionFlagUrl {
            get {
                if (HighLogic.LoadedSceneIsEditor) {
                    return string.IsNullOrEmpty(EditorLogic.FlagURL) ? HighLogic.CurrentGame.flagURL : EditorLogic.FlagURL;
                }

                if (HighLogic.LoadedSceneIsFlight) {
                    return string.IsNullOrEmpty(part.flagURL) ? HighLogic.CurrentGame.flagURL : part.flagURL;
                }

                // If we are not in game, use the default flag (for icon rendering)
                return DefaultFlag;
            }
        }

        public override void OnStart(StartState state) {
            if (HighLogic.LoadedSceneIsEditor) {
                // Register flag change event
                GameEvents.onMissionFlagSelect.Add(OnEditorFlagSelected);

                // Register reset button event
                Events[nameof(ResetFlagButton)].guiActiveEditor = useCustomFlag;
            }

            base.OnStart(state);
        }

        public override void OnDestroy() {
            if (HighLogic.LoadedSceneIsEditor) {
                // Unregister flag change event
                GameEvents.onMissionFlagSelect.Remove(OnEditorFlagSelected);
            }

            base.OnDestroy();
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "#LOC_ConformalDecals_gui-select-flag")]
        public void SelectFlagButton() {
            // Button for selecting a flag
            // This is a bit of a hack to bring up the stock flag selection menu
            // When its done, it calls OnCustomFlagSelected()

            // ReSharper disable once PossibleNullReferenceException
            var flagBrowser = (Instantiate((Object) (new FlagBrowserGUIButton(null, null, null, null)).FlagBrowserPrefab) as GameObject).GetComponent<FlagBrowser>();
            flagBrowser.OnFlagSelected = OnCustomFlagSelected;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "#LOC_ConformalDecals_gui-reset-flag")]
        public void ResetFlagButton() {
            // we are no longer using a custom flag, so instead use the mission or agency flag
            SetFlag("Mission", false, true);

            // disable the reset button, since it no longer makes sense
            Events[nameof(ResetFlagButton)].guiActiveEditor = false;
        }

        private void OnCustomFlagSelected(FlagBrowser.FlagEntry newFlagEntry) {
            // Callback for when a flag is selected in the menu spawned by SelectFlag()

            // we are now using a custom flag with the URL of the new flag entry
            SetFlag(newFlagEntry.textureInfo.name, true, true);

            // make sure the reset button is now available
            Events[nameof(ResetFlagButton)].guiActiveEditor = true;
        }

        private void OnEditorFlagSelected(string newFlagUrl) {
            // Callback for when a new mission flag is selected in the editor
            // Since this callback is called for all modules, we only need to update this module
            // Updating symmetry counterparts would be redundent

            // if we are using the mission flag, update it. otherwise ignore the call
            if (!useCustomFlag) {
                SetFlag(newFlagUrl, false, false);
            }
        }

        private void SetFlag(string newFlagUrl, bool isCustom, bool recursive) {
            // Function to set the flag URL, the custom flag
            
            // Set values
            flagUrl = newFlagUrl;
            useCustomFlag = isCustom;
            
            // Update material and projection
            UpdateAll();
            
            // Update symmetry counterparts if called to
            if (recursive) {
                foreach (var counterpart in part.symmetryCounterparts) {
                    var decal = counterpart.GetComponent<ModuleConformalFlag>();
                    decal.SetFlag(newFlagUrl, isCustom, false);
                }
            }
        }

        protected override void UpdateMaterials() {
            // get the decal material property for the decal texture
            var textureProperty = materialProperties.AddOrGetTextureProperty("_Decal", true);

            if (useCustomFlag) { // set the texture to the custom flag
                textureProperty.TextureUrl = flagUrl;
            } else { // set the texture to the mission flag
                textureProperty.TextureUrl = MissionFlagUrl;
            }

            base.UpdateMaterials();
        }
    }
}