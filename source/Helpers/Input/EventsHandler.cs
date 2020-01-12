﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Sidekick.Helpers.Localization;
using Sidekick.Helpers.NativeMethods;
using Sidekick.Helpers.POEPriceInfoAPI;
using Sidekick.Helpers.POETradeAPI;
using Sidekick.Windows.Overlay;
using Sidekick.Windows.PricePrediction;

namespace Sidekick.Helpers
{
    public static class EventsHandler
    {
        private static IKeyboardMouseEvents _globalHook;

        public static void Initialize()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHookKeyPressHandler;
            //_globalHook.MouseWheelExt += GlobalHookMouseScrollHandler;

            // #TODO: Remap all actions to json read local file for allowing user bindings
            var exit = Sequence.FromString("Shift+Z, Shift+Z");
            var assignment = new Dictionary<Sequence, Action>
            {
                { exit, ExitApplication }
            };

            _globalHook.OnSequence(assignment);
        }

        private static void GlobalHookKeyPressHandler(object sender, KeyEventArgs e)
        {
            if (!TradeClient.IsReady)
            {
                return;
            }

            if (OverlayController.IsDisplayed && e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                OverlayController.Hide();
            }
            else if (ProcessHelper.IsPathOfExileInFocus())
            {
                if (!OverlayController.IsDisplayed && e.Modifiers == Keys.Control && e.KeyCode == Keys.D)
                {
                    e.Handled = true;
                    Task.Run(TriggerItemFetch);
                }
                else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.W)
                {
                    e.Handled = true;
                    Task.Run(TriggerItemWiki);
                }
                else if(e.Modifiers == Keys.None && e.KeyCode == Keys.F5)
                {
                    e.Handled = true;
                    Task.Run(TriggerHideout);
                }
                else if(e.Modifiers == Keys.Alt && e.KeyCode == Keys.D)      // Better solution would beo on Ctrl + D and switch Action for rare item
                {
                    e.Handled = true;
                    Task.Run(TriggerItemPrediction);
                }
            }
        }

        private static void GlobalHookMouseScrollHandler(object sender, MouseEventExtArgs e)
        {
            if (!TradeClient.IsReady || !ProcessHelper.IsPathOfExileInFocus())
            {
                return;
            }

            // Ctrl + Scroll wheel to move between stash tabs.
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) > 0)
            {
                e.Handled = true;
                string key = e.Delta > 0 ? Input.KeyCommands.STASH_LEFT : Input.KeyCommands.STASH_RIGHT;
                SendKeys.Send(key);
            }
        }

        private static async void TriggerItemFetch()
        {
            Logger.Log("TriggerItemFetch()");

            Item item = TriggerCopyAction();
            if (item != null)
            {
                OverlayController.Open();

                var queryResult = await TradeClient.GetListings(item);

                if (queryResult != null)
                {
                    OverlayController.SetQueryResult(queryResult);
                    return;
                }
            }

            OverlayController.Hide();
        }

        private static async void TriggerItemPrediction()
        {
            Logger.Log("TriggerItemPrediction()");
            var itemText = GetItemText();
            LanguageSettings.DetectLanguage(itemText);

            if (!itemText.Contains(LanguageSettings.Provider.DescriptionRarity))     // Trigger only on item
            {
                return;
            }

            if(!itemText.Contains(LanguageSettings.Provider.RarityRare))        // Only trigger on rare items atm
            {
                return;
            }

            var pred = await PriceInfoClient.GetItemPricePrediction(itemText);
            PricePredictionViewController.Open();

            if(pred != null)
            {
                PricePredictionViewController.SetResult(pred);
                return;
            }

            PricePredictionViewController.Hide();
        }

        private static void TriggerItemWiki()
        {
            Logger.Log("TriggerItemWiki()");

            var item = TriggerCopyAction();
            if (item != null)
            {
                POEWikiAPI.POEWikiHelper.Open(item);
            }
        }

        /// <summary>
        /// Triggers the goto hideout command and restores the chat to your previous entry
        /// </summary>
        private static void TriggerHideout()
        {
            Logger.Log("TriggerHideout()");

            SendKeys.SendWait(Input.KeyCommands.HIDEOUT);
        }

        private static void ExitApplication()
        {
            Logger.Log("ExitApplication()");

            Application.Exit();
        }

        private static string GetItemText()
        {
            // Trigger copy action.
            SendKeys.SendWait(Input.KeyCommands.COPY);
            Thread.Sleep(100);

            // Retrieve clipboard.
            var itemText = ClipboardHelper.GetText();
            return itemText;
        }

        private static Item TriggerCopyAction()
        {
            var itemText = GetItemText();

            // Detect the language of the item in the clipboard.
            LanguageSettings.DetectLanguage(itemText);       

            // Parse and return item
            return ItemParser.ParseItem(itemText);
        }

        public static void Dispose()
        {
            _globalHook?.Dispose();
        }
    }
}
