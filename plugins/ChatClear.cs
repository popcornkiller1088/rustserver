using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Oxide.Plugins
{
    [Info("Chat Clear", "Wulf", "2.0.0")]
    [Description("Clears the chat for player(s) when joining the server or on command")]
    class ChatClear : CovalencePlugin
    {
        #region Configuration

        private Configuration config;

        public class Configuration
        {
            [JsonProperty("Clear chat on connect")]
            public bool ClearOnConnect = true;

            [JsonProperty("Exclude admin from clearing")]
            public bool ExcludeAdmin = false;

            [JsonProperty("Number of lines to clear (ex. 300)")]
            public int NumberOfLines = 300;

            [JsonProperty("Show chat cleared message")]
            public bool ShowMessage = false;

            public string ToJson() => JsonConvert.SerializeObject(this);

            public Dictionary<string, object> ToDictionary() => JsonConvert.DeserializeObject<Dictionary<string, object>>(ToJson());
        }

        protected override void LoadDefaultConfig() => config = new Configuration();

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<Configuration>();
                if (config == null)
                {
                    throw new JsonException();
                }

                if (!config.ToDictionary().Keys.SequenceEqual(Config.ToDictionary(x => x.Key, x => x.Value).Keys))
                {
                    LogWarning("Configuration appears to be outdated; updating and saving");
                    SaveConfig();
                }
            }
            catch
            {
                LogWarning($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Log($"Configuration changes saved to {Name}.json");
            Config.WriteObject(config, true);
        }

        #endregion Configuration

        #region Localization

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CommandClearAll"] = "clearall",
                ["CommandClearSelf"] = "clear",
                ["ChatCleared"] = "Chat has been cleared, mmm fresh!",
                ["NotAllowed"] = "You are not allowed to use the '{0}' command"
            }, this);
        }

        #endregion Localization

        #region Initialization

        private const string permAll = "chatclear.all";
        private const string permSelf = "chatclear.self";

        private void Init()
        {
            AddLocalizedCommand(nameof(CommandClearAll));
            AddLocalizedCommand(nameof(CommandClearSelf));

            permission.RegisterPermission(permAll, this);
            permission.RegisterPermission(permSelf, this);
            MigratePermission("chatcleaner.all", permAll);
            MigratePermission("chatcleaner.self", permSelf);

            if (!config.ClearOnConnect)
            {
                Unsubscribe(nameof(OnUserConnected));
            }
        }

        #endregion Initialization

        #region Chat Clearing

        private void ClearChat(IPlayer player)
        {
            if (config.ExcludeAdmin && player.IsAdmin)
            {
                return;
            }

            player.Message(new string('\n', config.NumberOfLines));

            if (config.ShowMessage)
            {
                Message(player, "ChatCleared");
            }
        }

        private void CommandClearAll(IPlayer player, string command)
        {
            if (!player.HasPermission(permAll))
            {
                Message(player, "NotAllowed", command);
                return;
            }

            foreach (IPlayer target in players.Connected)
            {
                ClearChat(target);
            }
        }

        private void CommandClearSelf(IPlayer player, string command)
        {
            if (!player.HasPermission(permSelf) || player.IsServer)
            {
                Message(player, "NotAllowed", command);
                return;
            }

            ClearChat(player);
        }

        private void OnUserConnected(IPlayer player) => ClearChat(player);

        #endregion Chat Clearing

        #region Helpers

        private void AddLocalizedCommand(string command)
        {
            foreach (string language in lang.GetLanguages(this))
            {
                Dictionary<string, string> messages = lang.GetMessages(language, this);
                foreach (KeyValuePair<string, string> message in messages)
                {
                    if (message.Key.Equals(command))
                    {
                        if (!string.IsNullOrEmpty(message.Value))
                        {
                            AddCovalenceCommand(message.Value, command);
                        }
                    }
                }
            }
        }

        private string GetLang(string langKey, string playerId = null, params object[] args)
        {
            return string.Format(lang.GetMessage(langKey, this, playerId), args);
        }

        private void Message(IPlayer player, string langKey, params object[] args)
        {
            player.Reply(GetLang(langKey, player.Id, args));
        }

        private void MigratePermission(string oldPerm, string newPerm)
        {
            foreach (string groupName in permission.GetPermissionGroups(oldPerm))
            {
                permission.GrantGroupPermission(groupName, newPerm, null);
                permission.RevokeGroupPermission(groupName, oldPerm);
            }

            foreach (string playerId in permission.GetPermissionUsers(oldPerm))
            {
                permission.GrantUserPermission(Regex.Replace(playerId, "[^0-9]", ""), newPerm, null);
                permission.RevokeUserPermission(Regex.Replace(playerId, "[^0-9]", ""), oldPerm);
            }
        }

        #endregion Helpers
    }
}
