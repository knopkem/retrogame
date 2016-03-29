#region File Description

//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using GameEngine.ScreenManager;

#endregion

namespace RetroGame.Screens
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    public class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        private static readonly string[] languages = {"English", "French", "German"};
        private static int currentLanguage;

        private static readonly string[] levels = {"Easy", "Standard", "Hard", "Nightmare"};
        private static int currentLevel;

        private static bool isAudioOn = true;

        private static string videoString = "VideoSettingsNotImplemented";
        private readonly MenuEntry audioMenuEntry;
        private readonly MenuEntry gameplayMenuEntry;
        private readonly MenuEntry languageMenuEntry;
        private readonly MenuEntry videoMenuEntry;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            gameplayMenuEntry = new MenuEntry(string.Empty);
            languageMenuEntry = new MenuEntry(string.Empty);
            audioMenuEntry = new MenuEntry(string.Empty);
            videoMenuEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            var back = new MenuEntry("Back");

            // Hook up menu event handlers.
            gameplayMenuEntry.Selected += GameplayMenuEntrySelected;
            languageMenuEntry.Selected += LanguageMenuEntrySelected;
            audioMenuEntry.Selected += AudioMenuEntrySelected;
            videoMenuEntry.Selected += VideoMenuEntrySelected;
            back.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(gameplayMenuEntry);
            MenuEntries.Add(languageMenuEntry);
            MenuEntries.Add(audioMenuEntry);
            MenuEntries.Add(videoMenuEntry);
            MenuEntries.Add(back);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        private void SetMenuEntryText()
        {
            gameplayMenuEntry.Text = "Game Difficulty: " + levels[currentLevel];
            languageMenuEntry.Text = "Language: " + languages[currentLanguage];
            audioMenuEntry.Text = "Audio: " + (isAudioOn ? "on" : "off");
            videoMenuEntry.Text = "Video: " + videoString;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Event handler for when the GamePlay menu entry is selected.
        /// </summary>
        private void GameplayMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentLevel = (currentLevel + 1)%levels.Length;

            SetMenuEntryText();
        }


        /// <summary>
        /// Event handler for when the Language menu entry is selected.
        /// </summary>
        private void LanguageMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentLanguage = (currentLanguage + 1)%languages.Length;

            SetMenuEntryText();
        }


        /// <summary>
        /// Event handler for when the Audio menu entry is selected.
        /// </summary>
        private void AudioMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            isAudioOn = !isAudioOn;

            SetMenuEntryText();
        }


        /// <summary>
        /// Event handler for when the video menu entry is selected.
        /// </summary>
        private void VideoMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            SetMenuEntryText();
        }

        #endregion
    }
}