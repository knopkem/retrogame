#region File Description

//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using GameEngine.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

#endregion

namespace RetroGame.Screens
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class GameMainMenuScreen : MenuScreen
    {
        #region Initialization

        private ContentManager _content;


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public GameMainMenuScreen()
            : base("")
        {
            // Create our menu entries.
            var targetMenuEntry = new MenuEntry("Show target infos");
            var hireMenuEntry = new MenuEntry("Hire team members");
            var fenceMenuEntry = new MenuEntry("Pick a fence");
            var planRaidMenuEntry = new MenuEntry("Plan the heist");

            // Hook up menu event handlers.
            targetMenuEntry.Selected += targetMenuEntry_Selected;
            hireMenuEntry.Selected += hireMenuEntry_Selected;
            planRaidMenuEntry.Selected += planRaidMenuEntry_Selected;

            // Add entries to the menu.
            MenuEntries.Add(targetMenuEntry);
            MenuEntries.Add(hireMenuEntry);
            MenuEntries.Add(fenceMenuEntry);
            MenuEntries.Add(planRaidMenuEntry);
        }

        private void targetMenuEntry_Selected(object sender, PlayerIndexEventArgs e)
        {
            //LoadingScreen.Load(ScreenManager, false, e.PlayerIndex,
            //      new InteractionGameplayScreen3D());
        }

        private void hireMenuEntry_Selected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, e.PlayerIndex,
                               new MemberSelectionScreen());
        }


        private void planRaidMenuEntry_Selected(object sender, PlayerIndexEventArgs e)
        {
            if (GameManager.Instance.Data.GlobalCharPool.GetNumberOfSelectedChars() == 0)
            {
                var confirmExitMessageBox = new MessageBoxScreen("You need to select at least one team member!");
                ScreenManager.AddScreen(confirmExitMessageBox, 0);
            }
            else
            {
                LoadingScreen.Load(ScreenManager, false, e.PlayerIndex, new PlanningGameplayScreen());
            }
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Quit To Main Menu?";

            var confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, e.PlayerIndex,
                               new BackgroundScreen(BackgroundScreen.ContentReference.MainBackground),
                               new MainMenuScreen());
        }


        public override void LoadContent()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");
        }

        #endregion
    }
}