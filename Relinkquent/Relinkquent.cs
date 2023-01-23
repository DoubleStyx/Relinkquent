using HarmonyLib;
using NeosModLoader;
using System;
using FrooxEngine;
using BaseX;
using FrooxEngine.UIX;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Relinkquent
{
    public class Relinkquent : NeosMod
    {
        public override string Name => "Relinkquent";
        public override string Author => "kka429";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/rassi0429/Relinkquent"; // this line is optional and can be omitted

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("dev.kokoa.relinkquent");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(FriendsDialog))]
        [HarmonyPatch("AddMessage")]
        class RelinkquentPatch
        {
            public static bool harmonyHasChecked = false;
            public static bool harmonyHasPatch = false;

            [HarmonyAfter("dev.kokoa.messagecopy")]
            static void Postfix(UIBuilder ___messagesUi, FriendsDialog __instance, ref Image __result, CloudX.Shared.Message message)
            {
                if (message.MessageType != CloudX.Shared.MessageType.Text) return;
                if (!harmonyHasChecked) harmonyHasPatch = Harmony.HasAnyPatches("dev.kokoa.messagecopy");
                List<Slot> child = ___messagesUi.Current.GetAllChildren();
                List<Button> buttonList = new List<Button>();

                // taken from https://github.com/rassi0429/MessageCopy
                if (!harmonyHasPatch)
                {
                    foreach (Slot c in child)
                    {
                        if (c.GetComponent<Text>() != null)
                        {
                            Text text = c.GetComponent<Text>();
                            string msg = text.Content;
                            text.Destroy();
                            var ui = new UIBuilder(c);
                            Button button = ui.Button(msg, new color(1, 1, 1, 0));
                            button.LocalPressed += (IButton btn, ButtonEventData _) => { btn.World.InputInterface.Clipboard.SetText(btn.LabelText); };
                            buttonList.Add(button);
                            var btnText = c.GetComponentInChildren<Text>();
                            btnText.Align = message.IsSent ? Alignment.MiddleRight : Alignment.MiddleLeft;
                        }
                    }
                }
                else
                {
                    foreach (Slot c in child)
                    {
                        Button button = c.GetComponent<Button>();
                        if (button != null)
                        {
                            buttonList.Add(button);
                        }
                    }
                }
                string expr = @"\w+:(\/?\/?)[^\s]+";
                foreach (Button button in buttonList)
                {
                    string msg = button.Slot[0].GetComponent<Text>().Content.Value;
                    MatchCollection mc = Regex.Matches(msg, expr);
                    foreach (Match url in mc)
                    {
                        if (Uri.IsWellFormedUriString(msg, UriKind.RelativeOrAbsolute))
                        {
                            Hyperlink link = button.Slot.AttachComponent<Hyperlink>();
                            link.URL.Value = new Uri(url.Value);
                            link.Reason.Value = "Clicked Hyperlink";
                        }
                    }
                }
            }
        }
    }
}