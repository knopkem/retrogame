using System;
using System.Collections.Generic;
using GameEngine.ScreenManager;

namespace RetroGame.Screens
{
    internal class DynamicMenuScreen : MenuScreen
    {
        public DynamicMenuScreen(List<string> menuItems) : base("")
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);

            foreach (string item in menuItems)
            {
                var entry = new MenuEntry(item);
                entry.SelectedExtended += entry_Selected2;
                // Add entries to the menu.
                MenuEntries.Add(entry);
            }

            // add exit entry
            var exitEntry = new MenuEntry("[Close Window]");
            exitEntry.Selected += exitEntry_Selected;
            MenuEntries.Add(exitEntry);
        }

        public event EventHandler<MenuEntry.CustomResultEventArgs> SelectedMenu;

        private void exitEntry_Selected(object sender, PlayerIndexEventArgs e)
        {
            ExitScreen();
        }

        private void entry_Selected2(object sender, MenuEntry.CustomResultEventArgs e)
        {
            // send event including the name of the menu
            SelectedMenu(this, e);
            // close the menu
            ExitScreen();
        }
    }
}